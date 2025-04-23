using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Services.Hashing;
using Services.Token;
using System.Text;
using Audit.Core;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


var logger = new LoggerConfiguration()
    .WriteTo.File("Logs/security-log.json", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Logger = logger;

Audit.Core.Configuration.Setup().UseSerilog();
builder.Host.UseSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.
        GetConnectionString("DefaultConnection"));
});

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();


builder.Services.AddSingleton<ITokenService, TokenService>();


builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                ctx.Token = ctx.Request.Cookies["AuthToken"];
                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


var app = builder.Build();



app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
