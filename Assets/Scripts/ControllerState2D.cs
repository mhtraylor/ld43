using UnityEngine;
using System.Collections;

public class ControllerState2D
{
	public bool IsCollidingUp { get; set; }
	public bool IsCollidingRight { get; set; }
	public bool IsCollidingDown { get; set; }
	public bool IsCollidingLeft { get; set; }

	public bool HasCollisions 
	{ 
		get { return IsCollidingUp || IsCollidingRight || IsCollidingDown || IsCollidingLeft; } 
	}


	// Resets to default states
	public void Reset()
	{
		IsCollidingUp =
			IsCollidingRight =
			IsCollidingDown =
			IsCollidingDown = false;
	}


	// Outputs a string of what the object is colliding with
	public override string ToString ()
	{
		return string.Format(
			"(controller: u:{0} r:{1} d:{2} l:{3})",
			IsCollidingUp,
			IsCollidingRight,
			IsCollidingDown,
			IsCollidingLeft);
	}
}
