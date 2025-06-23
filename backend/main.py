from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from pydantic import BaseModel
from pymongo import MongoClient
from passlib.context import CryptContext
from jose import JWTError, jwt
from datetime import datetime, timedelta
from typing import List
import os
import logging

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI()

# MongoDB connection - use environment variable
MONGODB_URL = os.getenv("MONGODB_URL", "mongodb://localhost:27017/")
client = MongoClient(MONGODB_URL)
db = client["Audionix_db"]
users_collection = db["users"]
stations_collection = db["stations"]  # Add stations collection

# Security - use environment variable
SECRET_KEY = os.getenv("SECRET_KEY", "your-secret-key")  # Fallback for development
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 30

security = HTTPBearer()
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# Allow frontend origin
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic models
class UserLogin(BaseModel):
    username: str
    password: str

class UserRegister(BaseModel):
    username: str
    password: str

class Token(BaseModel):
    access_token: str
    token_type: str

class StationCreate(BaseModel):
    callLetters: str

class StationResponse(BaseModel):
    id: str
    callLetters: str
    createdAt: datetime
    createdBy: str

# Helper functions
def verify_password(plain_password, hashed_password):
    return pwd_context.verify(plain_password, hashed_password)

def get_password_hash(password):
    return pwd_context.hash(password)

def create_access_token(data: dict):
    to_encode = data.copy()
    expire = datetime.utcnow() + timedelta(minutes=ACCESS_TOKEN_EXPIRE_MINUTES)
    to_encode.update({"exp": expire})
    encoded_jwt = jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)
    return encoded_jwt

def get_current_user(credentials: HTTPAuthorizationCredentials = Depends(security)):
    credentials_exception = HTTPException(
        status_code=status.HTTP_401_UNAUTHORIZED,
        detail="Could not validate credentials",
        headers={"WWW-Authenticate": "Bearer"},
    )
    try:
        payload = jwt.decode(credentials.credentials, SECRET_KEY, algorithms=[ALGORITHM])
        username: str = payload.get("sub")
        if username is None:
            raise credentials_exception
    except JWTError as e:
        logger.error(f"JWT Error: {e}")
        raise credentials_exception

    user = users_collection.find_one({"username": username})
    if user is None:
        logger.error(f"User not found: {username}")
        raise credentials_exception
    return user

# Auth endpoints
@app.post("/api/register", response_model=Token)
def register(user: UserRegister):
    logger.info(f"Registration attempt for username: {user.username}")
    # Check if user exists
    if users_collection.find_one({"username": user.username}):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Username already registered"
        )

    # Create user
    hashed_password = get_password_hash(user.password)
    user_doc = {
        "username": user.username,
        "hashed_password": hashed_password,
        "created_at": datetime.utcnow()
    }
    users_collection.insert_one(user_doc)

    # Create token
    access_token = create_access_token(data={"sub": user.username})
    logger.info(f"User registered successfully: {user.username}")
    return {"access_token": access_token, "token_type": "bearer"}

@app.post("/api/login", response_model=Token)
def login(user: UserLogin):
    logger.info(f"Login attempt for username: {user.username}")
    db_user = users_collection.find_one({"username": user.username})
    if not db_user or not verify_password(user.password, db_user["hashed_password"]):
        logger.warning(f"Failed login attempt for username: {user.username}")
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Incorrect username or password",
            headers={"WWW-Authenticate": "Bearer"},
        )

    access_token = create_access_token(data={"sub": user.username})
    logger.info(f"User logged in successfully: {user.username}")
    return {"access_token": access_token, "token_type": "bearer"}

@app.get("/api/me")
def read_users_me(current_user: dict = Depends(get_current_user)):
    logger.info(f"User info requested: {current_user['username']}")
    return {"username": current_user["username"]}

@app.get("/api/hello")
def read_root(current_user: dict = Depends(get_current_user)):
    logger.info(f"Hello endpoint accessed by: {current_user['username']}")
    return {"message": f"Hello {current_user['username']} from FastAPI"}

# Station endpoints
@app.post("/api/stations")
def create_station(station: StationCreate, current_user: dict = Depends(get_current_user)):
    logger.info(f"Station creation attempt by {current_user['username']}: {station.callLetters}")
    
    try:
        # Validate call letters format
        call_letters = station.callLetters.strip().upper()
        logger.info(f"Processed call letters: {call_letters}")
        
        if not call_letters:
            logger.warning("Empty call letters provided")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Call letters are required"
            )
        
        if len(call_letters) > 8:
            logger.warning(f"Call letters too long: {call_letters}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Call letters must be 8 characters or less"
            )
        
        # Check if station already exists
        existing_station = stations_collection.find_one({"callLetters": call_letters})
        if existing_station:
            logger.warning(f"Station already exists: {call_letters}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Station with these call letters already exists"
            )
        
        # Create station document
        station_doc = {
            "callLetters": call_letters,
            "createdAt": datetime.utcnow(),
            "createdBy": current_user["username"]
        }
        
        logger.info(f"Creating station document: {station_doc}")
        
        # Insert into database
        result = stations_collection.insert_one(station_doc)
        logger.info(f"Station created with ID: {result.inserted_id}")
        
        response_data = {
            "id": str(result.inserted_id),
            "callLetters": call_letters,
            "createdAt": station_doc["createdAt"],
            "createdBy": station_doc["createdBy"]
        }
        
        logger.info(f"Returning station data: {response_data}")
        return response_data
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Unexpected error creating station: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error"
        )

@app.get("/api/stations")
def get_stations(current_user: dict = Depends(get_current_user)):
    logger.info(f"Stations list requested by: {current_user['username']}")
    
    try:
        # Get all stations
        stations = list(stations_collection.find({}))
        logger.info(f"Found {len(stations)} stations in database")
        
        # Convert ObjectId to string and format response
        formatted_stations = []
        for station in stations:
            formatted_station = {
                "id": str(station["_id"]),
                "callLetters": station["callLetters"],
                "createdAt": station["createdAt"],
                "createdBy": station.get("createdBy", "Unknown")
            }
            formatted_stations.append(formatted_station)
        
        logger.info(f"Returning {len(formatted_stations)} formatted stations")
        return formatted_stations
        
    except Exception as e:
        logger.error(f"Error fetching stations: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error fetching stations"
        )

@app.delete("/api/stations/{station_id}")
def delete_station(station_id: str, current_user: dict = Depends(get_current_user)):
    logger.info(f"Station deletion requested by {current_user['username']}: {station_id}")
    
    from bson import ObjectId
    from bson.errors import InvalidId
    
    try:
        # Convert string ID to ObjectId
        object_id = ObjectId(station_id)
    except InvalidId:
        logger.warning(f"Invalid station ID format: {station_id}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid station ID format"
        )
    
    # Find and delete the station
    result = stations_collection.delete_one({"_id": object_id})
    
    if result.deleted_count == 0:
        logger.warning(f"Station not found for deletion: {station_id}")
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Station not found"
        )
    
    logger.info(f"Station deleted successfully: {station_id}")
    return {"message": "Station deleted successfully"}

# Add a simple health check endpoint
@app.get("/health")
def health_check():
    return {"status": "healthy", "timestamp": datetime.utcnow()}