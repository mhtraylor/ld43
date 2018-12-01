using UnityEngine;
using System.Collections;

public class CharacterController2D : MonoBehaviour 
{
	// Handle debug mode for raycasts
	public bool _rayDebug = false;


	// Skin of the character used for corner collisions
	private const float SkinWidth = 0.02f;

	// Amount of raycasts evenly distributed from the character
	private const int TotalHorizontalRays = 8;
	private const int TotalVerticalRays = 4;

	// Masked layer used for collision detection
	public LayerMask PlatformMask;

	// Character states and default parameters
	public ControllerParameters2D DefaultParameters;
	public ControllerState2D State { get; private set; }

	public ControllerParameters2D Parameters { 
		get { return _overrideParameters ?? DefaultParameters; }
	}

	// What the character is standing on
	public GameObject StandingOn { get; private set; }

	// Velocities
	public Vector2 Velocity { get { return _velocity; } }
	public Vector3 PlatformVelocity { get; set; }

	// Whether or not to handle collisions
	public bool HandleCollisions { get; set; }


	// Character variables
	private Vector2 _velocity;
	private Transform _transform;
	private Vector3 _localScale;
	private BoxCollider2D _boxCollider;
	private GameObject _lastStandingOn;
	private ControllerParameters2D _overrideParameters;

	// Used when a character is standing on a moving platform
	private Vector3
		_activeGlobalPlatformPoint,
		_activeLocalPlatformPoint;

	// Raycast variables
	private Vector3
		_raycastTopLeft,
		_raycastBottomLeft,
		_raycastBottomRight;

	private float
		_verticalDistanceBetweenRays,
		_horizontalDistanceBetweenRays;

	
	public void Awake()
	{
		HandleCollisions = true;
		State = new ControllerState2D();

		_transform = transform;
		_localScale = transform.localScale;
		_boxCollider = GetComponent<BoxCollider2D>();

		var colliderWidth = _boxCollider.size.x * Mathf.Abs(_localScale.x) - (2 * SkinWidth);
		_horizontalDistanceBetweenRays = colliderWidth / (TotalVerticalRays - 1);

		var colliderHeight = _boxCollider.size.y * Mathf.Abs(_localScale.y) - (2 * SkinWidth);
		_verticalDistanceBetweenRays = colliderHeight / (TotalHorizontalRays - 1);

	}

	// Adds force to the character
	public void AddForce(Vector2 force)
	{
		_velocity += force;
	}

	// Sets the force amount
	public void SetForce(Vector2 force)
	{
		_velocity = force;
	}

	// Sets the horizontal force amount
	public void SetHorizontalForce(float x)
	{
		_velocity.x = x;
	}

	// Sets the vertical force amount
	public void SetVerticalForce(float y)
	{
		_velocity.y = y;
	}

	// Runs after all other Update methods
	public void LateUpdate()
	{
		// Move the character
		Move(Velocity * Time.deltaTime);
	}

	// Moves the player
	private void Move(Vector2 deltaMovement)
	{
		// Reset the player state
		State.Reset();

		// Handle collisions first
		if (HandleCollisions)
		{
			// Check if we are on a moving platform
			HandlePlatforms();

			// Set our raycast origins
			CalculateRayOrigins();

			// Character is moving left or right, set the raycasts
			if (Mathf.Abs(deltaMovement.x) > 0.001f)
				MoveHorizontally(ref deltaMovement);

			// Character is moving up or down, set the raycasts
			if (Mathf.Abs(deltaMovement.y) > 0.001f)
				MoveVertically(ref deltaMovement);

			// TODO: fix the characters placement so it doesn't bounce off walls
			// CorrectHorizontalPlacement(ref deltaMovement, true);
			// CorrectHorizontalPlacement(ref deltaMovement, false);
		}

		// Move the character
		_transform.Translate(deltaMovement, Space.World);

		// Update the velocities
		if (Time.deltaTime > 0)
			_velocity = deltaMovement / Time.deltaTime;

		_velocity.x = Mathf.Min(_velocity.x, Parameters.MaxVelocity.x);
		_velocity.y = Mathf.Min(_velocity.y, Parameters.MaxVelocity.y);

		// Handle moving platforms
		if (StandingOn != null)
		{
			// TODO: move the character according to a moving platform
		}
	}

	// Handles when a character is standing on a moving platform
	private void HandlePlatforms() 
	{
		/* TODO: Implement moving platforms */
		StandingOn = null;
	}

	// Precomputes where the raycasts will be shot out from based on the current position of the character
	private void CalculateRayOrigins()
	{
		var size = new Vector2(_boxCollider.size.x * Mathf.Abs(_localScale.x), _boxCollider.size.y * Mathf.Abs(_localScale.y)) / 2;
		var center = new Vector2(_boxCollider.offset.x * _localScale.x, _boxCollider.offset.y * _localScale.y);

		_raycastTopLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y + size.y - SkinWidth);
		_raycastBottomLeft = _transform.position + new Vector3(center.x - size.x + SkinWidth, center.y - size.y + SkinWidth);
		_raycastBottomRight = _transform.position + new Vector3(center.x + size.x - SkinWidth, center.y - size.y + SkinWidth);
	}

	// Handles horizontal movement raycasts
	private void MoveHorizontally(ref Vector2 deltaMovement)
	{
		// Figure out which way the character is moving
		var isGoingRight = deltaMovement.x > 0;

		// Set up the raycasts
		var rayDistance = Mathf.Abs(deltaMovement.x) + SkinWidth;
		var rayDirection = isGoingRight ? Vector2.right : -Vector2.right;
		var rayOrigin = isGoingRight ? _raycastBottomRight : _raycastBottomLeft;

		// Construct the raycasts
		for (var i = 0; i < TotalHorizontalRays; i++)
		{
			// Create a new ray vector based on the vertical distance between the rays
			var rayVector = new Vector2(rayOrigin.x, rayOrigin.y + (i * _verticalDistanceBetweenRays));

#if UNITY_EDITOR
			// Used to show raycasts in game
			if (_rayDebug)
				Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);
#endif

			// Check if the raycast hit something
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);

			// If the raycast didn't hit anything, continue the loop
			if (!raycastHit)
				continue;

			// Get the distance from the character to the object that the raycast collided with and limit the movement to that distance
			// "The furthest the character can move without hitting the obstacle that the raycast hit"
			deltaMovement.x = raycastHit.point.x - rayVector.x;

			// Set a new distance for the remaining raycasts (since the character can't hit anything further)
			rayDistance = Mathf.Abs(deltaMovement.x);

			// Set the appropriate collider state
			if(isGoingRight)
			{
				deltaMovement.x -= SkinWidth;
				State.IsCollidingRight = true;
			}
			else
			{
				deltaMovement.x += SkinWidth;
				State.IsCollidingLeft = true;
			}

			if (rayDistance < SkinWidth + 0.0001f)
				break;
		}
	}

	// Handles vertical movement raycasts
	private void MoveVertically(ref Vector2 deltaMovement)
	{
		// Figure out which way the character is moving
		var isGoingUp = deltaMovement.y > 0;

		// Set up the raycasts
		var rayDistance = Mathf.Abs(deltaMovement.y) + SkinWidth;
		var rayDirection = isGoingUp ? Vector2.up : -Vector2.up;
		var rayOrigin = isGoingUp ? _raycastTopLeft : _raycastBottomLeft;

		// Set the ray origin's x to that of the character's horizontal movement
		rayOrigin.x += deltaMovement.x;

		// Construct the raycasts
		for (var i = 0; i < TotalVerticalRays; i++)
		{
			// Create a new ray vector based on the horizontal distance between the rays
			var rayVector = new Vector2(rayOrigin.x + (i * _horizontalDistanceBetweenRays), rayOrigin.y);

#if UNITY_EDITOR
			// Used to show raycasts in game
			if (_rayDebug)
				Debug.DrawRay(rayVector, rayDirection * rayDistance, Color.red);
#endif

			// Check if the raycast hit something
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rayDistance, PlatformMask);

			// If the raycast didn't hit anything, continue the loop
			if (!raycastHit)
				continue;

			// Get the distance from the character to the object that the raycast collided with and limit the movement to that distance
			// "The furthest the character can move without hitting the obstacle that the raycast hit"
			deltaMovement.y = raycastHit.point.y - rayVector.y;

			// Set a new distance for the remaining raycasts (since the character can't hit anything further)
			rayDistance = Mathf.Abs(deltaMovement.y);

			// Set the appropriate collider state
			if (isGoingUp)
			{
				deltaMovement.y -= SkinWidth;
				State.IsCollidingUp = true;
			}
			else
			{
				deltaMovement.y += SkinWidth;
				State.IsCollidingDown = true;
			}

			if (rayDistance < SkinWidth + 0.0001f)
				break;
		}
	}

	// Handles entering box collider triggers
	public void OnTriggerEnter2D(Collider2D other)
	{
		// Get the game object's parameters
		var parameters = other.gameObject.GetComponent<ControllerPhysicsVolume2D>();

		// Check if there are parameters
		if (parameters == null)
			return;

		// Override the character's parameters with the game object's physics parameters
		_overrideParameters = parameters.Parameters;
	}

	// Handles leaving box collider trigger
	public void OnTriggerExit2D(Collider2D other)
	{
		// Get the game object's parameters
		var parameters = other.gameObject.GetComponent<ControllerPhysicsVolume2D>();

		// Check if there are parameters
		if (parameters == null)
			return;

		// Reset the character's parameters
		_overrideParameters = null;
	}
}
