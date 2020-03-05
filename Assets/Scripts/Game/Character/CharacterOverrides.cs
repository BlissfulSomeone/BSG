using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ECharacterOverrideState
{
	Unchanged,
	Enable,
	Disable
}

[System.Serializable]
public struct FloatOverride
{
	[SerializeField] public ECharacterOverrideState State;
	[SerializeField] public float Value;
}

[System.Serializable]
public class CharacterOverrides
{
	[SerializeField] [Tooltip("If true, applying this override will interrupt all current overrides.")] public bool Interrupt;
	[SerializeField] public ECharacterOverrideState CanRecieveInput;
	[SerializeField] public ECharacterOverrideState ReceiveKnockbackFromExplosions;
	[SerializeField] public ECharacterOverrideState ClampMaxSpeed;
	[SerializeField] public ECharacterOverrideState CanBeHurt;
	[SerializeField] public ECharacterOverrideState AffectedByGravity;
	[SerializeField] public FloatOverride AirControl;
	
	public void Reset(ECharacterOverrideState resetToState = ECharacterOverrideState.Unchanged)
	{
		CanRecieveInput = resetToState;
		ReceiveKnockbackFromExplosions = resetToState;
		ClampMaxSpeed = resetToState;
		CanBeHurt = resetToState;
		AffectedByGravity = resetToState;
		AirControl.State = ECharacterOverrideState.Unchanged;
	}

	public static CharacterOverrides TransferOverrides(CharacterOverrides to, CharacterOverrides from)
	{
		CharacterOverrides result = new CharacterOverrides();
		result.CanRecieveInput = TransferState(to.CanRecieveInput, from.CanRecieveInput);
		result.ReceiveKnockbackFromExplosions = TransferState(to.ReceiveKnockbackFromExplosions, from.ReceiveKnockbackFromExplosions);
		result.ClampMaxSpeed = TransferState(to.ClampMaxSpeed, from.ClampMaxSpeed);
		result.CanBeHurt = TransferState(to.CanBeHurt, from.CanBeHurt);
		result.AffectedByGravity = TransferState(to.AffectedByGravity, from.AffectedByGravity);
		result.AirControl = TransferValue(to.AirControl, from.AirControl);
		return result;
	}

	private static ECharacterOverrideState TransferState(ECharacterOverrideState to, ECharacterOverrideState from)
	{
		if (to == ECharacterOverrideState.Disable)
			return ECharacterOverrideState.Disable;
		if (from == ECharacterOverrideState.Unchanged)
			return to;
		return from;
	}

	private static FloatOverride TransferValue(FloatOverride to, FloatOverride from)
	{
		if (from.State == ECharacterOverrideState.Enable)
			return new FloatOverride { State = ECharacterOverrideState.Enable, Value = Mathf.Min(to.Value, from.Value) };
		return to;
	}
}

public struct CharacterOverridesInstance
{
	public CharacterOverrides OverrideRef;
	public Coroutine CoroutineRef;
	public System.Action OnFinishedCallback;
	public System.Action OnInterruptedCallback;

	public CharacterOverridesInstance(CharacterOverrides overrideReference, Coroutine coroutineReference, System.Action onFinishedCallback, System.Action onInterruptedCallback)
	{
		OverrideRef = overrideReference;
		CoroutineRef = coroutineReference;
		OnFinishedCallback = onFinishedCallback;
		OnInterruptedCallback = onInterruptedCallback;
	}
}
