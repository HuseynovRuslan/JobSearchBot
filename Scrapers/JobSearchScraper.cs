using HtmlAgilityPack;
using JobSearchBot.Models;
using System.Text.RegularExpressions;

namespace JobSearchBot.Scrapers;

public class JobSearchScraper : IJobScraper
{
    public string SourceName => "JobSearch.az";
    private const string BaseUrl = "https://jobsearch.az";
    private const string VacanciesUrl = "https://jobsearch.az/vacancies";

    public async Task<List<JobListing>> GetJobsAsync()
    {
        var jobs = new List<JobListing>();
        
        try
        {
            var web = new HtmlWeb();
            var doc = await Task.Run(() => web.Load(VacanciesUrl));

            var jobNodes = doc.DocumentNode.SelectNodes("//a[contains(@class, 'list__item')]");

            if (jobNodes == null)
            {
                Console.WriteLine($"⚠️ [{SourceName}] No jobs found.");
                return jobs;
            }

            foreach (var jobNode in jobNodes)
            {
                try
                {
                    string link = jobNode.Attributes["href"]?.Value ?? "";
                    if (string.IsNullOrEmpty(link)) continue;

                    if (!link.StartsWith("http"))
                        link = BaseUrl + link;

                    string fullText = jobNode.InnerText.Trim();
                    fullText = Regex.Replace(fullText, @"\s+", " ").Trim();
                    
                    // Remove date and salary info
                    var cleanedText = Regex.Replace(fullText, @"\b(Bu gün|Dünən|\d+[.,]?\d*K?)\b", "", RegexOptions.IgnoreCase).Trim();
                    cleanedText = Regex.Replace(cleanedText, @"\s+", " ").Trim();

                    jobs.Add(new JobListing
                    {
                        Title = cleanedText,
                        Company = "", // JobSearch doesn't show company in list
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
