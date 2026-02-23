using Microsoft.AspNetCore.Mvc;
using PermissionAuth.Authorization;

namespace PermissionAuth.Controllers;

/// <summary>
/// Example controller — adding this file alone will auto-create these permissions in DB:
///   Products.GetAll
///   Products.GetById
///   Products.Create
///   Products.Update
///   Products.Delete
/// No manual DB work needed!
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll()
        => Ok(new[] {
            new { Id = 1, Name = "Laptop",  Price = 999 },
            new { Id = 2, Name = "Monitor", Price = 399 }
        });

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => Ok(new { Id = id, Name = "Laptop", Price = 999 });

    [HttpPost]
    public IActionResult Create([FromBody] object body)
        => Ok(new { message = "Product created", data = body });

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] object body)
        => Ok(new { message = $"Product {id} updated", data = body });

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
        => Ok(new { message = $"Product {id} deleted" });
}


/// <summary>
/// Another example — Orders controller.
/// Auto-creates: Orders.GetAll, Orders.GetById, Orders.Create, Orders.Ship
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() => Ok(new[] { new { Id = 1, Status = "Pending" } });
    [HttpGet("{id}")]
    [RequirePermission(skip: true)]

    public IActionResult GetById(int id) => Ok(new { Id = id, Status = "Pending" });

    [HttpPost]
    [RequirePermission(skip : true)]
    public IActionResult Create([FromBody] object body) => Ok(new { message = "Order created" });

    // Explicit override — still auto-discovered as "Orders.Ship"
    [HttpPost("{id}/ship")]
    [RequirePermission("Orders.Ship")]
    public IActionResult Ship(int id) => Ok(new { message = $"Order {id} shipped" });
}
