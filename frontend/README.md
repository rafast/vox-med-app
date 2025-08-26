# VoxMed Frontend

A responsive Vue.js web application for the VoxMed medical platform.

## Technologies

- Vue.js 3 with Composition API
- TypeScript
- Vue Router for navigation
- Pinia for state management
- Vite for build tooling
- ESLint & Prettier for code quality

## Getting Started

### Prerequisites

- Node.js (v18 or higher)
- npm or yarn

### Installation & Development

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint

# Format code
npm run format
```

The application will be available at `http://localhost:5173`

## Project Structure

```
frontend/
├── src/
│   ├── components/     # Reusable Vue components
│   ├── views/         # Page components
│   ├── router/        # Vue Router configuration
│   ├── stores/        # Pinia stores
│   ├── assets/        # Static assets
│   └── main.ts        # Application entry point
├── public/            # Public static files
├── index.html         # Main HTML template
└── vite.config.ts     # Vite configuration
```

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Features

- ✅ TypeScript support
- ✅ Vue Router for SPA navigation
- ✅ Pinia for centralized state management
- ✅ ESLint + Prettier for code quality
- ✅ Responsive design ready
- ✅ Modern build tooling with Vite

## Development Guidelines

1. Use TypeScript for all new components
2. Follow Vue 3 Composition API patterns
3. Use Pinia stores for global state
4. Implement responsive design principles
5. Follow ESLint rules and use Prettier for formatting

## Next Steps

1. Configure API integration with the backend
2. Implement authentication system
3. Create medical-specific components and views
4. Add responsive design framework (e.g., Tailwind CSS, Vuetify)
5. Implement error handling and loading states
