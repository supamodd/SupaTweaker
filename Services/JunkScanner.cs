using System.IO;
using Microsoft.Win32;

namespace SupaTweaker.Services;

public class JunkCategory
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Hint { get; init; } = "";
    public string Group { get; init; } = "system";
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
        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var profile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var progData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var steam = SteamPath();

        return
        [
            new() { Id = "drivers", Title = "Пакеты драйверов / загрузки обновлений", Hint = "SoftwareDistribution, DriverStore\\Temp", Group = "system",
                Paths = [@"C:\Windows\SoftwareDistribution\Download", @"C:\Windows\System32\DriverStore\Temp", @"C:\Windows\System32\DriverStore\FileRepository\Temp"] },
            new() { Id = "temp", Title = "Временные файлы", Hint = "TEMP пользователя и Windows", Group = "system",
                Paths = [Path.GetTempPath(), @"C:\Windows\Temp", Path.Combine(local, "Temp")] },
            new() { Id = "do", Title = "Файлы оптимизации доставки", Hint = "Delivery Optimization", Group = "system",
                Paths =
                [
                    @"C:\Windows\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache",
                    @"C:\Windows\SoftwareDistribution\DeliveryOptimization",
                    Path.Combine(local, @"Microsoft\Windows\DeliveryOptimization")
                ]},
            new() { Id = "dumps", Title = "Мини-дампы", Hint = "MEMORY.DMP, Minidump, LiveKernelReports", Group = "system",
                Paths = [@"C:\Windows\MEMORY.DMP", @"C:\Windows\Minidump", @"C:\Windows\LiveKernelReports"] },
            new() { Id = "logs", Title = "Журналы Windows", Hint = "CBS, DISM, Panther, MeasuredBoot", Group = "system",
                Paths = [@"C:\Windows\Logs\CBS", @"C:\Windows\Logs\DISM", @"C:\Windows\Logs\WindowsUpdate", @"C:\Windows\Panther", @"C:\Windows\Logs\MeasuredBoot"] },
            new() { Id = "defender", Title = "Кэш Microsoft Defender", Hint = "история сканирования и логи", Group = "system",
                Paths =
                [
                    @"C:\ProgramData\Microsoft\Windows Defender\Scans\History",
                    @"C:\ProgramData\Microsoft\Windows Defender\Support",
                    @"C:\ProgramData\Microsoft\Windows Defender\LocalCopy"
                ]},
            new() { Id = "inet", Title = "Временные файлы Интернета", Hint = "INetCache, WebCache", Group = "system",
                Paths =
                [
                    Path.Combine(local, @"Microsoft\Windows\INetCache"),
                    Path.Combine(local, @"Microsoft\Windows\INetCookies"),
                    Path.Combine(local, @"Microsoft\Windows\WebCache")
                ]},
            new() { Id = "thumb", Title = "Кэш эскизов", Hint = "thumbcache проводника", Group = "system",
                Paths = [Path.Combine(local, @"Microsoft\Windows\Explorer")] },
            new() { Id = "diag", Title = "Диагностические данные", Hint = "Diagnosis, SleepStudy, WER", Group = "system",
                Paths =
                [
                    @"C:\ProgramData\Microsoft\Diagnosis",
                    @"C:\Windows\System32\SleepStudy",
                    @"C:\ProgramData\Microsoft\Windows\WER",
                    Path.Combine(local, @"Microsoft\Windows\WER"),
                    Path.Combine(local, "Diagnostics")
                ]},
            new() { Id = "recycle", Title = "Корзина", Hint = "все диски", Group = "system", Recycle = true },
            new() { Id = "prefetch", Title = "Prefetch", Hint = "ускорение запуска", Group = "system", Paths = [@"C:\Windows\Prefetch"], Selected = false },
            new() { Id = "font", Title = "Кэш шрифтов", Hint = "FontCache", Group = "system",
                Paths = [@"C:\Windows\ServiceProfiles\LocalService\AppData\Local\FontCache", Path.Combine(local, "FontCache")] },
            new() { Id = "dx", Title = "Кэш DirectX", Hint = "D3DSCache", Group = "system", Paths = [Path.Combine(local, "D3DSCache")] },

            new() { Id = "nvidia", Title = "NVIDIA", Hint = "шейдеры, NV_Cache, установщики", Group = "apps", Selected = false,
                Paths =
                [
                    Path.Combine(local, "NVIDIA"),
                    Path.Combine(local, "NVIDIA Corporation"),
                    Path.Combine(progData, @"NVIDIA Corporation\Downloader"),
                    Path.Combine(progData, @"NVIDIA Corporation\NV_Cache"),
                    Path.Combine(progData, @"NVIDIA Corporation\GeForce Experience\CefCache"),
                    Path.Combine(progData, @"NVIDIA Corporation\NVIDIA App"),
                    @"C:\NVIDIA"
                ]},
            new() { Id = "steam", Title = "Steam", Hint = "shadercache, appcache, htmlcache", Group = "apps", Selected = false,
                Paths = steam == null ? [] :
                [
                    Path.Combine(steam, "appcache"),
                    Path.Combine(steam, "logs"),
                    Path.Combine(steam, "dumps"),
                    Path.Combine(steam, "htmlcache"),
                    Path.Combine(steam, "steamapps", "shadercache"),
                    Path.Combine(steam, "steamapps", "temp")
                ]},
            new() { Id = "discord", Title = "Discord", Hint = "Cache, Code Cache, GPUCache", Group = "apps", Selected = false,
                Paths =
                [
                    Path.Combine(roaming, @"discord\Cache"),
                    Path.Combine(roaming, @"discord\Code Cache"),
                    Path.Combine(roaming, @"discord\GPUCache"),
                    Path.Combine(roaming, @"discord\CachedData")
                ]},
            new() { Id = "nuget", Title = "NuGet", Hint = "http-cache и plugins-cache", Group = "apps", Selected = false,
                Paths =
                [
                    Path.Combine(local, @"NuGet\v3-cache"),
                    Path.Combine(local, @"NuGet\plugins-cache"),
                    Path.Combine(profile, @".nuget\http-cache")
                ]},
            new() { Id = "edge", Title = "Microsoft Edge", Hint = "Cache, GPU, Code Cache (все профили)", Group = "apps", Selected = false,
                Paths = BrowserCaches(Path.Combine(local, @"Microsoft\Edge\User Data")) },
            new() { Id = "chrome", Title = "Google Chrome", Hint = "Cache всех профилей", Group = "apps", Selected = false,
                Paths = BrowserCaches(Path.Combine(local, @"Google\Chrome\User Data")) },
            new() { Id = "firefox", Title = "Firefox", Hint = "cache2", Group = "apps", Selected = false,
                Paths = FirefoxCaches(Path.Combine(roaming, @"Mozilla\Firefox\Profiles"), Path.Combine(local, @"Mozilla\Firefox\Profiles")) },
            new() { Id = "adobe", Title = "Adobe", Hint = "Media Cache, Peak Files", Group = "apps", Selected = false,
                Paths =
                [
                    Path.Combine(roaming, @"Adobe\Common\Media Cache Files"),
                    Path.Combine(roaming, @"Adobe\Common\Media Cache"),
                    Path.Combine(roaming, @"Adobe\Common\Peak Files"),
                    Path.Combine(local, @"Adobe\CameraRaw\Cache")
                ]},
            new() { Id = "spotify", Title = "Spotify", Hint = "Data / Storage", Group = "apps", Selected = false,
                Paths = [Path.Combine(local, @"Spotify\Data"), Path.Combine(local, @"Spotify\Storage")] },
            new() { Id = "teams", Title = "Microsoft Teams", Hint = "Cache и tmp", Group = "apps", Selected = false,
                Paths =
                [
                    Path.Combine(roaming, @"Microsoft\Teams\Cache"),
                    Path.Combine(roaming, @"Microsoft\Teams\tmp"),
                    Path.Combine(local, @"Packages\MSTeams_8wekyb3d8bbwe\LocalCache")
                ]},
            new() { Id = "epic", Title = "Epic Games", Hint = "webcache лаунчера", Group = "apps", Selected = false,
                Paths = [Path.Combine(local, @"EpicGamesLauncher\Saved\webcache"), Path.Combine(local, @"EpicGamesLauncher\Saved\Logs")] }
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

    private static string? SteamPath()
    {
        try
        {
            using var k = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            return k?.GetValue("SteamPath")?.ToString()?.Replace('/', '\\');
        }
        catch { return null; }
    }

    private static string[] BrowserCaches(string userData)
    {
        if (!Directory.Exists(userData)) return [];
        var list = new List<string>();
        foreach (var dir in Directory.GetDirectories(userData))
        {
            foreach (var name in new[] { "Cache", "Code Cache", "GPUCache", "Service Worker", "DawnCache" })
            {
                var p = Path.Combine(dir, name);
                if (Directory.Exists(p)) list.Add(p);
            }
        }
        return [.. list];
    }

    private static string[] FirefoxCaches(params string[] roots)
    {
        var list = new List<string>();
        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            foreach (var dir in Directory.GetDirectories(root))
            {
                var c = Path.Combine(dir, "cache2");
                if (Directory.Exists(c)) list.Add(c);
            }
        }
        return [.. list];
    }

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
