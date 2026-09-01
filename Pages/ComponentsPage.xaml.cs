using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class ComponentsPage : Page
{
    public ComponentsPage() => InitializeComponent();

    private void Opt(object s, RoutedEventArgs e) => WinUtil.Open("optionalfeatures.exe");

    private void Appwiz(object s, RoutedEventArgs e) => WinUtil.Open("control.exe", "appwiz.cpl");

    private void Fod(object s, RoutedEventArgs e) => WinUtil.Open("ms-settings:optionalfeatures");
}
