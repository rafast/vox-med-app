#!/bin/bash

# VoxMed Docker Management Script
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}          VoxMed Docker Manager${NC}"
    echo -e "${BLUE}============================================${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

show_help() {
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  dev          Start development environment"
    echo "  prod         Start production environment"
    echo "  build        Build all images"
    echo "  stop         Stop all services"
    echo "  restart      Restart all services"
    echo "  logs         Show logs for all services"
    echo "  clean        Clean up containers, images, and volumes"
    echo "  health       Check health of all services"
    echo "  db-reset     Reset database (WARNING: destroys data)"
    echo "  help         Show this help message"
}

start_dev() {
    print_header
    echo "🚀 Starting development environment..."
    
    # Check if .env file exists
    if [ ! -f .env ]; then
        print_warning "Creating .env file from template"
        cp .env.example .env 2>/dev/null || echo "No .env.example found"
    fi
    
    docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
    print_success "Development environment started!"
    echo ""
    echo "📋 Services available:"
    echo "   Frontend: http://localhost:3000"
    echo "   Backend:  http://localhost:5000"
    echo "   Database: localhost:5432 (voxmed/VoxMed@2025!)"
    echo ""
    echo "📝 Useful commands:"
    echo "   ./docker-manager.sh logs    - View logs"
    echo "   ./docker-manager.sh health  - Check health"
}

start_prod() {
    print_header
    echo "🏭 Starting production environment..."
    
    docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
    print_success "Production environment started!"
    echo ""
    echo "📋 Services available:"
    echo "   Application: http://localhost"
    echo "   API:        http://localhost/api"
}

build_images() {
    print_header
    echo "🔨 Building all images..."
    
    docker-compose build --no-cache
    print_success "All images built successfully!"
}

stop_services() {
    print_header
    echo "🛑 Stopping all services..."
    
    docker-compose down
    print_success "All services stopped!"
}

restart_services() {
    print_header
    echo "🔄 Restarting all services..."
    
    docker-compose restart
    print_success "All services restarted!"
}

show_logs() {
    print_header
    echo "📋 Showing logs for all services..."
    echo "Press Ctrl+C to exit"
    echo ""
    
    docker-compose logs -f
}

clean_environment() {
    print_header
    echo "🧹 Cleaning up Docker environment..."
    
    read -p "This will remove all containers, images, and volumes. Continue? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose down -v --rmi all
        docker system prune -f
        print_success "Environment cleaned!"
    else
        print_warning "Cleanup cancelled"
    fi
}

check_health() {
    print_header
    echo "🔍 Checking service health..."
    echo ""
    
    # Check if services are running
    if ! docker-compose ps | grep -q "Up"; then
        print_error "No services are running"
        return 1
    fi
    
    # Check backend health
    echo -n "Backend API: "
    if curl -f -s http://localhost:5000/health > /dev/null 2>&1; then
        print_success "Healthy"
    else
        print_error "Unhealthy"
    fi
    
    # Check frontend health
    echo -n "Frontend: "
    if curl -f -s http://localhost:3000/health > /dev/null 2>&1; then
        print_success "Healthy"
    else
        print_error "Unhealthy"
    fi
    
    # Check database health
    echo -n "Database: "
    if docker-compose exec -T database pg_isready -U voxmed -d voxmeddb > /dev/null 2>&1; then
        print_success "Healthy"
    else
        print_error "Unhealthy"
    fi
}

reset_database() {
    print_header
    echo "🗃️  Resetting database..."
    
    read -p "This will destroy all database data. Continue? (y/N) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker-compose stop database
        docker volume rm voxmed-app_voxmed-db-data 2>/dev/null || true
        docker-compose up -d database
        print_success "Database reset complete!"
    else
        print_warning "Database reset cancelled"
    fi
}

# Main script logic
case "${1:-help}" in
    dev)
        start_dev
        ;;
    prod)
        start_prod
        ;;
    build)
        build_images
        ;;
    stop)
        stop_services
        ;;
    restart)
        restart_services
        ;;
    logs)
        show_logs
        ;;
    clean)
        clean_environment
        ;;
    health)
        check_health
        ;;
    db-reset)
        reset_database
        ;;
    help|*)
        show_help
        ;;
esac
