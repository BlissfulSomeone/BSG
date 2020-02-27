using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
	[System.Serializable]
	public struct ChunkSettings
	{
		[SerializeField] public float ChunkWidth;
		[SerializeField] public int NumberOfColumns;
		[SerializeField] public int NumberOfRows;
		[SerializeField] public int NumberOfLayers;
		[SerializeField] public TileData[] TileData;
		[Tooltip("Lower value will create bigger, smoother veins of rocks. Higher value will create a more chaotic, noisy distribution of rocks.")] [SerializeField] public float GenerationPerlinSize;
		[Tooltip("The higher the value, the sooner the rocks will spawn.")] [SerializeField] public float GenerationRampSpeed;
		[SerializeField] public Color BackLayerTint;

		public float TileSize { get { return ChunkWidth / NumberOfColumns; } }
		public float ChunkHeight { get { return TileSize * NumberOfRows; } }
		public int NumberOfTiles { get { return NumberOfColumns * NumberOfRows * NumberOfLayers; } }
	}

	[SerializeField] private ChunkSettings mChunkSettings;
	[SerializeField] private int mGenerateChunksAhead;
	[SerializeField] private GameObject mDebrisPrefab;

	public ChunkSettings Settings { get { return mChunkSettings; } }

	private int mChunksSpawned;
	private List<Chunk> mChunks;
	private List<Chunk> Chunks { get { if (mChunks == null) mChunks = new List<Chunk>(); return mChunks; } }

	private void Update()
	{
		float depth = GameController.Instance.FurthestDepth;
		if (depth + mChunkSettings.ChunkHeight * mGenerateChunksAhead >= mChunksSpawned * mChunkSettings.ChunkHeight)
		{
			CreateChunk(false);
		}
	}

	public void CreateChunk(bool empty)
	{
		GameObject chunkObject = new GameObject("Chunk");

		chunkObject.transform.SetParent(transform);
		chunkObject.transform.Reset();
		chunkObject.transform.position = new Vector2(0.0f, (mChunksSpawned * mChunkSettings.ChunkHeight) * -1).ToVec3();

		Chunk chunk = chunkObject.AddComponent<Chunk>();
		chunk.Generate(mChunkSettings, empty);
		chunk.OnTileDestroyed += OnTileDestroyed;
		Chunks.Add(chunk);

		++mChunksSpawned;
	}

	private void OnTileDestroyed(Vector3 tilePosition, int tileId, ExplosionInstance explosionInstance)
	{
		const int SPLITS = 2;
		for (int x = 0; x < SPLITS; ++x)
		{
			float positionX = tilePosition.x - (SPLITS == 1 ? 0 : (mChunkSettings.TileSize / (SPLITS * 2) - x * mChunkSettings.TileSize / SPLITS));
			for (int y = 0; y < SPLITS; ++y)
			{
				float positionY = tilePosition.y - (SPLITS == 1 ? 0 : (mChunkSettings.TileSize / (SPLITS * 2) - y * mChunkSettings.TileSize / SPLITS));
				for (int z = 0; z < SPLITS; ++z)
				{
					float positionZ = tilePosition.z - (SPLITS == 1 ? 0 : (mChunkSettings.TileSize / (SPLITS * 2) - z * mChunkSettings.TileSize / SPLITS));

					Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

					GameObject debrisObject = Instantiate(mDebrisPrefab);
					debrisObject.transform.Reset();
					debrisObject.transform.position = spawnPosition;
					debrisObject.transform.localScale = Vector3.one * mChunkSettings.TileSize / SPLITS;

					MeshRenderer debrisMeshRenderer = debrisObject.GetComponent<MeshRenderer>();
					debrisMeshRenderer.material = mChunkSettings.TileData[tileId].Material;

					Rigidbody debrisRigidbody = debrisObject.GetComponent<Rigidbody>();
					Vector3 alignedExplosionSource = new Vector3(explosionInstance.Position.x, explosionInstance.Position.y, spawnPosition.z);
					Vector3 delta = spawnPosition - alignedExplosionSource;
					float distance = delta.magnitude;
					float falloff = 1.0f - (distance / explosionInstance.ExplosionData.Radius);
					float force = 10.0f * falloff * Random.Range(0.25f, 1.0f);
					float forceZ = Random.Range(force / 2, -force / 2);
					debrisRigidbody.velocity += delta.normalized * force + Vector3.up * force + Vector3.forward * forceZ;
					debrisRigidbody.angularVelocity += Random.onUnitSphere * force;

					Destroy(debrisObject, 3.0f);
				}
			}
		}
	}

	public void Explode(ExplosionInstance explosionInstance)
	{
		foreach (Chunk chunk in Chunks)
		{
			chunk.Explode(explosionInstance);
		}
	}
}
