using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

	public GameObject pauseMenu;

	void Start () {
		this.pauseMenu.SetActive(false);
	}

	void Update () {
		if (GameManager.IsPaused) {
			if (Input.GetKeyUp(KeyCode.Q)) {
				this.OnQuit();
			}
			if (Input.GetKeyUp(KeyCode.Return)) {
				this.OnResume();
			}
		} else {
			if (Input.GetKeyUp(KeyCode.P)) {
				this.OnPause();
			}
		}
	}

	public void OnPause() {
		GameManager.PauseGame();
		this.pauseMenu.SetActive(true);
	}

	public void OnQuit() {
		this.pauseMenu.SetActive(false);
		GameManager.QuitGame();
	}

	public void OnResume() {
		this.pauseMenu.SetActive(false);
		GameManager.ResumeGame();
	}
}
