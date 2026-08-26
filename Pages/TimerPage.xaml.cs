using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class TimerPage : Page
{
    public TimerPage() => InitializeComponent();

    private void Start(object s, RoutedEventArgs e)
    {
        if (!int.TryParse(Mins.Text, out var m) || m < 1) { MessageBox.Show("Укажите минуты"); return; }
        var sec = m * 60;
        var flag = Action.SelectedIndex == 1 ? "/r" : "/s";
        WinUtil.Run("shutdown.exe", $"{flag} /t {sec}");
        MainWindow.Instance?.SetStatus($"Таймер: {m} мин.");
    }

    private void Cancel(object s, RoutedEventArgs e)
    {
        WinUtil.Run("shutdown.exe", "/a");
        MainWindow.Instance?.SetStatus("Таймер отменён");
    }
}
