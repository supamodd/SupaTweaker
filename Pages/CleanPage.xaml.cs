using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class CleanPage : Page
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };

    public CleanPage()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            RefreshRam();
            _timer.Tick += (_, _) => RefreshRam();
            _timer.Start();
        };
        Unloaded += (_, _) => _timer.Stop();
    }

    private void RefreshRam()
    {
        var r = MemoryCleaner.Read();
        var used = r.Total > 0 ? 1.0 - (double)r.Avail / r.Total : 0;
        RamLabel.Text = $"Занято {MemoryCleaner.Format(r.Total - r.Avail)} из {MemoryCleaner.Format(r.Total)}  ·  свободно {MemoryCleaner.Format(r.Avail)}";
        var max = Math.Max(1, ((Border)RamBar.Parent).ActualWidth);
        if (max < 2) max = 400;
        RamBar.Width = Math.Max(8, used * max);
    }

    private void CleanRam(object s, RoutedEventArgs e)
    {
        var before = MemoryCleaner.Read();
        MemoryCleaner.Clean();
        RefreshRam();
        var after = MemoryCleaner.Read();
        var freed = (long)after.Avail - (long)before.Avail;
        var mb = Math.Max(0, freed) / 1024d / 1024d;
        MainWindow.Instance?.SetStatus($"ОЗУ: освобождено примерно {mb:0} МБ");
    }

    private void Go(object s, RoutedEventArgs e)
    {
        long bytes = 0;
        if (Temp.IsChecked == true)
            bytes += Wipe(Path.GetTempPath());
        if (Prefetch.IsChecked == true)
            bytes += Wipe(@"C:\Windows\Prefetch");
        if (Recycle.IsChecked == true)
            WinUtil.Run("powershell.exe", "-NoProfile -Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"", true);
        MainWindow.Instance?.SetStatus($"Очистка файлов, примерно {bytes / 1024 / 1024} МБ");
    }

    private static long Wipe(string dir)
    {
        long n = 0;
        if (!Directory.Exists(dir)) return 0;
        foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
        {
            try
            {
                var fi = new FileInfo(f);
                n += fi.Length;
                fi.Delete();
            }
            catch { }
        }
        return n;
    }
}
