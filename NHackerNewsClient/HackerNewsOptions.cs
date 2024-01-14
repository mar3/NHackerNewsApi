using System.ComponentModel.DataAnnotations;

namespace NHackerNewsClient;

public class HackerNewsOptions
{
    public const string SectionName = "HackerNews";
    
    [Required, Url] public required string BaseUrl { get; set; }
    [Required] public int CacheDurationMilliseconds { get; set; } 
}