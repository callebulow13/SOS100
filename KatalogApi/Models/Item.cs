namespace KatalogApi.Models;

public enum ItemType
{
    Elektronik,
    Bok,
    Rapport,
    Annat
}

public enum ItemStatus
{
    Tillgänglig,
    Utlånad,
    Saknas,
    Trasig
}
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public ItemStatus Status { get; set; }
    public string Placement { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    
}