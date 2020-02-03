﻿using System.Collections;
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
	public UIMenu mHealthBarHUDPrefab;
	public UIMenu mGameOverMenuPrefab;
	public Explosion mExplosionPrefab;

	public ChunkController ChunkControllerInstance { get { return mChunkControllerInstance; } }
	public CameraController CameraControllerInstance { get { return mCameraControllerInstance; } }
	public Character PlayerCharacterInstance { get { return mPlayerInstance; } }
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
		mPlayerInstance.transform.position = new Vector3(0.0f, 10.0f, 0.0f);
		mUIControllerInstance.PushMenu(UIController.ELayer.HUD, mDepthMeterHUDPrefab);
		mUIControllerInstance.PushMenu(UIController.ELayer.HUD, mHealthBarHUDPrefab);

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
			explosion.ExplosionData = new ExplosionData(ray.GetPoint(distance), 2.5f, 0.0f, true);
			explosion.transform.position = ray.GetPoint(distance);	// Set explosion source.
			explosion.transform.localScale = Vector3.one * 2.5f;	// Set explosion size. localScale = explosion radius :)
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
        cameraTargetPosition.y += mPlayerInstance.Velocity.y * mCameraControllerInstance.CameraLookAhead;
		mCameraControllerInstance.SetTargetPosition(cameraTargetPosition);
	}

	public void Explode(ExplosionData explosionData)
	{
		mChunkControllerInstance.Explode(explosionData);
		Collider[] colliders = Physics.OverlapSphere(explosionData.Position, explosionData.Radius);
		foreach (Collider collider in colliders)
		{
			Triggerable triggerable = collider.gameObject.GetComponent<Triggerable>();
			if (triggerable != null)
			{
				triggerable.Trigger(explosionData);
				if (triggerable.HasPhysics == true)
				{
					Vector3 delta = collider.transform.position - explosionData.Position;
					float distance = delta.magnitude;
					if (distance <= explosionData.Radius)
					{
						triggerable.FakePhysics.AddExplosionForce(10.0f, explosionData.Position, explosionData.Radius, 1.0f);
					}
				}
			}
		}
	}
}
