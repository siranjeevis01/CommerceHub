FROM node:20-alpine AS build
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build:all

FROM nginx:alpine AS runtime
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
# Shell is the main app - copy entire shell dist
COPY --from=build /app/dist/shell /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
