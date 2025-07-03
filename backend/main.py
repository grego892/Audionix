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

# Setup
app = FastAPI(title="Authentication API")
security = HTTPBearer()
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Customize this in production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Config
SECRET_KEY = os.environ.get('SECRET_KEY', 'default-dev-key')
MONGO_URL = os.environ.get('MONGO_URL', 'mongodb://localhost:27017')
client = AsyncIOMotorClient(MONGO_URL)
db = client.user_database
users_collection = db.users

# Ensure email index
@app.on_event("startup")
async def startup_db_client():
    await users_collection.create_index("email", unique=True)

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

async def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    try:
        token = credentials.credentials
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

# Run the application with uvicorn
if __name__ == "__main__":
    import uvicorn
    port = int(os.environ.get("PORT", 8001))
    uvicorn.run("main:app", host="0.0.0.0", port=port, reload=os.environ.get('FASTAPI_ENV') == 'development')