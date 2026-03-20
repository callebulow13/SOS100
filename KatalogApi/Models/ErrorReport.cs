namespace KatalogApi.Models;

public class ErrorReport
{
    public int Id { get; set; }
    
    // Kopplingen till prylen som är trasig
    public int ItemId { get; set; } 
    
    public DateTime ReportDate { get; set; }
    
    // Vem som rapporterade felet (t.ex. medarbetarens namn)
    public string ReporterName { get; set; } = string.Empty;
    
    // Vad som är fel
    public string Description { get; set; } = string.Empty;
    
    // En smart flagga för IT-teamet: Är felet åtgärdat?
    public bool IsResolved { get; set; } = false; 
    
    // Navigeringsegenskap för Entity Framework
    public Item? Item { get; set; }
}