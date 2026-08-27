using System.IO;
using System.Windows;
using System.Windows.Media;

namespace SupaTweaker;

public partial class App : Application
{
    public static FontFamily UiFont { get; private set; } =
        new("Segoe UI Variable Display, Segoe UI");

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var loaded = LoadFromDisk() ?? LoadFromPack();
        if (loaded != null)
        {
            UiFont = loaded;
            Resources["AppFont"] = loaded;
        }
    }

    private static FontFamily? LoadFromDisk()
    {
        string[] dirs =
        [
            Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts"),
            Path.Combine(AppContext.BaseDirectory, "Fonts"),
            Path.Combine(AppContext.BaseDirectory, "Assets"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Assets", "Fonts"))
        ];

        foreach (var dir in dirs)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var file in Directory.GetFiles(dir, "*.ttf"))
            {
                try
                {
                    var gt = new GlyphTypeface(new Uri(file));
                    var face = "Inter";
                    foreach (var kv in gt.FamilyNames)
                    {
                        if (!string.IsNullOrWhiteSpace(kv.Value))
                        {
                            face = kv.Value;
                            break;
                        }
                    }

                    var folder = Path.GetDirectoryName(file)!.Replace('\\', '/') + "/";
                    if (!folder.StartsWith('/')) folder = "/" + folder;
                    return new FontFamily(new Uri("file://" + folder), "./#" + face);
                }
                catch
                {
                    // следующий файл
                }
            }
        }
        return null;
    }

    private static FontFamily? LoadFromPack()
    {
        try
        {
            var folder = new Uri("pack://application:,,,/Assets/Fonts/");
            _ = new GlyphTypeface(new Uri("pack://application:,,,/Assets/Fonts/InterVariable.ttf"));
            return new FontFamily(folder, "./#Inter");
        }
        catch
        {
            return null;
        }
    }
}
