using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBombPattern : GameEventBase
{
	[SerializeField] private float bombsPerSecond;
	[SerializeField] private Bomb[] bombPrefabs;

	protected override void Execute_Internal()
	{
		GameController.Instance.BombSpawnerInstance.SetAllowedBombs(bombPrefabs);
		GameController.Instance.BombSpawnerInstance.SetBPS(bombsPerSecond);
	}
}
