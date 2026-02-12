using Godot;

namespace Gauniv.Game;

public partial class GlassBackground : Control
{
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        QueueRedraw();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        var rect = GetRect();
        var width = rect.Size.X;
        var height = rect.Size.Y;

        DrawRect(rect, new Color(0.06f, 0.11f, 0.17f), true);

        var steps = 16;
        for (var i = 0; i < steps; i++)
        {
            var t = i / (float)(steps - 1);
            var stripe = new Color(
                0.12f + 0.06f * t,
                0.20f + 0.08f * t,
                0.30f + 0.10f * t,
                0.16f);
            var y0 = (height / steps) * i;
            var y1 = (height / steps) * (i + 1);
            DrawRect(new Rect2(0, y0, width, y1 - y0), stripe, true);
        }

        DrawCircle(new Vector2(width * 0.18f, height * 0.22f), 220, new Color(0.60f, 0.78f, 1.00f, 0.14f));
        DrawCircle(new Vector2(width * 0.86f, height * 0.30f), 260, new Color(0.72f, 0.56f, 0.96f, 0.11f));
        DrawCircle(new Vector2(width * 0.50f, height * 0.92f), 360, new Color(0.50f, 0.86f, 0.90f, 0.12f));

        var glassLine = new Color(0.88f, 0.96f, 1.00f, 0.10f);
        for (var y = 30; y < height; y += 28)
        {
            DrawLine(new Vector2(0, y), new Vector2(width, y), glassLine, 1f);
        }
    }
}
