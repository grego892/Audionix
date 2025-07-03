FROM node:latest

WORKDIR /app
COPY package*.json ./
RUN npm install --production
COPY . .
RUN npm run build

# Install serve for static file serving
RUN npm install -g serve

EXPOSE 8080
# The -l 8080 flag specifies the port, and the -s flag serves as single-page application
# Adding the 0.0.0.0 host to ensure it binds to all network interfaces
CMD ["serve", "-s", "dist", "-l", "tcp://0.0.0.0:8080"]