using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	// Which way the player is facing
	private enum FacingDirection { Up, Right, Down, Left };
	private FacingDirection IsFacing;
	public Vector2 Direction;

	// Sprite Cache
	public Sprite SpriteUp;
	public Sprite SpriteDown;
	public Sprite SpriteHorizontal;

	private Animator animator;

	// Character initial local scale
	private Vector3 _initialScale;

	// Character controller 2D component
	private CharacterController2D _characterController;

	// Normalized value for player movement (-1 or 1)
	private float _normalizedHorizontalSpeed;
	private float _normalizedVerticalSpeed;

	// Maximum speed the player can achieve
	public float MaxSpeed = 7;

	// Determines how quickly the player speeds up
	public float SpeedAcceleration = 10f;

	// Flag for when the player is strafing
	private bool _isStrafing;

	// Maximum and current health of the player
	public int MaxHealth = 100;
	public int Health { get; private set; }

	// Effect fired when the player takes damage
	public GameObject TakeDamageEffect;

	// Flag for setting the player dead
	public bool IsDead { get; private set; }

	// Weapon variables
	private WeaponSystem2D _weaponSystem;
	public Transform GunBarrel;
	private Vector3 _gunBarrelInitialPosition;

	// Player sprite renderer
	private SpriteRenderer _spriteRenderer;

	// Player animator
	public Animator Animator;

	// Player audio variables
	public AudioClip SoundPlayerSpawn;
	public AudioClip SoundPlayerHit;
	public AudioClip SoundPlayerDeath;
	public AudioClip SoundPlayerPickup;


	public void Awake()
	{
		_characterController = GetComponent<CharacterController2D>();
		_weaponSystem = GetComponentInChildren<WeaponSystem2D>();
		_spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		_initialScale = transform.localScale;
		_gunBarrelInitialPosition = GunBarrel.transform.localPosition;

		Health = MaxHealth;
		SetFacingDirectionRight();
	}

	public void Start()
	{
		// Set the default weapon
		if (_weaponSystem != null) 
		{
			_weaponSystem.WeaponType = WeaponSystem2D.WeaponPresetType.Standard;

			_weaponSystem.BulletCount = 1;
			_weaponSystem.BulletSpacing = 1f;
			_weaponSystem.BulletSpread = 0.05f;
			_weaponSystem.BulletSpeed = 25f;
			_weaponSystem.BulletRandomness = 0.15f;
			_weaponSystem.WeaponFireRate = 0.15f;
		}

		animator = GetComponent <Animator>();
	}


	public void Update()
	{
		// If the player isn't dead, handle the user input
		if (!IsDead)
			HandleInput();

		// Set the player's movement
		if (IsDead)
		{
			_characterController.SetHorizontalForce(0);
			_characterController.SetVerticalForce(0);
		}
		else
		{
			_characterController.SetHorizontalForce(Mathf.Lerp(_characterController.Velocity.x, _normalizedHorizontalSpeed * MaxSpeed, Time.deltaTime * SpeedAcceleration));
			_characterController.SetVerticalForce(Mathf.Lerp(_characterController.Velocity.y, _normalizedVerticalSpeed * MaxSpeed, Time.deltaTime * SpeedAcceleration));
		}
	}

	// Handles the input from the user
	private void HandleInput()
	{
		// Set strafing flag
		_isStrafing = Input.GetKey(KeyCode.LeftShift);

		// If move right input is called
		if (Input.GetKey(KeyCode.D))
		{
			// If the user is holding left and right, don't move the player
			if (Input.GetKey(KeyCode.A))
			{
				_normalizedHorizontalSpeed = 0;
			}
			else
			{
				// Set the correct sprite
				if (_spriteRenderer.sprite != SpriteHorizontal && _normalizedVerticalSpeed == 0 && !_isStrafing)
					_spriteRenderer.sprite = SpriteHorizontal;

				// Set horizontal speed and player direction
				_normalizedHorizontalSpeed = 1;
				if (!IsFacingRight() && !_isStrafing && _normalizedVerticalSpeed == 0)
					SetFacingDirectionRight();

				animator.SetBool("isWalkingHorizontal", true);
			}
		}
		// If move left input is called
		else if (Input.GetKey(KeyCode.A))
		{
			// Set the correct sprite
			if (_spriteRenderer.sprite != SpriteHorizontal && _normalizedVerticalSpeed == 0 && !_isStrafing)
				_spriteRenderer.sprite = SpriteHorizontal;

			// Set horizontal speed and player direction
			_normalizedHorizontalSpeed = -1;
			if(!IsFacingLeft() && !_isStrafing && _normalizedVerticalSpeed == 0)
				SetFacingDirectionLeft();

			animator.SetBool("isWalkingHorizontal", true);
		}
		// No horizontal input was received
		else
		{
			_normalizedHorizontalSpeed = 0;
			animator.SetBool("isWalkingHorizontal", false);
		}

		// If move up input is called
		if (Input.GetKey(KeyCode.W))
		{
			// If the user is holding up and down, don't move the player
			if (Input.GetKey(KeyCode.S))
			{
				_normalizedVerticalSpeed = 0;
			}
			else
			{
				// Set the correct sprite
				if (_spriteRenderer.sprite != SpriteUp && _normalizedHorizontalSpeed == 0 && !_isStrafing)
					_spriteRenderer.sprite = SpriteUp;

				// Set vertical speed and flip the player if necessary
				_normalizedVerticalSpeed = 1;
				if (!IsFacingUp() && !_isStrafing && _normalizedHorizontalSpeed == 0)
					SetFacingDirectionUp();
				
				animator.SetBool("isWalkingUp", true);
			}
		}
		// If move down input is called
		else if (Input.GetKey(KeyCode.S))
		{
			// Set the correct sprite
			if (_spriteRenderer.sprite != SpriteDown && _normalizedHorizontalSpeed == 0 && !_isStrafing)
				_spriteRenderer.sprite = SpriteDown;

			// Set the vertical speed and flip the player if necessary
			_normalizedVerticalSpeed = -1;
			if (!IsFacingDown() && !_isStrafing && _normalizedHorizontalSpeed == 0)
				SetFacingDirectionDown();

			animator.SetBool("isWalkingDown", true);
		}
		// No vertical input was received
		else
		{
			animator.SetBool("isWalkingUp", false);
			animator.SetBool("isWalkingDown", false);
			_normalizedVerticalSpeed = 0;
		}
	}

	
	/**
	 * Below is the methods for setting the player's facing direction and checking for them
	 * 
	 * SetFacingDirectionUp()
	 * SetFacingDirectionRight()
	 * SetFacingDirectionDown()
	 * SetFacingDirectionLeft()
	 * 
	 * bool IsFacingDirectionUp()
	 * bool IsFacingDirectionRight()
	 * bool IsFacingDirectionDown()
	 * bool IsFacingDirectionLeft()
	 **/

	// Change the direction the player is facing to up
	public void SetFacingDirectionUp()
	{ 
		if (IsFacingUp())
			return;

		IsFacing = FacingDirection.Up;
		Direction = Vector2.up;

		// Reset the local scale
		transform.localScale = new Vector3(_initialScale.x, _initialScale.y, _initialScale.z);

		// Set the barrel position and rotation
		GunBarrel.transform.localPosition = new Vector3(0.34f, 2.1f, 0);
		GunBarrel.transform.eulerAngles = new Vector3(0, 0, 90);
	}

	// Change the direction the player is facing to right
	public void SetFacingDirectionRight()
	{ 
		if (IsFacingRight())
			return;

		IsFacing = FacingDirection.Right;
		Direction = Vector2.right;

		// Reset the local scale
		transform.localScale = new Vector3(_initialScale.x, _initialScale.y, _initialScale.z);

		// Reset the barrel position
		GunBarrel.transform.localPosition = _gunBarrelInitialPosition;
		GunBarrel.transform.eulerAngles = new Vector3(0, 0, 0);
	}

	// Change the direction the player is facing to down
	public void SetFacingDirectionDown()
	{ 
		if (IsFacingDown())
			return;

		IsFacing = FacingDirection.Down;
		Direction = -Vector2.up;

		// Reset the local scale
		transform.localScale = new Vector3(_initialScale.x, _initialScale.y, _initialScale.z);

		// Set the barrel position
		GunBarrel.transform.localPosition = new Vector3(-0.28f, 0.4f, 0);
		GunBarrel.transform.eulerAngles = new Vector3(0, 0, 270);
	}

	// Change the direction the player is facing to left
	public void SetFacingDirectionLeft()
	{ 
		if (IsFacingLeft())
			return;

		IsFacing = FacingDirection.Left;
		Direction = -Vector2.right;

		// Reset the barrel position
		GunBarrel.transform.localPosition = _gunBarrelInitialPosition;
		GunBarrel.transform.eulerAngles = new Vector3(0, 0, 0);

		// Flip the local scale
		transform.localScale = new Vector3(-_initialScale.x, _initialScale.y, _initialScale.z);
	}

	// Check if the player is facing up
	public bool IsFacingUp() 
	{
		if (IsFacing == FacingDirection.Up)
			return true;

		return false;
	}

	// Check if the player is facing right
	public bool IsFacingRight()
	{
		if (IsFacing == FacingDirection.Right)
			return true;
		
		return false;
	}

	// Check if the player is facing down
	public bool IsFacingDown()
	{
		if (IsFacing == FacingDirection.Down)
			return true;
		
		return false;
	}

	// Check if the player is facing left
	public bool IsFacingLeft()
	{
		if (IsFacing == FacingDirection.Left)
			return true;
		
		return false;
	}
}
