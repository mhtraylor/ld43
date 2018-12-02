using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public enum Scenes { MENU = 1, GAME }

	public static bool IsPaused { get; set; }

	void Awake() {
		DontDestroyOnLoad(this.gameObject);
	}

	void Start () {
		ShowMenu();
	}

	void Update () {

	}

	static void ShowMenu() {
		SceneManager.LoadSceneAsync((int)Scenes.MENU);
	}

	public static void PlayGame() {
		SceneManager.LoadSceneAsync((int)Scenes.GAME);
		IsPaused = false;
	}

	public static void PauseGame() {
		Time.timeScale = 0;
		IsPaused = true;
	}

	public static void QuitGame() {
		ShowMenu();
	}

	public static void ResumeGame() {
		Time.timeScale = 1;
		IsPaused = false;
	}

}
