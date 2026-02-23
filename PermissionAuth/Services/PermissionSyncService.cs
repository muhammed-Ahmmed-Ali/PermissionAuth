using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionAuth.Data;
using PermissionAuth.Models;

namespace PermissionAuth.Services;


public class PermissionSyncService(AppDbContext db, ILogger<PermissionSyncService> logger)
{
    public async Task SyncAsync(Assembly assembly)
    {
        var discovered = DiscoverPermissions(assembly);

        var existing =  db.Permissions
            .Select(p => p.Name)
            .ToHashSet();

        var toAdd = discovered.Where(p => !existing.Contains(p.Name)).ToList();

        if (toAdd.Count > 0)
        {
            db.Permissions.AddRange(toAdd);
            await db.SaveChangesAsync();
            logger.LogInformation("New permisssions Added");

        }
        else
        {
            logger.LogInformation("Permissions up to date. No new permissions added.");
        }
    }

    private static List<Permission> DiscoverPermissions(Assembly assembly)
    {
        var permissions = new List<Permission>();

        var controllerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract
                     && !t.IsInterface
                     && t.IsSubclassOf(typeof(ControllerBase)));

        foreach (var controller in controllerTypes)
        {
            var module = controller.Name.Replace("Controller", "");
            if (string.IsNullOrEmpty(module)) continue;

            var actions = controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName
                         && m.GetCustomAttribute<NonActionAttribute>() == null);

            foreach (var action in actions)
            {
                var actionName = action.GetCustomAttribute<ActionNameAttribute>()?.Name ?? action.Name;

                if (actionName.EndsWith("Async"))
                    actionName = actionName[..^5];

                var permName = $"{module}.{actionName}";

                if (permissions.Any(p => p.Name == permName)) continue;

                permissions.Add(new Permission
                {
                    Name= permName,
                    Module= module,
                    Action= actionName,
                    Description= $"blaallalala"
                });
            }
        }

        return permissions;
    }
}
