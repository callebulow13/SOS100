namespace SOS100_MVC.Models;
using SOS100_MVC.Dtos;

public class EditProfileViewModel
{
    public User? User { get; set; }
    public PasswordDto? PasswordDto { get; set; }
}