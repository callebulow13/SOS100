using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Dtos;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserServiceDbContext _dbContext;
    
    public UserController(UserServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto login)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u =>
                u.Username == login.Username &&
                u.Password == login.Password);
        if (user == null)
            return Unauthorized();
        
        return Ok(new
        {
            user.UserID,
            user.Username,
            user.Role
        });
    }
    
    [HttpGet]
    public User[] GetUsers()
    {
        User[] users = _dbContext.Users.ToArray();
        return users;
    }

    [HttpPost]
    public void AddUser(User user)
    {
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }
}