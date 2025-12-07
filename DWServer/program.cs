using DWServer.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Adăugăm serviciile MVC
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Configurăm conexiunea la baza de date ---

// 1. Încercăm întâi să luăm DATABASE_URL (pentru Render)
var renderConnection = builder.Configuration.GetValue<string>("DATABASE_URL");

// 2. Dacă e null → suntem local → folosim DefaultConnection din appsettings.json
var connectionString = renderConnection ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Adăugăm contextul
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// Middleware pentru Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
    db.Database.Migrate();
}

app.Run();
