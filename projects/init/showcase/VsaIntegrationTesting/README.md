# Warehouse Management System

A simple warehouse management system built using .NET 8 and vertical slice architecture. The system allows managing products and their stock levels in a warehouse.

## Project Overview

This project demonstrates vertical slice architecture implementation with the following features:

- Product management
- Stock level tracking
- Pick and unpick operations for products

## Core Features

### 1. Product Management

#### Get Product

- Endpoint: `GET /api/products/{id}`
- Retrieves a single product by ID
- Returns:
  - Product details (ID, Name, Description, Price)
  - 404 Not Found if product doesn't exist
- Error Codes:
  - `GetProduct.NotFound`: When product with specified ID doesn't exist

#### Create Product

- Endpoint: `POST /api/products`
- Creates a new product with initial stock level
- Required fields:
  - Name (non-empty)
  - Description (non-empty)
  - Price (greater than 0)
  - InitialStock (number)
- Business Rules:
  - Initial stock must not exceed maximum stock level (default: 1000)
  - Initial stock must not be negative
- Returns: Product ID
- Error Codes:
  - `CreateArticle.Validation`: For input validation failures
  - `StockLevel.ExceedsMaximum`: When initial stock exceeds maximum
  - `StockLevel.BelowMinimum`: When initial stock is negative

#### Update Product

- Endpoint: `PUT /api/products/{id}`
- Updates existing product details
- Required fields:
  - Name (non-empty)
  - Description (non-empty)
  - Price (greater than 0)
- Business Rules:
  - Cannot update non-existent product
  - Price must be greater than 0
- Error Codes:
  - `UpdateProduct.Validation`: For input validation failures
  - `UpdateProduct.NotFound`: When product doesn't exist

#### Search Products

- Endpoint: `GET /api/products`
- Search and filter products
- Optional query parameters:
  - searchTerm (searches in name and description)
  - minPrice
  - maxPrice
- Returns: List of products matching criteria
- Note: Returns empty list instead of error when no products found

### 2. Stock Management

#### Pick Product

- Endpoint: `POST /api/products/{productId}/pick`
- Reduces stock level for a product
- Required fields:
  - ProductId
  - PickCount (greater than 0)
- Business Rules:
  - Cannot pick from non-existent product
  - Cannot pick more than available stock
  - Pick count must be greater than 0
  - Stock level cannot go below 0
- Error Codes:
  - `PickProduct.Validation`: For input validation failures
  - `PickProduct.NoProductExist`: When product doesn't exist
  - `StockLevel.BelowMinimum`: When pick would result in negative stock
  - `PickProduct.InsufficientStock`: When trying to pick more than available

#### Unpick Product

- Endpoint: `POST /api/products/{productId}/unpick`
- Increases stock level for a product
- Required fields:
  - ProductId
  - UnpickCount (greater than 0)
- Business Rules:
  - Cannot unpick to non-existent product
  - UnpickCount must be greater than 0
  - Cannot exceed maximum stock level (default: 1000)
  - Final stock level after unpick cannot exceed maximum stock level
- Error Codes:
  - `UnpickProduct.Validation`: For input validation failures
  - `UnpickProduct.Validation`: When product doesn't exist
  - `StockLevel.ExceedsMaximum`: When unpick would exceed maximum stock level

### Stock Level Business Rules

- Maximum stock level: 1000 (configurable)
- Minimum stock level: 0
- Stock operations must maintain levels between min and max
- Stock level is tracked per product
- Stock level cannot be negative
- Stock level cannot exceed maximum limit

### Error Handling

All operations use a Result pattern that can contain either a success value or an error. Common error patterns:

#### Validation Errors

- Input validation errors: `{Operation}.Validation`
- Entity validation errors: `{Entity}.{Rule}`

#### Stock Level Errors

- `StockLevel.ExceedsMaximum`: Stock operation would exceed maximum limit
- `StockLevel.BelowMinimum`: Stock operation would result in negative stock

#### Not Found Errors

- `{Operation}.NotFound`: Resource not found
- `{Operation}.NoProductExist`: Product doesn't exist

#### Business Rule Violations

- `PickProduct.InsufficientStock`: Insufficient stock for pick operation
