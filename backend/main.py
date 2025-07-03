from fastapi import FastAPI, Depends, HTTPException, Request, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi.responses import JSONResponse
from pydantic import BaseModel, EmailStr
from typing import Optional
import jwt
import datetime
from passlib.context import CryptContext
from motor.motor_asyncio import AsyncIOMotorClient
import os
from pymongo.errors import DuplicateKeyError

# Models
class UserCreate(BaseModel):
    name: str
    email: EmailStr
    password: str

class UserLogin(BaseModel):
    email: EmailStr
    password: str

class UserResponse(BaseModel):
    name: str
    email: EmailStr

class TokenResponse(BaseModel):
    token: str
    user: UserResponse

# Additional models
class ForgotPasswordRequest(BaseModel):
    email: EmailStr

class ResetPasswordRequest(BaseModel):
    token: str
    new_password: str

# Setup
app = FastAPI(title="Authentication API")
security = HTTPBearer()
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# Update the CORS settings for better security
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:3000",  # Development frontend
        "http://localhost:5173",  # Vite default port
        "https://yourdomain.com"  # Production domain
    ],
    allow_credentials=True,
    allow_methods=["GET", "POST", "PUT", "DELETE"],
    allow_headers=["*"],
)

# Set secure cookie options
from fastapi.middleware.httpsredirect import HTTPSRedirectMiddleware
if os.environ.get('ENVIRONMENT') == 'production':
    app.add_middleware(HTTPSRedirectMiddleware)

# Config
SECRET_KEY = os.environ.get('SECRET_KEY', 'default-dev-key')
MONGO_URL = os.environ.get('MONGO_URL', 'mongodb://localhost:27017')
client = AsyncIOMotorClient(MONGO_URL)
db = client.user_database
users_collection = db.users

# Create a blacklist collection
token_blacklist = db.token_blacklist

# Ensure email index
@app.on_event("startup")
async def startup_db_client():
    await users_collection.create_index("email", unique=True)
    await token_blacklist.create_index("expires", expireAfterSeconds=0)  # TTL index

# Helper functions
def verify_password(plain_password, hashed_password):
    return pwd_context.verify(plain_password, hashed_password)

def get_password_hash(password):
    return pwd_context.hash(password)

def create_token(email: str):
    payload = {
        'email': email,
        'exp': datetime.datetime.utcnow() + datetime.timedelta(days=1)
    }
    return jwt.encode(payload, SECRET_KEY, algorithm="HS256")

# Generate a password reset token
def create_reset_token(email: str):
    payload = {
        'email': email,
        'reset': True,
        'exp': datetime.datetime.utcnow() + datetime.timedelta(hours=1)
    }
    return jwt.encode(payload, SECRET_KEY, algorithm="HS256")

# Update the get_current_user function to check the blacklist
async def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    try:
        token = credentials.credentials
        
        # Check if token is blacklisted
        blacklisted = await token_blacklist.find_one({"token": token})
        if blacklisted:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED, 
                detail="Token has been revoked"
            )
            
        payload = jwt.decode(token, SECRET_KEY, algorithms=["HS256"])
        email = payload.get("email")
        if email is None:
            raise HTTPException(
                status_code=status.HTTP_401_UNAUTHORIZED, 
                detail="Invalid authentication credentials"
            )
    except jwt.PyJWTError:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, 
            detail="Invalid authentication credentials"
        )
        
    user = await users_collection.find_one({"email": email})
    if user is None:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED, 
            detail="User not found"
        )
    
    return user

# Middleware for logging
@app.middleware("http")
async def log_requests(request: Request, call_next):
    print(f"Request: {request.method} {request.url.path}")
    response = await call_next(request)
    return response

# Add more specific error messages
@app.exception_handler(HTTPException)
async def http_exception_handler(request, exc):
    return JSONResponse(
        status_code=exc.status_code,
        content={
            "detail": exc.detail,
            "status_code": exc.status_code
        }
    )

@app.exception_handler(Exception)
async def general_exception_handler(request, exc):
    # Log the exception
    print(f"Unexpected error: {str(exc)}")
    return JSONResponse(
        status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
        content={
            "detail": "An unexpected error occurred",
            "status_code": status.HTTP_500_INTERNAL_SERVER_ERROR
        }
    )

# Routes
@app.post("/api/auth/register", response_model=TokenResponse, status_code=status.HTTP_201_CREATED)
async def register(user_data: UserCreate):
    try:
        hashed_password = get_password_hash(user_data.password)
        
        new_user = {
            "name": user_data.name,
            "email": user_data.email,
            "password": hashed_password
        }
        
        await users_collection.insert_one(new_user)
        
        token = create_token(new_user["email"])
        
        return {
            "token": token,
            "user": {
                "name": new_user["name"],
                "email": new_user["email"]
            }
        }
    except DuplicateKeyError:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Email already exists!"
        )

@app.post("/api/auth/login", response_model=TokenResponse)
async def login(user_data: UserLogin):
    user = await users_collection.find_one({"email": user_data.email})
    
    if not user or not verify_password(user_data.password, user["password"]):
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid credentials!"
        )
    
    token = create_token(user["email"])
    
    return {
        "token": token,
        "user": {
            "name": user["name"],
            "email": user["email"]
        }
    }

@app.get("/api/auth/validate", response_model=UserResponse)
async def validate_token(current_user: dict = Depends(get_current_user)):
    return {
        "name": current_user["name"],
        "email": current_user["email"]
    }

@app.get("/api/health")
async def health_check():
    return {"status": "healthy"}

@app.post("/api/auth/refresh", response_model=TokenResponse)
async def refresh_token(current_user: dict = Depends(get_current_user)):
    # Create a new token
    token = create_token(current_user["email"])
    
    return {
        "token": token,
        "user": {
            "name": current_user["name"],
            "email": current_user["email"]
        }
    }

# Enhanced logout with token blacklisting
@app.post("/api/auth/logout")
async def logout(credentials: HTTPAuthorizationCredentials = Depends(security)):
    token = credentials.credentials
    
    try:
        # Get token expiration from payload
        payload = jwt.decode(token, SECRET_KEY, algorithms=["HS256"], options={"verify_signature": True})
        exp = payload.get("exp", 0)
        
        # Add token to blacklist
        await token_blacklist.insert_one({
            "token": token,
            "expires": datetime.datetime.fromtimestamp(exp),
            "blacklisted_at": datetime.datetime.utcnow()
        })
        
        return {"detail": "Successfully logged out"}
    except jwt.PyJWTError:
        # If token is invalid, just return success
        return {"detail": "Successfully logged out"}

# Add password reset endpoints
@app.post("/api/auth/forgot-password")
async def forgot_password(request: ForgotPasswordRequest):
    user = await users_collection.find_one({"email": request.email})
    
    if user:
        # Generate reset token
        reset_token = create_reset_token(user["email"])
        
        # In a real application, you would send an email with a reset link
        # For demonstration, just return the token
        print(f"Password reset token for {request.email}: {reset_token}")
        
        # Store the reset token in the user document with expiration
        await users_collection.update_one(
            {"email": request.email},
            {"$set": {
                "reset_token": reset_token,
                "reset_token_expires": datetime.datetime.utcnow() + datetime.timedelta(hours=1)
            }}
        )
    
    # Always return success to prevent email enumeration
    return {"detail": "If your email is registered, you will receive reset instructions"}

@app.post("/api/auth/reset-password")
async def reset_password(request: ResetPasswordRequest):
    try:
        # Verify the token
        payload = jwt.decode(request.token, SECRET_KEY, algorithms=["HS256"])
        email = payload.get("email")
        is_reset = payload.get("reset", False)
        
        if not email or not is_reset:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Invalid token"
            )
            
        # Find the user
        user = await users_collection.find_one({
            "email": email,
            "reset_token": request.token,
            "reset_token_expires": {"$gt": datetime.datetime.utcnow()}
        })
        
        if not user:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Token expired or invalid"
            )
            
        # Update the password
        hashed_password = get_password_hash(request.new_password)
        await users_collection.update_one(
            {"email": email},
            {
                "$set": {"password": hashed_password},
                "$unset": {"reset_token": "", "reset_token_expires": ""}
            }
        )
        
        return {"detail": "Password updated successfully"}
        
    except jwt.PyJWTError:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid token"
        )

import asyncio
from datetime import datetime, timedelta

# Cleanup function to remove expired blacklist entries
async def cleanup_expired_tokens():
    while True:
        try:
            # Remove tokens that have expired more than a day ago
            # This is a safety net in addition to the TTL index
            cutoff = datetime.utcnow() - timedelta(days=1)
            result = await token_blacklist.delete_many({"expires": {"$lt": cutoff}})
            print(f"Cleaned up {result.deleted_count} expired tokens")
        except Exception as e:
            print(f"Error in token cleanup: {str(e)}")
        
        # Run once a day
        await asyncio.sleep(86400)  # 24 hours

# Start the cleanup task on startup
@app.on_event("startup")
async def start_token_cleanup():
    asyncio.create_task(cleanup_expired_tokens())

# Run the application with uvicorn
if __name__ == "__main__":
    import uvicorn
    port = int(os.environ.get("PORT", 8001))
    uvicorn.run("main:app", host="0.0.0.0", port=port, reload=os.environ.get('FASTAPI_ENV') == 'development')