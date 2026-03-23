namespace SOS100_LoanApi.Dtos;

public class ItemDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    // De saknade fälten för att inte raderas vid PUT!
    public int Type { get; set; } 
    public string Description { get; set; }
    public int Status { get; set; }
    public string Placement { get; set; }
    public DateTime PurchaseDate { get; set; }
}