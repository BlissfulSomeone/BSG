using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Character))]
public class CharacterOverrideController : MonoBehaviour
{
	private Character mOwner;
	public Character Owner { get { return mOwner; } }

	private CharacterOverrides mCurrentOverrides = new CharacterOverrides();
	private List<CharacterOverridesInstance> mOverridesStack = new List<CharacterOverridesInstance>();

	public CharacterOverrides CurrentOverrides { get { return mCurrentOverrides; } }
	
	private void Awake()
	{
		mOwner = GetComponent<Character>();
	}

	private void FixedUpdate()
	{
		UpdateOverrides();
	}

	private void UpdateOverrides()
	{
		mCurrentOverrides.Reset(ECharacterOverrideState.Enable);
		foreach (CharacterOverridesInstance overrides in mOverridesStack)
		{
			mCurrentOverrides = CharacterOverrides.TransferOverrides(mCurrentOverrides, overrides.OverrideRef);
		}

		Owner.Triggerable.HasPhysics = IsOverrideEnabled(CurrentOverrides.ReceiveKnockbackFromExplosions);
		Owner.FakePhysics.IsAffectedByGravity = IsOverrideEnabled(CurrentOverrides.AffectedByGravity);
		if (IsOverrideEnabled(CurrentOverrides.AirControl))
		{
			Owner.FakePhysics.SetAirControl(CurrentOverrides.AirControl.Value);
		}
		else
		{
			Owner.FakePhysics.ResetAirControl();
		}
	}

	public void ApplyCharacterOverrides(CharacterOverrides characterOverrides, float duration = 0.0f, System.Action onFinishedCallback = null, System.Action onInterruptedCallback = null)
	{
		if (characterOverrides.Interrupt)
		{
			for (int i = 0; i < mOverridesStack.Count; ++i)
			{
				if (mOverridesStack[i].CoroutineRef != null)
				{
					StopCoroutine(mOverridesStack[i].CoroutineRef);
				}
				if (mOverridesStack[i].OnInterruptedCallback != null)
				{
					mOverridesStack[i].OnInterruptedCallback.Invoke();
				}
			}
			mOverridesStack.Clear();
		}

		if (duration == 0.0f)
		{
			ApplyCharacterOverrides_Internal(characterOverrides, null, onFinishedCallback, onInterruptedCallback);
		}
		else
		{
			Coroutine coroutine = StartCoroutine(Coroutine_ApplyCharacterOverrides(characterOverrides, duration));
			ApplyCharacterOverrides_Internal(characterOverrides, coroutine, onFinishedCallback, onInterruptedCallback);
		}
	}

	private void ApplyCharacterOverrides_Internal(CharacterOverrides characterOverrides, Coroutine coroutine, System.Action onFinishedCallback = null, System.Action onInterruptedCallback = null)
	{
		foreach (CharacterOverridesInstance overrides in mOverridesStack)
		{
			if (overrides.OverrideRef == characterOverrides)
			{
				Debug.LogError("The same instance of character overrides cannot be applied twice.");
				return;
			}
		}
		mOverridesStack.Add(new CharacterOverridesInstance(characterOverrides, coroutine, onFinishedCallback, onInterruptedCallback));
		UpdateOverrides();
	}

	private IEnumerator Coroutine_ApplyCharacterOverrides(CharacterOverrides characterOverrides, float duration = 0.0f, System.Action onFinishedCallback = null, System.Action onInterruptedCallback = null)
	{
		yield return new WaitForSeconds(duration);
		RemoveCharacterOverrides(characterOverrides);
	}

	public void RemoveCharacterOverrides(CharacterOverrides characterOverrides)
	{
		for (int i = 0; i < mOverridesStack.Count; ++i)
		{
			if (mOverridesStack[i].OverrideRef == characterOverrides)
			{
				if (mOverridesStack[i].CoroutineRef != null)
				{
					StopCoroutine(mOverridesStack[i].CoroutineRef);
				}
				if (mOverridesStack[i].OnFinishedCallback != null)
				{
					mOverridesStack[i].OnFinishedCallback.Invoke();
				}
				mOverridesStack.RemoveAt(i);
				return;
			}
		}
	}

	public bool IsCharacterOverrideApplied(CharacterOverrides characterOverrides)
	{
		foreach (CharacterOverridesInstance overrides in mOverridesStack)
		{
			if (overrides.OverrideRef == characterOverrides)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOverrideEnabled(ECharacterOverrideState overrideState)
	{
		return overrideState == ECharacterOverrideState.Enable;
	}
	
	public bool IsOverrideEnabled(FloatOverride floatOverride)
	{
		return floatOverride.State == ECharacterOverrideState.Enable;
	}
}
