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

	private void Start()
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

	private void OnTileDestroyed(Chunk.TileInstance aTileInstance, Vector3 aTilePosition)
	{
		Transform debrisTransform = Instantiate(mDebrisPrefab, aTilePosition, Quaternion.identity);

		SpriteRenderer debrisSpriteRenderer = debrisTransform.GetComponent<SpriteRenderer>();
		debrisSpriteRenderer.sprite = aTileInstance.CurrentSprite;

		Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Rigidbody debrisRigidbody = debrisTransform.GetComponent<Rigidbody>();
		debrisRigidbody.AddExplosionForce(10.0f, mouseWorldPosition, 3.0f, 1.0f, ForceMode.VelocityChange);

		Destroy(debrisTransform.gameObject, 3.0f);
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) == true)
		{
			Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mChunkGenerator.Explode(mouseWorldPosition, 3.0f);
		}
		if (Input.GetMouseButtonDown(1) == true)
		{
			Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Rigidbody testObject = Instantiate(mTestObject, mouseWorldPosition, Quaternion.identity);
			testObject.AddForce(Random.Range(-8.0f, 8.0f), Random.Range(-8.0f, 8.0f), 0.0f, ForceMode.VelocityChange);
		}

		RenderQueue renderQueue = new RenderQueue();
		mChunkGenerator.Render(renderQueue);
		mChunkRenderer.Render(renderQueue);
	}

	public void CreateChunk(Chunk aChunk)
	{
		mChunkGenerator.AddChunk(aChunk);
	}
}
