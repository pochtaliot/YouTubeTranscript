using System.Text.RegularExpressions;

namespace YouTubeTranscript.Services;

public class AppRunner
{
    public async Task Run(string[] args)
    {
        string videoId = string.Empty;
        string languageCode = "en";
        foreach (var arg in args)
        {
            if (arg.StartsWith("--videoId="))
                videoId = arg[10..];
            else if (arg.StartsWith("--lang="))
                languageCode = arg[7..];
        }

        if (string.IsNullOrEmpty(videoId))
        {
            Console.WriteLine("Please provide a YouTube video ID using --videoId=<id>.");
            return;
        }

        var youTubeTranscriptRetriever = new YouTubeTranscriptRetriever(new YoutubeExplode.YoutubeClient());
        var result = await youTubeTranscriptRetriever.GetTranscriptAsync(videoId, languageCode);

        if (!string.IsNullOrEmpty(result.Error))
        {
            Console.WriteLine($"Error: {result.Error}");
            return;
        }

        var cleanedTranscript = Regex.Replace(result.Transcript, @"\s+", " ");
        Console.WriteLine(cleanedTranscript);
    }
}