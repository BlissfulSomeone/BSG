using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	public enum ELayer
	{
		HUD,
		Background,
		Menus
	}

	private Dictionary<ELayer, UILayer> mLayers;

	private void Awake()
	{
		mLayers = new Dictionary<ELayer, UILayer>();

		CreateLayer(ELayer.HUD, UILayer.ELayerType.Multi);
		CreateLayer(ELayer.Background);
		CreateLayer(ELayer.Menus);
	}

	private UILayer CreateLayer(ELayer layerIndex, UILayer.ELayerType layerType = UILayer.ELayerType.Single)
	{
		if (mLayers.ContainsKey(layerIndex) == true)
			return mLayers[layerIndex];

		GameObject layerObject = new GameObject("UI Layer [" + layerIndex.ToString() + "]");
		layerObject.transform.SetParent(transform);
		layerObject.transform.Reset();

		UILayer layer = layerObject.AddComponent<UILayer>();
		layer.LayerType = layerType;
		mLayers.Add(layerIndex, layer);

		return layer;
	}

	public UIMenu PushMenu(ELayer layerIndex, UIMenu menuPrefab)
	{
		if (mLayers.ContainsKey(layerIndex) == false)
		{
			Debug.LogError("Attempting to create a menu for a non-existent layer [" + layerIndex.ToString() + "].");
			return null;
		}

		return mLayers[layerIndex].PushMenu(menuPrefab);
	}

	public void PopMenu(ELayer layerIndex)
	{
		if (mLayers.ContainsKey(layerIndex) == false)
		{
			Debug.LogError("Attempting to pop a menu from a non-existent layer [" + layerIndex.ToString() + "].");
			return;
		}

		mLayers[layerIndex].PopMenu();
	}

	public void PopAllMenus(ELayer layerIndex)
	{
		if (mLayers.ContainsKey(layerIndex) == false)
		{
			Debug.LogError("Attempting to pop a menu from a non-existent layer [" + layerIndex.ToString() + "].");
			return;
		}
		
		mLayers[layerIndex].PopAllMenus();
	}
}
