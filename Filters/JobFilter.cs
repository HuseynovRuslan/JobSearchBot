namespace JobSearchBot.Filters;

public static class JobFilter
{
    // ✅ QƏBUL OLUNAN açar sözlər (spesifik .NET/C#)
    private static readonly string[] IncludeKeywords = {
        // .NET spesifik
        ".net", "dotnet", "asp.net", "c#", "csharp",
        ".net core", "asp.net core", "web api", "webapi",
        "mvc", "blazor", "entity framework", "ef core",
        "signalr", "maui", "wpf", "winforms",
        
        // Rol adları (.NET ilə bağlı)
        ".net developer", "c# developer", ".net engineer",
        "backend developer", "full stack developer", "fullstack developer",
        "software developer", "software engineer",
        
        // ERP (yalnız texniki/developer)
        "erp developer", "erp programmer", "erp specialist",
        
        // Azərbaycan dilində
        "proqramçı", "bəkend", "proqram mühəndisi",
        "backend mühəndis"
    };
    
    // ❌ XARİC OLUNAN açar sözlər (qeyri-proqramlaşdırma mühəndisləri)
    private static readonly string[] ExcludeKeywords = {
        // Keyfiyyət/Nəzarət
        "keyfiyyət", "keyfiyyətə nəzarət", "quality control",
        "qa/qc", "qc inspector", "quality inspector",
        
        // Texniki (proqramlaşdırma olmayan)
        "texniki nəzarət", "texniki servis", "servis mühəndis",
        "texniki ofis mühəndis",
        
        // Mühəndislik sahələri (proqramlaşdırma olmayan)
        "korroziya", "mexanika mühəndis", "proses mühəndis",
        "elektrik mühəndis", "layihə mühəndis", "tikinti mühəndis",
        "inşaat mühəndis", "topoqraf", "geodezist",
        
        // Sənaye
        "neft", "qaz", "oil", "gas", "drilling",
        "pipeline", "boru kəməri",
        
        // Digər
        "həkim", "tibb", "nurse", "əczaçı"
    };
    
    public static bool IsDotNetJob(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return false;
            
        var lower = title.ToLower();
        
        // Əvvəlcə xaric olunanları yoxla
        if (ExcludeKeywords.Any(k => lower.Contains(k)))
            return false;
            
        // Sonra daxil olunanları yoxla
        return IncludeKeywords.Any(k => lower.Contains(k));
    }
}
