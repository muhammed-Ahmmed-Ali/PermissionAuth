namespace PermissionAuth.Authorization;


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequirePermissionAttribute(string? permission = null, bool skip = false) : Attribute
{
    public string? Permission { get; } = permission;
    public bool Skip { get; } = skip;
}
