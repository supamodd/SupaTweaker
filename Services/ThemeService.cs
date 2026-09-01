using System.Windows;
using System.Windows.Media;

namespace SupaTweaker.Services;

public static class ThemeService
{
    public const string Standard = "standard";
    public const string Dark = "dark";
    public const string Light = "light";

    public static void Apply(string? id)
    {
        id = (id ?? Standard).ToLowerInvariant();
        if (id is not (Standard or Dark or Light)) id = Standard;
        AppSettings.Theme = id;

        switch (id)
        {
            case Dark:
                Solid("BrushBgDeep", "#000000");
                Solid("BrushBgPane", "#0A0A0A");
                Solid("BrushBgCard", "#141414");
                Solid("BrushWindow", "#050505");
                Solid("BrushText", "#F2F2F2");
                Solid("BrushMuted", "#8A8A8A");
                Solid("BrushAccent", "#E8E8E8");
                Solid("BrushAccent2", "#B0B0B0");
                Solid("BrushDanger", "#FF5C7A");
                Solid("BrushOk", "#3DDC97");
                Solid("BrushTrack", "#2A2A2A");
                Solid("BrushInput", "#0C0C0C");
                Solid("BrushHairline", "#33FFFFFF");
                Solid("BrushOnInk", "#111111");
                Solid("BrushNavActive", "#33FFFFFF");
                Solid("BrushStatusBar", "#AA000000");
                Gradient("BrushHero", "#3A3A3A", "#1A1A1A");
                Gradient("BrushPane", "#101010", "#050505");
                break;
            case Light:
                Solid("BrushBgDeep", "#EEF1F6");
                Solid("BrushBgPane", "#F7F8FB");
                Solid("BrushBgCard", "#FFFFFF");
                Solid("BrushWindow", "#F4F6FA");
                Solid("BrushText", "#1A2030");
                Solid("BrushMuted", "#5C6578");
                Solid("BrushAccent", "#4F6EF7");
                Solid("BrushAccent2", "#0D9488");
                Solid("BrushDanger", "#E11D48");
                Solid("BrushOk", "#059669");
                Solid("BrushTrack", "#D8DEEA");
                Solid("BrushInput", "#FFFFFF");
                Solid("BrushHairline", "#22000000");
                Solid("BrushOnInk", "#FFFFFF");
                Solid("BrushNavActive", "#334F6EF7");
                Solid("BrushStatusBar", "#CCFFFFFF");
                Gradient("BrushHero", "#6C8CFF", "#3EE0C6");
                Gradient("BrushPane", "#FFFFFF", "#EEF1F7");
                break;
            default:
                Solid("BrushBgDeep", "#070A12");
                Solid("BrushBgPane", "#0E1422");
                Solid("BrushBgCard", "#141C2C");
                Solid("BrushWindow", "#080B14");
                Solid("BrushText", "#F4F7FF");
                Solid("BrushMuted", "#8B97B3");
                Solid("BrushAccent", "#6C8CFF");
                Solid("BrushAccent2", "#3EE0C6");
                Solid("BrushDanger", "#FF5C7A");
                Solid("BrushOk", "#3DDC97");
                Solid("BrushTrack", "#243044");
                Solid("BrushInput", "#0B1220");
                Solid("BrushHairline", "#28FFFFFF");
                Solid("BrushOnInk", "#061018");
                Solid("BrushNavActive", "#286C8CFF");
                Solid("BrushStatusBar", "#66070A12");
                Gradient("BrushHero", "#6C8CFF", "#3EE0C6");
                Gradient("BrushPane", "#121A2C", "#0B101C");
                break;
        }
    }

    private static void Solid(string key, string hex) =>
        Application.Current.Resources[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

    private static void Gradient(string key, string a, string b)
    {
        Application.Current.Resources[key] = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString(a),
            (Color)ColorConverter.ConvertFromString(b),
            new Point(0, 0), new Point(1, 1));
    }
}
