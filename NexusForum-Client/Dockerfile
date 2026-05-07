# Stage 1: build Angular app with Node
FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .
# Production build outputs to dist/NexusForum-Client/browser
RUN npm run build

# Stage 2: serve with nginx — no Node runtime in final image
FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist/NexusForum-Client/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
