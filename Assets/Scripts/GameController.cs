using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	[SerializeField] protected ChunkController mChunkControllerPrefab;
	[SerializeField] protected CameraController mCameraControllerPrefab;
	[SerializeField] protected Character mPlayerPrefab;

	protected ChunkController mChunkControllerInstance;
	protected CameraController mCameraControllerInstance;
	protected Character mPlayerInstance;

	private const int CHUNK_HEIGHT = 10;
	private float mFurthestDepth = 0.0f;
	private int mChunksSpawned = 0;

	private void Awake()
	{
		mChunkControllerInstance = Instantiate(mChunkControllerPrefab, Vector3.zero, Quaternion.identity);
		mCameraControllerInstance = Instantiate(mCameraControllerPrefab, new Vector3(0.0f, 0.0f, 10.0f), Quaternion.identity);
		mPlayerInstance = Instantiate(mPlayerPrefab, Vector3.zero, Quaternion.identity);
	}
	
	private void Update()
	{
		mCameraControllerInstance.SetTargetPosition(mPlayerInstance.transform.position);

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
}
