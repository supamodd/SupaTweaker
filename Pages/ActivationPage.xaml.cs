using System.Diagnostics;
using System.IO;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SupaTweaker.Services;

namespace SupaTweaker.Pages;

public partial class ActivationPage : Page
{
    // KMS-ключ клиента (GVLK) для Windows 10/11 Pro и адрес корпоративного KMS-сервера.
    private const string KmsKey = "W269N-WFGWX-YVC9B-4J6C9-T83GX";
    private const string KmsServer = "kms.supamoddcomp.ir";

    private bool _busy;

    public ActivationPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        string caption = await Task.Run(GetOsCaption);
        if (caption.Length > 0) EditionLine.Text = $"Редакция: {caption}";
        if (LooksLikeHome(caption))
            EditionWarning.Visibility = Visibility.Visible;
        await RefreshStatusAsync();
    }

    private void RefreshClick(object sender, RoutedEventArgs e)
    {
        if (!_busy) _ = RefreshStatusAsync();
    }

    // ── Одна кнопка: ipk → skms → ato → проверка ─────────────────────────
    private async void ActivateClick(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        _busy = true;
        ActivateBtn.IsEnabled = false;
        RefreshBtn.IsEnabled = false;
        HintLine.Visibility = Visibility.Collapsed;
        Log.Text = "";

        AppendLog($"KMS-сервер: {KmsServer}");
        AppendLog($"Ключ:       {KmsKey}");
        AppendLog(Rule());

        int code = await RunStep("Устанавливаю корпоративный ключ", $"/ipk \"{KmsKey}\"", 60_000);
        if (code != 0) { Finish(false, "Ключ не установлен — дальнейшая активация невозможна."); return; }

        code = await RunStep("Указываю KMS-сервер компании", $"/skms {KmsServer}", 60_000);
        if (code != 0) { Finish(false, "Не удалось задать KMS-сервер — дальнейшая активация невозможна."); return; }

        AppendLog("");
        AppendLog("Ключ применён. Ожидание 3 секунды (случай с лицензиями)...");
        await Task.Delay(3000);

        code = await RunStep("Активирую (может занять до минуты)", "/ato", 180_000);

        bool activated = await RefreshStatusAsync(writeLog: true);
        Finish(activated, "Активация не подтвердилась — смотрите вывод slmgr выше.");
    }

    private void Finish(bool ok, string message)
    {
        AppendLog(Rule());
        if (ok)
        {
            AppendLog("Windows активирована");
            MainWindow.Instance?.SetStatus("Windows активирована через корпоративный KMS");
        }
        else
        {
            AppendLog(message);
            string hint = BuildHint(Log.Text);
            if (hint.Length > 0) AppendLog("Частые причины: " + hint);
            HintLine.Text = "Активация не выполнена. Если ошибка повторяется — обратитесь к системному администратору.";
            HintLine.Visibility = Visibility.Visible;
            MainWindow.Instance?.SetStatus("Активация Windows не удалась");
        }
        _busy = false;
        ActivateBtn.IsEnabled = true;
        RefreshBtn.IsEnabled = true;
    }

    private static string Rule() => new('-', 46);

    // ── Статус ───────────────────────────────────────────────────────────
    private async Task<bool> RefreshStatusAsync(bool writeLog = false)
    {
        StatusBig.Text = "Проверка…";
        StatusSub.Text = "Запрос лицензионного состояния (slmgr /xpr)…";
        StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x8B, 0x97, 0xB3));

        var xpr = await Task.Run(() => RunSlmgr("/xpr", 30_000));
        if (writeLog)
        {
            AppendLog("Проверка статуса — slmgr /xpr");
            AppendLog(xpr.Text);
        }

        var dlv = await Task.Run(() => RunSlmgr("/dlv", 30_000));
        string channel = ExtractChannel(dlv.Text);
        bool activated = IsActivated(xpr.Text);
        string extra = channel.Length > 0 ? $", канал: {channel}" : "";

        if (activated)
        {
            StatusBig.Text = "АКТИВИРОВАНА";
            StatusBig.Foreground = (Brush)FindResource("BrushOk");
            StatusDot.Fill = new SolidColorBrush((Color)FindResource("Ok"));
            StatusSub.Text = (xpr.Text.Contains("KMS", StringComparison.OrdinalIgnoreCase)
                ? $"KMS-лицензия (сервер: {KmsServer}), продлевается автоматически"
                : "Лицензия активирована") + extra;
        }
        else
        {
            StatusBig.Text = "НЕ АКТИВИРОВАНА";
            StatusBig.Foreground = (Brush)FindResource("BrushDanger");
            StatusDot.Fill = new SolidColorBrush((Color)FindResource("Danger"));
            StatusSub.Text = "Лицензия не активирована — нажмите «Активировать Windows»" + extra;
        }
        return activated;
    }

    private static bool IsActivated(string xpr)
    {
        if (xpr.Contains("не активирован", StringComparison.OrdinalIgnoreCase) ||
            xpr.Contains("not activated", StringComparison.OrdinalIgnoreCase))
            return false;
        return xpr.Contains("активирован", StringComparison.OrdinalIgnoreCase) ||
               xpr.Contains("activated", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractChannel(string dlv)
    {
        var m = Regex.Match(dlv, @"(VOLUME_KMSCLIENT|VOLUME_MAK|RETAIL)", RegexOptions.IgnoreCase);
        return m.Success ? m.Value.ToUpperInvariant() : "";
    }

    private static bool LooksLikeHome(string caption)
    {
        return caption.Contains("Home", StringComparison.OrdinalIgnoreCase) ||
               caption.Contains("Домашняя", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetOsCaption()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
            using var col = searcher.Get();
            foreach (ManagementObject m in col)
                return m["Caption"]?.ToString() ?? "";
        }
        catch { }
        return "";
    }

    // ── Запуск slmgr с захватом вывода ────────────────────────────────────
    private async Task<int> RunStep(string title, string args, int timeoutMs)
    {
        AppendLog($"{title} — slmgr {args}");
        (int code, string text) = await Task.Run(() => RunSlmgr(args, timeoutMs));
        AppendLog(text);
        AppendLog(code == 0 ? "   [OK] выполнено" : $"   [ОШИБКА] код {code:X8}");
        return code;
    }

    private static (int Code, string Text) RunSlmgr(string args, int timeoutMs)
    {
        var psi = new ProcessStartInfo("slmgr.exe", args)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var p = Process.Start(psi);
        if (p == null) return (-1, "Не удалось запустить slmgr.exe");

        // Читаем оба потока параллельно, чтобы не зациклиться на буферах.
        var outTask = Task.Run(() => ReadAll(p.StandardOutput.BaseStream));
        var errTask = Task.Run(() => ReadAll(p.StandardError.BaseStream));

        if (!p.WaitForExit(timeoutMs))
        {
            try { p.Kill(); } catch { }
            return (-1, "Команда не завершилась за отведённое время.");
        }

        string outText = outTask.Wait(5000) ? DecodeOem(outTask.Result) : "";
        string errText = errTask.Wait(5000) ? DecodeOem(errTask.Result) : "";
        string text = Clean(outText) + Clean(errText);
        return (p.ExitCode, text);
    }

    private static byte[] ReadAll(Stream s)
    {
        using var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }

    // slmgr печатает в OEM-кодопэйдж консоли; на русскоязычных Windows это CP866.
    private static string DecodeOem(byte[] bytes)
    {
        string s = Encoding.GetEncoding(866).GetString(bytes);
        bool hasHigh = s.Any(c => c > 127);
        bool hasCyrillic = s.Any(c => c >= 'А' && c <= 'я');
        if (!hasHigh || hasCyrillic) return s;
        foreach (int cp in new[] { 1251, 437 })
        {
            string t = Encoding.GetEncoding(cp).GetString(bytes);
            if (t.Any(char.IsLetter)) return t;
        }
        return s;
    }

    private static string Clean(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        var lines = s.Replace("\r", "").Split('\n')
                     .Select(l => l.TrimEnd())
                     .Where(l => l.Length > 0)
                     .ToList();
        return lines.Count == 0 ? "" : string.Join("\n", lines) + "\n";
    }

    private static string BuildHint(string text)
    {
        var hints = new List<string>();
        if (text.Contains("0xC004F074", StringComparison.OrdinalIgnoreCase))
            hints.Add("0xC004F074 — ПК не может связаться с KMS-сервером: проверьте сеть, DNS-запись kms.supamoddcomp.ir, доступ к порту 1688 (TCP) и системное время");
        if (text.Contains("0xC004F064", StringComparison.OrdinalIgnoreCase))
            hints.Add("0xC004F064 — ключ не подходит под установленную редакцию Windows (ключ рассчитан на Pro)");
        if (text.Contains("0x8007232A", StringComparison.OrdinalIgnoreCase) ||
            text.Contains("0x80070490", StringComparison.OrdinalIgnoreCase))
            hints.Add("сбой сети/DNS: проверьте подключение к корпоративной сети");
        return hints.Count > 0
            ? string.Join(" | ", hints)
            : "проверьте доступ к kms.supamoddcomp.ir (порт 1688, TCP) и редакцию Windows (ключ — для Pro)";
    }

    private void AppendLog(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        Log.Text += text + "\n";
        Log.CaretIndex = Log.Text.Length;
        Log.ScrollToEnd();
    }
}
