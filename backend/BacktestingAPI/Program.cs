using BacktestingAPI.Data;
using BacktestingAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for React dev server
const string CorsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// DbContext - SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=backtesting.db";
    options.UseSqlite(cs);
});

// Repositories
builder.Services.AddScoped<StockDataRepository>();

// Services
builder.Services.AddHttpClient<AlphaVantageService>();
builder.Services.AddSingleton<StrategyService>();
builder.Services.AddScoped<BacktestingEngine>();

var app = builder.Build();

// Apply migrations / create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        // Seed minimal sample data if DB is empty
        if (!db.StockData.Any())
        {
            var today = DateTime.UtcNow.Date;
            var list = new List<BacktestingAPI.Models.StockData>();
            decimal price = 100m;
            var rng = new Random(42); // Fixed seed for reproducibility
            for (int i = 30; i >= 1; i--)
            {
                var date = today.AddDays(-i);
                price += (decimal)(rng.NextDouble() - 0.5); // small drift
                list.Add(new BacktestingAPI.Models.StockData
                {
                    Symbol = "AAPL",
                    Date = date,
                    Open = price - 0.5m,
                    High = price + 0.8m,
                    Low = price - 0.8m,
                    Close = price,
                    Volume = 1000000,
                    LastUpdated = DateTime.UtcNow
                });
            }
            db.StockData.AddRange(list);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        logger.LogError(ex, "Database migration failed");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);

app.MapControllers();

app.Run();
