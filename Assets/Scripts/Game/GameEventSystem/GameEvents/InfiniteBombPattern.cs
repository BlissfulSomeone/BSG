using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBombPattern : GameEventBase
{
	[SerializeField] private float bombsPerSecond;
	[SerializeField] private Bomb[] bombPrefabs;

	private Bomb[] BombPrefabs { get { if (bombPrefabs == null) bombPrefabs = new Bomb[0]; return bombPrefabs; } }

	protected override void Execute_Internal()
	{
		GameController.Instance.BombSpawnerInstance.SetAllowedBombs(bombPrefabs);
		GameController.Instance.BombSpawnerInstance.SetBPS(bombsPerSecond);
	}

	public void AddBombPrefab(Bomb aBombPrefab)
	{
		System.Array.Resize(ref bombPrefabs, BombPrefabs.Length + 1);
		bombPrefabs[bombPrefabs.Length - 1] = aBombPrefab;
	}
}
