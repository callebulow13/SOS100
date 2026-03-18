using System.Text.Json.Serialization;

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
    
    // 2. Använd JsonIgnore så dina kamrater inte får med denna i sina anrop
    [JsonIgnore] 
    public List<ServiceLogEntry> ServiceLogs { get; set; } = new List<ServiceLogEntry>();
}

