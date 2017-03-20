﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Settings.
/// </summary>
public class Settings : MonoBehaviour
{
	public Image MenuButtonImage;
	public Sprite MenuOpenedSprite;
	public Sprite MenuClosedSprite;
	public GameObject SettingContent;
	bool isMenuOpened = false;

	/// <summary>
	/// Raises the menu button pressed event.
	/// </summary>
	public void OnMenuButtonPressed ()
	{
		if (InputManager.instance.canInput (1F)) {
			AudioManager.instance.PlayButtonClickSound ();
			if (!isMenuOpened) {
				OpenMenu ();
				return;
			} 
			GameController.instance.OnBackButtonPressed ();
		}
	}

	/// <summary>
	/// Opens the menu.
	/// </summary>
	void OpenMenu ()
	{
		isMenuOpened = true;
		MenuButtonImage.sprite = MenuOpenedSprite;
		GetComponent<Animator> ().Play ("Open-Settings");
		if (GameController.instance.WindowStack.Peek () != null && GameController.instance.WindowStack.Peek ().name == "GamePlay") {
			GamePlay.instance.TogglePauseGame(true);
		}
		GameController.instance.PushWindow (gameObject);
	}

	/// <summary>
	/// Closes the menu.
	/// </summary>
	public void CloseMenu ()
	{
		AudioManager.instance.PlayButtonClickSound ();
		isMenuOpened = false;
		MenuButtonImage.sprite = MenuClosedSprite;
		GetComponent<Animator> ().Play ("Close-Settings");
	}

	/// <summary>
	/// Raises the leader board button pressed event.
	/// </summary>
	public void OnLeaderBoardButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			Debug.Log ("Leaderboard stuff goes here..");

			/*if (UM_GameServiceManager.Instance.IsConnected) {
				UM_GameServiceManager.Instance.ShowLeaderBoardsUI ();
			} else {
				UM_GameServiceManager.Instance.Connect ();
			}*/
		}
	}

	/// <summary>
	/// Raises the achievements button pressed event.
	/// </summary>
	public void OnAchievementsButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			Debug.Log ("Achievement stuff goes here..");
			/*if (UM_GameServiceManager.Instance.IsConnected) {
				UM_GameServiceManager.Instance.ShowAchievementsUI ();
			} else {
				UM_GameServiceManager.Instance.Connect ();
			}*/
		}
	}

	/// <summary>
	/// Raises the about button pressed event.
	/// </summary>
	public void OnAboutButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			Debug.Log ("About stuff goes here..");
		}
	}

	/// <summary>
	/// Raises the store button pressed event.
	/// </summary>
	public void OnStoreButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			CloseMenu ();
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.SpawnUIScreen ("Store", true);
		}
	}

	/// <summary>
	/// Raises the home button pressed event.
	/// </summary>
	public void OnHomeButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnBackButtonPressed ();
			GameController.instance.OnBackButtonPressed ();
		}
	}

	/// <summary>
	/// Raises the restart button pressed event.
	/// </summary>
	public void OnRestartButtonPressed ()
	{
		if (InputManager.instance.canInput ()) {
			AudioManager.instance.PlayButtonClickSound ();
			GameController.instance.OnBackButtonPressed ();
			BlockTrayManager.instance.ResetGame ();
			GameDataManager.instance.ResetGameData ();
		}
	}

	void OnEnable()
	{
	}

	void OnDisable()
	{
		#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
		GamePlay.instance.TogglePauseGame(false);
		#endif

	}
}