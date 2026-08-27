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
        UiFont = LoadInter() ?? UiFont;
        Resources["AppFont"] = UiFont;
    }

    private static FontFamily? LoadInter()
    {
        string[] files = ["InterVariable.ttf", "Inter.ttf", "Inter-Regular.ttf", "InterDisplay.ttf"];
        foreach (var file in files)
        {
            try
            {
                var fileUri = new Uri($"pack://application:,,,/Assets/Fonts/{file}");
                var face = new GlyphTypeface(fileUri);
                var familyName =
                    FirstName(face.Win32FamilyNames) ??
                    FirstName(face.FamilyNames) ??
                    "Inter";
                return new FontFamily(new Uri("pack://application:,,,/Assets/Fonts/"), "./#" + familyName);
            }
            catch
            {
                // следующий файл
            }
        }
        return null;
    }

    private static string? FirstName(LanguageSpecificStringDictionary names)
    {
        if (names.Count == 0) return null;
        foreach (var v in names.Values)
            if (!string.IsNullOrWhiteSpace(v)) return v;
        return null;
    }
}
