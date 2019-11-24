using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestedGameEvent : GameEventBase
{
	[SerializeField] private GameEventController mGameEventControllerPrefab;

	protected override void Execute_Internal()
	{
		GameEventController gameEventController = Instantiate(mGameEventControllerPrefab);
		gameEventController.transform.Reset();
		gameEventController.DepthOffset = depthToTrigger;
	}
}
