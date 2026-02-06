using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using JobSearchBot.Models;
using JobSearchBot.Scrapers;
using JobSearchBot.Filters;

class Program
{
    private static readonly string BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? "";
    private static readonly long ChatId = long.TryParse(Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID"), out var id) ? id : 0;
    private static readonly HashSet<string> _sentJobLinks = new HashSet<string>();
    
    // All scrapers
    private static readonly List<IJobScraper> _scrapers = new List<IJobScraper>
    {
        new JobSearchScraper(),
        new BusyScraper(),
        new BossScraper(),
        new SmartJobScraper(),
        new AzJobScraper()
    };

    static async Task Main(string[] args)
    {
        if (string.IsNullOrEmpty(BotToken) || ChatId == 0)
        {
            Console.WriteLine("❌ Error: Environment variables not found!");
            Console.WriteLine("\n📝 Please set the following variables:");
            Console.WriteLine("  TELEGRAM_BOT_TOKEN = your bot token");
            Console.WriteLine("  TELEGRAM_CHAT_ID = your chat id");
            Console.WriteLine("\n💡 On Windows:");
            Console.WriteLine("  $env:TELEGRAM_BOT_TOKEN=\"your_token\"");
            Console.WriteLine("  $env:TELEGRAM_CHAT_ID=\"your_chat_id\"");
            return;
        }

        var botClient = new TelegramBotClient(BotToken);
        
        Console.WriteLine("🤖 .NET Job Bot started...");
        Console.WriteLine($"📱 Chat ID: {ChatId}");
        Console.WriteLine($"🌐 Monitoring {_scrapers.Count} job sites:");
        foreach (var scraper in _scrapers)
        {
            Console.WriteLine($"   • {scraper.SourceName}");
        }
        Console.WriteLine("⏰ Checking every 15 minutes.\n");

        try
        {
            await botClient.SendMessage(
                chatId: ChatId,
                text: "✅ *.NET Job Bot started!*\n\n" +
                      "🔍 Monitoring for .NET/C# jobs from:\n" +
                      string.Join("\n", _scrapers.Select(s => $"   • {s.SourceName}")),
                parseMode: ParseMode.Markdown
            );
            Console.WriteLine("✅ Test message sent - Telegram connection successful!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Telegram error: {ex.Message}");
            Console.WriteLine("\n⚠️ Please check:");
            Console.WriteLine("  1. Is the Bot Token correct?");
            Console.WriteLine("  2. Is the Chat ID correct?");
            Console.WriteLine("  3. Did you start the bot with /start in Telegram?");
            return;
        }

        await CheckAllSitesForJobs(botClient, isFirstRun: true);

        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(15));

            try
            {
                await CheckAllSitesForJobs(botClient, isFirstRun: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error occurred: {ex.Message}");
            }
        }
    }

    private static async Task CheckAllSitesForJobs(TelegramBotClient bot, bool isFirstRun)
    {
        Console.WriteLine($"\n[{DateTime.Now:HH:mm:ss}] ═══════════════════════════════════════");
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Checking all job sites...");

        int totalJobs = 0;
        int dotNetJobs = 0;
        int newJobsSent = 0;

        foreach (var scraper in _scrapers)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🔍 Fetching from {scraper.SourceName}...");
                
                var jobs = await scraper.GetJobsAsync();
                totalJobs += jobs.Count;
                
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]    Found {jobs.Count} jobs");

                foreach (var job in jobs)
                {
                    // Skip if already sent
                    if (_sentJobLinks.Contains(job.Link))
                        continue;

                    // Apply .NET filter
                    if (!JobFilter.IsDotNetJob(job.Title))
                        continue;

                    dotNetJobs++;

                    if (isFirstRun)
                    {
                        _sentJobLinks.Add(job.Link);
                        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]    📝 Cached: {job.Title}");
                        continue;
                    }

                    // Send to Telegram
                    string message = FormatJobMessage(job);
                    
                    await bot.SendMessage(
                        chatId: ChatId,
                        text: message,
                        parseMode: ParseMode.Markdown,
                        linkPreviewOptions: new Telegram.Bot.Types.LinkPreviewOptions { IsDisabled = true }
                    );

                    _sentJobLinks.Add(job.Link);
                    newJobsSent++;
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]    ✅ Sent: {job.Title}");

                    // Delay to avoid rate limiting
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ❌ Error with {scraper.SourceName}: {ex.Message}");
            }
        }

        // Summary
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ───────────────────────────────────────");
        if (isFirstRun)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🚀 Initial run complete:");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]    Total jobs scanned: {totalJobs}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]    .NET jobs cached: {dotNetJobs}");
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}]    Monitoring {_sentJobLinks.Count} total links");
        }
        else
        {
            if (newJobsSent > 0)
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] 🎉 {newJobsSent} new .NET job(s) sent!");
            else
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ℹ️ No new .NET jobs found.");
        }
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] ═══════════════════════════════════════\n");
    }

    private static string FormatJobMessage(JobListing job)
    {
        var lines = new List<string>
        {
            "📢 *Yeni İş Elanı!*",
            "",
            $"💼 *{EscapeMarkdown(job.Title)}*"
        };

        if (!string.IsNullOrEmpty(job.Company))
        {
            lines.Add($"🏢 {EscapeMarkdown(job.Company)}");
        }

        lines.Add($"📍 {job.Source}");
        lines.Add($"🔗 [Elana bax]({job.Link})");

        return string.Join("\n", lines);
    }

    private static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text
            .Replace("_", "\\_")
            .Replace("*", "\\*")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("`", "\\`");
    }
}