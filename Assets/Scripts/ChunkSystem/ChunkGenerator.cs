using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator
{
	public delegate void OnChunkCreatedHandler(Chunk aChunk);
	public OnChunkCreatedHandler OnChunkCreated;

	private List<Chunk> mChunks;

	public void Generate()
	{
		mChunks = new List<Chunk>();

		AddChunk(new ChunkEmpty(new Vector2(-9.5f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2Int(20, 10)));
		AddChunk(new ChunkDirt(new Vector2(-9.5f, -10.0f), new Vector2(1.0f, 1.0f), new Vector2Int(20, 10)));
		AddChunk(new ChunkDirt(new Vector2(-9.5f, -20.0f), new Vector2(1.0f, 1.0f), new Vector2Int(20, 10)));
		AddChunk(new ChunkDirt(new Vector2(-9.5f, -30.0f), new Vector2(1.0f, 1.0f), new Vector2Int(20, 10)));
	}

	private void AddChunk(Chunk aChunk)
	{
		mChunks.Add(aChunk);
		OnChunkCreated?.Invoke(aChunk);
	}

	private void AddChunk(Vector2 aPosition, Vector2 aTileSize, Vector2Int aGridSize)
	{
		Chunk chunk = new Chunk(aPosition, aTileSize, aGridSize);
		mChunks.Add(chunk);
		OnChunkCreated?.Invoke(chunk);
	}

	public void Explode(Vector2 aExplosionPoint, float aExplosionRadius)
	{
		foreach (Chunk chunk in mChunks)
		{
			chunk.Explode(aExplosionPoint, aExplosionRadius);
		}
	}

	public void Render(RenderQueue aRenderQueue)
	{
		foreach (Chunk chunk in mChunks)
		{
			chunk.Render(aRenderQueue);
		}
	}
}
