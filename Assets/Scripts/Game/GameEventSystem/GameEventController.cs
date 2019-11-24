using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameEventController : MonoBehaviour
{
	private float mDepthOffset;
	public float DepthOffset { get { return mDepthOffset; } set { mDepthOffset = value; } }

	[SerializeField] private List<GameEventBase> mGameEvents;
	private List<GameEventBase> GameEvents
	{
		get
		{
			if (mGameEvents == null)
			{
				mGameEvents = new List<GameEventBase>();
			}
			return mGameEvents;
		}
	}

	[MenuItem("GameObject/Bomb Survival Game/Game Event Controller", false, -100)]
	public static void CreateGameEventController(MenuCommand aMenuCommand)
	{
		GameObject gameEventControllerObject = new GameObject("Game Event Controller");
		GameObjectUtility.SetParentAndAlign(gameEventControllerObject, Selection.activeTransform?.gameObject);
		gameEventControllerObject.transform.Reset(true);
		gameEventControllerObject.AddComponent<GameEventController>();
	}

	private void Awake()
	{
		FetchEvents();
	}

	public void FetchEvents()
	{
		if (GameEvents.Count == 0 && transform.childCount > 0)
		{
			GameEventBase[] gameEvents = GetComponentsInChildren<GameEventBase>();
			GameEvents.AddRange(gameEvents);
		}
	}

	public void AddEvent(GameEventBase aGameEvent)
	{
		GameEvents.Add(aGameEvent);
	}

	public void ClearEvents()
	{
		GameEvents.Clear();
	}

	private void Update()
	{
		float depth = GameController.Instance.FurthestDepth - DepthOffset;
		for (int i = 0; i < GameEvents.Count; ++i)
		{
			GameEventBase gameEvent = GameEvents[i];
			if (gameEvent.Evaluate(depth) == true)
			{
				if (gameEvent.HasBeenTriggered == false)
				{
					gameEvent.Execute();
				}
			}
		}
		for (int i = GameEvents.Count - 1; i >= 0; --i)
		{
			GameEventBase gameEvent = GameEvents[i];
			if (gameEvent.HasBeenTriggered == true)
			{
				GameEvents.Remove(gameEvent);
			}
		}
	}
}
