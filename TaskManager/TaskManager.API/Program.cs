using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Security;
using TaskManager.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register services BEFORE Build
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
