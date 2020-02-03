using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GameEventNone : IGameEventActions
{
	public string[] OnGetProperties()
	{
		return new string[] { };
	}

	public void OnAction(GameEventData gameEventData)
	{
	}

	public void OnGizmos(GameEventData gameEventData)
	{
	}
}
