using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour 
{
	// Owner of the bullet
	public enum BulletOwner	{ Player, Enemy	}
	public BulletOwner Owner { get; set; }

	// Speed of the bullet
	public float Speed = 1f;

	// Direction of the bullet
	public Vector2 Direction = new Vector2(1f, 0);

	// Sprite used for the bullet
	public SpriteRenderer BulletSprite;

	// Direction angle the bullet is moving
	public float DirectionAngle;

	// Position of the bullet
	public float BulletXPosition;
	public float BulletYPosition;



	public void Awake()
	{
		// Reference the bullet sprite
		BulletSprite = gameObject.GetComponent<SpriteRenderer>();
	}


	public void OnEnable()
	{
		// When the bullet is enabled, ensure it faces the correct direction
		transform.eulerAngles = new Vector3(0.0f, 0.0f, DirectionAngle * Mathf.Rad2Deg);
	}


	public void Update()
	{
		if (gameObject != null)
		{
			// Account for bullet movement at any angle
			BulletXPosition += Mathf.Cos(DirectionAngle) * Speed * Time.deltaTime;
			BulletYPosition += Mathf.Sin(DirectionAngle) * Speed * Time.deltaTime;

			transform.position = new Vector2(BulletXPosition, BulletYPosition);

			// If the bullet is no longer visible by the main camera, then set it back to disabled
			if (!BulletSprite.isVisible) 
				Destroy(gameObject);
		}
	}
}
