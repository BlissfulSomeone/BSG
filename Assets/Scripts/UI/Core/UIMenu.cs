using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas), typeof(CanvasGroup))]
public class UIMenu : MonoBehaviour
{
	private CanvasGroup mCanvasGroup;
	private CanvasGroup CanvasGroup { get { if (mCanvasGroup == null) mCanvasGroup = GetComponent<CanvasGroup>(); return mCanvasGroup; } }

	public float Alpha { get { return CanvasGroup.alpha; } set { CanvasGroup.alpha = value; } }
	public bool Interactable { get { return CanvasGroup.interactable; } set { CanvasGroup.interactable = value; } }

	private UILayer mParentLayer;

	public void Initialize(UILayer aParentLayer)
	{
		mParentLayer = aParentLayer;
	}
}
