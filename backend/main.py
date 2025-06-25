from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from pydantic import BaseModel
from pymongo import MongoClient
from passlib.context import CryptContext
from jose import JWTError, jwt
from datetime import datetime, timedelta
from typing import List, Optional
import os
import logging

# Add these imports at the top with your other imports
from bson import ObjectId
from bson.errors import InvalidId

# Set up logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Configure FastAPI with OpenAPI security scheme
app = FastAPI(
    title="Your API",
    description="Your API with JWT Authentication",
    version="1.0.0"
)

# MongoDB connection - use environment variable
MONGODB_URL = os.getenv("MONGODB_URL", "mongodb://localhost:27017/")
client = MongoClient(MONGODB_URL)
db = client["Audionix_db"]
users_collection = db["users"]
stations_collection = db["stations"]

# Security - use environment variable
SECRET_KEY = os.getenv("SECRET_KEY", "your-secret-key")
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_MINUTES = 30

# Configure HTTPBearer with OpenAPI integration
security = HTTPBearer(
    scheme_name="JWT",
    description="Enter your JWT token"
)
pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

# Allow frontend origin
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000"],
    allow_methods=["*"],
    allow_headers=["*"],
)

# Rest of your code remains the same...
# (Keep all your existing Pydantic models, helper functions, and endpoints as they are)
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

# Add this Pydantic model after your existing models
class UserPreferences(BaseModel):
    theme: Optional[str] = "light"
    selectedStationId: Optional[str] = None

class UserPreferencesResponse(BaseModel):
    theme: str
    selectedStationId: Optional[str] = None

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
        if not credentials or not credentials.credentials:
            logger.error("No credentials provided")
            raise credentials_exception
            
        payload = jwt.decode(credentials.credentials, SECRET_KEY, algorithms=[ALGORITHM])
        username: str = payload.get("sub")
        if username is None:
            logger.error("Username not found in token payload")
            raise credentials_exception
            
    except JWTError as e:
        logger.error(f"JWT Error: {e}")
        raise credentials_exception
    except Exception as e:
        logger.error(f"Unexpected error in token validation: {e}")
        raise credentials_exception

    try:
        user = users_collection.find_one({"username": username})
        if user is None:
            logger.error(f"User not found in database: {username}")
            raise credentials_exception
        return user
    except Exception as e:
        logger.error(f"Database error when fetching user: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error"
        )

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
    try:
        logger.info(f"User info requested: {current_user.get('username', 'Unknown')}")
        return {"username": current_user.get("username", "Unknown")}
    except Exception as e:
        logger.error(f"Error in /api/me endpoint: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error"
        )

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

# Add these endpoints before the health check endpoint

@app.get("/api/preferences", response_model=UserPreferencesResponse)
def get_user_preferences(current_user: dict = Depends(get_current_user)):
    logger.info(f"Getting preferences for user: {current_user['username']}")
    
    try:
        # Get user preferences from database
        user = users_collection.find_one({"username": current_user["username"]})
        if not user:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="User not found"
            )
        
        preferences = user.get("preferences", {})
        
        return {
            "theme": preferences.get("theme", "light"),
            "selectedStationId": preferences.get("selectedStationId")
        }
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting user preferences: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error getting user preferences"
        )

@app.put("/api/preferences")
def update_user_preferences(
    preferences: UserPreferences, 
    current_user: dict = Depends(get_current_user)
):
    logger.info(f"Updating preferences for user: {current_user['username']}")
    logger.info(f"New preferences: theme={preferences.theme}, selectedStationId={preferences.selectedStationId}")
    
    try:
        # Prepare update data - only include fields that are provided
        update_data = {}
        if preferences.theme is not None:
            update_data["preferences.theme"] = preferences.theme
        if preferences.selectedStationId is not None:
            # Validate station exists if provided
            if preferences.selectedStationId:
                try:
                    station_id = ObjectId(preferences.selectedStationId)
                    station = stations_collection.find_one({"_id": station_id})
                    if not station:
                        raise HTTPException(
                            status_code=status.HTTP_400_BAD_REQUEST,
                            detail="Selected station does not exist"
                        )
                except InvalidId:
                    raise HTTPException(
                        status_code=status.HTTP_400_BAD_REQUEST,
                        detail="Invalid station ID format"
                    )
            update_data["preferences.selectedStationId"] = preferences.selectedStationId
        
        if not update_data:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="No preferences provided to update"
            )
        
        # Update user preferences
        result = users_collection.update_one(
            {"username": current_user["username"]},
            {"$set": update_data}
        )
        
        if result.matched_count == 0:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="User not found"
            )
        
        logger.info(f"Successfully updated preferences for user: {current_user['username']}")
        return {"message": "Preferences updated successfully"}
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error updating user preferences: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error updating user preferences"
        )

# Add a simple health check endpoint
@app.get("/health")
def health_check():
    return {"status": "healthy", "timestamp": datetime.utcnow()}