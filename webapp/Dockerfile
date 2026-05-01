# Stage 1: Build
FROM node:18-alpine AS builder

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

RUN npm run build

# Stage 2: Runtime
FROM node:18-alpine

WORKDIR /app

COPY --from=builder /app/dist /app/dist
COPY package*.json ./

RUN npm ci --only=production

EXPOSE 5173

CMD ["npm", "start"]
