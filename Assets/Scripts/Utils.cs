using UnityEngine;
using System.Collections;

public static class Utils
{
	public static Vector3 BottomLeftWorldPos()
	{
		return Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
	}

	public static Vector3 TopRightWorldPos()
	{
		return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
	}
}
