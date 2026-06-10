using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using ExcursionTextbook.ViewModels;

namespace ExcursionTextbook.Views;

public partial class VideoView : UserControl
{
    private const string VirtualHost = "appassets.local";
    private bool _initStarted;
    private bool _ready;

    public VideoView()
    {
        InitializeComponent();
        IsVisibleChanged += OnIsVisibleChanged;
    }

    // Initialize WebView2 lazily on first show, and stop playback when the panel is hidden
    // so video audio doesn't keep running after switching to another tab.
    private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!_initStarted)
        {
            _initStarted = true;
            try
            {
                await VideoWebView.EnsureCoreWebView2Async();
                SetupPlayerPage();
                _ready = true;
            }
            catch
            {
                // WebView2 Runtime not available — leave the panel blank rather than crash.
                return;
            }
        }

        if (!_ready || VideoWebView.CoreWebView2 == null) return;

        if (IsVisible)
            VideoWebView.CoreWebView2.Navigate($"https://{VirtualHost}/player.html");
        else
            VideoWebView.CoreWebView2.Navigate("about:blank");
    }

    // YouTube returns "Error 153" when the embed is loaded as a top-level document with no
    // valid origin/referrer. Hosting the iframe on a virtual https host gives the page a real
    // origin that the player accepts.
    private void SetupPlayerPage()
    {
        var embedUrl = (DataContext as MainViewModel)?.VideoEmbedUrl ?? string.Empty;

        var folder = Path.Combine(Path.GetTempPath(), "ExcursionTextbookPlayer");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "player.html"), BuildPlayerHtml(embedUrl));

        VideoWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            VirtualHost, folder, CoreWebView2HostResourceAccessKind.Allow);
    }

    private static string BuildPlayerHtml(string embedUrl) => $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<style>
  html, body {{ margin:0; padding:0; height:100%; background:#000; overflow:hidden; }}
  iframe {{ position:absolute; inset:0; width:100%; height:100%; border:0; }}
</style>
</head>
<body>
  <iframe src='{embedUrl}'
          title='YouTube video player'
          allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share'
          allowfullscreen></iframe>
</body>
</html>";
}
