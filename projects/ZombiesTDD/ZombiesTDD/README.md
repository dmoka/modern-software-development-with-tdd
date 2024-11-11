# Product Searcher Specification

The **Product Searcher** is a utility to filter and retrieve products based on various criteria. This project will implement its functionality using TDD, following the **Z.O.M.B.I.E.S.** principles.

---

## Specification

### **Search Functionality**

Create a searcher module that must filter a collection of products based on the following criteria:

1. **Search Term**: Filter products whose name or description contains the search term (case-insensitive).
2. **Price Range**:
   - Products must have a price greater than or equal to a specified minimum price.
   - Products must have a price less than or equal to a specified maximum price.
3. **Quality Status**: Filter products based on their stock's quality (e.g., Available, Damaged, Expired).
4. **Sorting**:
   - Products can be sorted by price in ascending or descending order.

---

### **Input**

The search method must accept a `SearchCriteria` object containing:
- `SearchTerm` (string)
- `MinPrice` (double, optional)
- `MaxPrice` (double, optional)
- `QualityStatus` (enum, optional)
- `SortByPriceAscending` (boolean, optional)
- `SortByPriceDescending` (boolean, optional)

---

### **Output**

The search method must return a `SearchResult` containing:
- A list of products that match the criteria.
- A message indicating the result of the search (e.g., "No products found", "3 products found").

---

### **Constraints**

1. If `MinPrice` or `MaxPrice` is negative, throw an exception.
2. If `MaxPrice` is less than `MinPrice`, throw an exception.
3. If no products match the criteria, return an empty result list with an appropriate message.
4. The search must handle edge cases (e.g., empty product list, null inputs) gracefully.

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