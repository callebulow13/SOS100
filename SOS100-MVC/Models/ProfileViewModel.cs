using SOS100_MVC.Dtos; // <-- Lägg till denna rad!

namespace SOS100_MVC.Models;

// "Korgen" som vi skickar till vyn
public class ProfileViewModel
{
    public User User { get; set; } = new User();
    
    // Nu vet den att den ska hämta LoanDto från SOS100_MVC.Dtos!
    public List<LoanDto> ActiveLoans { get; set; } = new List<LoanDto>(); 
}