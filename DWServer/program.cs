using DWServer.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Adăugăm serviciile MVC
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Configurăm conexiunea la baza de date ---
var renderConnection = builder.Configuration.GetValue<string>("DATABASE_URL");
string connectionString;

if (!string.IsNullOrEmpty(renderConnection))
{
    // Parsează DATABASE_URL în format Npgsql
    var databaseUri = new Uri(renderConnection);
    var userInfo = databaseUri.UserInfo.Split(':');

    var builderNpgsql = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.AbsolutePath.TrimStart('/'),
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    connectionString = builderNpgsql.ToString();
}
else
{
    // Local → folosește DefaultConnection din appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

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

// Aplica migrațiile automat
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
    db.Database.Migrate();
}

app.Run();
