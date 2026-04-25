# syntax=docker/dockerfile:1.7

FROM node:22-alpine AS build
WORKDIR /app

ARG VITE_API_URL=http://localhost:5000/api
ARG VITE_SIGNALR_URL=http://localhost:5000/chatHub
ENV VITE_API_URL=${VITE_API_URL}
ENV VITE_SIGNALR_URL=${VITE_SIGNALR_URL}
ENV CI=true

COPY package.json package-lock.json ./
RUN --mount=type=cache,target=/root/.npm npm ci --no-audit --no-fund

COPY . .
RUN npm run build

FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime

WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist ./
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
  CMD wget -q -O /dev/null http://127.0.0.1:8080/healthz || exit 1

CMD ["nginx", "-g", "daemon off;"]
