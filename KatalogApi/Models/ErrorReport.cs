namespace KatalogApi.Models;

public class ErrorReport
{
    public int Id { get; set; }
    
    public int ItemId { get; set; } 
    
    public DateTime ReportDate { get; set; }
    
    public string ReporterName { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public bool IsResolved { get; set; } = false; 
    
    public Item? Item { get; set; }
}