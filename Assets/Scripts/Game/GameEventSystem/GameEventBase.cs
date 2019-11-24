using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GameEventBase : MonoBehaviour
{
	[SerializeField] protected float depthToTrigger;
	[SerializeField] protected float delay;

	public bool HasBeenTriggered { get; private set; } = false;
	
	public bool Evaluate(float depth)
	{
		return depth >= depthToTrigger;
	}

	public void Execute()
	{
		if (HasBeenTriggered == false)
		{
			HasBeenTriggered = true;
			StartCoroutine(Coroutine_Execute());
		}
	}

	private IEnumerator Coroutine_Execute()
	{
		yield return new WaitForSeconds(delay);

		Execute_Internal();
	}

	protected virtual void Execute_Internal()
	{ }
}
