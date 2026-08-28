using System.IO;

namespace SupaTweaker.Services;

public class JunkCategory
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Hint { get; init; } = "";
    public string[] Paths { get; init; } = [];
    public bool Recycle { get; init; }
    public long Bytes { get; set; }
    public bool Selected { get; set; } = true;
    public string SizeText => JunkScanner.Format(Bytes);
}

public static class JunkScanner
{
    public static List<JunkCategory> Categories()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userTemp = Path.GetTempPath();
        return
        [
            new() { Id = "temp", Title = "Временные файлы", Hint = "TEMP пользователя и Windows", Paths = [userTemp, @"C:\Windows\Temp"] },
            new() { Id = "recycle", Title = "Корзина", Hint = "Удалённые файлы на всех дисках", Recycle = true },
            new() { Id = "wu", Title = "Кэш Windows Update", Hint = "SoftwareDistribution\\Download", Paths = [@"C:\Windows\SoftwareDistribution\Download"] },
            new() { Id = "do", Title = "Delivery Optimization", Hint = "Кэш обновлений с других ПК", Paths = [@"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache"] },
            new() { Id = "thumb", Title = "Эскизы проводника", Hint = "thumbcache", Paths = [Path.Combine(local, @"Microsoft\Windows\Explorer")] },
            new() { Id = "dumps", Title = "Дампы памяти", Hint = "MEMORY.DMP и Minidump", Paths = [@"C:\Windows\MEMORY.DMP", @"C:\Windows\Minidump"] },
            new() { Id = "wer", Title = "Отчёты об ошибках", Hint = "Windows Error Reporting", Paths = [@"C:\ProgramData\Microsoft\Windows\WER", Path.Combine(local, @"Microsoft\Windows\WER")] },
            new() { Id = "dx", Title = "Кэш DirectX / шейдеры", Hint = "D3DSCache", Paths = [Path.Combine(local, @"D3DSCache")] },
            new() { Id = "edge", Title = "Кэш Microsoft Edge", Hint = "Cache, Code Cache, GPUCache", Paths =
            [
                Path.Combine(local, @"Microsoft\Edge\User Data\Default\Cache"),
                Path.Combine(local, @"Microsoft\Edge\User Data\Default\Code Cache"),
                Path.Combine(local, @"Microsoft\Edge\User Data\Default\GPUCache")
            ]},
            new() { Id = "chrome", Title = "Кэш Google Chrome", Hint = "Cache, Code Cache, GPUCache", Paths =
            [
                Path.Combine(local, @"Google\Chrome\User Data\Default\Cache"),
                Path.Combine(local, @"Google\Chrome\User Data\Default\Code Cache"),
                Path.Combine(local, @"Google\Chrome\User Data\Default\GPUCache")
            ]},
            new() { Id = "prefetch", Title = "Prefetch", Hint = "Ускорение запуска программ", Paths = [@"C:\Windows\Prefetch"], Selected = false },
            new() { Id = "logs", Title = "Журналы Windows", Hint = "CBS и setup logs", Paths = [@"C:\Windows\Logs\CBS", @"C:\Windows\Logs\DISM"] }
        ];
    }

    public static void Scan(IEnumerable<JunkCategory> cats, Action<JunkCategory>? each = null)
    {
        foreach (var c in cats)
        {
            long n = 0;
            if (c.Recycle) n = RecycleSize();
            else foreach (var p in c.Paths) n += SizeOf(p);
            c.Bytes = n;
            each?.Invoke(c);
        }
    }

    public static long Clean(IEnumerable<JunkCategory> cats)
    {
        long n = 0;
        foreach (var c in cats.Where(x => x.Selected && x.Bytes > 0))
        {
            if (c.Recycle)
            {
                WinUtil.Run("powershell.exe", "-NoProfile -Command \"Clear-RecycleBin -Force -ErrorAction SilentlyContinue\"", true);
                n += c.Bytes;
                continue;
            }
            foreach (var p in c.Paths) n += Wipe(p);
        }
        return n;
    }

    public static string Format(long bytes)
    {
        if (bytes < 1024) return $"{bytes} Б";
        if (bytes < 1024 * 1024) return $"{bytes / 1024d:0.#} КБ";
        if (bytes < 1024L * 1024 * 1024) return $"{bytes / 1024d / 1024d:0.0} МБ";
        return $"{bytes / 1024d / 1024d / 1024d:0.00} ГБ";
    }

    public static string Level(long bytes) =>
        bytes switch
        {
            < 200L * 1024 * 1024 => "Чисто",
            < 2L * 1024 * 1024 * 1024 => "Немного мусора",
            < 8L * 1024 * 1024 * 1024 => "Засорено",
            _ => "Сильно засорено"
        };

    private static long RecycleSize()
    {
        long n = 0;
        foreach (var d in DriveInfo.GetDrives().Where(x => x.IsReady && x.DriveType == DriveType.Fixed))
            n += SizeOf(Path.Combine(d.RootDirectory.FullName, "$Recycle.Bin"));
        return n;
    }

    private static long SizeOf(string path)
    {
        try
        {
            if (File.Exists(path)) return new FileInfo(path).Length;
            if (!Directory.Exists(path)) return 0;
            long n = 0;
            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try { n += new FileInfo(f).Length; } catch { }
            }
            return n;
        }
        catch { return 0; }
    }

    private static long Wipe(string path)
    {
        long n = 0;
        try
        {
            if (File.Exists(path))
            {
                n = new FileInfo(path).Length;
                File.Delete(path);
                return n;
            }
            if (!Directory.Exists(path)) return 0;
            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(f);
                    n += fi.Length;
                    fi.Delete();
                }
                catch { }
            }
        }
        catch { }
        return n;
    }
}
