using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class WeaponSystem2D : MonoBehaviour
{
	// Weapon types
	public enum WeaponPresetType
	{
		Standard,
		Shotgun,
		//Homing,
		Charge,
		Seismic,
		CimbalMonkey,
		GIJoe,
		PowerRanger,
		HeMan,
		SuperSoaker,
		Nothing
	}

	// Current weapon the character is using
	private WeaponPresetType _weaponType;

	// Event for when a weapon change is called
	public delegate void WeaponTypeChangeHandler();
	public event WeaponTypeChangeHandler WeaponChanged;

	// This handles what weapon is selected
	public WeaponPresetType WeaponType
	{
		get { return _weaponType; }
		set
		{
			if (WeaponType == value) return;

			// Set the property and fire off the event
			_weaponType = value;
			if (WeaponChanged != null)
				WeaponChanged();

			if (_weaponType == WeaponPresetType.Seismic)
				_lineRenderer.enabled = true;
			else
				_lineRenderer.enabled = false;

			Debug.Log(_weaponType);
		}
	}

	// Where the bullets shoot from
	public Transform GunBarrel;

	// Bullet type (projectile)
	public Bullet BulletType;

// USED FOR TESTING
	public GameObject BulletPrefab;

	// Bullet properties
	[Range(0, 20)]   public int BulletCount;
	[Range(0f, 5f)]  public float BulletSpacing;
	[Range(5f, 35f)] public float BulletSpeed;
	[Range(0f, 5f)]  public float BulletSpread;
	[Range(0f, 5f)]  public float BulletRandomness;
	[Range(0f, 3f)]	 public float WeaponFireRate;

	// Position of the bullet
	private float _bulletXPosition;
	private float _bulletYPosition;

	// Bullet spacing and spread calculation properties
	private float _bulletSpreadInitial;
	private float _bulletSpacingInitial;
	private float _bulletSpreadIncrement;
	private float _bulletSpacingIncrement;

	// Angle of the bullet shot
	private float _aimAngle;

	// Cool down timer for shots
	private float _coolDown;

	// Gun specific properties
	private float _chargeTimer;

	// Reference to the player
	private PlayerController _playerController;

	// Direction the player is facing
	private Vector2 _playerDirection;

	// Line Renderer for beam
	private LineRenderer _lineRenderer;



	public void Awake()
	{
		// Subscribe to the event
		WeaponChanged += WeaponTypeChangedHandler;

		// Get the player controller
		_playerController = GetComponentInParent<PlayerController>();

		_lineRenderer = GetComponent<LineRenderer>();
		_lineRenderer.enabled = false;
	}


	public void Update()
	{
		// Check if we need to reset the direction and do so
		if (_playerDirection != _playerController.Direction)
		{
			_playerDirection = Vector2.zero;
			_playerDirection = _playerController.Direction;
		}

		// Start the shooting process
		CalculateAimAngles(_playerDirection);
		HandleShooting();
		HandleWeaponSwap();
	}


	// Angle the bullet needs to travel
	private void CalculateAimAngles(Vector2 facingDirection)
	{
		_aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);

		if (_aimAngle < 0f)
			_aimAngle = Mathf.PI * 2 + _aimAngle;
	}


	// Adjust the cooldown and check if we are to fire the weapon
	private void HandleShooting()
	{
		_coolDown -= Time.deltaTime;

		if (_weaponType == WeaponPresetType.Charge)
		{
			if (Input.GetButton("Fire1"))
				_chargeTimer += Time.deltaTime;

			if (Input.GetButtonUp("Fire1"))
				ShootWithCooldown();
		}
		else if (_weaponType == WeaponPresetType.Seismic)
		{
			if (Input.GetButton("Fire1"))
				ShootBeam();

			if (Input.GetButtonUp("Fire2"))
				ShootWithCooldown();
		}
		else if (Input.GetButton("Fire1"))
			ShootWithCooldown();
	}


	// Cool down check before firing the weapon
	private void ShootWithCooldown()
	{
		if (_coolDown > 0.0f)
			return;

		// Fire the weapon and reset the cooldown
		ShootWeapon();
		_coolDown = WeaponFireRate;

		// Reset the charge timer if needed
		if (_weaponType == WeaponPresetType.Charge)
			_chargeTimer = 0f;
	}


	// Shoot a laser beam!
	private void ShootBeam()
	{
		if (_coolDown > 0.0f)
			return;

		var beamRotation = Vector2.zero;

		// Set the starting position and localscale/rotation of the bullet
		if (_playerDirection.x != 0)
		{
			if (_playerController.IsFacingRight())
				beamRotation = Vector2.right;
			else
				beamRotation = -Vector2.right;
		}
		else
		{
			if(_playerController.IsFacingUp())
				beamRotation = Vector2.up;
			else
				beamRotation = -Vector2.up;
		}

		// Shoot a raycast
		RaycastHit2D hit = Physics2D.Raycast(new Vector2(GunBarrel.transform.position.x, GunBarrel.transform.position.y), beamRotation);

			if (hit.collider != null)
				Debug.Log(hit.collider.gameObject.name);
			else
				Debug.Log("MISS");

		// Reset the cooldown
		_coolDown = WeaponFireRate;
	}


	// Shoot some bullets
	private void ShootWeapon()
	{
		// Set the spacing and spread properties
		if (BulletCount > 1)
		{
			_bulletSpreadInitial = -BulletSpread / 2;
			_bulletSpacingInitial = -BulletSpacing / 2;
			_bulletSpreadIncrement = BulletSpread / (BulletCount - 1);
			_bulletSpacingIncrement = BulletSpacing / (BulletCount - 1);
		}
		else
		{
			_bulletSpreadInitial = 0f;
			_bulletSpacingInitial = 0f;
			_bulletSpreadIncrement = 0f;
			_bulletSpacingIncrement = 0f;
		}

		// Set the weapon multiplier
		var multiplier = 1;
		if (_weaponType == WeaponPresetType.Charge)
			multiplier = GetChargeMultiplier();

		// Fire the bullets
		for (var i = 0; i < BulletCount; i++)
		{
			// Instantiate a new bullet
			GameObject bullet = (GameObject) Instantiate(BulletPrefab);

			var bulletLocalScale = Vector3.one;
			var bulletRotation = Vector3.zero;

			// Get the bullet's default local scale and rotation
			var bulletCurrentLocalScale = bullet.transform.localScale;
			var bulletCurrentRotation = bullet.transform.eulerAngles;

			// Assign the offsets
			var offsetX = Mathf.Cos(_aimAngle - Mathf.PI / 2) * (_bulletSpacingInitial - i * _bulletSpacingIncrement);
			var offsetY = Mathf.Sin(_aimAngle - Mathf.PI / 2) * (_bulletSpacingInitial - i * _bulletSpacingIncrement);

			// Set the starting position and localscale/rotation of the bullet
			if (_playerDirection.x != 0)
			{
				if (_playerController.IsFacingLeft())
					bulletLocalScale = new Vector3(-bulletCurrentLocalScale.x, bulletCurrentLocalScale.y, bulletCurrentLocalScale.z);

				_bulletXPosition = GunBarrel.transform.position.x + offsetX;
				_bulletYPosition = GunBarrel.transform.position.y + Random.Range(0f, 1f) * BulletRandomness - BulletRandomness / 2;
			}
			else
			{
				if(_playerController.IsFacingUp())
				{
					bulletRotation = new Vector3(bulletCurrentRotation.x, bulletCurrentRotation.y, 90);
				}
				else
				{
					bulletRotation = new Vector3(bulletCurrentRotation.x, bulletCurrentRotation.y, 270);
					bulletLocalScale = new Vector3(bulletCurrentLocalScale.x, -bulletCurrentLocalScale.y, bulletCurrentLocalScale.z);
				}

				_bulletXPosition = GunBarrel.transform.position.x + Random.Range(0f, 1f) * BulletRandomness - BulletRandomness / 2;
				_bulletYPosition = GunBarrel.transform.position.y + offsetY;
			}

			var bulletComponent = (Bullet) bullet.GetComponent(typeof(Bullet));

			bulletComponent.DirectionAngle = _aimAngle + _bulletSpreadInitial + i * _bulletSpreadIncrement;
			bulletComponent.Speed = BulletSpeed * multiplier;

			bullet.transform.localScale = bulletLocalScale;
			bullet.transform.eulerAngles = bulletRotation;
			bullet.transform.position = new Vector3(_bulletXPosition, _bulletYPosition, 0f);

			bulletComponent.BulletXPosition = bullet.transform.position.x;
			bulletComponent.BulletYPosition = bullet.transform.position.y;

			bullet.SetActive(true);
		}
	}


	// Gets the charge gun multiplier for damage and speed
	private int GetChargeMultiplier()
	{
		var chargeMultiplier = 0;
		var chargeLevel = 0;

		if (_chargeTimer > 2.0f)
			chargeLevel = 2;
		else if (_chargeTimer > 1.0f)
			chargeLevel = 1;

		switch(chargeLevel)
		{
			case 2:
			chargeMultiplier = 4;
			break;

			case 1:
			chargeMultiplier = 2;
			break;

			default:
			chargeMultiplier = 1;
			break;
		}

		return chargeMultiplier;
	}


	// Changes the properties of the weapon
	private void WeaponTypeChangedHandler()
	{
		switch (WeaponType)
		{
			case WeaponPresetType.Standard:
			BulletCount = 1;
			BulletRandomness = 0.15f;
			BulletSpacing = 1f;
			BulletSpeed = 25f;
			BulletSpread = 0.05f;
			WeaponFireRate = 0.15f;
			break;

			case WeaponPresetType.Shotgun:
			BulletCount = 5;
			BulletRandomness = 0.5f;
			BulletSpacing = 0.5f;
			BulletSpeed = 30f;
			BulletSpread = 0.65f;
			WeaponFireRate = 0.5f;
			break;

			case WeaponPresetType.Charge:
			BulletCount = 1;
			BulletRandomness = 0.05f;
			BulletSpacing = 1f;
			BulletSpeed = 20f;
			BulletSpread = 0.05f;
			WeaponFireRate = 0.5f;
			break;

			case WeaponPresetType.Seismic:
			BulletCount = 1;
			BulletRandomness = 0.05f;
			BulletSpacing = 1f;
			BulletSpeed = 5f;
			BulletSpread = 0.05f;
			WeaponFireRate = 0.5f;
			break;

			default:
			BulletCount = 1;
			BulletRandomness = 0.15f;
			BulletSpacing = 1f;
			BulletSpeed = 25f;
			BulletSpread = 0.05f;
			WeaponFireRate = 0.15f;
			break;
		}
	}


	// Check if we need to change the weapon
	private void HandleWeaponSwap()
	{
		if (Input.GetAxisRaw("Mouse ScrollWheel") == 0)
			return;

		if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
			SelectNextWeapon();
		else
			SelectPreviousWeapon();
	}


	// Selects the next weapon in the list
	private void SelectNextWeapon()
	{
		var currentIndex = (int) _weaponType;
		var nextIndex = (int) currentIndex + 1;

		if (nextIndex >= (int) Enum.GetValues(typeof(WeaponPresetType)).Length)
			nextIndex = 0;

		WeaponType = (WeaponPresetType) Enum.GetValues(typeof(WeaponPresetType)).GetValue(nextIndex);
	}


	// Selects the previous weapon in the list
	private void SelectPreviousWeapon()
	{
		var currentIndex = (int) _weaponType;
		var nextIndex = (int) currentIndex - 1;

		if (nextIndex < 0)
			nextIndex = (int) Enum.GetValues(typeof(WeaponPresetType)).Length - 1;

		WeaponType = (WeaponPresetType) Enum.GetValues(typeof(WeaponPresetType)).GetValue(nextIndex);
	}
}
