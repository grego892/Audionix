# Build stage
FROM node:18-alpine AS build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --only=production

# Copy source code
COPY . .

# Build the app
RUN npm run build

# Production stage with nginx
FROM nginx:alpine

# Copy built app to nginx (adjust path based on your build output)
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx server configuration
COPY ../nginx/default.conf /etc/nginx/conf.d/default.conf

# Expose port 80 (to match docker-compose port mapping)
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]