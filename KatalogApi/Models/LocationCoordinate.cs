namespace KatalogApi.Models;

public class LocationCoordinate
{
    public int Id { get; set; }
    
    // Denna kolumn är den logiska "kopplingen" till din Item-tabell (fältet Placement)
    public string LocationName { get; set; } = string.Empty; 
    
    public int X { get; set; }
    public int Y { get; set; }
}