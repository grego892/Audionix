FROM python:3.11-slim

WORKDIR /app

# Install system dependencies minimally
RUN apt-get update && apt-get install -y --no-install-recommends gcc \
    && rm -rf /var/lib/apt/lists/*

# Copy and install Python dependencies separately for better caching
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Copy application code
COPY . .

# Create 'uploads' directory (use volume mapping for persistence)
RUN mkdir -p uploads

# Set environment variables (optional)
ENV MONGODB_URL=mongodb://admin:password123@mongodb:27017/Audionix_db?authSource=admin
ENV SECRET_KEY=your-production-secret-key-here
ENV CORS_ORIGINS=https://djgrego.com

# Expose application port
EXPOSE 8001

# Run the app (optimized for production)
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8001", "--workers", "2"]