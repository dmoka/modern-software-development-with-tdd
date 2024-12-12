# Product Domain Entity Specification

We are building the **Product** domain entity for an inventory management system. This entity will handle product details, stock levels, and operations like picking and unpicking items. This document outlines the features and behaviors we need to implement.

---

## **Product**

### **Attributes**
- `Id`: Unique identifier for the product. It is generated during creation.
- `Name`: Name of the product
- `Description`: Short description of the product.
- `Price`: Monetary value of the product.
- `LastOperation`: Stores the last operation performed on the product. Values: None, Picked or Unpicked.
- `StockLevel`: Tracks the current stock level.

### **Behavior**
- **Create Product**:
  - The LastOperation should be set to `None`.
  - The StockLevel should be created with an initial quantity of 20.

- **Pick Items**:
  - Reduce the stock by a specified quantity.
  - LastOperation should be set to `Picked`
  - Ensure:
    - The quantity picked does not exceed available stock.
    - The product's quality status is `Available`.
    - The pick quantity does not exceed the maximum limit per operation.

### Self-exercise

- **Unpick Items**:
  - Increase the stock by a specified quantity.
  - LastOperation should be set to `Unpicked`
  - Ensure:
    - The total stock after unpicking does not exceed the maximum stock level.
    - The product's quality status is Available.

---

## **StockLevel** Quality Status
Indicates if the stock is available, expired, or damaged.

Set to `Available` on default

## **Constraints**
- **Maximum stock level**: 50 items.
- **Maximum pick quantity per operation**: 10 items.
- Picking/Unpicking is only allowed when stock quality status is `Available`. The other statuses are:
  - **Expired**: Indicates the product has exceeded its shelf life.
  - **Damaged**: Indicates the product is not in usable condition.