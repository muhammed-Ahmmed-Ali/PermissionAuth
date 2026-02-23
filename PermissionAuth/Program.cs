using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PermissionAuth.Authorization;
using PermissionAuth.Data;
using PermissionAuth.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer= true,
            ValidateAudience= true,
            ValidateLifetime= true,
            ValidateIssuerSigningKey = true,
            ValidIssuer= builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey= new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddScoped<PermissionSyncService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddControllers();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db   = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var sync = scope.ServiceProvider.GetRequiredService<PermissionSyncService>();

    await db.Database.MigrateAsync();

    await sync.SyncAsync(Assembly.GetExecutingAssembly());
}

app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<PermissionMiddleware>();  
app.MapControllers();
app.Run();
