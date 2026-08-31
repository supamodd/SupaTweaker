using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace SupaTweaker.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly Window _window;

    public TrayService(Window window)
    {
        _window = window;
        _icon = new NotifyIcon
        {
            Text = "SupaTweaker",
            Icon = LoadIcon(),
            Visible = true
        };
        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) ShowWindow();
        };

        var menu = new ContextMenuStrip();
        menu.Items.Add("Открыть", null, (_, _) => ShowWindow());
        menu.Items.Add("Выход", null, (_, _) => Exit());
        _icon.ContextMenuStrip = menu;
    }

    public void HideToTray()
    {
        _window.Hide();
        _window.ShowInTaskbar = false;
    }

    public void ShowWindow()
    {
        _window.Show();
        _window.ShowInTaskbar = true;
        _window.WindowState = WindowState.Normal;
        _window.Activate();
    }

    public void Exit()
    {
        _icon.Visible = false;
        if (_window is MainWindow mw) mw.ExitApp();
        else Application.Current.Shutdown();
    }

    private static Icon LoadIcon()
    {
        try
        {
            var pack = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/SupaTweakerIcon.ico"));
            if (pack != null) return new Icon(pack.Stream);
        }
        catch { }

        foreach (var p in new[]
                 {
                     Path.Combine(AppContext.BaseDirectory, "Assets", "SupaTweakerIcon.ico"),
                     Path.Combine(AppContext.BaseDirectory, "SupaTweakerIcon.ico")
                 })
        {
            if (File.Exists(p)) return new Icon(p);
        }

        return SystemIcons.Application;
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
