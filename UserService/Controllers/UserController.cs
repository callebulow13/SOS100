using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
                u.Username == login.Username);
        if (user == null)
            return Unauthorized();
        
        var passwordHasher = new PasswordHasher<User>();
        
        var result = passwordHasher.VerifyHashedPassword(
            user, 
            user.Password,
            login.Password
            );
        
        if (result == PasswordVerificationResult.Failed)
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
    public async Task<IActionResult>AddUser(User user)
    {
        var passwordHasher = new PasswordHasher<User>();
        user.Password = passwordHasher.HashPassword(user, user.Password);
        
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok();
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
    [HttpPut("profile/{id}")]
    public IActionResult UpdateProfile(int id, User updatedUser)
    {
        var user = _dbContext.Users.Find(id);
        if (user == null)
            return NotFound();
        
        user.Username = updatedUser.Username;
        user.Email = updatedUser.Email;
        user.FirstName = updatedUser.FirstName;
        user.LastName = updatedUser.LastName;

        _dbContext.SaveChanges();
        
        return NoContent();
    }
    
    [HttpPut("changePassword/{id}")]
    public IActionResult UpdatePassword(int id, PasswordDto passwordDto)
    {
        User? user;
        try
        {
            user = _dbContext.Users.Find(id);

            if (user == null)
            {
                return NotFound();
            }
            var passwordHasher = new PasswordHasher<User>();
        
            var result = passwordHasher.VerifyHashedPassword(
                user, 
                user.Password,
                passwordDto.Password
            );
            Console.WriteLine($"INPUT: '{passwordDto.Password}'");
            Console.WriteLine($"HASH: '{user.Password}'");
            if (result == PasswordVerificationResult.Success)
            {
                user.Password = passwordHasher.HashPassword(user, passwordDto.NewPassword);
                _dbContext.SaveChanges();
                
            }
            else
            {
                return BadRequest("Fel nuvarande lösenord");
            }
        }
        catch
        {
            return Content("Kan inte nå UserService");
        }
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