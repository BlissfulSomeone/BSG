using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	private static GameController mInstance = null;
	public static GameController Instance { get { return mInstance; } }

	[SerializeField] protected ChunkController mChunkControllerPrefab;
	[SerializeField] protected CameraController mCameraControllerPrefab;
	[SerializeField] protected Character mPlayerPrefab;

	protected ChunkController mChunkControllerInstance;
	protected CameraController mCameraControllerInstance;
	protected BombController mBombControllerInstance;
	protected Character mPlayerInstance;

	public Bomb mBombPrefab;

	public CameraController CameraControllerInstance { get { return mCameraControllerInstance; } }
	public BombController BombControllerInstance { get { return mBombControllerInstance; } }

	private const int CHUNK_HEIGHT = 10;
	private float mFurthestDepth = 0.0f;
	private int mChunksSpawned = 0;

	private void Awake()
	{
		if (mInstance != null)
		{
			Destroy(gameObject);
			return;
		}
		mInstance = this;

		mChunkControllerInstance = Instantiate(mChunkControllerPrefab, Vector3.zero, Quaternion.identity);
		mCameraControllerInstance = Instantiate(mCameraControllerPrefab, new Vector3(0.0f, 0.0f, 10.0f), Quaternion.identity);
		mPlayerInstance = Instantiate(mPlayerPrefab, Vector3.zero, Quaternion.identity);

		GameObject bombControllerObject = new GameObject("Bomb Controller");
		bombControllerObject.transform.SetParent(transform);
		mBombControllerInstance = bombControllerObject.AddComponent<BombController>();
	}
	
	private void Update()
	{
		UpdateCamera();
		UpdateChunks();

		if (Input.GetMouseButtonDown(0) == true)
		{
			Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Explode(mouseWorldPosition, 3.0f);
		}
		if (Input.GetMouseButtonDown(1) == true)
		{
			Bomb bomb = Instantiate(mBombPrefab);
			bomb.transform.position = new Vector2(Random.Range(-8.0f, 8.0f), CHUNK_HEIGHT);
			bomb.Init(Random.Range(0, 2) == 0);
			mBombControllerInstance.SpawnBomb(bomb);
		}
	}

	private void UpdateCamera()
	{
		Vector3 cameraTargetPosition = mPlayerInstance.transform.position;
		cameraTargetPosition.x = 0.0f;
		mCameraControllerInstance.SetTargetPosition(cameraTargetPosition);
	}

	private void UpdateChunks()
	{
		mFurthestDepth = Mathf.Max(mFurthestDepth, -mPlayerInstance.transform.position.y);

		if (mFurthestDepth + CHUNK_HEIGHT * 3 >= mChunksSpawned * CHUNK_HEIGHT)
		{
			if (mChunksSpawned == 0)
				mChunkControllerInstance.CreateChunk(new ChunkEmpty(new Vector2(-9.5f, -mChunksSpawned * CHUNK_HEIGHT), new Vector2(1.0f, 1.0f), new Vector2Int(20, CHUNK_HEIGHT)));
			else
				mChunkControllerInstance.CreateChunk(new ChunkDirt(new Vector2(-9.5f, -mChunksSpawned * CHUNK_HEIGHT), new Vector2(1.0f, 1.0f), new Vector2Int(20, CHUNK_HEIGHT)));

			++mChunksSpawned;
		}
	}

	public void Explode(Vector2 aExplosionSource, float aExplosionRadius)
	{
		mChunkControllerInstance.Explode(aExplosionSource, aExplosionRadius);
		mBombControllerInstance.Explode(aExplosionSource, aExplosionRadius);
	}
}
