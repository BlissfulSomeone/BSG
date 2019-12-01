using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveBombsEvent : GameEventBase
{
	private enum ERemoveBombAction
	{
		RemoveSilently,
		Explode,
	}
	[SerializeField] private ERemoveBombAction removeBombAction;

	protected override void Execute_Internal()
	{
		Bomb[] bombs = FindObjectsOfType<Bomb>();
		foreach (Bomb bomb in bombs)
		{
			switch (removeBombAction)
			{
				case ERemoveBombAction.RemoveSilently:
					Destroy(bomb.gameObject);
					break;

				case ERemoveBombAction.Explode:
					bomb.Explode();
					break;
			}
		}
	}
}
