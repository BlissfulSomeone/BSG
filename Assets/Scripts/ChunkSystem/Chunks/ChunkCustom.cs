using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCustom : Chunk
{
	public ChunkCustom(Vector2 aPosition, Vector2 aTileSize, Vector2Int aGridSize)
		: base(aPosition, aTileSize, aGridSize)
	{
		CustomChunkData customChunk = Resources.Load<CustomChunkData>("CustomChunks/TestChunk");

		if (customChunk.mReflectedTileData == null || customChunk.mReflectedTileData.Length == 0)
		{
			Debug.LogError("Custom chunk [" + customChunk.name + "] doesn't have any ReflectedTileData.");
		}

		string text = customChunk.mChunkDataAsset.text;
		string[] lines = text.Split('\n');

		for (int y = 0; y < lines.Length; ++y)
		{
			string line = lines[y];
			for (int x = 0; x < line.Length; ++x)
			{
				string s = line[x].ToString();
				if (s == "\r")
					continue;

				int value;
				if (int.TryParse(s, out value) == false)
				{
					Debug.LogError("Non number value [" + s + "] in custom chunk [" + customChunk.name + "] at col: " + x.ToString() + ", row: " + y.ToString());
					continue;
				}

				if (value >= customChunk.mReflectedTileData.Length)
				{
					Debug.LogError("Custom chunk [" + customChunk.name + "] doesn't have a ReflectionTileData for index: " + value.ToString());
					continue;
				}

				SetTile(x, lines.Length - 1 - y, customChunk.mReflectedTileData[value]);
			}
		}
	}
}
