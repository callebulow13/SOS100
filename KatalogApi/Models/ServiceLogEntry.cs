namespace KatalogApi.Models;

public class ServiceLogEntry
{
    public int Id { get; set; }
    
    // Främmande nyckel (Foreign Key) som kopplar loggen till en specifik pryl
    public int ItemId { get; set; } 
    
    public DateTime ServiceDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    
    // Navigeringsegenskap - hjälper Entity Framework att förstå relationen
    public Item? Item { get; set; }
}