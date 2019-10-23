using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
	private TileData mTileData;

	private Vector2Int mPosition;
	public Vector2Int Position { get { return mPosition; } set { mPosition = value; } }

	public Tile(TileData aTileData, Vector2Int aPosition)
	{
		mTileData = aTileData;
		mPosition = aPosition;
	}
}
