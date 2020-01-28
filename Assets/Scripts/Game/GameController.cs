using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
	private static GameController mInstance = null;
	public static GameController Instance { get { return mInstance; } }

	[Header("Controllers")]
	[SerializeField] protected ChunkController mChunkControllerPrefab;
	[SerializeField] protected CameraController mCameraControllerPrefab;
	[SerializeField] private UIController mUIControllerPrefab;
	[SerializeField] protected Character mPlayerPrefab;
	[SerializeField] private InfiniteBombSpawner mInfiniteBombSpawnerPrefab;
	[SerializeField] protected GameEvent mGameEventRootPrefab;

	protected ChunkController mChunkControllerInstance;
	protected CameraController mCameraControllerInstance;
	protected UIController mUIControllerInstance;
	protected Character mPlayerInstance;
	protected InfiniteBombSpawner mInfiniteBombSpawnerInstance;
	protected GameEventController mGameEventControllerInstance;
	
	[Header("Temporary stuff")]
	public UIMenu mDepthMeterHUDPrefab;
	public UIMenu mGameOverMenuPrefab;
	public Explosion mExplosionPrefab;

	public CameraController CameraControllerInstance { get { return mCameraControllerInstance; } }
	public InfiniteBombSpawner BombSpawnerInstance { get { return mInfiniteBombSpawnerInstance; } }

	private const int CHUNK_HEIGHT = 10;
	private float mFurthestDepth = 0.0f;
	private int mChunksSpawned = 0;

	public float FurthestDepth { get { return mFurthestDepth; } }
	
	private void Awake()
	{
		if (mInstance != null)
		{
			Destroy(gameObject);
			return;
		}
		mInstance = this;

		Setup();
	}

	private void Setup()
	{
		mFurthestDepth = 0.0f;
		mChunksSpawned = 0;
		
		mChunkControllerInstance = SpawnControllerPrefab(mChunkControllerPrefab);
		mCameraControllerInstance = SpawnControllerPrefab(mCameraControllerPrefab);
		mUIControllerInstance = SpawnControllerPrefab(mUIControllerPrefab);
		mPlayerInstance = SpawnControllerPrefab(mPlayerPrefab);
		mInfiniteBombSpawnerInstance = SpawnControllerPrefab(mInfiniteBombSpawnerPrefab);

		GameObject go = new GameObject("Game Event Controller");
		go.transform.SetParent(transform);
		go.transform.Reset();
		mGameEventControllerInstance = go.AddComponent<GameEventController>();
		mGameEventControllerInstance.GameEventRoot = Instantiate(mGameEventRootPrefab);
	
		mPlayerInstance.OnKilled += OnPlayerKilled;
		mUIControllerInstance.PushMenu(UIController.ELayer.HUD, mDepthMeterHUDPrefab);
	}

	private T SpawnControllerPrefab<T>(T aControllerPrefab) where T : MonoBehaviour
	{
		T controller = Instantiate(aControllerPrefab);
		controller.transform.SetParent(transform);
		controller.transform.Reset();
		return controller;
	}

	private void OnPlayerKilled(Character aKilledCharacter)
	{
		if (aKilledCharacter != mPlayerInstance)
		{
			Debug.LogError("We only have one character in the game so this REALLY shouldn't happen. But yknow. Safety checks.");
			return;
		}

		aKilledCharacter.OnKilled -= OnPlayerKilled;

		mUIControllerInstance.PushMenu(UIController.ELayer.Menus, mGameOverMenuPrefab);
	}

	private void Start()
	{
		//mChunkControllerInstance.CreateChunk(new ChunkEmpty(new Vector2(-9.5f, 0.0f), Vector2.one, new Vector2Int(20, 10)));
		//mChunkControllerInstance.CreateChunk(new ChunkCustom(new Vector2(-9.5f, -10.0f), Vector2.one, new Vector2Int(20, 10)));
	}

	private void Update()
	{
		if (mPlayerInstance != null)
		{
			UpdateCamera();
			UpdateChunks();
		}

		if (Input.GetMouseButtonDown(0) == true)
		{
			Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Explosion explosion = Instantiate(mExplosionPrefab);
			explosion.transform.position = mouseWorldPosition;		// Set explosion source.
			explosion.transform.localScale = Vector3.one * 2.0f;	// Set explosion size. localScale = explosion radius :)
		}
		if (Input.GetKeyDown(KeyCode.R) == true && mPlayerInstance == null)
		{
			// Super naive way to reset game
			if (mChunkControllerInstance != null) Destroy(mChunkControllerInstance.gameObject);
			if (mCameraControllerInstance != null) Destroy(mCameraControllerInstance.gameObject);
			if (mUIControllerInstance != null) Destroy(mUIControllerInstance.gameObject);
			if (mPlayerInstance != null) Destroy(mPlayerInstance.gameObject);
			if (mInfiniteBombSpawnerInstance != null) Destroy(mInfiniteBombSpawnerInstance.gameObject);
			if (mGameEventControllerInstance != null) Destroy(mGameEventControllerInstance.gameObject);
			Globals.DestroyAllOfType<Triggerable>();
			Globals.DestroyAllOfType<Explosion>();
			Globals.DestroyAllOfType<GameEventController>();

			Setup();
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
		Collider[] colliders = Physics.OverlapSphere(aExplosionSource, aExplosionRadius);
		foreach (Collider collider in colliders)
		{
			Triggerable triggerable = collider.gameObject.GetComponent<Triggerable>();
			if (triggerable != null)
			{
				triggerable.Trigger(aExplosionSource, aExplosionRadius);
				if (triggerable.HasPhysics == true)
				{
					Vector2 delta = collider.transform.position.ToVec2() - aExplosionSource;
					float distance = delta.magnitude;
					if (distance <= aExplosionRadius)
					{
						triggerable.FakePhysics.AddExplosionForce(10.0f, aExplosionSource, aExplosionRadius, 1.25f);
					}
				}
			}
		}
	}
}
