using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class SmartJobScraper : IJobScraper
{
    public string SourceName => "SmartJob.az";
    private const string BaseUrl = "https://smartjob.az";
    private const string VacanciesUrl = "https://smartjob.az/vacancies";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(VacanciesUrl));

            // SmartJob.az uses vacancy cards with specific structure
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

                    if (processedLinks.Contains(link)) continue;
                    processedLinks.Add(link);

                    string fullText = jobNode.InnerText.Trim();
                    fullText = Regex.Replace(fullText, @"\s+", " ").Trim();
                    
                    if (fullText.Length < 5) continue;

                    // Try to extract company from parent/sibling elements
                    string company = "";
                    var companyNode = jobNode.ParentNode?.SelectSingleNode(".//a[contains(@href, '/company/')]");
                    if (companyNode != null)
                    {
                        company = companyNode.InnerText.Trim();
                    }

                    // Clean up title
                    string title = Regex.Replace(fullText, @"\b(Tam iş günü|Yarım ştat|Bakı)\b", "", RegexOptions.IgnoreCase).Trim();
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
