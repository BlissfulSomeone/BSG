using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventNested : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { "NestedEvent" };
	}

	public void OnAction(GameEventData gameEventData)
	{
		GameObject go = new GameObject("Nested Game Event Controller");
		go.transform.SetParent(GameController.Instance.transform);
		go.transform.Reset();
		GameEventController nestedGameEventController = go.AddComponent<GameEventController>();
		nestedGameEventController.GameEventRoot = Object.Instantiate(gameEventData.NestedEvent);
		nestedGameEventController.DepthOffset = gameEventData.TriggerAtDepth;
	}

	public void OnGizmos(GameEventData gameEventData)
	{
	}
}
