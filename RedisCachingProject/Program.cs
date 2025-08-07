using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using RedisCachingProject.Data;
using RedisCachingProject.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// InMemory DB
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("UserDb"));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();


// بعد app.Run(); ضيف الكود ده أو فوقه بشوية
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // لو مفيش بيانات، نضيف
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Name ="Romisaa", Email = "Romisaa@example.com" },
            new User { Name = "Khaled", Email = "khaled@example.com" },
            new User { Name = "Mohamed", Email = "Mohamed@example.com" }
        );

        db.SaveChanges();
    }
}
app.Run();
