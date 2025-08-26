.PHONY: help dev prod build stop restart logs clean health db-reset install

# Default target
help: ## Show this help message
	@echo "VoxMed Docker Commands:"
	@echo ""
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}'

dev: ## Start development environment
	@./docker-manager.sh dev

prod: ## Start production environment
	@./docker-manager.sh prod

build: ## Build all Docker images
	@./docker-manager.sh build

stop: ## Stop all services
	@./docker-manager.sh stop

restart: ## Restart all services
	@./docker-manager.sh restart

logs: ## Show logs for all services
	@./docker-manager.sh logs

clean: ## Clean up Docker environment
	@./docker-manager.sh clean

health: ## Check health of all services
	@./docker-manager.sh health

db-reset: ## Reset database (WARNING: destroys data)
	@./docker-manager.sh db-reset

install: ## Install dependencies locally
	@echo "Installing backend dependencies..."
	@cd backend && dotnet restore
	@echo "Installing frontend dependencies..."
	@cd frontend && npm install
	@echo "Dependencies installed!"

test: ## Run tests for both projects
	@echo "Running backend tests..."
	@cd backend && dotnet test
	@echo "Running frontend tests..."
	@cd frontend && npm run test:unit

format: ## Format code for both projects
	@echo "Formatting backend code..."
	@cd backend && dotnet format
	@echo "Formatting frontend code..."
	@cd frontend && npm run format

lint: ## Lint code for both projects
	@echo "Linting backend code..."
	@cd backend && dotnet format --verify-no-changes
	@echo "Linting frontend code..."
	@cd frontend && npm run lint

setup: install ## Complete development setup
	@echo "Setting up development environment..."
	@cp .env.example .env 2>/dev/null || echo ".env already exists"
	@echo "Setup complete! Run 'make dev' to start development environment."
