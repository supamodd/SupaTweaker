using SupaTweaker.Pages;
using SupaTweaker.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Application = System.Windows.Application;

namespace SupaTweaker;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }
    private TrayService? _tray;
    private bool _reallyExit;

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        ApplyUiFont(this);
        Loaded += OnLoaded;
        Closing += OnClosing;
        TrySetWindowIcon();
        if (WinUtil.IsAdmin())
        {
            AdminBadge.Text = "режим администратора";
            AdminBadge.Foreground = (Brush)FindResource("BrushOk");
            AdminChip.Background = new SolidColorBrush(Color.FromArgb(0x28, 0x3D, 0xDC, 0x97));
        }
        else
        {
            AdminBadge.Text = "запустите от имени администратора";
            AdminBadge.Foreground = (Brush)FindResource("BrushDanger");
            AdminChip.Background = new SolidColorBrush(Color.FromArgb(0x28, 0xFF, 0x5C, 0x7A));
        }
        Navigate("home");
    }

    private void TrySetWindowIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/Assets/SupaTweakerIcon.ico");
            if (Application.GetResourceStream(uri) != null)
                Icon = System.Windows.Media.Imaging.BitmapFrame.Create(uri);
        }
        catch { }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyUiFont(this);
        ShowInTaskbar = true;
        WindowState = WindowState.Normal;
        try { _tray ??= new TrayService(this); }
        catch { }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_reallyExit) return;
        e.Cancel = true;
        _tray?.HideToTray();
    }

    public void ExitApp()
    {
        _reallyExit = true;
        _tray?.Dispose();
        Close();
        Application.Current.Shutdown();
    }

    private void Title_Drag(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2) Max_Click(sender, e);
        else if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove();
    }

    private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Max_Click(object sender, RoutedEventArgs e) =>
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        if (_tray != null) _tray.HideToTray();
        else Hide();
    }

    public void SetStatus(string text) => StatusText.Text = $"{DateTime.Now:HH:mm:ss}  {text}";

    private bool _firstNav = true;
    private bool _navBusy;
    private string _currentTag = "";

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag)
            Navigate(tag);
    }

    private async void Navigate(string tag)
    {
        if (_navBusy || tag == _currentTag) return;
        _currentTag = tag;

        string title;
        string sub;
        Page page;

        switch (tag)
        {
            case "explorer":
                title = "Проводник";
                sub = "Рабочий стол и Explorer";
                page = new ExplorerPage();
                break;
            case "update":
                title = "Windows Update";
                sub = "Политики обновлений";
                page = new UpdatePage();
                break;
            case "system":
                title = "Система";
                sub = "Восстановление и служебные функции";
                page = new SystemPage();
                break;
            case "personal":
                title = "Персонализация";
                sub = "Тема, панель задач, меню Пуск";
                page = new PersonalPage();
                break;
            case "uwp":
                title = "UWP";
                sub = "Удаление встроенных приложений";
                page = new UwpPage();
                break;
            case "quick":
                title = "Быстрая настройка";
                sub = "Набор безопасных твиков одним кликом";
                page = new QuickPage();
                break;
            case "adv":
                title = "Дополнительно";
                sub = "Контекстное меню и скрытые опции";
                page = new AdvancedPage();
                break;
            case "comp":
                title = "Компоненты";
                sub = "Дополнительные возможности Windows";
                page = new ComponentsPage();
                break;
            case "perf":
                title = "Производительность";
                sub = "Питание, визуальные эффекты, службы";
                page = new PerfPage();
                break;
            case "sat":
                title = "Таймер";
                sub = "Отложенное выключение или перезагрузка";
                page = new TimerPage();
                break;
            case "proc":
                title = "Процессы";
                sub = "Диспетчер задач внутри приложения";
                page = new ProcessPage();
                break;
            case "startup":
                title = "Автозагрузка";
                sub = "Программы при входе в Windows";
                page = new StartupPage();
                break;
            case "info":
                title = "О системе";
                sub = "Характеристики этого ПК";
                page = new InfoPage();
                break;
            case "clean":
                title = "Очистка";
                sub = "Файлы, корзина и оперативная память";
                page = new CleanPage();
                break;
            case "utils":
                title = "Утилиты";
                sub = "Скачать установщики на рабочий стол";
                page = new UtilitiesPage();
                break;
            case "settings":
                title = "Настройки";
                sub = "О программе";
                page = new SettingsPage();
                break;
            default:
                title = "Главная";
                sub = "Обзор и быстрые действия";
                page = new HomePage();
                break;
        }

        PageTitle.Text = title;
        PageSub.Text = sub;
        void Show()
        {
            ContentHost.Navigate(page);
            while (ContentHost.CanGoBack) ContentHost.RemoveBackEntry();
            page.Loaded += (_, _) => ApplyUiFont(page);
            ApplyUiFont(this);
        }

        if (_firstNav)
        {
            _firstNav = false;
            Show();
            return;
        }

        _navBusy = true;
        await UiMotion.FadeSwap(ContentHost, Show);
        _navBusy = false;
    }

    private static void ApplyUiFont(DependencyObject root)
    {
        var font = App.UiFont;
        TextElement.SetFontFamily(root, font);
        if (root is Control c)
            c.FontFamily = font;
        if (root is TextBlock tb)
            tb.FontFamily = font;

        int n = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < n; i++)
            ApplyUiFont(VisualTreeHelper.GetChild(root, i));

        if (root is ContentControl cc && cc.Content is DependencyObject d1)
            ApplyUiFont(d1);
        if (root is Decorator dec && dec.Child != null)
            ApplyUiFont(dec.Child);
        if (root is Panel panel)
        {
            foreach (UIElement child in panel.Children)
                if (child != null) ApplyUiFont(child);
        }
        if (root is Frame f && f.Content is DependencyObject d2)
            ApplyUiFont(d2);
    }
}
