# Stage 1: Build backend (.NET)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-backend
WORKDIR /app
COPY backend/ ./backend/
WORKDIR /app/backend
RUN dotnet publish VoxMed.sln -c Release -o /app/publish

# Stage 2: Build frontend (Vue.js)
FROM node:20-alpine AS build-frontend
WORKDIR /app
COPY frontend/ ./frontend/
WORKDIR /app/frontend
RUN npm install && npm run build

# Stage 3: Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-backend /app/publish .
# Optionally copy frontend build to a public folder if served by backend
# COPY --from=build-frontend /app/frontend/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "VoxMedApi.dll"]
