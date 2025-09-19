using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("m2_db"));

// Add OpenAPI/Scalar for API docs
builder.Services.AddOpenApi();

var app = builder.Build();

// API docs
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();             // JSON spec
    app.MapScalarApiReference();  // UI docs with Scalar
}

// Health check
app.MapGet("/health", () => Results.Ok("m2 ok"));

// CRUD: Orders (example for M2)
app.MapGet("/orders", async (AppDbContext db) => await db.Orders.ToListAsync());

app.MapPost("/orders", async (Order o, AppDbContext db) =>
{
    db.Orders.Add(o);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{o.Id}", o);
});

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Orders.Any())
    {
        db.Orders.AddRange(
            new Order { ItemName = "Laptop", Quantity = 2 },
            new Order { ItemName = "Phone", Quantity = 5 }
        );
        db.SaveChanges();
    }
}

app.Run();

// Models + DbContext
class Order
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
    public DbSet<Order> Orders => Set<Order>();
}
