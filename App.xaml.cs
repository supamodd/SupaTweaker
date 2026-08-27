using System.Windows;
using System.Windows.Media;

namespace SupaTweaker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/Fonts/");
            Resources["AppFont"] = new FontFamily(uri, "./#Inter");
        }
        catch
        {
            // остаётся Segoe UI Variable из Theme.xaml
        }
    }
}
