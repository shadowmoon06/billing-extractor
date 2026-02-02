# Build stage
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files
COPY package.json package-lock.json* ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Remove any .env files that might override build args (in case .dockerignore doesn't work)
RUN rm -f .env .env.* 2>/dev/null || true

# Build argument for API URL
ARG VITE_API_URL=http://localhost:8080
ENV VITE_API_URL=$VITE_API_URL

# Debug: print the API URL being used
RUN echo "Building with VITE_API_URL=$VITE_API_URL"

# Build the application
RUN npm run build

# Runtime stage
FROM nginx:alpine AS runtime

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built files
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
