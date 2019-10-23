using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkDirt : Chunk
{
	public ChunkDirt(Vector2 aPosition, Vector2 aTileSize, Vector2Int aGridSize)
		: base(aPosition, aTileSize, aGridSize)
	{
		for (int y = 0; y < mGridSize.y; ++y)
		{
			for (int x = 0; x < mGridSize.x; ++x)
			{
				SetTile(x, y, (x == 0 || x == mGridSize.x - 1) ? mMetalTileData : mDirtTileData);
			}
		}
	}
}
