using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class BossScraper : IJobScraper
{
    public string SourceName => "Boss.az";
    private const string BaseUrl = "https://boss.az";
    private const string VacanciesUrl = "https://boss.az/vacancies";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(VacanciesUrl));

            // Boss.az uses vacancy items
            var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/vacancies/')]");

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
                    
                    // Skip category links
                    if (link.Contains("?") || link.EndsWith("/vacancies") || link.EndsWith("/vacancies/"))
                        continue;

                    if (!link.StartsWith("http"))
                        link = BaseUrl + link;

                    if (processedLinks.Contains(link)) continue;
                    processedLinks.Add(link);

                    string fullText = jobNode.InnerText.Trim();
                    fullText = Regex.Replace(fullText, @"\s+", " ").Trim();
                    
                    if (fullText.Length < 5) continue;

                    // Clean up title
                    string title = Regex.Replace(fullText, @"\b(bugün|dünən|Bu gün|Dünən|Premium|Bakı|Sumqayıt)\b", "", RegexOptions.IgnoreCase).Trim();
                    title = Regex.Replace(title, @"\s+", " ").Trim();

                    jobs.Add(new JobListing
                    {
                        Title = title,
                        Company = "",
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
