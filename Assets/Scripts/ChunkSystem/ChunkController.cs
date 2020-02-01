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
		[SerializeField] public TileData[] TileData;

		public float TileSize { get { return ChunkWidth / NumberOfColumns; } }
		public float ChunkHeight { get { return TileSize * NumberOfRows; } }
		public int NumberOfTiles { get { return NumberOfColumns * NumberOfRows; } }
	}

	[SerializeField] private ChunkSettings mChunkSettings;
	[SerializeField] private int mGenerateChunksAhead;
	[SerializeField] private GameObject mDebrisPrefab;

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
		chunkObject.transform.position = new Vector2(0.0f, (mChunkSettings.ChunkHeight / 2 + mChunksSpawned * mChunkSettings.ChunkHeight) * -1).ToVec3();

		Chunk chunk = chunkObject.AddComponent<Chunk>();
		chunk.Generate(mChunkSettings, empty);
		chunk.OnTileDestroyed += OnTileDestroyed;
		Chunks.Add(chunk);

		++mChunksSpawned;
	}

	private void OnTileDestroyed(Vector3 tilePosition, int tileId, Vector3 explosionSource, float explosionRadius)
	{
		for (int x = 0; x < 2; ++x)
		{
			float positionX = tilePosition.x - mChunkSettings.TileSize / 4 + x * mChunkSettings.TileSize / 2;
			for (int y = 0; y < 2; ++y)
			{
				float positionY = tilePosition.y - mChunkSettings.TileSize / 4 + y * mChunkSettings.TileSize / 2;
				for (int z = 0; z < 2; ++z)
				{
					float positionZ = tilePosition.z - mChunkSettings.TileSize / 4 + z * mChunkSettings.TileSize / 2;

					Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

					GameObject debrisObject = Instantiate(mDebrisPrefab);
					debrisObject.transform.Reset();
					debrisObject.transform.position = spawnPosition;
					debrisObject.transform.localScale = Vector3.one * mChunkSettings.TileSize / 2;

					MeshRenderer debrisMeshRenderer = debrisObject.GetComponent<MeshRenderer>();
					debrisMeshRenderer.material = mChunkSettings.TileData[tileId].Material;

					Rigidbody debrisRigidbody = debrisObject.GetComponent<Rigidbody>();
					Vector3 delta = spawnPosition - explosionSource;
					float distance = delta.magnitude;
					float falloff = 1.0f - (distance / explosionRadius);
					float force = 10.0f * falloff;
					float forceZ = Random.Range(force / 2, -force / 2);
					debrisRigidbody.velocity += delta.normalized * force + Vector3.up * force + Vector3.forward * forceZ;
					debrisRigidbody.angularVelocity += Random.onUnitSphere * force;

					Destroy(debrisObject, 3.0f);
				}
			}
		}

		//GameObject debrisObject = Instantiate(mDebrisPrefab);
		//debrisObject.transform.Reset();
		//debrisObject.transform.position = tilePosition;
		//debrisObject.transform.localScale = Vector3.one * mChunkSettings.TileSize;
		//
		//MeshRenderer debrisMeshRenderer = debrisObject.GetComponent<MeshRenderer>();
		//debrisMeshRenderer.material = mChunkSettings.TileData[tileId].Material;
		//
		//Rigidbody debrisRigidbody = debrisObject.GetComponent<Rigidbody>();
		////debrisRigidbody.AddExplosionForce(15.0f, explosionSource, explosionRadius, 1.0f, ForceMode.VelocityChange);
		//Vector3 delta = tilePosition - explosionSource;
		//float distance = delta.magnitude;
		//float falloff = 1.0f - (distance / explosionRadius);
		//float force = 10.0f * falloff;
		//debrisRigidbody.velocity += delta.normalized * force + Vector3.up * force + Vector3.forward * Random.Range(force / 2, -force / 2);
		//
		//Destroy(debrisObject, 3.0f);
	}

	public void Explode(Vector2 explosionSource, float explosionRadius)
	{
		foreach (Chunk chunk in Chunks)
		{
			chunk.Explode(explosionSource, explosionRadius);
		}
	}

	//[SerializeField] private Mesh mRenderMesh;
	//[SerializeField] private Material mRenderMaterial;
	//
	//[SerializeField] private Transform mDebrisPrefab;
	//
	//private ChunkGenerator mChunkGenerator;
	//private ChunkRenderer mChunkRenderer;
	//
	//private void Awake()
	//{
	//	mChunkGenerator = new ChunkGenerator();
	//	mChunkGenerator.OnChunkCreated += OnChunkCreated;
	//	mChunkGenerator.Generate();
	//
	//	mChunkRenderer = new ChunkRenderer();
	//	mChunkRenderer.Initialize(mRenderMesh, mRenderMaterial);
	//}
	//
	//private void OnDestroy()
	//{
	//	if (mChunkGenerator != null)
	//		mChunkGenerator.OnChunkCreated -= OnChunkCreated;
	//}
	//
	//private void OnChunkCreated(Chunk aChunk)
	//{
	//	GameObject colliderObject = new GameObject("Chunk Collider");
	//
	//	colliderObject.transform.SetParent(transform);
	//	colliderObject.transform.position = Vector3.zero;
	//	colliderObject.transform.rotation = Quaternion.identity;
	//	colliderObject.transform.localScale = Vector3.one;
	//
	//	ChunkCollider chunkCollider = colliderObject.AddComponent<ChunkCollider>();
	//	chunkCollider.AttachToChunk(aChunk);
	//
	//	aChunk.OnTileDestroyed += OnTileDestroyed;
	//}
	//
	//private void OnTileDestroyed(Chunk.TileInstance aTileInstance, Vector3 aTilePosition, Chunk.TileDestroyData aTileDestroyData)
	//{
	//	Transform debrisTransform = Instantiate(mDebrisPrefab, aTilePosition, Quaternion.identity);
	//
	//	SpriteRenderer debrisSpriteRenderer = debrisTransform.GetComponent<SpriteRenderer>();
	//	debrisSpriteRenderer.sprite = aTileInstance.CurrentSprite;
	//	
	//	Rigidbody debrisRigidbody = debrisTransform.GetComponent<Rigidbody>();
	//	debrisRigidbody.AddExplosionForce(15.0f, aTileDestroyData.destroySource, aTileDestroyData.destoryStrength, 1.0f, ForceMode.VelocityChange);
	//
	//	Destroy(debrisTransform.gameObject, 3.0f);
	//}
	//
	//private void Update()
	//{
	//	RenderQueue renderQueue = new RenderQueue();
	//	mChunkGenerator.Render(renderQueue);
	//	mChunkRenderer.Render(renderQueue);
	//}
	//
	//public void CreateChunk(Chunk aChunk)
	//{
	//	mChunkGenerator.AddChunk(aChunk);
	//}
	//
	//public void Explode(Vector2 aExplosionSource, float aExplosionRadius)
	//{
	//	mChunkGenerator.Explode(aExplosionSource, aExplosionRadius);
	//}
}
