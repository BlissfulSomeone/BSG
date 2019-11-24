using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagedBombPattern : GameEventBase
{
	private StagedBombSpawner[] bombSpawners;

	private void Awake()
	{
		bombSpawners = GetComponentsInChildren<StagedBombSpawner>();
		foreach (StagedBombSpawner bombSpawner in bombSpawners)
		{
			bombSpawner.gameObject.SetActive(false);
		}
	}

	protected override void Execute_Internal()
	{
		foreach (StagedBombSpawner bombSpawner in bombSpawners)
		{
			bombSpawner.gameObject.SetActive(true);
		}
	}
}
