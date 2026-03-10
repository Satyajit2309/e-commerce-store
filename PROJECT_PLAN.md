# Mini E-Commerce Store - Detailed Implementation Artifact

## 1. Project Overview

### 1.1 Objective
Build an intermediate-level Blazor web application that simulates a small online store. The project should help strengthen practical .NET skills across UI development, data modeling, authentication/authorization, and layered application design.

### 1.2 Learning Outcomes
By completing this project, you will gain hands-on experience with:
- Blazor component-driven UI development
- ASP.NET Core Identity (authentication and role-based authorization)
- EF Core code-first modeling, migrations, and relational design
- Service-oriented architecture with dependency injection
- End-to-end workflow: browse -> cart -> checkout -> order management
- Basic testing and production readiness concepts

### 1.3 MVP Scope
- Customer registration/login/logout
- Product listing, filtering, sorting, and details
- Shopping cart management
- Checkout flow with order creation
- Customer order history
- Admin product/category management
- Admin order status updates

## 2. Proposed Technology Stack

### 2.1 Core Stack
- .NET 8/9 Blazor Web App (Interactive Server for simpler learning path)
- ASP.NET Core Identity
- Entity Framework Core (Code-First)
- SQL Server Express or SQLite (SQLite recommended for easier local setup)
- Bootstrap for initial UI styling

### 2.2 Tooling
- Visual Studio 2022 or VS Code + C# Dev Kit
- EF Core CLI tools (`dotnet ef`)
- Git for source control
- Optional: xUnit for tests

## 3. Architecture Plan

### 3.1 High-Level Layers
- UI Layer: Blazor components/pages (`Components/Pages`)
- Application Layer: Business/services interfaces and implementations
- Data Layer: EF Core context, entity configurations, repositories (if used)
- Identity Layer: User/role management and authorization policies

### 3.2 Architectural Principles
- Keep business logic out of UI components as much as possible
- Use service interfaces for core operations (products, cart, orders)
- Prefer async DB/API operations
- Use DTO/view models where needed to avoid overexposing entities

## 4. Domain Model and Data Design

### 4.1 Entities (Initial)
- `ApplicationUser`
  - Id, Email, FullName, CreatedAt
- `Category`
  - Id, Name, Description
- `Product`
  - Id, Name, Description, Price, Stock, ImageUrl, IsActive, CategoryId
- `CartItem`
  - Id, UserId, ProductId, Quantity, AddedAt
- `Order`
  - Id, UserId, OrderDate, Status, TotalAmount, ShippingAddress, PhoneNumber
- `OrderItem`
  - Id, OrderId, ProductId, Quantity, UnitPrice, LineTotal

### 4.2 Key Relationships
- Category 1 -> many Products
- User 1 -> many CartItems
- User 1 -> many Orders
- Order 1 -> many OrderItems
- Product referenced by CartItems and OrderItems

### 4.3 Order Status Workflow
- `Pending` -> `Paid` -> `Shipped` -> `Delivered`
- Admin may set `Cancelled` from `Pending` or `Paid`

## 5. Feature Breakdown and Milestones

## Milestone 0 - Foundation Setup
### Goal
Create stable project baseline and conventions.

### Tasks
- Verify project runs (`dotnet run`)
- Add solution folders (if needed): `Data`, `Models`, `Services`, `Contracts`
- Configure app settings for database connection
- Add EF Core + Identity packages if missing
- Enable detailed errors in development

### Deliverable
- Running base app with clean project structure

## Milestone 1 - Identity and Authorization
### Goal
Enable user authentication and role-based access.

### Tasks
- Configure ASP.NET Core Identity
- Create roles: `Admin`, `Customer`
- Build role seeding on startup
- Protect admin pages using `[Authorize(Roles = "Admin")]`
- Add auth-aware navbar items

### Acceptance Criteria
- User can register/login/logout
- Admin-only page blocks non-admin users
- Roles are seeded automatically

## Milestone 2 - Catalog (Products + Categories)
### Goal
Deliver browseable storefront.

### Tasks
- Build category and product entities + migrations
- Seed sample categories/products
- Create pages:
  - Product listing
  - Product details
- Add search/filter/sort:
  - Search by name/description
  - Filter by category
  - Sort by price/name
- Add pagination (optional in MVP, recommended)

### Acceptance Criteria
- Product list loads from DB
- Search/filter/sort works correctly
- Product details page shows complete info

## Milestone 3 - Shopping Cart
### Goal
Enable user cart operations.

### Tasks
- Create cart service interface + implementation
- Add cart persistence in DB (`CartItems`)
- Cart operations:
  - Add item
  - Increase/decrease quantity
  - Remove item
  - Clear cart
- Compute subtotal and total
- Add cart badge count in navbar

### Acceptance Criteria
- Logged-in user can manage cart reliably
- Totals are correct after every operation

## Milestone 4 - Checkout and Orders
### Goal
Convert cart to order with history tracking.

### Tasks
- Build checkout form with validation
- Create order transaction logic:
  - Validate stock
  - Create `Order` + `OrderItems`
  - Reduce product stock
  - Clear cart
- Create customer pages:
  - My Orders
  - Order Details

### Acceptance Criteria
- Checkout creates order records successfully
- Stock is reduced after successful order
- User can view historical orders

## Milestone 5 - Admin Panel
### Goal
Provide store management operations.

### Tasks
- Admin CRUD for categories
- Admin CRUD for products
- Stock management
- Order management page to update status
- Dashboard summary cards:
  - Total orders
  - Revenue
  - Low stock products count

### Acceptance Criteria
- Admin can manage catalog and order status
- Non-admin users cannot access admin panel

## Milestone 6 - Hardening and Polish
### Goal
Improve reliability and user experience.

### Tasks
- Add server-side validation and error handling
- Handle edge cases (out-of-stock, invalid quantity)
- Improve UI/UX consistency
- Add toast/snackbar notifications
- Add logging for order lifecycle events

### Acceptance Criteria
- No major unhandled exceptions during normal flows
- Important actions provide user feedback

## 6. Suggested Folder Structure

```text
/e-commerce-store
  /Components
    /Pages
      /Admin
      /Catalog
      /Cart
      /Orders
      /Account
    /Shared
  /Data
    ApplicationDbContext.cs
    DbInitializer.cs
    /Configurations
  /Models
    /Entities
    /DTOs
    /ViewModels
  /Services
    /Interfaces
    /Implementations
  /Constants
    Roles.cs
    OrderStatuses.cs
  Program.cs
```

## 7. Service Contract Draft

### 7.1 Product Service
- `GetProductsAsync(filter, sort, page)`
- `GetProductByIdAsync(id)`
- `CreateProductAsync(product)`
- `UpdateProductAsync(product)`
- `DeleteProductAsync(id)`

### 7.2 Cart Service
- `GetCartAsync(userId)`
- `AddToCartAsync(userId, productId, quantity)`
- `UpdateQuantityAsync(userId, productId, quantity)`
- `RemoveFromCartAsync(userId, productId)`
- `ClearCartAsync(userId)`

### 7.3 Order Service
- `PlaceOrderAsync(userId, checkoutModel)`
- `GetOrdersForUserAsync(userId)`
- `GetOrderByIdAsync(orderId, userId)`
- `UpdateOrderStatusAsync(orderId, status)`

## 8. Security and Validation Plan

- Require authenticated user for cart/checkout/orders
- Restrict admin routes by role
- Validate product price and stock boundaries
- Prevent checkout with empty cart
- Verify stock on server before placing order
- Avoid trust in client-calculated totals

## 9. Testing Strategy

### 9.1 Manual Test Scenarios (Minimum)
- Register + login + logout flow
- Add/remove cart items and verify totals
- Checkout with valid and invalid data
- Admin CRUD for products and categories
- Unauthorized user tries opening admin routes

### 9.2 Automated Testing (Optional but Recommended)
- Unit tests for services (cart/order total logic)
- Integration tests for order placement and stock updates
- Authorization tests for admin endpoints/pages

## 10. Dev Workflow Plan

### 10.1 Branching
- `main` stable
- feature branches per milestone (`feature/m2-catalog`, etc.)

### 10.2 Commit Convention
- `feat: add product listing with category filter`
- `fix: prevent checkout when cart is empty`
- `refactor: move order logic into service`

### 10.3 Done Criteria Per Milestone
- Functionality implemented
- Basic validation and error handling in place
- Smoke-tested manually
- Changes committed with clear message

## 11. Risks and Mitigations

- Risk: Mixing UI and business logic in components
  - Mitigation: Keep logic in services, keep components thin
- Risk: Complex state management in cart/order flow
  - Mitigation: Centralize through a cart service and explicit update methods
- Risk: Authorization gaps on admin pages
  - Mitigation: Enforce role checks at route/component and service entry points

## 12. Timeline (5 Weeks)

- Week 1: Foundation + Identity + role seeding
- Week 2: Catalog + search/filter/sort
- Week 3: Cart + calculations + cart UX
- Week 4: Checkout + order history + stock updates
- Week 5: Admin panel + polish + regression testing

## 13. Next Execution Steps (Immediate)

1. Add required NuGet packages (Identity + EF Core provider)
2. Create `ApplicationDbContext` and entity models
3. Configure DB connection and run first migration
4. Implement role seeding (`Admin`, `Customer`)
5. Build catalog pages with seeded data

---
This document is the working implementation artifact for the project and should be updated at the end of each milestone with decisions, blockers, and completed items.
