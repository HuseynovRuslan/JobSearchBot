using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class AzJobScraper : IJobScraper
{
    public string SourceName => "AzJob.az";
    private const string BaseUrl = "https://azjob.az";
    private const string VacanciesUrl = "https://azjob.az/vacancies";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(VacanciesUrl));

            // AzJob.az uses links ending with .html
            var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '.html')]");

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
                    
                    // Skip non-vacancy links
                    if (!link.Contains("-") || link.Contains("page") || link.Contains("category"))
                        continue;

                    if (!link.StartsWith("http"))
                        link = BaseUrl + "/" + link.TrimStart('/');

                    if (processedLinks.Contains(link)) continue;
                    processedLinks.Add(link);

                    string fullText = jobNode.InnerText.Trim();
                    fullText = Regex.Replace(fullText, @"\s+", " ").Trim();
                    
                    if (fullText.Length < 5) continue;

                    // AzJob titles often end with "tələb olunur" - clean it up
                    string title = Regex.Replace(fullText, @"\s*tələb olunur\s*$", "", RegexOptions.IgnoreCase).Trim();
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
