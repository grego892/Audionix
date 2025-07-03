FROM python:3.11-slim

WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
# Add uvicorn explicitly since it's used but not in requirements.txt
RUN pip install uvicorn gunicorn

COPY . .

# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s CMD curl -f http://localhost:8000/api/health || exit 1

# Use gunicorn for production with uvicorn workers
CMD ["gunicorn", "--bind", "0.0.0.0:8000", "--workers", "4", "--worker-class", "uvicorn.workers.UvicornWorker", "main:app"]