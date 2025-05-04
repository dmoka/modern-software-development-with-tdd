## Feature: Syncronizing Inventory

### Synchronizing Inventory Between Systems

As a system administrator,

I want the inventory synchronization process to keep internal stock levels consistent with external warehouse data,

So that I can ensure accurate inventory records and timely stock updates.

### Behaviours

The main `Sync` method coordinates the inventory synchronization process:

1. Fetch all products from the internal repository.
1. Retrieve stock levels from the external warehouse service.
   - If no stock data is available, log a warning for the product.
1. For each product:
   - Sync the internal and external stock levels.
   - Mark the product as out of stock if the external stock level is `0`.
1. If there are no updates for any products:
   - Log info that no significant changes occurred.
   - Only log if there are products to be updated.

### Notify Admin feature (Self-exercise)

Sends notifications email to admins about the completion of the entire synchronization process.

The email should be sent to "admin@tdd.com"

- **Scenarios:**
  - If there are any updates during the sync process, notify the admin about the completion.
	  - Subject: "Inventory Sync Completed"
	  - Body: "The inventory synchronization process completed successfully."
  - If any error occurrs globally in the sync process, notify the admin about the failure.
	  - Subject: "Alert: Unexpected Sync Error"
	  - Body: "The inventory synchronization process failed unexpectedly: <error>."
