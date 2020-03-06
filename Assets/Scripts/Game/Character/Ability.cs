using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private string mInputName;
	protected string InputName { get { return mInputName; } }

	private bool mIsRunning;
	public bool IsRunning { get { return mIsRunning; } }

	protected bool mIsPassiveUpdate;
	public bool IsPassiveUpdate { get { return mIsPassiveUpdate; } }

	public virtual void Initialize(Character owner, string inputName)
	{
		mOwner = owner;
		mInputName = inputName;
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
		if (IsRunning || IsPassiveUpdate)
		{
			UpdateAbility_Internal();
		}
	}

	public void FixedUpdateAbility()
	{
		if (IsRunning || IsPassiveUpdate)
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
