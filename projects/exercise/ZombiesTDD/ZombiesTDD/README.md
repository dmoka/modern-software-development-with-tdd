# Product Searcher Specification

The **Product Searcher** is a utility to filter and retrieve products based on various criteria. This project will implement its functionality using TDD, following the **Z.O.M.B.I.E.S.** principles.

---

## Specification

### **Search Functionality**

Create a searcher module that must filter a collection of products based on the following criteria:

1. If no products match the criteria, return an empty result list.
1. The search must throw error when null provided as products input.
1. **Search Term**: Filter products whose name or description contains the search term (case-insensitive).
1. **Quality Status**: Filter products based on their stock's quality (e.g., Available, Damaged, Expired).
1. **Price Range** (Self-exercise):
   - Price properties are optional parameters
   - Products must have a price greater than or equal to a specified minimum price.
   - Products must have a price less than or equal to a specified maximum price.
   - If `MinPrice` or `MaxPrice` is negative, throw an exception.
   - If `MaxPrice` is less than `MinPrice`, throw an exception.
1. **Sorting** (Self-exercise):
   - Products can be sorted by one of these:
   - by name in ascending order
   - by name in descending order
   - by price in ascending order
   - by price in descending orde

---

### **Input**

The search method the following properties:

- `SearchTerm` (string)
- `MinPrice` (decimal, optional)
- `MaxPrice` (decimal, optional)
- `QualityStatus` (enum)
- `Sorting` (enum)

---

### **Output**

List of prodicts according to the search criteria.

---

### **Example Scenarios**

1. **Zero Case**: No products match the search criteria.
2. **One Case**: Only one product matches the search criteria.
3. **Many Case**: Multiple products match the search criteria.
4. **Boundary Conditions**:
   - `MinPrice = 0`
   - `MaxPrice = MaxAllowed`
   - SearchTerm = empty or whitespace
5. **Exceptional Cases**:
   - Invalid price range (`MaxPrice < MinPrice`).
   - Negative values for `MinPrice` or `MaxPrice`.

---
