# Restaurant API

REST API built with ASP.NET Core for a restaurant management system.

This backend provides authentication, role-based authorization, product management, order management, user administration and audit tracking.

## Features

- JWT Authentication
- Role-Based Authorization (Admin / User)
- User Registration and Login
- Product CRUD Operations
- Soft Delete for Products
- Product Pagination
- Order Management
- Order Status Tracking
- Admin User Management
- Audit Logging
- Global Exception Handling
- Request Validation with FluentValidation
- Entity Framework Core Integration
- SQLite Database

## Tech Stack

- ASP.NET Core
- Entity Framework Core
- SQLite
- JWT Bearer Authentication
- FluentValidation
- C#
- REST API

## Main Endpoints

### Auth

- `POST /api/auth/login`
- `POST /api/auth/register`

### Products

- `GET /api/products`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `PUT /api/products/{id}/deactivate`

### Orders

- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PUT /api/orders/{id}/status`

### Admin Users

- `GET /api/admin/users`
- `PUT /api/admin/users/{id}/activate`
- `PUT /api/admin/users/{id}/deactivate`

## Architecture

The project follows a layered structure:

```text
RestaurantApi/
├── Controllers/
├── Data/
├── DTOs/
├── Models/
├── Services/
├── Validators/
├── Middleware/
└── Exceptions/
