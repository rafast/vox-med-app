# VoxMed App

A medical application with a .NET 8.0 REST API backend and Vue.js frontend, fully containerized with Docker.

## 🚀 Quick Start

### Prerequisites
- Docker & Docker Compose
- Git

### One-Command Setup
```bash
# Clone the repository
git clone <your-repo-url>
cd vox-med-app

# Start development environment
make dev
# or
./docker-manager.sh dev
```

That's it! Your application will be running at:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Database**: localhost:5432

## 🐳 Docker Commands

We provide multiple ways to manage your Docker environment:

### Using Make (Recommended)
```bash
make help          # Show available commands
make dev           # Start development environment
make prod          # Start production environment
make build         # Build all images
make stop          # Stop all services
make logs          # View logs
make health        # Check service health
make clean         # Clean up everything
```

### Using Docker Manager Script
```bash
./docker-manager.sh dev      # Development mode
./docker-manager.sh prod     # Production mode
./docker-manager.sh health   # Health check
./docker-manager.sh logs     # View logs
```

### Using Docker Compose Directly
```bash
# Development
docker-compose up -d

# Production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# View logs
docker-compose logs -f
```

## 📁 Project Structure

```
vox-med-app/
├── backend/                 # .NET 8.0 REST API
│   ├── Controllers/         # API Controllers
│   ├── Dockerfile          # Backend container config
│   └── README.md           # Backend documentation
├── frontend/               # Vue.js responsive web app
│   ├── src/               # Source code
│   ├── Dockerfile         # Frontend container config
│   ├── nginx.conf         # Nginx configuration
│   └── README.md          # Frontend documentation
├── .github/               # GitHub Actions CI/CD
│   └── workflows/         # Automated pipelines
├── docker-compose.yml     # Main Docker configuration
├── docker-compose.override.yml  # Development overrides
├── docker-compose.prod.yml      # Production configuration
├── docker-manager.sh      # Docker management script
├── Makefile              # Make commands
├── .env.example          # Environment variables template
└── README.md             # This file
```

## 🏗️ Architecture

### Backend (.NET 8.0 REST API)
- ASP.NET Core Web API
- Entity Framework Core ready
- Swagger/OpenAPI documentation
- Health checks
- CORS configured

### Frontend (Vue.js)
- Vue.js 3 with TypeScript
- Vue Router for navigation
- Pinia for state management
- Vite for fast development
- Nginx for production serving

### Database
- PostgreSQL 15 (containerized)
- Automatic health checks
- Data persistence with Docker volumes
- Pre-configured with medical domain schema

### DevOps
- Multi-stage Docker builds
- Development hot-reload
- Production optimizations
- GitHub Actions CI/CD
- Security scanning
- Automated testing

## 🛠️ Development

### Local Development (with Docker)
```bash
# Start everything
make dev

# View logs
make logs

# Check health
make health

# Stop everything
make stop
```

### Local Development (without Docker)
```bash
# Backend
cd backend
dotnet restore
dotnet run

# Frontend (in another terminal)
cd frontend
npm install
npm run dev
```

## 🚀 Production Deployment

### Using Docker Compose
```bash
# Build and start production environment
make prod

# Or manually
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Environment Variables
Copy `.env.example` to `.env` and configure:
```bash
cp .env.example .env
# Edit .env with your configuration
```

## 🧪 Testing & Quality

### Run Tests
```bash
make test           # Run all tests
```

### Code Quality
```bash
make lint           # Lint all code
make format         # Format all code
```

### Security Scanning
Security scans run automatically in CI/CD, or manually:
```bash
# Scan for vulnerabilities
docker run --rm -v $(pwd):/workspace aquasec/trivy fs /workspace
```

## 🔧 CI/CD Pipeline

The project includes a complete GitHub Actions pipeline:

- ✅ **Automated Testing**: Backend (.NET) and Frontend (Vue.js)
- ✅ **Code Quality**: Linting and formatting checks
- ✅ **Security Scanning**: Vulnerability detection
- ✅ **Docker Building**: Multi-platform image builds
- ✅ **Container Registry**: Automatic image publishing
- ✅ **Environment Deployment**: Staging and production

## 📊 Monitoring & Health

### Health Checks
All services include health endpoints:
```bash
# Check all services
make health

# Individual checks
curl http://localhost:5000/health    # Backend
curl http://localhost:3000/health    # Frontend
```

### Logs
```bash
# All services
make logs

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
```

## 🔒 Security Features

- Container security best practices
- Non-root user containers
- Security headers in Nginx
- Automated vulnerability scanning
- Environment variable protection
- HTTPS ready configuration

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Run tests: `make test`
5. Check code quality: `make lint`
6. Submit a pull request

## 📝 License

This project is licensed under the MIT License.
