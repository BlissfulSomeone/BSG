using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
	public struct TileInstance
	{
		private TileData tileData;
		private Sprite currentSprite;
		private int health;

		public TileData TileData { get { return tileData; } }
		public Sprite CurrentSprite { get { return currentSprite; } }

		public TileInstance(TileData aTileData)
		{
			tileData = aTileData;
			currentSprite = tileData.sprites[tileData.sprites.Length - 1];
			health = tileData.sprites.Length - 1;
		}

		public bool Damage(int aDamage)
		{
			if (tileData.indestructible == true)
				return false;

			health -= aDamage;
			if (health >= 0)
			{
				currentSprite = tileData.sprites[health];
				return false;
			}
			currentSprite = tileData.sprites[0];
			return true;
		}
	}

	public delegate void OnTileDestroyedHandler(TileInstance aTileInstance, Vector3 aTilePosition);
	public OnTileDestroyedHandler OnTileDestroyed;

	private TileInstance[] mTiles;

	private Vector2 mPosition;
	private Vector2 mTileSize;
	protected Vector2Int mGridSize;
	public Vector2 Position { get { return mPosition; } }
	public Vector2 TileSize { get { return mTileSize; } }
	public Vector2Int GridSize { get { return mGridSize; } }

	protected TileData mEmptyTileData;
	protected TileData mMetalTileData;
	protected TileData mDirtTileData;
	protected TileData mRocksTileData;

	private Sprite mDebugSprite;

	public Chunk(Vector2 aPosition, Vector2 aTileSize, Vector2Int aGridSize)
	{
		mEmptyTileData = Resources.Load<TileData>("Tiles/TileVoid");
		mMetalTileData = Resources.Load<TileData>("Tiles/TileMetal");
		mDirtTileData = Resources.Load<TileData>("Tiles/TileDirt");
		mRocksTileData = Resources.Load<TileData>("Tiles/TileRocks");

		mDebugSprite = Resources.Load<Sprite>("Textures/Sprites/SprDebugPoint");

		mPosition = aPosition;
		mTileSize = aTileSize;
		mGridSize = aGridSize;
		mTiles = new TileInstance[mGridSize.x * mGridSize.y];
		for (int y = 0; y < mGridSize.y; ++y)
		{
			for (int x = 0; x < mGridSize.x; ++x)
			{
				SetTile(x, y, (x == 0 || x == mGridSize.x - 1) ? mMetalTileData : (Random.Range(0, 4) == 0 ? mRocksTileData : mDirtTileData));
			}
		}
	}

	protected void SetTile(int aX, int aY, TileData aTileData)
	{
		mTiles[aX + aY * mGridSize.x] = new TileInstance(aTileData);
	}

	public TileInstance GetTile(int aX, int aY)
	{
		return mTiles[aX + aY * mGridSize.x];
	}

	protected ref TileInstance GetTileReference(int aX, int aY)
	{
		return ref mTiles[aX + aY * mGridSize.x];
	}

	public bool IsCollision(int aX, int aY)
	{
		if (aX < 0 || aX >= mGridSize.x || aY < 0 || aY >= mGridSize.y)
			return false;

		return GetTile(aX, aY).TileData.hasCollision;
	}

	public void Explode(Vector2 aExplosionPoint, float aExplosionRadius)
	{
		for (int y = 0; y < mGridSize.y; ++y)
		{
			for (int x = 0; x < mGridSize.x; ++x)
			{
				Vector2 tilePosition = new Vector2(
					mPosition.x + x * mTileSize.x + mTileSize.x * 0.5f,
					mPosition.y + y * mTileSize.y + mTileSize.y * 0.5f);

				if (Vector2.Distance(tilePosition, aExplosionPoint) < aExplosionRadius)
				{
					if (GetTileReference(x, y).Damage(1) == true)
					{
						TileInstance destroyedTile = GetTile(x, y);
						SetTile(x, y, mEmptyTileData);
						OnTileDestroyed?.Invoke(destroyedTile, tilePosition);
					}
				}
			}
		}
	}

	public void Render(RenderQueue aRenderQueue)
	{
		for (int y = 0; y < mGridSize.y; ++y)
		{
			for (int x = 0; x < mGridSize.x; ++x)
			{
				aRenderQueue.AddSprite(GetTileReference(x, y).CurrentSprite, new Vector3(mPosition.x + x * mTileSize.x, mPosition.y + y * mTileSize.y, 0.0f), Vector3.one);
			}
		}
	}
}
