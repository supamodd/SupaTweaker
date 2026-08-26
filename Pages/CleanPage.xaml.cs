using System.IO;
using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class CleanPage : Page
{
    public CleanPage() => InitializeComponent();

    private void Go(object s, RoutedEventArgs e)
    {
        long bytes = 0;
        if (Temp.IsChecked == true)
            bytes += Wipe(Path.GetTempPath());
        if (Prefetch.IsChecked == true)
            bytes += Wipe(@"C:\Windows\Prefetch");
        if (Recycle.IsChecked == true)
            WinUtil.Run("powershell.exe", "-NoProfile -Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"", true);
        MainWindow.Instance?.SetStatus($"Очистка завершена, примерно {bytes / 1024 / 1024} МБ");
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
