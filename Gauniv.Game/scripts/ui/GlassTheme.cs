using Godot;

namespace Gauniv.Game;

public static class GlassTheme
{
    public static Theme Build()
    {
        var theme = new Theme();

        var panel = GlassBox(
            new Color(0.10f, 0.16f, 0.24f, 0.58f),
            new Color(0.82f, 0.92f, 1.00f, 0.34f),
            1,
            16,
            12,
            new Color(0.04f, 0.08f, 0.14f, 0.40f));

        var inner = GlassBox(
            new Color(0.08f, 0.14f, 0.21f, 0.48f),
            new Color(0.76f, 0.90f, 1.00f, 0.24f),
            1,
            14,
            10,
            new Color(0.02f, 0.05f, 0.09f, 0.34f));

        var buttonNormal = GlassBox(
            new Color(0.15f, 0.22f, 0.31f, 0.54f),
            new Color(0.84f, 0.93f, 1.00f, 0.36f),
            1,
            14,
            8,
            new Color(0.03f, 0.07f, 0.11f, 0.32f));

        var buttonHover = GlassBox(
            new Color(0.20f, 0.29f, 0.40f, 0.60f),
            new Color(0.95f, 0.98f, 1.00f, 0.52f),
            1,
            14,
            8,
            new Color(0.03f, 0.08f, 0.14f, 0.40f));

        var buttonPressed = GlassBox(
            new Color(0.14f, 0.20f, 0.28f, 0.68f),
            new Color(0.95f, 0.98f, 1.00f, 0.58f),
            1,
            14,
            8,
            new Color(0.02f, 0.06f, 0.10f, 0.44f));

        var inputNormal = GlassBox(
            new Color(0.08f, 0.13f, 0.20f, 0.58f),
            new Color(0.78f, 0.90f, 1.00f, 0.34f),
            1,
            12,
            8,
            new Color(0.03f, 0.06f, 0.11f, 0.30f));

        var inputFocus = GlassBox(
            new Color(0.10f, 0.18f, 0.27f, 0.66f),
            new Color(0.95f, 0.98f, 1.00f, 0.62f),
            1,
            12,
            8,
            new Color(0.03f, 0.08f, 0.14f, 0.38f));

        var listCursor = GlassBox(
            new Color(0.24f, 0.39f, 0.52f, 0.62f),
            new Color(0.96f, 0.99f, 1.00f, 0.65f),
            1,
            10,
            4,
            new Color(0.04f, 0.08f, 0.12f, 0.36f));

        theme.SetStylebox("panel", "PanelContainer", panel);

        theme.SetStylebox("normal", "Button", buttonNormal);
        theme.SetStylebox("hover", "Button", buttonHover);
        theme.SetStylebox("pressed", "Button", buttonPressed);
        theme.SetStylebox("focus", "Button", buttonHover);
        theme.SetStylebox("disabled", "Button", GlassBox(
            new Color(0.14f, 0.14f, 0.16f, 0.45f),
            new Color(0.56f, 0.60f, 0.66f, 0.30f),
            1, 14, 8, new Color(0.02f, 0.03f, 0.05f, 0.22f)));

        theme.SetStylebox("normal", "LineEdit", inputNormal);
        theme.SetStylebox("focus", "LineEdit", inputFocus);
        theme.SetStylebox("read_only", "LineEdit", inputNormal);

        theme.SetStylebox("normal", "TextEdit", inputNormal);
        theme.SetStylebox("focus", "TextEdit", inputFocus);

        theme.SetStylebox("panel", "ItemList", inner);
        theme.SetStylebox("focus", "ItemList", inputFocus);
        theme.SetStylebox("cursor", "ItemList", listCursor);
        theme.SetStylebox("cursor_unfocused", "ItemList", listCursor);
        theme.SetStylebox("normal", "RichTextLabel", inner);

        theme.SetColor("font_color", "Label", new Color(0.93f, 0.97f, 1.00f));
        theme.SetColor("font_outline_color", "Label", new Color(0.05f, 0.09f, 0.13f, 0.7f));
        theme.SetConstant("outline_size", "Label", 1);

        theme.SetColor("font_color", "Button", new Color(0.94f, 0.98f, 1.00f));
        theme.SetColor("font_hover_color", "Button", new Color(1.00f, 1.00f, 1.00f));
        theme.SetColor("font_pressed_color", "Button", new Color(0.92f, 0.98f, 1.00f));
        theme.SetColor("font_disabled_color", "Button", new Color(0.62f, 0.66f, 0.73f));

        theme.SetColor("font_color", "LineEdit", new Color(0.92f, 0.97f, 1.00f));
        theme.SetColor("font_placeholder_color", "LineEdit", new Color(0.72f, 0.80f, 0.88f, 0.72f));
        theme.SetColor("font_color", "ItemList", new Color(0.91f, 0.97f, 1.00f));
        theme.SetColor("font_selected_color", "ItemList", new Color(1.00f, 1.00f, 1.00f));
        theme.SetColor("font_hovered_color", "ItemList", new Color(0.97f, 0.99f, 1.00f));
        theme.SetColor("default_color", "RichTextLabel", new Color(0.86f, 0.95f, 1.00f));

        theme.SetConstant("font_size", "Label", 18);
        theme.SetConstant("font_size", "Button", 17);
        theme.SetConstant("font_size", "LineEdit", 17);
        theme.SetConstant("font_size", "ItemList", 16);
        theme.SetConstant("font_size", "RichTextLabel", 14);

        theme.SetConstant("h_separation", "HBoxContainer", 10);
        theme.SetConstant("v_separation", "VBoxContainer", 10);
        theme.SetConstant("line_separation", "RichTextLabel", 2);

        return theme;
    }

    private static StyleBoxFlat GlassBox(
        Color bg,
        Color border,
        int borderWidth,
        int radius,
        int padding,
        Color shadow)
    {
        return new StyleBoxFlat
        {
            BgColor = bg,
            BorderColor = border,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            BorderWidthLeft = borderWidth,
            ContentMarginLeft = padding,
            ContentMarginRight = padding,
            ContentMarginTop = padding * 0.7f,
            ContentMarginBottom = padding * 0.7f,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            CornerRadiusBottomRight = radius,
            CornerRadiusBottomLeft = radius,
            ShadowColor = shadow,
            ShadowSize = 10,
            ShadowOffset = new Vector2(0, 3)
        };
    }
}
