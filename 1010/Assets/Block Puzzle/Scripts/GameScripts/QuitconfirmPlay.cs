﻿using UnityEngine;
using System.Collections;

public class QuitconfirmPlay : MonoBehaviour
{
	//bool pressedQuitButton = false;
	/// <summary>
	/// Raises the close button pressed event.
	/// </summary>
	public void OnCloseButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnCloseButtonPressed ();
		}
	}

	/// <summary>
	/// Raises the ok button pressed event.
	/// </summary>
	public void OnOkButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			//pressedQuitButton = true;
			AudioManager.instance.PlayButtonClickSound ();

			BlockTrayManager.instance.ResetGame ();
			GameDataManager.instance.ResetGameData ();

			GameController.instance.OnCloseButtonPressed ();
			GameObject gamePlayScreen = GameController.instance.WindowStack.Pop ();
			GameController.instance.SpawnUIScreen("MainScreen",true);
			Destroy(gamePlayScreen);
		}
	}

	void OnEnable()
	{
		if (GamePlay.instance != null) {
			GamePlay.instance.TogglePauseGame(true);
		}
	}

	void OnDisable()
	{
		if (GamePlay.instance != null) {
			GamePlay.instance.TogglePauseGame(false);
		}
	}
}
