using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class BusyScraper : IJobScraper
{
    public string SourceName => "Busy.az";
    private const string BaseUrl = "https://busy.az";
    private const string VacanciesUrl = "https://busy.az/vacancies";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(VacanciesUrl));

            // Busy.az uses vacancy cards with links
            var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/vacancy/')]");

            if (jobNodes == null)
            {
                Console.WriteLine($"⚠️ [{SourceName}] No jobs found.");
                return jobs;
            }

            var processedLinks = new HashSet<string>();

            foreach (var jobNode in jobNodes)
            {
                try
                {
                    string link = jobNode.Attributes["href"]?.Value ?? "";
                    if (string.IsNullOrEmpty(link)) continue;

                    if (!link.StartsWith("http"))
                        link = BaseUrl + link;

                    // Avoid duplicates
                    if (processedLinks.Contains(link)) continue;
                    processedLinks.Add(link);

                    string fullText = jobNode.InnerText.Trim();
                    fullText = Regex.Replace(fullText, @"\s+", " ").Trim();
                    
                    // Skip if too short or looks like navigation
                    if (fullText.Length < 5 || fullText.ToLower().Contains("ətraflı")) continue;

                    // Try to extract company name (usually after the title)
                    string title = fullText;
                    string company = "";
                    
                    // Remove date patterns
                    title = Regex.Replace(title, @"\b(bugün|dünən|Bu gün|Dünən|\d+\s*gün\s*əvvəl|Premium|Bakı)\b", "", RegexOptions.IgnoreCase).Trim();
                    title = Regex.Replace(title, @"\s+", " ").Trim();

                    jobs.Add(new JobListing
                    {
                        Title = title,
                        Company = company,
                        Link = link,
                        Source = SourceName
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ [{SourceName}] Error parsing job: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [{SourceName}] Error fetching jobs: {ex.Message}");
        }

        return jobs;
    }
}
