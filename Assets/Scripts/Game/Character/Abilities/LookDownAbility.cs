using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookDownAbility : Ability
{
	[SerializeField] private CameraController.ViewInfo mAdditiveViewInfo;

	private System.Guid mCurrentAdditiveGuid;

	protected override void StartAbility_Internal()
	{
		mCurrentAdditiveGuid = GameController.Instance.CameraControllerInstance.PushAdditiveViewInfo(mAdditiveViewInfo);
	}

	protected override void StopAbility_Internal()
	{
		GameController.Instance.CameraControllerInstance.RemoveAdditiveViewInfo(mCurrentAdditiveGuid);
	}
}
