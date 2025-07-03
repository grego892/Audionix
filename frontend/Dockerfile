FROM node:18

WORKDIR /app
COPY package*.json ./

# Remove the --production flag to install ALL dependencies including dev dependencies
RUN npm install

COPY . .
# Build the project
RUN npm run build

# After building, we can prune the dev dependencies to make the image smaller
RUN npm prune --production

# Install serve for static file serving
RUN npm install -g serve

EXPOSE 8080
# Make sure to bind to all interfaces
CMD ["serve", "-s", "dist", "-l", "tcp://0.0.0.0:8080"]