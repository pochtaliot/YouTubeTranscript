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
    protected bool isLoading;
    protected bool showLanguageDropdown;
    protected string selectedLanguage = "en";
    protected string languageFilter = string.Empty;
    protected ElementReference languageSearchInputRef;
    private bool prevShowLanguageDropdown;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (showLanguageDropdown && !prevShowLanguageDropdown)
        {
            prevShowLanguageDropdown = true;
            if (JS is not null)
                await JS.InvokeVoidAsync("ytFocusElement", languageSearchInputRef);
        }
        else if (!showLanguageDropdown && prevShowLanguageDropdown)
        {
            prevShowLanguageDropdown = false;
        }
    }

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
            var result = await retriever.GetTranscriptAsync(videoId, selectedLanguage);
            if (!string.IsNullOrEmpty(result.Error))
                error = result.Error;
            else
                transcript = Regex.Replace(result.Transcript, "\\s+", " ");
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
        if (!string.IsNullOrEmpty(transcript) && JS is not null)
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", transcript);
    }

    protected static string ExtractVideoId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;
        try
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (query.AllKeys.Contains("v"))
                return query["v"] ?? string.Empty;
            if (uri.Host.Contains("youtu.be"))
                return uri.AbsolutePath.Trim('/');
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

    protected List<KeyValuePair<string, string>> FilteredLanguages =>
        string.IsNullOrWhiteSpace(languageFilter)
            ? LanguageList.Languages.ToList()
            : LanguageList.Languages.Where(l => l.Value.Contains(languageFilter, StringComparison.OrdinalIgnoreCase) || l.Key.Contains(languageFilter, StringComparison.OrdinalIgnoreCase)).ToList();

    protected void ToggleLanguageDropdown() =>
        showLanguageDropdown = !showLanguageDropdown;

    protected void OnLanguageFilterChanged(ChangeEventArgs e) =>
        languageFilter = e.Value?.ToString() ?? string.Empty;

    protected void SelectLanguage(string langCode)
    {
        selectedLanguage = langCode;
        showLanguageDropdown = false;
    }
}
