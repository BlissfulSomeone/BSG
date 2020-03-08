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
		[Tooltip("The higher the value, the sooner the rocks will spawn.")] [SerializeField] public float GenerationAmount;
		[SerializeField] public Color BackLayerTint;
		[SerializeField] [Tooltip("Which layer the gameplay will take place on, 0 being the layer closest to the camera. This will be internally clamped between [0] and [NumberOfLayers].")] public int PlayableLayer;

		public float TileSize { get { return ChunkWidth / NumberOfColumns; } }
		public float ChunkHeight { get { return TileSize * NumberOfRows; } }
		public int NumberOfTiles { get { return NumberOfColumns * NumberOfRows * NumberOfLayers; } }
		public int ClampedPlayableLayer { get { return Mathf.Clamp(PlayableLayer, 0, NumberOfLayers); } }

		public int GetSubmeshIndex(int tileId, int depth, int variant)
		{
			int submeshIndex = 0;
			for (int i = 0; i < TileData.Length; ++i)
			{
				if (i < tileId)
				{
					submeshIndex += TileData[i].Depth1Materials.Length;
					submeshIndex += TileData[i].Depth2Materials.Length;
					submeshIndex += TileData[i].Depth3Materials.Length;
				}
				else
				{
					if (depth == 1)
						submeshIndex += variant;
					else if (depth == 2)
						submeshIndex += TileData[i].Depth1Materials.Length + variant;
					else if (depth == 3)
						submeshIndex += TileData[i].Depth1Materials.Length + TileData[i].Depth2Materials.Length + variant;
					break;
				}
			}
			return submeshIndex;
		}

		public Material GetMaterial(int tileId, int depth, int variant)
		{
			if (depth == 1)
				return TileData[tileId].Depth1Materials[variant];
			else if (depth == 2)
				return TileData[tileId].Depth2Materials[variant];
			else if (depth == 3)
				return TileData[tileId].Depth3Materials[variant];
			return null;
		}
	}

	[SerializeField] private ChunkSettings mChunkSettings;
	[SerializeField] private int mGenerateChunksAhead;
	[SerializeField] private GameObject mDebrisPrefab;

	public ChunkSettings Settings { get { return mChunkSettings; } }

	private int mChunksSpawned;
	private List<Chunk> mChunks;
	private List<Chunk> Chunks { get { if (mChunks == null) mChunks = new List<Chunk>(); return mChunks; } }

	private void Awake()
	{
		mChunks = new List<Chunk>();
	}

	private void LateUpdate()
	{
		float depth = GameController.Instance.FurthestDepth;
		if (depth + mChunkSettings.ChunkHeight * mGenerateChunksAhead >= mChunksSpawned * mChunkSettings.ChunkHeight)
		{
			CreateChunk(mChunksSpawned == 0);
		}
	}

	public void CreateChunk(bool empty)
	{
		GameObject chunkObject = new GameObject("Chunk " + mChunksSpawned.ToString());

		chunkObject.transform.SetParent(transform);
		chunkObject.transform.Reset();
		chunkObject.transform.position = new Vector3(0.0f, (mChunksSpawned * mChunkSettings.ChunkHeight) * -1, (mChunkSettings.ClampedPlayableLayer * mChunkSettings.TileSize) * -1);

		Chunk chunk = chunkObject.AddComponent<Chunk>();
		chunk.Generate(this, mChunksSpawned, mChunkSettings, empty);
		chunk.OnTileDestroyed += OnTileDestroyed;
		Chunks.Add(chunk);

		++mChunksSpawned;
	}

	private void OnTileDestroyed(Vector3 tilePosition, int tileId, int depth, int variant, ExplosionInstance explosionInstance)
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
					debrisObject.transform.position = spawnPosition;
					debrisObject.transform.localScale = Vector3.one * mChunkSettings.TileSize / SPLITS;

					MeshRenderer debrisMeshRenderer = debrisObject.GetComponent<MeshRenderer>();
					debrisMeshRenderer.material = mChunkSettings.GetMaterial(tileId, depth, variant);
					debrisMeshRenderer.material.color = mChunkSettings.TileData[tileId].TileColor;

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

	public Chunk GetChunk(int index)
	{
		if (index < 0 || index >= mChunks.Count)
			return null;
		return mChunks[index];
	}

	public void Explode(ExplosionInstance explosionInstance)
	{
		foreach (Chunk chunk in Chunks)
		{
			chunk.Explode(explosionInstance);
		}
	}

	public void SetGenerationSettings(float chaos, float amount)
	{
		mChunkSettings.GenerationPerlinSize = chaos;
		mChunkSettings.GenerationAmount = amount;
	}
}
