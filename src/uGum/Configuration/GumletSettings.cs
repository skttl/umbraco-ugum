namespace uGum.Configuration;

public class GumletSettings
{
    public string ApiBasePath { get; set; } = "https://api.gumlet.com";
    public string? ApiKey { get; set; }
    public string? SourceId { get; set; }
    public string Format { get; set; } = "hls";
    public string[] Resolution { get; set; } = ["240p", "360p", "480p", "720p", "1080p"];
    public bool KeepOriginal { get; set; } = false;
}
