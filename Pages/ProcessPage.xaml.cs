using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SupaTweaker.Pages;

public partial class ProcessPage : Page
{
    public record Row(int Id, string ProcessName, string MemoryMb);

    public ProcessPage()
    {
        InitializeComponent();
        Refresh(null!, null!);
    }

    private void Refresh(object s, RoutedEventArgs e)
    {
        var q = Filter.Text?.Trim() ?? "";
        var rows = Process.GetProcesses()
            .Select(p =>
            {
                try { return new Row(p.Id, p.ProcessName, (p.WorkingSet64 / 1024 / 1024).ToString()); }
                catch { return null; }
            })
            .Where(r => r != null && (q.Length == 0 || r!.ProcessName.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(r => r!.ProcessName)
            .ToList();
        List.ItemsSource = rows;
        MainWindow.Instance?.SetStatus($"Процессов: {rows.Count}");
    }

    private void Kill(object s, RoutedEventArgs e)
    {
        if (List.SelectedItem is not Row row) return;
        try
        {
            Process.GetProcessById(row.Id).Kill();
            MainWindow.Instance?.SetStatus($"Завершён {row.ProcessName}");
            Refresh(s, e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
