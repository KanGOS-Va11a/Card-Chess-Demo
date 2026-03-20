using Godot;

namespace CardChessDemo.Battle.Actors;

public partial class BattleActorTokenBase : Node2D
{
    [Export] public Color AccentColor { get; set; } = Colors.White;
    [Export] public Color ShadowColor { get; set; } = new(0, 0, 0, 0.2f);
    [Export] public Vector2 TokenSize { get; set; } = new(12.0f, 12.0f);
    [Export] public float FloatAmplitude { get; set; } = 0.6f;
    [Export] public float FloatSpeed { get; set; } = 3.2f;

    private Vector2 _basePosition;

    public override void _Ready()
    {
        _basePosition = Position;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        float t = (float)Time.GetTicksMsec() * 0.001f * FloatSpeed;
        Position = _basePosition + new Vector2(0.0f, Mathf.Sin(t) * FloatAmplitude);
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (HasNode("Sprite2D"))
        {
            return;
        }

        Rect2 rect = new Rect2(-TokenSize * 0.5f, TokenSize);
        DrawRect(new Rect2(rect.Position + new Vector2(0, 2), rect.Size), ShadowColor, true);
        DrawRect(rect, AccentColor, true);
        DrawRect(rect, Colors.Black, false, 1.0f);
    }
}
