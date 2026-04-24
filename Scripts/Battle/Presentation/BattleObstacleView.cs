using System.Linq;
using Godot;

namespace CardChessDemo.Battle.Presentation;

public partial class BattleObstacleView : BattleAnimatedViewBase
{
    private const string BreakableObstaclePath = "res://Assets/Tilemap/Battle/obstacle/breakable_obstacle.png";
    private const string BreakableArakawaObstaclePath = "res://Assets/Tilemap/Battle/obstacle/breakable_obstacle_arakawa.png";
    private const string SlowObstaclePath = "res://Assets/Tilemap/Battle/obstacle/slow_obstacle.png";
    private const string UnbreakableObstaclePath = "res://Assets/Tilemap/Battle/obstacle/unbreakable_obstacle.png";

    protected override SpriteFrames BuildFallbackFrames()
    {
        Texture2D? obstacleTexture = ResolveObstacleTexture();
        if (obstacleTexture != null)
        {
            return BuildTextureFrames(obstacleTexture);
        }

        if (State?.DefinitionId == "battle_obstacle_wall")
        {
            return CreateFrames(new Color(0.42f, 0.44f, 0.48f), new Color(0.60f, 0.63f, 0.68f));
        }

        if (State?.DefinitionId == "battle_obstacle_slow")
        {
            return CreateFrames(new Color(0.62f, 0.48f, 0.26f), new Color(0.80f, 0.68f, 0.36f));
        }

        return CreateFrames(new Color(0.72f, 0.26f, 0.24f), new Color(0.92f, 0.58f, 0.44f));
    }

    protected override void ConfigureAnimatedSprite(AnimatedSprite2D sprite)
    {
        sprite.Centered = false;
        sprite.Position = ResolveBottomAlignedSpriteOffset(sprite);
    }

    private Texture2D? ResolveObstacleTexture()
    {
        if (State == null)
        {
            return null;
        }

        if (State.ObjectId.StartsWith("arakawa_wall_", System.StringComparison.Ordinal))
        {
            return ResourceLoader.Load<Texture2D>(BreakableArakawaObstaclePath)
                ?? ResourceLoader.Load<Texture2D>(BreakableObstaclePath);
        }

        if (State.DefinitionId == "battle_obstacle_slow")
        {
            return ResourceLoader.Load<Texture2D>(SlowObstaclePath);
        }

        if (State.DefinitionId == "battle_obstacle_wall")
        {
            return ResourceLoader.Load<Texture2D>(UnbreakableObstaclePath);
        }

        return ResourceLoader.Load<Texture2D>(BreakableObstaclePath);
    }

    private static SpriteFrames BuildTextureFrames(Texture2D texture)
    {
        SpriteFrames frames = new();
        AddSingleFrameAnimation(frames, "idle", texture, true);
        AddSingleFrameAnimation(frames, "move", texture, true);
        AddSingleFrameAnimation(frames, "action", texture, false);
        AddSingleFrameAnimation(frames, "hit", texture, false);
        AddSingleFrameAnimation(frames, "defend", texture, false);
        AddSingleFrameAnimation(frames, "defeat", texture, false);
        return frames;
    }

    private static void AddSingleFrameAnimation(SpriteFrames frames, string animationName, Texture2D texture, bool loop)
    {
        frames.AddAnimation(animationName);
        frames.SetAnimationLoop(animationName, loop);
        frames.SetAnimationSpeed(animationName, 1.0d);
        frames.AddFrame(animationName, texture);
    }

    private static Vector2 ResolveBottomAlignedSpriteOffset(AnimatedSprite2D sprite)
    {
        Texture2D? texture = null;
        SpriteFrames? frames = sprite.SpriteFrames;
        if (frames != null)
        {
            string animationName = frames.HasAnimation("idle") ? "idle" : frames.GetAnimationNames().FirstOrDefault().ToString();
            if (!string.IsNullOrWhiteSpace(animationName) && frames.HasAnimation(animationName) && frames.GetFrameCount(animationName) > 0)
            {
                texture = frames.GetFrameTexture(animationName, 0);
            }
        }

        Vector2 textureSize = texture?.GetSize() ?? new Vector2(16.0f, 16.0f);
        return new Vector2(-textureSize.X * 0.5f, 8.0f - textureSize.Y);
    }
}
