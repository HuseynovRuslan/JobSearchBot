using HtmlAgilityPack;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Text.RegularExpressions;

class Program
{
    private static readonly string BotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? "";
    private static readonly long ChatId = long.TryParse(Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID"), out var id) ? id : 0;
    private static readonly HashSet<string> _sentJobLinks = new HashSet<string>();

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
        Console.WriteLine("🤖 Bot started... Monitoring JobSearch.az");
        Console.WriteLine($"📱 Chat ID: {ChatId}");
        Console.WriteLine("⏰ Checking every 15 minutes.\n");

        try
        {
            await botClient.SendMessage(
                chatId: ChatId,
                text: "✅ *JobSearch Bot started!*\n\nMonitoring new programming job listings...",
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

        await CheckForJobs(botClient, isFirstRun: true);

        while (true)
        {
            await Task.Delay(TimeSpan.FromMinutes(15));

            try
            {
                await CheckForJobs(botClient, isFirstRun: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error occurred: {ex.Message}");
            }
        }
    }

    private static async Task CheckForJobs(TelegramBotClient bot, bool isFirstRun)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Checking for jobs...");

        string url = "https://jobsearch.az/vacancies";
        var web = new HtmlWeb();
        var doc = await Task.Run(() => web.Load(url));

        var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'list__item')]");

        if (jobNodes == null)
        {
            Console.WriteLine("⚠️ No jobs found. Site structure may have changed.");
            return;
        }

        Console.WriteLine($"📋 Found {jobNodes.Count} jobs.");

        int newJobsCount = 0;
        int progJobsFound = 0;

        foreach (var jobNode in jobNodes)
        {
            try
            {
                string link = jobNode.Attributes["href"]?.Value ?? "";
                if (string.IsNullOrEmpty(link)) continue;

                if (!link.StartsWith("http"))
                    link = "https://jobsearch.az" + link;

                if (_sentJobLinks.Contains(link))
                    continue;

                string fullText = jobNode.InnerText.Trim();
                fullText = Regex.Replace(fullText, @"\s+", " ").Trim();
                
                string title = fullText;
                string company = "";
                
                var cleanedText = Regex.Replace(fullText, @"\b(Bu gün|Dünən|\d+[.,]?\d*K?)\b", "", RegexOptions.IgnoreCase).Trim();
                cleanedText = Regex.Replace(cleanedText, @"\s+", " ").Trim();
                
                if (!string.IsNullOrEmpty(cleanedText))
                {
                    title = cleanedText;
                }

                if (!IsProgrammingJob(title))
                    continue;

                progJobsFound++;

                if (isFirstRun)
                {
                    _sentJobLinks.Add(link);
                    Console.WriteLine($"📝 Cached: {title}");
                    continue;
                }

                string message = $"📢 *New Job Listing!*\n\n" +
                                 $"💼 *{EscapeMarkdown(title)}*\n" +
                                 $"🔗 [View Job]({link})";

                await bot.SendMessage(
                    chatId: ChatId,
                    text: message,
                    parseMode: ParseMode.Markdown
                );

                _sentJobLinks.Add(link);
                newJobsCount++;
                Console.WriteLine($"✅ Sent: {title}");

                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error processing job: {ex.Message}");
            }
        }

        if (!isFirstRun)
        {
            if (newJobsCount > 0)
                Console.WriteLine($"🎉 {newJobsCount} new programming job(s) sent!");
            else
                Console.WriteLine("ℹ️ No new programming jobs found.");
        }
        else
        {
            Console.WriteLine($"\n🚀 Initial run - {progJobsFound} programming job(s) cached.");
            Console.WriteLine($"📊 Monitoring {_sentJobLinks.Count} total jobs.\n");
        }
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

    private static bool IsProgrammingJob(string title)
    {
        if (string.IsNullOrEmpty(title)) return false;

        string[] keywords = {
            ".net developer", "backend developer", "software developer", "software engineer",
            "c# developer", ".net engineer",
            "c#", ".net", "asp.net", "asp.net core", "web api",
            "entity framework", "ef core", "dapper", "linq", "signalr",
            "sql", "mssql", "sql server", "postgresql", "t-sql",
            "clean architecture", "onion architecture", "cqrs", "solid", "oop", "rest api",
            "docker", "nginx", "git",
            "proqramçı", "developer", "mühəndis", "bəkend", ".net mütəxəssisi"
        };

        string lowerTitle = title.ToLower();
        return keywords.Any(k => lowerTitle.Contains(k));
    }
}