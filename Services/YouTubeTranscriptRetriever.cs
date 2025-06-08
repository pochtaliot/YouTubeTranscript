namespace YouTubeTranscript.Services;

public class YouTubeTranscriptRetriever
{
    private readonly YoutubeExplode.YoutubeClient _youtubeClient;

    public YouTubeTranscriptRetriever(YoutubeExplode.YoutubeClient? youtubeClient = null)
        => _youtubeClient = youtubeClient ?? new YoutubeExplode.YoutubeClient();

    public async Task<(string Transcript, string Error)> GetTranscriptAsync(string videoId, string languageCode = "en")
    {
        try
        {
            var trackManifest = await _youtubeClient.Videos.ClosedCaptions.GetManifestAsync(videoId);
            var trackInfo = trackManifest.GetByLanguage(languageCode) ?? trackManifest.TryGetByLanguage("en");

            if (trackInfo == null)
                return (string.Empty, "No transcript available for the specified language");

            var track = await _youtubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
            var transcript = string.Join(Environment.NewLine, track.Captions.Select(c => c.Text));
            return (transcript, string.Empty);
        }
        catch (Exception ex)
        {
            return (string.Empty, $"Failed to retrieve transcript: {ex.Message}");
        }
    }
}