using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class GlorriScraper : IJobScraper
{
    public string SourceName => "Jobs.Glorri.az";
    private const string BaseUrl = "https://jobs.glorri.az";
    private const string VacanciesUrl = "https://jobs.glorri.az/vacancies";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(VacanciesUrl));

            // Glorri uses links containing /vacancies/
            var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/vacancies/') and contains(@href, '?isLocal=true')]");

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
                    
                    // Skip company pages and non-job links
                    if (link.Contains("/companies/") || !link.Contains("/vacancies/"))
                        continue;

                    if (!link.StartsWith("http"))
                        link = BaseUrl + link;

                    if (processedLinks.Contains(link)) continue;
                    processedLinks.Add(link);

                    // Get the job title from the link or inner text
                    string title = jobNode.InnerText.Trim();
                    title = Regex.Replace(title, @"\s+", " ").Trim();
                    
                    if (title.Length < 3) continue;

                    // Try to get company from nearby elements
                    string company = "";
                    var parentCard = jobNode.Ancestors().FirstOrDefault(a => a.HasClass("card") || a.HasClass("job-item"));
                    if (parentCard != null)
                    {
                        var companyNode = parentCard.SelectSingleNode(".//*[contains(@class, 'company')]");
                        if (companyNode != null)
                        {
                            company = companyNode.InnerText.Trim();
                        }
                    }

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
