using System.Windows;
using System.Windows.Controls;
using SupaTweaker.Pages;
using SupaTweaker.Services;

namespace SupaTweaker;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        AdminBadge.Text = WinUtil.IsAdmin() ? "режим администратора" : "запустите от имени администратора";
        AdminBadge.Foreground = WinUtil.IsAdmin()
            ? (System.Windows.Media.Brush)FindResource("BrushOk")
            : (System.Windows.Media.Brush)FindResource("BrushDanger");
        Navigate("home");
    }

    public void SetStatus(string text) => StatusText.Text = $"{DateTime.Now:HH:mm:ss}  {text}";

    private void Nav_Click(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton rb && rb.Tag is string tag)
            Navigate(tag);
    }

    private void Navigate(string tag)
    {
        Page page;
        (PageTitle.Text, PageSub.Text, page) = tag switch
        {
            "explorer" => ("Проводник", "Рабочий стол и Explorer", new ExplorerPage()),
            "update" => ("Windows Update", "Политики обновлений", new UpdatePage()),
            "system" => ("Система", "Восстановление и служебные функции", new SystemPage()),
            "personal" => ("Персонализация", "Тема, панель задач, меню Пуск", new PersonalPage()),
            "uwp" => ("UWP", "Удаление встроенных приложений", new UwpPage()),
            "quick" => ("Быстрая настройка", "Набор безопасных твиков одним кликом", new QuickPage()),
            "adv" => ("Дополнительно", "Контекстное меню и скрытые опции", new AdvancedPage()),
            "comp" => ("Компоненты", "Дополнительные возможности Windows", new ComponentsPage()),
            "perf" => ("Производительность", "Питание, визуальные эффекты, службы", new PerfPage()),
            "sat" => ("Таймер", "Отложенное выключение или перезагрузка", new TimerPage()),
            "proc" => ("Процессы", "Диспетчер задач внутри приложения", new ProcessPage()),
            "info" => ("О системе", "Характеристики этого ПК", new InfoPage()),
            "clean" => ("Очистка", "Временные файлы и корзина", new CleanPage()),
            "settings" => ("Настройки", "О программе", new SettingsPage()),
            _ => ("Главная", "Обзор и быстрые действия", new HomePage())
        };
        ContentHost.Navigate(page);
    }
}
