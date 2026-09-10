using System.Text.Json.Serialization;

namespace uGum.Services;

public interface IGumletService
{
    Task<GumletAsset?> GetAsset(string assetId, CancellationToken cancellationToken = default);
    Task<GumletAsset> CreateAsset(byte[] bytes, string? title = null, string? creatorId = null, string? externalId = null, CancellationToken cancellationToken = default);
    Task DeleteAsset(string assetId, CancellationToken cancellationToken = default);
}

public sealed class GumletAsset
{
    [JsonPropertyName("asset_id")]
    public string? AssetId { get; set; }
    public string? Status { get; set; }
    public GumletOutput? Output { get; set; }
    [JsonPropertyName("upload_url")]
    public string? UploadUrl { get; set; }
}

public sealed class GumletOutput
{
    [JsonPropertyName("status_url")] public string? StatusUrl { get; set; }
    [JsonPropertyName("playback_url")] public string? PlaybackUrl { get; set; }
}
