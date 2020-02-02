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

	public ChunkController ChunkControllerInstance { get { return mChunkControllerInstance; } }
	public CameraController CameraControllerInstance { get { return mCameraControllerInstance; } }
	public InfiniteBombSpawner BombSpawnerInstance { get { return mInfiniteBombSpawnerInstance; } }
	
	private float mFurthestDepth = 0.0f;
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

		mChunkControllerInstance.CreateChunk(true);
		mChunkControllerInstance.CreateChunk(false);
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
	
	private void Update()
	{
		if (mPlayerInstance != null)
		{
			mFurthestDepth = Mathf.Max(mFurthestDepth, -mPlayerInstance.transform.position.y);
			UpdateCamera();
		}

		if (Input.GetMouseButtonDown(0) == true)
		{
			//Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Plane plane = new Plane(Vector3.back, 0.0f);
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			float distance;
			plane.Raycast(ray, out distance);
			Explosion explosion = Instantiate(mExplosionPrefab);
			explosion.transform.position = ray.GetPoint(distance);	// Set explosion source.
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

	public void Explode(Vector3 explosionSource, float explosionRadius)
	{
		mChunkControllerInstance.Explode(explosionSource, explosionRadius);
		Collider[] colliders = Physics.OverlapSphere(explosionSource, explosionRadius);
		foreach (Collider collider in colliders)
		{
			Triggerable triggerable = collider.gameObject.GetComponent<Triggerable>();
			if (triggerable != null)
			{
				triggerable.Trigger(explosionSource, explosionRadius);
				if (triggerable.HasPhysics == true)
				{
					Vector3 delta = collider.transform.position - explosionSource;
					float distance = delta.magnitude;
					if (distance <= explosionRadius)
					{
						triggerable.FakePhysics.AddExplosionForce(10.0f, explosionSource, explosionRadius, 1.0f);
					}
				}
			}
		}
	}
}
