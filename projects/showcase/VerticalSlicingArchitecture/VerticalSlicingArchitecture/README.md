## Project Overview

A simple warehouse management system built using .NET 8 and vertical slice architecture. The system allows managing products and their stock levels in a warehouse.

## Core Features

### 1. Product Management

#### Create Product

- Endpoint: `POST /api/products`
- Creates a new product with initial stock level
- Required fields:
  - Name (non-empty)
  - Description (non-empty)
  - Price (greater than 0)
  - InitialStock (number)
- Business Rules:
  - Initial stock must not exceed maximum stock level (default: 50)
  - Initial stock must be minimum 10
  - Can't have multiple product with same name.
  - Business validation errors should return 409 conflict
- Returns: Product ID with correct location in header
- Error Codes:
  - `CreateProduct.Validation`: For input validation failures
  - `StockLevel.ExceedsMaximum`: When initial stock exceeds maximum
  - `StockLevel.BelowMinimum`: When initial stock is below minimum
  - `CreateProduct.AlreadyExists`: When product already exists

#### Get Product

- Endpoint: `GET /api/products/{id}`
- Retrieves a single product by ID
- Returns:
  - Product details (Name, Description, Price, StockLevel(Quantity))
  - 404 Not Found if product doesn't exist
- Error Codes:
  - `GetProduct.NotFound`: When product with specified ID doesn't exist

#### Update Product (homework)

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

#### Search Products (homework)

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
- Error Codes:
  - `PickProduct.Validation`: For input validation failures
  - `PickProduct.NotFound`: When product doesn't exist
  - `PickProduct.InsufficientStock`: When trying to pick more than available

#### Unpick Product (Self-exercise)

- Endpoint: `POST /api/products/{productId}/unpick`
- Increases stock level for a product
- Required fields:
  - ProductId
  - UnpickCount (greater than 0)
- Business Rules:
  - Cannot unpick to non-existent product
  - Cannot exceed maximum stock level (default: 50)
- Error Codes:
  - `UnpickProduct.Validation`: For input validation failures
  - `UnpickProduct.NotFound`: When product doesn't exist
  - `StockLevel.ExceedsMaximum`: When unpick would exceed maximum stock level

### Error Handling

Erros are returned as JSON response with appropriate status code, error code and description. Example:

```json
{
  "code": "PickProduct.InsufficientStock",
  "description": "Insufficient stock available for picking"
}
```