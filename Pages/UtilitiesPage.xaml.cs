using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SupaTweaker.Pages;

public class UtilApp : INotifyPropertyChanged
{
    public string Title { get; init; } = "";
    public string Hint { get; init; } = "";
    public string Url { get; init; } = "";
    public string FileName { get; init; } = "";
    public bool Selected { get; set; }

    private string _status = "";
    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public partial class UtilitiesPage : Page
{
    private static readonly HttpClient Http = CreateClient();
    private bool _busy;

    public UtilitiesPage()
    {
        InitializeComponent();
        Apps.ItemsSource = new List<UtilApp>
        {
            new() { Title = "Steam", Hint = "Клиент Valve", FileName = "SteamSetup.exe",
                Url = "https://cdn.cloudflare.steamstatic.com/client/installer/SteamSetup.exe" },
            new() { Title = "Discord", Hint = "Голосовой чат", FileName = "DiscordSetup.exe",
                Url = "https://discord.com/api/downloads/distributions/app/installers/latest?channel=stable&platform=win&arch=x64" },
            new() { Title = "NVIDIA App", Hint = "Панель NVIDIA (официальный установщик)", FileName = "NVIDIA_App.exe",
                Url = "https://us.download.nvidia.com/nvapp/client/11.0.8.299/NVIDIA_app_v11.0.8.299.exe" },
            new() { Title = "AMD Software", Hint = "Adrenalin / автоустановка драйвера", FileName = "AMD_Adrenalin.exe",
                Url = "https://drivers.amd.com/drivers/installer/25.10/whql/amd-software-adrenalin-edition-25.10.1-minimalsetup-250901_web.exe" },
            new() { Title = "Google Chrome", Hint = "Браузер", FileName = "ChromeSetup.exe",
                Url = "https://dl.google.com/chrome/install/latest/chrome_installer.exe" },
            new() { Title = "Obhod Launcher", Hint = "архив исходников с GitHub", FileName = "ObhodLauncher-master.zip",
                Url = "https://github.com/supamodd/ObhodLauncher/archive/refs/heads/master.zip" }
        };
    }

    private static HttpClient CreateClient()
    {
        var h = new HttpClientHandler { AllowAutoRedirect = true };
        var c = new HttpClient(h) { Timeout = TimeSpan.FromMinutes(30) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) SupaTweaker");
        c.DefaultRequestHeaders.TryAddWithoutValidation("Referer", "https://www.nvidia.com/");
        return c;
    }

    private async void DownloadClick(object s, RoutedEventArgs e)
    {
        if (_busy) return;
        var list = Apps.Items.Cast<UtilApp>().Where(x => x.Selected).ToList();
        if (list.Count == 0)
        {
            StatusLine.Text = "Ничего не выбрано";
            return;
        }

        _busy = true;
        var desk = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var ok = 0;
        foreach (var app in list)
        {
            app.Status = "скачивание…";
            StatusLine.Text = $"Скачиваю {app.Title}…";
            try
            {
                var dest = Path.Combine(desk, app.FileName);
                await DownloadTo(app, dest);
                app.Status = "готово";
                ok++;
            }
            catch (Exception ex)
            {
                app.Status = "ошибка";
                StatusLine.Text = $"{app.Title}: {ex.Message}";
            }
        }
        StatusLine.Text = $"Готово: {ok} из {list.Count}. Файлы на рабочем столе.";
        MainWindow.Instance?.SetStatus(StatusLine.Text);
        _busy = false;
    }

    private static async Task DownloadTo(UtilApp app, string dest)
    {
        using var resp = await Http.GetAsync(app.Url, HttpCompletionOption.ResponseHeadersRead);
        resp.EnsureSuccessStatusCode();
        var total = resp.Content.Headers.ContentLength ?? 0;
        await using var input = await resp.Content.ReadAsStreamAsync();
        await using var output = File.Create(dest);
        var buf = new byte[81920];
        long got = 0;
        int n;
        while ((n = await input.ReadAsync(buf)) > 0)
        {
            await output.WriteAsync(buf.AsMemory(0, n));
            got += n;
            if (total > 0)
                app.Status = $"{got * 100 / total}%";
            else
                app.Status = $"{got / 1024 / 1024} МБ";
        }
    }

    private void OpenDesktop(object s, RoutedEventArgs e)
    {
        var desk = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        Process.Start(new ProcessStartInfo("explorer.exe", desk) { UseShellExecute = true });
    }
}
