using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core InMemory
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("m1_db"));

// Add OpenAPI/Scalar for API docs
builder.Services.AddOpenApi();

var app = builder.Build();

// API docs
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();       // JSON spec
    app.MapScalarApiReference(); // UI docs with Scalar
}

// Health check
app.MapGet("/health", () => Results.Ok("m1 ok"));

// CRUD: Products
app.MapGet("/products", async (AppDbContext db) => await db.Products.ToListAsync());

app.MapPost("/products", async (Product p, AppDbContext db) =>
{
    db.Products.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{p.Id}", p);
});

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Products.AddRange(
        new Product { Name = "Apple", Price = 0.5m },
        new Product { Name = "Banana", Price = 0.3m }
    );
    db.SaveChanges();
}

app.Run();

// Models + DbContext
class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }
    public DbSet<Product> Products => Set<Product>();
}
