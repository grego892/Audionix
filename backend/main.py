from fastapi import FastAPI, Depends, HTTPException, status, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from fastapi.responses import FileResponse
from pydantic import BaseModel
from pymongo import MongoClient
from passlib.context import CryptContext
from jose import JWTError, jwt
from datetime import datetime, timedelta
from typing import List, Optional
import os
import logging
import shutil
import uuid
from pathlib import Path

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
audio_files_collection = db["audio_files"]

# Create uploads directory if it doesn't exist
UPLOAD_DIR = Path("uploads")
UPLOAD_DIR.mkdir(exist_ok=True)

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
from fastapi.middleware.cors import CORSMiddleware

app.add_middleware(
    CORSMiddleware,
    allow_origins=["https://audionix.djgrego.com", "http://localhost", "http://127.0.0.1", "http://localhost:8081"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Pydantic models
class UserLogin(BaseModel):
    username: str
    password: str

class UserCreate(BaseModel):
    username: str
    password: str
    isAdmin: Optional[bool] = False

class UserResponse(BaseModel):
    id: str
    username: str
    isAdmin: bool
    createdAt: datetime

class PasswordChange(BaseModel):
    newPassword: str

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

class UserPreferences(BaseModel):
    theme: Optional[str] = "light"
    selectedStationId: Optional[str] = None

class UserPreferencesResponse(BaseModel):
    theme: str
    selectedStationId: Optional[str] = None

# Add new models for audio files
class AudioFileResponse(BaseModel):
    id: str
    filename: str
    originalName: str
    fileSize: int
    uploadedAt: datetime
    uploadedBy: str
    stationId: Optional[str] = None
    mimeType: str
    duration: Optional[float] = None

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

def get_admin_user(current_user: dict = Depends(get_current_user)):
    if not current_user.get("isAdmin", False):
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Admin access required"
        )
    return current_user

# Initialize default admin user if none exists
def init_admin_user():
    admin_user = users_collection.find_one({"isAdmin": True})
    if not admin_user:
        default_admin = {
            "username": "admin",
            "hashed_password": get_password_hash("admin123"),
            "isAdmin": True,
            "created_at": datetime.utcnow()
        }
        users_collection.insert_one(default_admin)
        logger.info("Default admin user created: admin/admin123")

# Call this on startup
init_admin_user()

# Authentication endpoints
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
        return {
            "username": current_user.get("username", "Unknown"),
            "isAdmin": current_user.get("isAdmin", False)
        }
    except Exception as e:
        logger.error(f"Error in /api/me endpoint: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal server error"
        )

# Admin-only user management endpoints
@app.get("/api/admin/users", response_model=List[UserResponse])
def get_all_users(admin_user: dict = Depends(get_admin_user)):
    logger.info(f"Admin {admin_user['username']} requested user list")
    try:
        users = list(users_collection.find({}))
        result = []
        for user in users:
            result.append({
                "id": str(user["_id"]),
                "username": user["username"],
                "isAdmin": user.get("isAdmin", False),
                "createdAt": user.get("created_at", datetime.utcnow())
            })
        return result
    except Exception as e:
        logger.error(f"Error fetching users: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error fetching users"
        )

@app.post("/api/admin/users", response_model=UserResponse)
def create_user(user_data: UserCreate, admin_user: dict = Depends(get_admin_user)):
    logger.info(f"Admin {admin_user['username']} creating user: {user_data.username}")
    
    if users_collection.find_one({"username": user_data.username}):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Username already exists"
        )

    hashed_password = get_password_hash(user_data.password)
    user_doc = {
        "username": user_data.username,
        "hashed_password": hashed_password,
        "isAdmin": user_data.isAdmin,
        "created_at": datetime.utcnow()
    }
    
    result = users_collection.insert_one(user_doc)
    logger.info(f"User created successfully: {user_data.username}")
    
    return {
        "id": str(result.inserted_id),
        "username": user_data.username,
        "isAdmin": user_data.isAdmin,
        "createdAt": user_doc["created_at"]
    }

@app.put("/api/admin/users/{user_id}/password")
def change_user_password(user_id: str, password_data: PasswordChange, admin_user: dict = Depends(get_admin_user)):
    logger.info(f"Admin {admin_user['username']} changing password for user: {user_id}")
    
    try:
        object_id = ObjectId(user_id)
    except InvalidId:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid user ID format"
        )
    
    # Validate password
    if len(password_data.newPassword) < 6:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Password must be at least 6 characters long"
        )
    
    # Check if user exists
    user_to_update = users_collection.find_one({"_id": object_id})
    if not user_to_update:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="User not found"
        )
    
    # Hash the new password
    hashed_password = get_password_hash(password_data.newPassword)
    
    # Update the user's password
    result = users_collection.update_one(
        {"_id": object_id},
        {"$set": {"hashed_password": hashed_password}}
    )
    
    if result.modified_count == 0:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to update password"
        )
    
    logger.info(f"Password changed successfully for user: {user_id}")
    return {"message": "Password changed successfully"}

@app.delete("/api/admin/users/{user_id}")
def delete_user(user_id: str, admin_user: dict = Depends(get_admin_user)):
    logger.info(f"Admin {admin_user['username']} deleting user: {user_id}")
    
    try:
        object_id = ObjectId(user_id)
    except InvalidId:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid user ID format"
        )
    
    # Prevent admin from deleting themselves
    user_to_delete = users_collection.find_one({"_id": object_id})
    if user_to_delete and user_to_delete["username"] == admin_user["username"]:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Cannot delete your own account"
        )
    
    result = users_collection.delete_one({"_id": object_id})
    
    if result.deleted_count == 0:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="User not found"
        )
    
    logger.info(f"User deleted successfully: {user_id}")
    return {"message": "User deleted successfully"}

# Station endpoints (keep all existing ones)
@app.post("/api/stations")
def create_station(station: StationCreate, current_user: dict = Depends(get_current_user)):
    logger.info(f"Station creation attempt by {current_user['username']}: {station.callLetters}")

    try:
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

        existing_station = stations_collection.find_one({"callLetters": call_letters})
        if existing_station:
            logger.warning(f"Station already exists: {call_letters}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Station with these call letters already exists"
            )

        station_doc = {
            "callLetters": call_letters,
            "createdAt": datetime.utcnow(),
            "createdBy": current_user["username"]
        }

        logger.info(f"Creating station document: {station_doc}")

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
        stations = list(stations_collection.find({}))
        logger.info(f"Found {len(stations)} stations in database")

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

    try:
        object_id = ObjectId(station_id)
    except InvalidId:
        logger.warning(f"Invalid station ID format: {station_id}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid station ID format"
        )

    result = stations_collection.delete_one({"_id": object_id})

    if result.deleted_count == 0:
        logger.warning(f"Station not found for deletion: {station_id}")
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Station not found"
        )

    logger.info(f"Station deleted successfully: {station_id}")
    return {"message": "Station deleted successfully"}

# Preferences endpoints (keep existing ones)
@app.get("/api/preferences", response_model=UserPreferencesResponse)
def get_user_preferences(current_user: dict = Depends(get_current_user)):
    logger.info(f"Getting preferences for user: {current_user['username']}")
    
    try:
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

# Audio file endpoints (keep all existing ones)
@app.post("/api/audio/upload")
async def upload_audio_file(
    station_id: str,
    file: UploadFile = File(...),
    current_user: dict = Depends(get_current_user)
):
    logger.info(f"File upload attempt by {current_user['username']} for station {station_id}: {file.filename}")

    try:
        # Validate station ID
        try:
            station_object_id = ObjectId(station_id)
            station = stations_collection.find_one({"_id": station_object_id})
            if not station:
                logger.warning(f"Station not found: {station_id}")
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Station not found"
                )
        except InvalidId:
            logger.warning(f"Invalid station ID format: {station_id}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Invalid station ID format"
            )

        # Validate file size and type
        if file.size > 100 * 1024 * 1024:  # 100MB limit
            logger.warning(f"File size too large: {file.filename}")
            raise HTTPException(
                status_code=status.HTTP_413_REQUEST_ENTITY_TOO_LARGE,
                detail="File size too large (max 100MB)"
            )

        allowed_mime_types = ["audio/mpeg", "audio/wav", "audio/x-wav", "audio/mp3"]  # Add other audio MIME types
        if file.content_type not in allowed_mime_types:
            logger.warning(f"Invalid file type: {file.content_type}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Invalid file type. Only audio files are allowed."
            )

        # Generate a unique filename
        file_extension = file.filename.split(".")[-1]
        unique_filename = f"{uuid.uuid4()}.{file_extension}"
        file_path = UPLOAD_DIR / unique_filename

        # Save the file
        with open(file_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        # Store metadata in database
        file_size = os.path.getsize(file_path)
        file_doc = {
            "filename": unique_filename,
            "originalName": file.filename,
            "fileSize": file_size,
            "uploadedAt": datetime.utcnow(),
            "uploadedBy": current_user["username"],
            "stationId": station_id,
            "mimeType": file.content_type
        }
        result = audio_files_collection.insert_one(file_doc)
        file_id = str(result.inserted_id)
        logger.info(f"File saved to database with ID: {file_id}")

        response_data = {
            "id": file_id,
            "filename": unique_filename,
            "originalName": file.filename,
            "fileSize": file_size,
            "uploadedAt": file_doc["uploadedAt"],
            "uploadedBy": file_doc["uploadedBy"],
            "stationId": file_doc["stationId"],
            "mimeType": file.content_type,
            "duration": None  # You can calculate the duration later if needed
        }
        return response_data

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error uploading file: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to upload file"
        )

@app.get("/api/audio/{file_id}")
async def get_audio_file(file_id: str, current_user: dict = Depends(get_current_user)):
    logger.info(f"Request to get audio file: {file_id}")
    try:
        # Validate file ID
        try:
            file_object_id = ObjectId(file_id)
            file_data = audio_files_collection.find_one({"_id": file_object_id})
        except InvalidId:
            logger.warning(f"Invalid audio file ID format: {file_id}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Invalid audio file ID format"
            )

        if not file_data:
            logger.warning(f"Audio file not found: {file_id}")
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Audio file not found"
            )

        file_path = UPLOAD_DIR / file_data["filename"]

        if not file_path.is_file():
            logger.error(f"File not found on server: {file_path}")
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Audio file not found on server"
            )

        logger.info(f"Serving audio file: {file_data['filename']}")
        return FileResponse(file_path, media_type=file_data["mimeType"], filename=file_data["originalName"])

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting audio file: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error retrieving audio file"
        )

# List audio files (optionally filter by station)
@app.get("/api/audio-files")
def list_audio_files(station_id: Optional[str] = None, current_user: dict = Depends(get_current_user)):
    query = {}
    if station_id:
        query["stationId"] = station_id
    files = list(audio_files_collection.find(query))
    result = []
    for f in files:
        result.append({
            "id": str(f["_id"]),
            "filename": f["filename"],
            "originalName": f["originalName"],
            "fileSize": f["fileSize"],
            "uploadedAt": f["uploadedAt"],
            "uploadedBy": f["uploadedBy"],
            "stationId": f.get("stationId"),
            "mimeType": f.get("mimeType"),
            "duration": f.get("duration"),
        })
    return result

# Delete audio file
@app.delete("/api/audio-files/{file_id}")
def delete_audio_file(file_id: str, current_user: dict = Depends(get_current_user)):
    try:
        file_object_id = ObjectId(file_id)
        file_data = audio_files_collection.find_one({"_id": file_object_id})
        if not file_data:
            raise HTTPException(status_code=404, detail="File not found")
        file_path = UPLOAD_DIR / file_data["filename"]
        if file_path.exists():
            file_path.unlink()
        audio_files_collection.delete_one({"_id": file_object_id})
        return {"message": "File deleted successfully"}
    except InvalidId:
        raise HTTPException(status_code=400, detail="Invalid file ID format")

# Upload audio file (to match frontend)
@app.post("/api/upload-audio")
async def upload_audio_file_alt(
    station_id: Optional[str] = None,
    file: UploadFile = File(...),
    current_user: dict = Depends(get_current_user)
):
    logger.info(f"File upload attempt by {current_user['username']} for station {station_id}: {file.filename}")

    try:
        # Validate station ID if provided
        if station_id:
            try:
                station_object_id = ObjectId(station_id)
                station = stations_collection.find_one({"_id": station_object_id})
                if not station:
                    logger.warning(f"Station not found: {station_id}")
                    raise HTTPException(
                        status_code=status.HTTP_400_BAD_REQUEST,
                        detail="Station not found"
                    )
            except InvalidId:
                logger.warning(f"Invalid station ID format: {station_id}")
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail="Invalid station ID format"
                )

        # Validate file size and type
        if file.size > 100 * 1024 * 1024:  # 100MB limit
            logger.warning(f"File size too large: {file.filename}")
            raise HTTPException(
                status_code=status.HTTP_413_REQUEST_ENTITY_TOO_LARGE,
                detail="File size too large (max 100MB)"
            )

        allowed_mime_types = ["audio/mpeg", "audio/wav", "audio/x-wav", "audio/mp3"]
        if file.content_type not in allowed_mime_types:
            logger.warning(f"Invalid file type: {file.content_type}")
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Invalid file type. Only audio files are allowed."
            )

        # Generate a unique filename
        file_extension = file.filename.split(".")[-1]
        unique_filename = f"{uuid.uuid4()}.{file_extension}"
        file_path = UPLOAD_DIR / unique_filename

        # Save the file
        with open(file_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        # Store metadata in database
        file_size = os.path.getsize(file_path)
        file_doc = {
            "filename": unique_filename,
            "originalName": file.filename,
            "fileSize": file_size,
            "uploadedAt": datetime.utcnow(),
            "uploadedBy": current_user["username"],
            "stationId": station_id,
            "mimeType": file.content_type
        }
        result = audio_files_collection.insert_one(file_doc)
        file_id = str(result.inserted_id)
        logger.info(f"File saved to database with ID: {file_id}")

        response_data = {
            "id": file_id,
            "filename": unique_filename,
            "originalName": file.filename,
            "fileSize": file_size,
            "uploadedAt": file_doc["uploadedAt"],
            "uploadedBy": file_doc["uploadedBy"],
            "stationId": file_doc["stationId"],
            "mimeType": file.content_type,
            "duration": None
        }
        return response_data

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error uploading file: {e}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to upload file"
        )

# Add a simple health check endpoint
@app.get("/health")
def health_check():
    return {"status": "healthy", "timestamp": datetime.utcnow()}