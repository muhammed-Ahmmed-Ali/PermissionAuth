using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using PermissionAuth.Data;

namespace PermissionAuth.Authorization;

public class PermissionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext db)
    {
        var endpoint = context.GetEndpoint();

        var skipAttr = endpoint?.Metadata.GetMetadata<RequirePermissionAttribute>();
        if (skipAttr?.Skip == true || endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await next(context);
            return;
        }

        var token = ExtractBearerToken(context);
        if (token == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Authorization token is required." });
            return;
        }

        if (!TryGetUserId(token, out var userId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid token." });
            return;
        }

        var required = ResolvePermission(endpoint, skipAttr);
        if (required == null)
        {
            await next(context);
            return;
        }

        var hasPermission = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Name == required);

        if (!hasPermission)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new
            {
                error              = "Access denied.",
                requiredPermission = required
            });
            return;
        }
        await next(context);
    }
    private static string? ExtractBearerToken(HttpContext ctx)
    {
        var header = ctx.Request.Headers.Authorization.FirstOrDefault();
        if (header == null || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;
        return header["Bearer ".Length..].Trim();
    }

    private static bool TryGetUserId(string token, out int userId)
    {
        userId = 0;
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var val = jwt.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            return int.TryParse(val, out userId);
        }
        catch { return false; }
    }

    private static string? ResolvePermission(Endpoint? endpoint, RequirePermissionAttribute? attr)
    {
        if (attr?.Permission != null) return attr.Permission;
        var desc = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (desc == null) return null;

        return $"{desc.ControllerName}.{desc.ActionName}";
    }
}
