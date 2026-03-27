using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using UserService.Data;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();



builder.Services.AddDbContext<UserServiceDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("AllowReact");

app.UseAuthorization();

app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    var hasher = new PasswordHasher<User>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            
                new User
                {
                    Username = "admin",
                    Password = hasher.HashPassword(null, "admin"),
                    Email = "admin@test.se",
                    FirstName = "Admin",
                    LastName = "User",
                    Role = "Admin"
                },

                new User
                {
                    Username = "user",
                    Password = hasher.HashPassword(null, "user"),
                    Email = "user@test.se",
                    FirstName = "Regular",
                    LastName = "User",
                    Role = "User"
                },

                new User
                {
                    Username = "calle",
                    Password = hasher.HashPassword(null, "123"),
                    Email = "calle@test.se",
                    FirstName = "Calle",
                    LastName = "Bülow",
                    Role = "User"
                },

                new User
                {
                    Username = "emma",
                    Password = hasher.HashPassword(null, "123"),
                    Email = "emma@test.se",
                    FirstName = "Emma",
                    LastName = "Johansson",
                    Role = "User"
                },

                new User
                {
                    Username = "oskar",
                    Password = hasher.HashPassword(null, "123"),
                    Email = "oskar@test.se",
                    FirstName = "Oskar",
                    LastName = "Lindberg",
                    Role = "User"
                },

                new User
                {
                    Username = "maria",
                    Password = hasher.HashPassword(null, "123"),
                    Email = "maria@test.se",
                    FirstName = "Maria",
                    LastName = "Svensson",
                    Role = "User"
                },

                new User
                {
                    Username = "erik",
                    Password = hasher.HashPassword(null, "123"),
                    Email = "erik@test.se",
                    FirstName = "Erik",
                    LastName = "Karlsson",
                    Role = "User"
                }
            );

        db.SaveChanges();
    }
}

app.Run();