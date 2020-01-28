using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameEventController : MonoBehaviour
{
	private int mDepthOffset = 0;
	public int DepthOffset { get { return mDepthOffset; } set { mDepthOffset = value; } }

	[SerializeField] private GameEvent mGameEventRoot;
	public GameEvent GameEventRoot { get { return mGameEventRoot; } set { mGameEventRoot = value; } }

	private void Start()
	{
		for (int i = 0; i < mGameEventRoot.Count; ++i)
		{
			GameEventData gameEvent = mGameEventRoot.GetGameEvent(i);
			gameEvent.TriggerAtDepth += DepthOffset;
			mGameEventRoot.UpdateGameEvent(gameEvent, i);
		}
	}

	private void Update()
	{
		if (mGameEventRoot == null || transform.parent == null)
			return;

		float depth = GameController.Instance.FurthestDepth;
		for (int i = 0; i < mGameEventRoot.Count; ++i)
		{
			GameEventData gameEvent = mGameEventRoot.GetGameEvent(i);
			if (gameEvent.TriggerAtDepth <= depth)
			{
				if (gameEvent.HasBeenTriggered == false)
				{
					GameEventCore.GameEventActions[(int)gameEvent.EventType].OnAction(gameEvent);
					gameEvent.HasBeenTriggered = true;
					mGameEventRoot.UpdateGameEvent(gameEvent, i);
				}
			}
		}
	}
}
