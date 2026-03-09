using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("{id}")]
    public ActionResult<User> GetUser(int id)
    {
        var user = _dbContext.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    

    [HttpPost]
    public void AddUser(User user)
    {
        _dbContext.Users.Add(user);
        _dbContext.SaveChanges();
    }

    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, User updatedUser)
    {
        var user = _dbContext.Users.Find(id);
        if (user == null)
            return NotFound();
        
        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;
        user.Role = updatedUser.Role;

        _dbContext.SaveChanges();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        var user = _dbContext.Users.Find(id);

        if (user == null)
            return NotFound();
        if (user.Username == "admin")
        {
            return BadRequest("Kan inte radera admin");
        }
        
        _dbContext.Users.Remove(user);
        _dbContext.SaveChanges();
        return NoContent();
    }
}