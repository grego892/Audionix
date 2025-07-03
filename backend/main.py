from flask import Flask, request, jsonify, make_response
import jwt
import datetime
from functools import wraps
from werkzeug.security import generate_password_hash, check_password_hash
from pymongo import MongoClient
import os
from flask_cors import CORS

app = Flask(__name__)
CORS(app)  # Enable CORS for all routes
app.config['SECRET_KEY'] = 'your-secret-key'  # Change this to a secure key

# Connect to MongoDB using environment variable
mongo_url = os.environ.get('MONGO_URL', 'mongodb://localhost:27017')
client = MongoClient(mongo_url)
db = client.user_database
users_collection = db.users

# Token required decorator
def token_required(f):
    @wraps(f)
    def decorated(*args, **kwargs):
        token = None
        
        if 'Authorization' in request.headers:
            auth_header = request.headers['Authorization']
            token = auth_header.split(" ")[1]
            
        if not token:
            return jsonify({'message': 'Token is missing!'}), 401
            
        try:
            data = jwt.decode(token, app.config['SECRET_KEY'], algorithms=["HS256"])
            current_user = users_collection.find_one({'email': data['email']})
            
            if not current_user:
                return jsonify({'message': 'User not found!'}), 401
                
        except:
            return jsonify({'message': 'Token is invalid!'}), 401
            
        return f(current_user, *args, **kwargs)
    
    return decorated

@app.route('/api/auth/register', methods=['POST'])
def register():
    data = request.get_json()
    
    # Check if email already exists
    if users_collection.find_one({'email': data['email']}):
        return jsonify({'message': 'Email already exists!'}), 400
        
    # Create new user
    hashed_password = generate_password_hash(data['password'])
    
    new_user = {
        'name': data['name'],
        'email': data['email'],
        'password': hashed_password
    }
    
    user_id = users_collection.insert_one(new_user).inserted_id
    
    # Generate token
    token = jwt.encode({
        'email': new_user['email'],
        'exp': datetime.datetime.utcnow() + datetime.timedelta(days=1)
    }, app.config['SECRET_KEY'], algorithm="HS256")
    
    return jsonify({
        'token': token,
        'user': {
            'name': new_user['name'],
            'email': new_user['email']
        }
    }), 201

@app.route('/api/auth/login', methods=['POST'])
def login():
    data = request.get_json()
    
    user = users_collection.find_one({'email': data['email']})
    
    if not user or not check_password_hash(user['password'], data['password']):
        return jsonify({'message': 'Invalid credentials!'}), 401
    
    # Generate token
    token = jwt.encode({
        'email': user['email'],
        'exp': datetime.datetime.utcnow() + datetime.timedelta(days=1)
    }, app.config['SECRET_KEY'], algorithm="HS256")
    
    return jsonify({
        'token': token,
        'user': {
            'name': user['name'],
            'email': user['email']
        }
    })

@app.route('/api/auth/validate', methods=['GET'])
@token_required
def validate_token(current_user):
    return jsonify({
        'user': {
            'name': current_user['name'],
            'email': current_user['email']
        }
    })

@app.route('/api/health', methods=['GET'])
def health_check():
    return jsonify({'status': 'healthy'})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=8000, debug=True)