using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class DjinniScraper : IJobScraper
{
    public string SourceName => "Djinni.co";
    private const string BaseUrl = "https://djinni.co";
    private const string JobsUrl = "https://djinni.co/jobs/";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(JobsUrl));

            // Djinni uses links in format /jobs/{id}-{slug}/
            var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@href, '/jobs/') and contains(@href, '-')]");

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
                    
                    // Skip non-job links (company pages, etc.)
                    if (link.Contains("company-") || !Regex.IsMatch(link, @"/jobs/\d+-"))
                        continue;

                    if (!link.StartsWith("http"))
                        link = BaseUrl + link;

                    if (processedLinks.Contains(link)) continue;
                    processedLinks.Add(link);

                    string title = jobNode.InnerText.Trim();
                    title = Regex.Replace(title, @"\s+", " ").Trim();
                    
                    if (title.Length < 3) continue;

                    // Try to get company from nearby elements
                    string company = "";
                    var parentItem = jobNode.Ancestors().FirstOrDefault(a => 
                        a.GetAttributeValue("class", "").Contains("list-jobs__item") ||
                        a.Name == "li");
                    if (parentItem != null)
                    {
                        var companyNode = parentItem.SelectSingleNode(".//a[contains(@href, 'company-')]");
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
