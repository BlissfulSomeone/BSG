using UnityEngine;

public class DisplayAsAttribute : PropertyAttribute
{
	public string mDisplayName;

	public DisplayAsAttribute(string displayName)
	{
		mDisplayName = displayName;
	}
}
