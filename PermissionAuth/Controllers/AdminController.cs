using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionAuth.Data;
using PermissionAuth.Models;

namespace PermissionAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(AppDbContext db) : ControllerBase
{
    //  Permissions

   
    [HttpGet("permissions")]
    public async Task<IActionResult> GetPermissions()
    {
        var permissions = await db.Permissions
            .OrderBy(p => p.Module).ThenBy(p => p.Action)
            .Select(p => new { p.Id, p.Name, p.Module, p.Action, p.Description })
            .ToListAsync();

        return Ok(permissions);
    }

    //  Roles

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await db.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Select(r => new
            {
                r.Id,
                r.Name,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Name)
            })
            .ToListAsync();

        return Ok(roles);
    }

    //Create Role
    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest req)
    {
        if (await db.Roles.AnyAsync(r => r.Name == req.Name))
            return Conflict(new { error = $"Role '{req.Name}' already exists." });

        var role = new Role { Name = req.Name };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        return Ok(new { role.Id, role.Name });
    }


    // Delete role 
    [HttpDelete("roles/{id}")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await db.Roles.FindAsync(id);
        if (role == null) return NotFound();

        db.Roles.Remove(role);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Assign Permission to role 

    [HttpPost("roles/{roleId}/permissions")]
    public async Task<IActionResult> AssignPermissions(int roleId, [FromBody] AssignPermissionsRequest req)
    {
        var role = await db.Roles.FindAsync(roleId);
        if (role == null) return NotFound(new { error = "Role not found." });

        var permissions = await db.Permissions
            .Where(p => req.PermissionNames.Contains(p.Name))
            .ToListAsync();

        var notFound = req.PermissionNames.Except(permissions.Select(p => p.Name)).ToList();
        if (notFound.Count > 0)
            return BadRequest(new { error = "Permissions not found", notFound });

        var existing =  db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToHashSet();

        var toAdd = permissions
            .Where(p => !existing.Contains(p.Id))
            .Select(p => new RolePermission { RoleId = roleId, PermissionId = p.Id })
            .ToList();

        db.RolePermissions.AddRange(toAdd);
        await db.SaveChangesAsync();

        return Ok(new { assigned = toAdd.Count, skippedAlreadyExist = permissions.Count - toAdd.Count });
    }

    [HttpDelete("roles/{roleId}/permissions/{permissionName}")]
    public async Task<IActionResult> RevokePermission(int roleId, string permissionName)
    {
        var rp = await db.RolePermissions
            .Include(x => x.Permission)
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.Permission.Name == permissionName);

        if (rp == null) return NotFound();

        db.RolePermissions.Remove(rp);
        await db.SaveChangesAsync();
        return NoContent();
    }

    //GetUsers

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                Roles = u.UserRoles.Select(ur => ur.Role.Name)
            })
            .ToListAsync();

        return Ok(users);
    }

    //Assign role to user
    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AssignRoleToUser(int userId, [FromBody] AssignRoleRequest req)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.Name == req.RoleName);
        if (role == null) return NotFound(new { error = $"Role '{req.RoleName}' not found." });

        var exists = await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
        if (!exists)
        {
            db.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
            await db.SaveChangesAsync();
        }

        return Ok(new { userId, role = role.Name });
    }

    //Delete role from user
    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<IActionResult> RevokeRoleFromUser(int userId, string roleName)
    {
        var ur = await db.UserRoles
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Role.Name == roleName);

        if (ur == null) return NotFound();

        db.UserRoles.Remove(ur);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
