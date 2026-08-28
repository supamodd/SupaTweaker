using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public class JunkVm : INotifyPropertyChanged
{
    public JunkCategory Cat { get; }
    public string Title => Cat.Title;
    public string Hint => Cat.Hint;
    public string SizeText => Cat.SizeText;
    public bool Selected
    {
        get => Cat.Selected;
        set { Cat.Selected = value; OnPropertyChanged(); }
    }
    public JunkVm(JunkCategory c) => Cat = c;
    public event PropertyChangedEventHandler? PropertyChanged;
    public void RefreshSize() => OnPropertyChanged(nameof(SizeText));
    private void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}

public partial class CleanPage : Page
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(2) };
    private List<JunkVm> _items = [];
    private bool _busy;

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

    private async void ScanClick(object s, RoutedEventArgs e)
    {
        if (_busy) return;
        _busy = true;
        ScanBtn.IsEnabled = false;
        CleanBtn.IsEnabled = false;
        JunkTotal.Text = "Сканирование…";
        JunkLevel.Text = "Считаем кэши и временные файлы";
        var cats = JunkScanner.Categories();
        _items = cats.Select(c => new JunkVm(c)).ToList();
        Cats.ItemsSource = _items;
        await Task.Run(() => JunkScanner.Scan(cats));
        foreach (var vm in _items) vm.RefreshSize();
        var total = cats.Sum(c => c.Bytes);
        JunkTotal.Text = JunkScanner.Format(total);
        JunkLevel.Text = JunkScanner.Level(total);
        UpdateSelected();
        ScanBtn.IsEnabled = true;
        CleanBtn.IsEnabled = total > 0;
        _busy = false;
        MainWindow.Instance?.SetStatus($"Найдено мусора: {JunkScanner.Format(total)}");
    }

    private void SelChanged(object s, RoutedEventArgs e) => UpdateSelected();

    private void UpdateSelected()
    {
        var n = _items.Where(x => x.Selected).Sum(x => x.Cat.Bytes);
        var c = _items.Count(x => x.Selected);
        SelectedLabel.Text = c == 0 ? "Ничего не выбрано" : $"К удалению: {JunkScanner.Format(n)}  ·  разделов {c}";
    }

    private async void CleanClick(object s, RoutedEventArgs e)
    {
        if (_busy || _items.Count == 0) return;
        _busy = true;
        CleanBtn.IsEnabled = false;
        var cats = _items.Select(x => x.Cat).ToList();
        var n = await Task.Run(() => JunkScanner.Clean(cats));
        MainWindow.Instance?.SetStatus($"Удалено примерно {JunkScanner.Format(n)}");
        _busy = false;
        ScanClick(s, e);
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
        var freed = Math.Max(0, (long)after.Avail - (long)before.Avail);
        MainWindow.Instance?.SetStatus($"ОЗУ: освобождено примерно {freed / 1024d / 1024d:0} МБ");
    }
}
