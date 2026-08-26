using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        AdminBadge.Text = WinUtil.IsAdmin()
            ? "режим администратора"
            : "запустите от имени администратора";
        AdminBadge.Foreground = WinUtil.IsAdmin()
            ? (Brush)FindResource("BrushOk")
            : (Brush)FindResource("BrushDanger");
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
            case "info":
                title = "О системе";
                sub = "Характеристики этого ПК";
                page = new InfoPage();
                break;
            case "clean":
                title = "Очистка";
                sub = "Временные файлы и корзина";
                page = new CleanPage();
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
        ContentHost.Navigate(page);
    }
}
