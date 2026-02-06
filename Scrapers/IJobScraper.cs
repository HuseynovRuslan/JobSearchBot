using JobSearchBot.Models;

namespace JobSearchBot.Scrapers;

public interface IJobScraper
{
    string SourceName { get; }
    Task<List<JobListing>> GetJobsAsync();
}
