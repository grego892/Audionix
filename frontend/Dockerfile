FROM node:latest

WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# Instead of copying to nginx, serve directly with a Node.js server
# You can use a simple static file server like 'serve'
RUN npm install -g serve

EXPOSE 8080
CMD ["serve", "-s", "dist", "-l", "8080"]