using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class ControllerParameters2D
{
	// Maximum velocity the object can achieve
	public Vector2 MaxVelocity = new Vector2(float.MaxValue, float.MaxValue);
}
