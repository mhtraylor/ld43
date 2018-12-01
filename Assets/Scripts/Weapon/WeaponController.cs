using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	public List<GameObject> weapons = new List<GameObject>();
	public WeaponSystem2D.WeaponPresetType currentWeaponType = WeaponSystem2D.WeaponPresetType.Nothing;
	public GameObject currentWeapon;


	void Start () {

	}

	void Update () {

	}

}
