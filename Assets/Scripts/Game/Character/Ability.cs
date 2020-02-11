using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private bool mIsRunning;
	public bool IsRunning { get { return mIsRunning; } }

	private bool mIsPassive;
	public bool IsPassive { get { return mIsPassive; } }

	public virtual void Initialize(Character owner)
	{
		mOwner = owner;
	}

	public void StartAbility()
	{
		mIsRunning = true;
		StartAbility_Internal();
	}

	public void StopAbility()
	{
		mIsRunning = false;
		StopAbility_Internal();
	}

	public void UpdateAbility()
	{
		if (IsRunning || IsPassive)
		{
			UpdateAbility_Internal();
		}
	}

	public void FixedUpdateAbility()
	{
		if (IsRunning || IsPassive)
		{
			FixedUpdateAbility_Internal();
		}
	}

	protected virtual void StartAbility_Internal()
	{
	}

	protected virtual void StopAbility_Internal()
	{
	}
	
	protected virtual void UpdateAbility_Internal()
	{
	}

	protected virtual void FixedUpdateAbility_Internal()
	{
	}
}
