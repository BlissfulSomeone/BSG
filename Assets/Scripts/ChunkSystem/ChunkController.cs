using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
	[SerializeField] private Mesh mRenderMesh;
	[SerializeField] private Material mRenderMaterial;

	[SerializeField] private Transform mDebrisPrefab;

	[SerializeField] private Rigidbody mTestObject;

	private ChunkGenerator mChunkGenerator;
	private ChunkRenderer mChunkRenderer;

	private void Awake()
	{
		mChunkGenerator = new ChunkGenerator();
		mChunkGenerator.OnChunkCreated += OnChunkCreated;
		mChunkGenerator.Generate();

		mChunkRenderer = new ChunkRenderer();
		mChunkRenderer.Initialize(mRenderMesh, mRenderMaterial);
	}

	private void OnDestroy()
	{
		if (mChunkGenerator != null)
			mChunkGenerator.OnChunkCreated -= OnChunkCreated;
	}

	private void OnChunkCreated(Chunk aChunk)
	{
		GameObject colliderObject = new GameObject("Chunk Collider");

		colliderObject.transform.SetParent(transform);
		colliderObject.transform.position = Vector3.zero;
		colliderObject.transform.rotation = Quaternion.identity;
		colliderObject.transform.localScale = Vector3.one;

		ChunkCollider chunkCollider = colliderObject.AddComponent<ChunkCollider>();
		chunkCollider.AttachToChunk(aChunk);

		aChunk.OnTileDestroyed += OnTileDestroyed;
	}

	private void OnTileDestroyed(Chunk.TileInstance aTileInstance, Vector3 aTilePosition, Chunk.TileDestroyData aTileDestroyData)
	{
		Transform debrisTransform = Instantiate(mDebrisPrefab, aTilePosition, Quaternion.identity);

		SpriteRenderer debrisSpriteRenderer = debrisTransform.GetComponent<SpriteRenderer>();
		debrisSpriteRenderer.sprite = aTileInstance.CurrentSprite;
		
		Rigidbody debrisRigidbody = debrisTransform.GetComponent<Rigidbody>();
		debrisRigidbody.AddExplosionForce(15.0f, aTileDestroyData.destroySource, aTileDestroyData.destoryStrength, 1.0f, ForceMode.VelocityChange);

		Destroy(debrisTransform.gameObject, 3.0f);
	}

	private void Update()
	{
		RenderQueue renderQueue = new RenderQueue();
		mChunkGenerator.Render(renderQueue);
		mChunkRenderer.Render(renderQueue);
	}

	public void CreateChunk(Chunk aChunk)
	{
		mChunkGenerator.AddChunk(aChunk);
	}

	public void Explode(Vector2 aExplosionSource, float aExplosionRadius)
	{
		mChunkGenerator.Explode(aExplosionSource, aExplosionRadius);
	}
}
