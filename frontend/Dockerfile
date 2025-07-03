FROM node:18-alpine  # Use specific version instead of latest

WORKDIR /app
COPY package*.json ./
RUN npm install --production  # Use production flag to skip dev dependencies
COPY . .
RUN npm run build

RUN npm install -g serve

EXPOSE 8080
# Add healthcheck
HEALTHCHECK --interval=30s --timeout=3s CMD wget -qO- http://localhost:8080 || exit 1
CMD ["serve", "-s", "dist", "-l", "8080"]