using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopInfiniteBombPattern : GameEventBase
{
	protected override void Execute_Internal()
	{
		GameController.Instance.BombSpawnerInstance.SetBPS(0.0f);
	}
}
