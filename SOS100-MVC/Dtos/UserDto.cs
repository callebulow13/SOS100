namespace SOS100_MVC.Dtos;

public class UserDto
{
    //Primary key
    public int? UserID { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
}