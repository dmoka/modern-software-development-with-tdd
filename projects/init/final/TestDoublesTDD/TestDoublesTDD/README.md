## Feature: Syncronizing Inventory

### Synchronizing Inventory Between Systems
As a system administrator,

I want the inventory synchronization process to keep internal stock levels consistent with external warehouse data,

So that I can ensure accurate inventory records and timely stock updates.

### Behaviours
The main `Sync` method coordinates the inventory synchronization process:

1. Fetch all products from the internal repository.
2. Retrieve stock levels from the external warehouse service.
   - If no stock data is available, log a warning for the product.
3. For each product:
   - Compare the internal and external stock levels.
   - Mark the product as out of stock if the external stock level is `0`.
4. If there are updates:
   - Publish the updated stock levels for external partners and resellers.
   - Send an email notification to the administrator.
5. If there are no updates for all the products:
   - Log that no significant changes occurred.
   - Only log if there are products to be updated.


//TODO: we might not need this at all
### Publish Stock Changes
Publishes updated stock levels for products with any changes.

- **Condition:**  
  Only executed if there are products with updated stock levels.
- **Action:**  
  Calls `IStockPublisher.Publish()` with the updated stock data.

---

### Notify Admin
Sends notifications about the synchronization process.
The email should be sent to "admin@tdd.com"

- **Scenarios:**
  - If there are any updates, notify the admin about the completion.
	- Subject: "Inventory Sync Completed"
	- Body: "The inventory synchronization process completed successfully."
  - If an error occurs, notify the admin about the failure.
	- Subject: "Alert: Unexpected Sycn Error"
	- Body: "The inventory synchronization process failed unexpectedly: <error>."

---

//TODO: measure time we might not need it either
### Error Handling
- If an error occurs during synchronization:
  - Logs the error using `ILogger`.
  - Sends an alert email to the admin using `INotificationService`.
