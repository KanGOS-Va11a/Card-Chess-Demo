using Godot;

namespace CardChessDemo.Battle.Rooms;

public static class BattleRoomTileSetFactory
{
	public const int FloorSourceId = 0;
	public const int SceneSourceId = 1;
	public const int AlternateFloorSourceId = 2;
	public static readonly Vector2I FloorAtlasCoords = Vector2I.Zero;
	public const int GenericEnemySceneTileId = 2;
	public const int DestructibleObstacleSceneTileId = 3;
	public const int IndestructibleObstacleSceneTileId = 4;
	public const int SlowObstacleSceneTileId = 5;
	public const int EscapeSceneTileId = 6;
	public const int FacingLeftSceneTileId = 7;
	public const int FacingUpSceneTileId = 8;
	public const int FacingRightSceneTileId = 9;
	public const int FacingDownSceneTileId = 10;
	public const int Scene01TutorialEnemySceneTileId = 11;
	public const int PirateBlockerEnemySceneTileId = 12;
	public const int PirateScoutEnemySceneTileId = 13;
	public const int PirateShockerEnemySceneTileId = 14;
	public const int PirateGunnerEnemySceneTileId = 15;
	public const int PiratePipeBomberEnemySceneTileId = 16;
	public const int PirateBruteEliteEnemySceneTileId = 17;
	public const int ScrapMedicEliteEnemySceneTileId = 18;
	public const int SewerGatekeeperEnemySceneTileId = 19;

	public static TileSet CreateTileSet(
		Texture2D floorTexture,
		Texture2D? alternateFloorTexture,
		PackedScene playerScene,
		PackedScene enemyScene,
		PackedScene obstacleScene,
		PackedScene escapeScene,
		PackedScene facingLeftScene,
		PackedScene facingUpScene,
		PackedScene facingRightScene,
		PackedScene facingDownScene,
		PackedScene scene01TutorialEnemyScene,
		PackedScene pirateBlockerScene,
		PackedScene pirateScoutScene,
		PackedScene pirateShockerScene,
		PackedScene pirateGunnerScene,
		PackedScene piratePipeBomberScene,
		PackedScene pirateBruteEliteScene,
		PackedScene scrapMedicEliteScene,
		PackedScene sewerGatekeeperScene,
		PackedScene obstacleWallScene,
		PackedScene obstacleSlowScene,
		int cellSizePixels)
	{
		TileSet tileSet = new TileSet
		{
			TileSize = new Vector2I(cellSizePixels, cellSizePixels),
		};

		TileSetAtlasSource atlasSource = new TileSetAtlasSource
		{
			Texture = floorTexture,
			TextureRegionSize = new Vector2I(cellSizePixels, cellSizePixels),
		};
		CreateAllAtlasTiles(atlasSource, floorTexture, cellSizePixels);
		tileSet.AddSource(atlasSource, FloorSourceId);

		if (alternateFloorTexture != null)
		{
			TileSetAtlasSource alternateAtlasSource = new TileSetAtlasSource
			{
				Texture = alternateFloorTexture,
				TextureRegionSize = new Vector2I(cellSizePixels, cellSizePixels),
			};
			CreateAllAtlasTiles(alternateAtlasSource, alternateFloorTexture, cellSizePixels);
			tileSet.AddSource(alternateAtlasSource, AlternateFloorSourceId);
		}

		TileSetScenesCollectionSource sceneSource = new();
		sceneSource.CreateSceneTile(playerScene);
		sceneSource.CreateSceneTile(enemyScene, GenericEnemySceneTileId);
		sceneSource.CreateSceneTile(obstacleScene, DestructibleObstacleSceneTileId);
		sceneSource.CreateSceneTile(escapeScene, EscapeSceneTileId);
		sceneSource.CreateSceneTile(facingLeftScene, FacingLeftSceneTileId);
		sceneSource.CreateSceneTile(facingUpScene, FacingUpSceneTileId);
		sceneSource.CreateSceneTile(facingRightScene, FacingRightSceneTileId);
		sceneSource.CreateSceneTile(facingDownScene, FacingDownSceneTileId);
		sceneSource.CreateSceneTile(scene01TutorialEnemyScene, Scene01TutorialEnemySceneTileId);
		sceneSource.CreateSceneTile(pirateBlockerScene, PirateBlockerEnemySceneTileId);
		sceneSource.CreateSceneTile(pirateScoutScene, PirateScoutEnemySceneTileId);
		sceneSource.CreateSceneTile(pirateShockerScene, PirateShockerEnemySceneTileId);
		sceneSource.CreateSceneTile(pirateGunnerScene, PirateGunnerEnemySceneTileId);
		sceneSource.CreateSceneTile(piratePipeBomberScene, PiratePipeBomberEnemySceneTileId);
		sceneSource.CreateSceneTile(pirateBruteEliteScene, PirateBruteEliteEnemySceneTileId);
		sceneSource.CreateSceneTile(scrapMedicEliteScene, ScrapMedicEliteEnemySceneTileId);
		sceneSource.CreateSceneTile(sewerGatekeeperScene, SewerGatekeeperEnemySceneTileId);
		sceneSource.CreateSceneTile(obstacleWallScene, IndestructibleObstacleSceneTileId);
		sceneSource.CreateSceneTile(obstacleSlowScene, SlowObstacleSceneTileId);
		tileSet.AddSource(sceneSource, SceneSourceId);

		return tileSet;
	}

	private static void CreateAllAtlasTiles(TileSetAtlasSource atlasSource, Texture2D floorTexture, int cellSizePixels)
	{
		if (floorTexture == null || cellSizePixels <= 0)
		{
			atlasSource.CreateTile(FloorAtlasCoords);
			return;
		}

		Vector2 textureSize = floorTexture.GetSize();
		int columns = Mathf.Max(1, Mathf.CeilToInt(textureSize.X / cellSizePixels));
		int rows = Mathf.Max(1, Mathf.CeilToInt(textureSize.Y / cellSizePixels));

		for (int y = 0; y < rows; y++)
		{
			for (int x = 0; x < columns; x++)
			{
				atlasSource.CreateTile(new Vector2I(x, y));
			}
		}
	}
}
