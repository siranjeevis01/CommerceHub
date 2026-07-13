FROM node:20-alpine AS build
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build:all

FROM nginx:alpine AS runtime
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/shell /usr/share/nginx/html
COPY --from=build /app/dist/admin /usr/share/nginx/html/admin
COPY --from=build /app/dist/vendor /usr/share/nginx/html/vendor
COPY --from=build /app/dist/storefront /usr/share/nginx/html/storefront
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
