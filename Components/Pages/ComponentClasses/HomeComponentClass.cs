using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;
using YouTubeTranscript.Services;

namespace YouTubeTranscript.Components.Pages.ComponentClasses;

public class HomeComponentClass : ComponentBase
{
    [Inject]
    protected IJSRuntime? JS { get; set; }
    protected string youtubeUrl = string.Empty;
    protected string transcript = string.Empty;
    protected string error = string.Empty;
    protected bool isLoading = false;

    protected async Task GetTranscriptAsync()
    {
        transcript = string.Empty;
        error = string.Empty;
        isLoading = true;
        
        try
        {
            var videoId = ExtractVideoId(youtubeUrl);
            if (string.IsNullOrEmpty(videoId))
            {
                error = "Invalid YouTube URL.";
                return;
            }
            var retriever = new YouTubeTranscriptRetriever();
            var result = await retriever.GetTranscriptAsync(videoId);
            if (!string.IsNullOrEmpty(result.Error))
                error = result.Error;
            else
                transcript = Regex.Replace(result.Transcript, @"\s+", " ");
        }
        catch (Exception ex)
        {
            error = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    protected async Task CopyTranscriptToClipboard()
    {
        if (!string.IsNullOrEmpty(transcript) && JS != null)
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", transcript);
        }
    }

    protected string ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        try
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (query.AllKeys.Contains("v"))
                return query["v"] ?? string.Empty;
            // Handle youtu.be short links
            if (uri.Host.Contains("youtu.be"))
                return uri.AbsolutePath.Trim('/');
            // Handle /embed/ links
            if (uri.AbsolutePath.Contains("/embed/"))
                return uri.Segments.Last();
        }
        catch { }
        return string.Empty;
    }

    protected async Task OnInputKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !isLoading)
            await GetTranscriptAsync();
    }
    protected void OnInputChanged(ChangeEventArgs e) =>
        youtubeUrl = e.Value?.ToString() ?? string.Empty;
}
