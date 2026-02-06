namespace JobSearchBot.Filters;

public static class JobFilter
{
    // ✅ QƏBUL OLUNAN açar sözlər (spesifik .NET/C# və digər vəzifələr)
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
        
        // Azərbaycan dilində (proqramlaşdırma)
        "proqramçı", "bəkend", "proqram mühəndisi",
        "backend mühəndis",
        
        // Yeni əlavə olunan vəzifələr
        "motokuryer", "moto kuryer", "moped kuryer", "kurier", "kuryer",
        "anbardar", "anbar işçisi", "warehouse", "anbar meneceri",
        "ofis meneceri", "ofis menecer", "office manager", "ofis müdiri",
        "stomatoloq assistenti", "diş həkimi köməkçisi", "dental assistant", "stomatoloji",
        "operator", "məlumat daxil edən", "data entry", "daxiletmə operatoru", "call center operator",
        "it texniki dəstək", "helpdesk", "help desk", "texniki dəstək", "it support", "technical support",
        "kitab satıcısı", "kitab mağazası", "bookseller", "book seller", "satıcı"
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
