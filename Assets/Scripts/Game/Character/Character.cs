using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BSGFakePhysics), typeof(BoxCollider), typeof(Triggerable))]
public class Character : MonoBehaviour
{
	private CharacterMovementController mMovementController;
	private CharacterHealthController mHealthController;
	private CharacterOverrideController mOverrideController;
	private CharacterAbilityController mAbilityController;

	public CharacterMovementController MovementController { get { if (mMovementController == null) mMovementController = GetComponent<CharacterMovementController>(); return mMovementController; } }
	public CharacterHealthController HealthController { get { if (mHealthController == null) mHealthController = GetComponent<CharacterHealthController>(); return mHealthController; } }
	public CharacterOverrideController OverrideController { get { if (mOverrideController == null) mOverrideController = GetComponent<CharacterOverrideController>(); return mOverrideController; } }
	public CharacterAbilityController AbilityController { get { if (mAbilityController == null) mAbilityController = GetComponent<CharacterAbilityController>(); return mAbilityController; } }

	private Renderer mRenderer;
	private BSGFakePhysics mFakePhysics;
	private Triggerable mTriggerable;
	
	public Renderer Renderer { get { if (mRenderer == null) mRenderer = GetComponent<Renderer>(); return mRenderer; } }
	public BSGFakePhysics FakePhysics { get { if (mFakePhysics == null) mFakePhysics = GetComponent<BSGFakePhysics>(); return mFakePhysics; } }
	public Triggerable Triggerable { get { if (mTriggerable == null) mTriggerable = GetComponent<Triggerable>(); return mTriggerable; } }
	
    private void Awake()
	{
		mMovementController = GetComponent<CharacterMovementController>();
		mOverrideController = GetComponent<CharacterOverrideController>();
		mAbilityController = GetComponent<CharacterAbilityController>();

		mFakePhysics = GetComponent<BSGFakePhysics>();
		mTriggerable = GetComponent<Triggerable>();
	}
}
