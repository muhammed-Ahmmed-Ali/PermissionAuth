# PermissionAuth — Auto Permission-Based Authorization (.NET 8)

## ⚡ Quick Start (3 steps)

### 1. Update connection string
Edit `appsettings.json`:
```json
"ConnectionStrings": {
  "Default": "Server=YOUR_SERVER;Database=PermissionAuthDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Run the project
```bash
dotnet run
```
✅ EF migrations run automatically  
✅ All permissions discovered and inserted to DB automatically  
✅ Swagger available at: https://localhost:5001/swagger

### 3. Test the flow

**Register a user:**
```
POST /api/auth/register
{ "username": "ahmed", "email": "ahmed@test.com", "password": "123456" }
```

**Login:**
```
POST /api/auth/login
{ "email": "ahmed@test.com", "password": "123456" }
```
→ Copy the token from the response

**See all auto-discovered permissions:**
```
GET /api/admin/permissions
Authorization: Bearer {token}
```

**Create a role:**
```
POST /api/admin/roles
{ "name": "Manager" }
```

**Assign permissions to role:**
```
POST /api/admin/roles/1/permissions
{ "permissionNames": ["Products.GetAll", "Products.Create", "Orders.GetAll"] }
```

**Assign role to user:**
```
POST /api/admin/users/1/roles
{ "roleName": "Manager" }
```

**Now test a protected endpoint:**
```
GET /api/products
Authorization: Bearer {token}
→ 200 OK  ✅ (user has Products.GetAll)

DELETE /api/products/1
Authorization: Bearer {token}
→ 403 Forbidden ❌ (user doesn't have Products.Delete)
```

---

## 🚀 How to Add a New Feature (Zero manual DB work)

Just create a new controller:

```csharp
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpGet]    public IActionResult GetAll()       => Ok();
    [HttpPost]   public IActionResult Create()       => Ok();
    [HttpDelete] public IActionResult Delete(int id) => Ok();
}
```

**On next app start → these permissions are auto-added to DB:**
- `Invoices.GetAll`
- `Invoices.Create`  
- `Invoices.Delete`

Then go to admin panel and assign them to roles. That's it! 🎉

---

## 📁 Project Structure

```
PermissionAuth/
├── Authorization/
│   ├── PermissionMiddleware.cs       ← checks JWT + permission on every request
│   └── RequirePermissionAttribute.cs ← optional attribute for overrides
├── Controllers/
│   ├── AuthController.cs             ← register, login, /me
│   ├── AdminController.cs            ← manage roles, permissions, users
│   └── ExampleControllers.cs         ← Products + Orders examples
├── Data/
│   └── AppDbContext.cs
├── Migrations/                       ← EF Core migrations (auto-applied)
├── Models/
│   └── Models.cs                     ← User, Role, Permission, DTOs
├── Services/
│   ├── JwtService.cs                 ← generates JWT tokens
│   └── PermissionSyncService.cs      ← 🔑 auto-discovers & syncs permissions
├── appsettings.json
└── Program.cs
```

---

## 🛡️ Permission Naming Convention

| Controller | Action | Permission Name |
|------------|--------|-----------------|
| ProductsController | GetAll | `Products.GetAll` |
| ProductsController | Create | `Products.Create` |
| OrdersController | Ship | `Orders.Ship` |

---

## 🎛️ Attribute Options

```csharp
// Auto-derive permission name (default behavior, no attribute needed)
[HttpGet]
public IActionResult GetAll() => Ok();

// Explicit override
[HttpPost("{id}/ship")]
[RequirePermission("Orders.Ship")]
public IActionResult Ship(int id) => Ok();

// Public endpoint — no auth needed
[HttpGet("health")]
[RequirePermission(skip: true)]
public IActionResult Health() => Ok();
```
