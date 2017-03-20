using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

/// <summary>
/// Game mode.
/// </summary>
public enum GameMode
{
	classic = 0,
	plus = 1,
	bomb = 2,
	hexa = 3,
	timer = 4
}

public enum UpcomingBlockFillMode
{
	AddNewOnAllEmpty = 0, 
	AddNewOnAnyUse = 1,
}

/// <summary>
/// Game play.
/// </summary>
public class GamePlay : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler,IBeginDragHandler
{

	public static GamePlay instance;
	public delegate void OnBackgroundChangeHandler();
	public event OnBackgroundChangeHandler OnBackgroundChange;

	public Text txtScore;
	public Text txtBestScore;
	public Text txtGameOverReason;

	public Transform sampleBlock;
	public Sprite bubbleSprite;
	public Sprite flatCircleSprite;

	public GameObject settingsPanel;

	bool blockSelected = false;
	RectTransform SelectedObject;
	Transform LastCheckedBlock;

//	public Color blockColor;
	public List<ThemeData> BackgroundThemes;
	public ThemeData previousTheme;
	public ThemeData currentTheme;
	List<ThemeData> BackgroundThemesCopy;

	List<Image> hintColorBlocks = new List<Image> ();
	public static GameMode GamePlayMode;


	public int Score;
	public int BestScore;
	public int pointsBeforeThemeChange;
	public int scoreAtLastThemeChange;

	public AudioClip SFX_BlockPlace;
	public AudioClip SFX_GameOver;

	public AudioClip[] SFX_SingleLine;
	public AudioClip[] SFX_DoubleLine;
	public AudioClip[] SFX_MultiLine;

	//Details For blocked in whuich Bombs are palced
	public List<BombModedetails> bombPlacingDetails = new List<BombModedetails>();
	public int TotalMoves = 0;

	public Sprite dynamiteImage,blockImage;

	/// <summary>
	/// Column List For hexa Mode
	/// </summary>
	public List<RowColumnData> column1List;
	public List<RowColumnData> column2List;

	/// <summary>
	/// Timer Mode Variables
	/// </summary>
	public RectTransform Timer;
	Image BarImage;

	/// <summary>
	/// Progress of bar
	/// </summary>
	public int TimerValue = 120;

	/// <summary>
	/// Mad Timer Value to empty the bar
	/// </summary>

	public int TotalTimerValue = 120;

	bool isGamePaused = false;
	public List<Color> ThemeColors;
	public Button btnUndo;

	public UpcomingBlockFillMode upcomingBlockFillMode;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		if (instance == null) {
			instance = this;
		}
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		BestScore = PlayerPrefs.GetInt ("bestscore_" + GamePlayMode.ToString (), 0);
		txtBestScore.text = "Best : " + BestScore.ToString ();
		BackgroundThemesCopy = new List<ThemeData> ();
		EnableSettingsMenu ();
		ChangeBackgroundTheme ();
	
		if (GamePlayMode == GameMode.timer) 
		{
			BarImage = Timer.GetComponent<Image> ();

			SetTimer ();
			Timer.parent.gameObject.SetActive (true);
		}

//		blockColor = (ThemeManager.instance.isDarkTheme) ? ThemeColors [0] : ThemeColors [1];

		if (GameDataManager.instance.lastMoveData != string.Empty && GamePlayMode != GameMode.bomb) 
		{
			btnUndo.gameObject.SetActive (true);
		}
	}

	public void TogglePauseGame(bool paused)
	{
		isGamePaused = paused;
		if (GamePlayMode == GameMode.timer) {
			if (isGamePaused) {
				EGTween.Pause (gameObject);
			} else {
				EGTween.Resume (gameObject);
			}
		}
	}

	/// <summary>
	/// Sets the timer For Bar Progress
	/// this method will set and reset the timer on clearing row and column
	/// </summary>
	/// <param name="timerToIncrease">Timer to increase.</param>
	public void SetTimer (int timerToIncrease = 0)
	{
		if (GamePlayMode == GameMode.timer) 
		{
			EGTween.Stop (gameObject);
			TimerValue = TimerValue + timerToIncrease;
			TimerValue = Mathf.Clamp (TimerValue, 0, TotalTimerValue);

			float currentBarProgress = ((float) TimerValue / (float) TotalTimerValue) * 100F;

			float barFillValue = (555.0F / (100 / currentBarProgress));
			Timer.sizeDelta = new Vector2 (barFillValue, Timer.sizeDelta.y);
			EGTween.ValueTo (gameObject, EGTween.Hash ("from", (float)Timer.sizeDelta.x, "to", 0.0f, "time", TimerValue, "onUpdate", "BarProgress", "onComplete", "OnBarCompelete", "onCompleteTarget", gameObject));

		}
	}

	void ChangeBackgroundTheme(){
		Sprite newBackground;
		Image currentBackground = GetComponent<Image> ();
		ThemeData newTheme;

		if(currentBackground.sprite == null){
			print ("no current theme");
			newTheme = BackgroundThemes [Random.Range (0, BackgroundThemes.Count - 1)];
			newBackground = newTheme.backgroundImage;
		} else{
			BackgroundThemesCopy.Clear ();
			foreach (var theme in BackgroundThemes) {
				BackgroundThemesCopy.Add (theme);
			}
			previousTheme = currentTheme;
			BackgroundThemesCopy.Remove (previousTheme);
			print ("there's already a background. Pick from : " + (BackgroundThemesCopy.Count - 1));
			newTheme = BackgroundThemesCopy [Random.Range (0, BackgroundThemesCopy.Count - 1)];
			newBackground = newTheme.backgroundImage;
			currentTheme = newTheme;
		}
		currentTheme = newTheme;
		currentBackground.sprite = newBackground;
		if(OnBackgroundChange != null){
			OnBackgroundChange ();
		}
	}

	public void RestartTimer(int _timerValue)
	{
		EGTween.Stop (gameObject);
		TimerValue = _timerValue;
		SetTimer (0);
	}

	/// <summary>
	/// set the current progress to bar.
	/// </summary>
	/// <param name="progress">Progress.</param>
	void BarProgress(float progress)
	{
		if (progress <= 150) 
		{
			BarImage.color = new Color (218.0f/255,46.0f/255,46.0f/255);
		} else {
			BarImage.color = new Color (60.0f/255,156.0f/255,15.0f/255);
		}
		Timer.sizeDelta = new Vector2(progress, Timer.sizeDelta.y);

		TimerValue = (int) ((progress * TotalTimerValue) / 555.0F);

	}

	/// <summary>
	/// Raises the bar compelete event.
	/// calling game over...
	/// </summary>
	void OnBarCompelete()
	{
		StartCoroutine (LoadGameOver ("Time bar was over!"));
	}

	/// <summary>
	/// Raises the undo button pressed event.
	/// </summary>
	public void OnUndoButtonPressed	()
	{
		BlockManager.instance.ProcessUndo ();	
		AudioManager.instance.PlayButtonClickSound ();
	}

	#region IPointerDownHandler implementation

	/// <summary>
	/// Raises the pointer down event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnPointerDown (PointerEventData eventData)
	{
		if (eventData.pointerCurrentRaycast.gameObject != null) {
			GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
			if (clickedObject.name.Contains ("objcontainer")) {
				if (clickedObject.transform.childCount > 0) {
					blockSelected = true;
					SelectedObject = clickedObject.transform.GetChild (0).GetComponent<RectTransform> ();
					SelectedObject.FindChild ("blocksContainer").localScale = Vector3.one;
					SelectedObject.localScale = new Vector3 (0.9f, 0.9f, 0.9f);

					Vector3 pos = Camera.main.ScreenToWorldPoint (eventData.position);
					pos.z = SelectedObject.position.z;
					SelectedObject.position = pos;
				}
			}
		}
	}

	#endregion

	//int layerMask = 1 << LayerMask.NameToLayer ("yer");
	int layerMask = 1;
	#region IBeginDragHandler implementation

	/// <summary>
	/// Raises the begin drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnBeginDrag (PointerEventData eventData)
	{
		if (blockSelected && SelectedObject != null) {
			Vector3 pos = Camera.main.ScreenToWorldPoint (eventData.position);
			pos.z = SelectedObject.position.z;
			// set the position of the selected object to the world point of the click so that the user can see the object fully.
			SelectedObject.position = pos;

			// shoot a ray from the collider object of the selected block towards the board.
			RaycastHit2D hit = Physics2D.Raycast (SelectedObject.GetComponent<Block> ().ObjectDetails.ColliderObject.position, Vector2.zero, layerMask);
			// checking if the user has dragged over a block on the board.
			if (hit.collider != null) {
				if (hit.collider.name.Contains ("Block_")) {
					if (LastCheckedBlock != hit.collider.transform) {
						LastCheckedBlock = hit.collider.transform;
						CheckForHintBlocks (hit.collider.transform);
					}
				} else {
					ResetBlockColor ();
				}
			} else {
				ResetBlockColor ();
			}
		}
	}

	#endregion

	#region IDragHandler implementation

	/// <summary>
	/// Raises the drag event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnDrag (PointerEventData eventData)
	{
		if (blockSelected && SelectedObject != null) {

			if (GameDataManager.instance.isHelpRunning == 1) {
				if (transform.GetComponent<ClassicHelp_Gameplay> ()) {
					transform.GetComponent<ClassicHelp_Gameplay> ().StophandAnimation ();
				}
			}

			Vector3 pos = Camera.main.ScreenToWorldPoint (eventData.position);
			pos.z = SelectedObject.position.z;
			SelectedObject.position = pos;
			// shoot a ray from the chosen collider object of the block towards the board...
			RaycastHit2D hit = Physics2D.Raycast (SelectedObject.GetComponent<Block> ().ObjectDetails.ColliderObject.position, Vector2.zero, layerMask);
			// if it collides with something...
			if (hit.collider != null) {
				// and it's a block...
				if (hit.collider.name.Contains ("Block_")) {
					if (LastCheckedBlock != hit.collider.transform) {
						LastCheckedBlock = hit.collider.transform;
						CheckForHintBlocks (hit.collider.transform);
					}
				} else {
					ResetBlockColor ();
				}
			} else {
				ResetBlockColor ();
			}
		}
	}

	#endregion

	#region IPointerUpHandler implementation

	/// <summary>
	/// Raises the pointer up event.
	/// </summary>
	/// <param name="eventData">Event data.</param>
	public void OnPointerUp (PointerEventData eventData)
	{
		ResetBlockColor ();
		if (blockSelected) {
			if (SelectedObject != null) {
				//int layerMask = 1 << LayerMask.NameToLayer ("objBlockLayer");

				int layerMask = 1;

				RaycastHit2D hit = Physics2D.Raycast (SelectedObject.GetComponent<Block> ().ObjectDetails.ColliderObject.position, Vector2.zero, layerMask);
				if (hit.collider != null) {
					if (hit.collider.name.Contains ("Block_")) {
						bool CanplaceObject = CheckForEmptyBlocks (hit.collider.transform);
						if (CanplaceObject) {
							StartCoroutine ("PlaceObject", hit.collider.transform);		
						} else {
						//	put the block back
							SelectedObject.localScale = new Vector3 (0.6f, 0.6f, 1);
							SelectedObject.GetComponent<Block> ().ResetScaling ();
							SelectedObject.anchoredPosition = Vector2.zero;
						}
					} else {
						//	put the block back
						SelectedObject.localScale = new Vector3 (0.6f, 0.6f, 1);
						SelectedObject.GetComponent<Block> ().ResetScaling ();
						SelectedObject.anchoredPosition = Vector2.zero;
					}
				} else {
					//	put the block back
					SelectedObject.localScale = new Vector3 (0.6f, 0.6f, 1);
					SelectedObject.GetComponent<Block> ().ResetScaling ();
					SelectedObject.anchoredPosition = Vector2.zero;
				}
			}
		}
	}

	#endregion

	/// <summary>
	/// Places the object on given block.
	/// </summary>
	/// <param name="BlockToCheck">Block to check.</param>
	IEnumerator PlaceObject (Transform BlockToCheck)
	{
		if (BlockToCheck != null) {
			string[] id = BlockToCheck.name.Split ('_');
			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);
				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);
					if (data != null) {
						if (!data.isFilled) {
							
							Color ColorToSet = SelectedObject.GetComponent<Block> ().ObjectDetails.blockColor;
							List<BlockShapeDetails> ObjectBlocks = SelectedObject.GetComponent<Block> ().ObjectDetails.objectBlocksids;
							SelectedObject.transform.position = data.block.transform.position;
							foreach (BlockShapeDetails s  in ObjectBlocks) {
								BlockData chkBlock = BlockManager.instance.BlockList.Find (o => o.rowId == (rowId + s.rowID) && o.columnId == (columnId + s.columnId));
								if (chkBlock != null && !chkBlock.isFilled) {
									chkBlock.block.sprite = bubbleSprite;
									chkBlock.block.color = ColorToSet;
									chkBlock.isFilled = true;
									GameDataManager.instance.createBlockElement (chkBlock.rowId, chkBlock.columnId, ColorToSet, true);
								}
							}
							AudioManager.instance.PlayOneShotClip (SFX_BlockPlace);
							StartCoroutine (UpdateScore (ObjectBlocks.Count * 10));
						}
					}
				}
			}

			Invoke("SaveBoardData",0.2F);
		}

		blockSelected = false;
		Destroy (SelectedObject.gameObject);
		SelectedObject = null;
		yield return StartCoroutine( CheckForRowColumn());

		if (GameDataManager.instance.isHelpRunning == 0) 
		{
			BlockTrayManager.instance.OnPlacingBlock ();
		} 
		else 
		{
			if(GameDataManager.instance.isHelpRunning == 1)
			{
				GetComponent<ClassicHelp_Gameplay> ().PlaceObject ();
			}
		}
		CheckForRestObjectPlacing ();
		TotalMoves++;
		if (GamePlayMode == GameMode.bomb) {
			BombCounter ();
		}
	}

	/// <summary>
	/// Saves the board data.
	/// </summary>
	void SaveBoardData()
	{
		if (GameDataManager.instance.GameDoc != null) 
		{
			GameDataManager.instance.GameDoc.Root.Element ("totalScore").Attribute ("score").SetValue (BestScore.ToString());
			GameDataManager.instance.GameDoc.Root.Element ("currentScore").Attribute ("score").SetValue (Score.ToString ());
			GameDataManager.instance.GameDoc.Root.Element ("currentTheme").Attribute ("id").SetValue (currentTheme.id);

			GameDataManager.instance.lastMoveData = GameDataManager.instance.currentMoveData;
			GameDataManager.instance.currentMoveData = GameDataManager.instance.GameDoc.ToString ();
		
			if (GamePlayMode != GameMode.bomb && btnUndo != null) {
				btnUndo.gameObject.SetActive (true);
			}
		}
	}

	/// <summary>
	/// Checks for row column Filled or Not.
	/// </summary>
	IEnumerator CheckForRowColumn ()
	{
		List<List<BlockData>> BlocksToDestroy = new List<List<BlockData>> ();
		
		//CheckColumns
		int Count = 0;

		if (GamePlayMode == GameMode.hexa) {
			//ColumnList1
			for (int i = 0; i < column1List.Count; i++) {
				List<BlockData> blocklist = new List<BlockData> ();
				int startRowId = column1List [i].RowStartIndex;
				int startcolumnId = column1List [i].columnStartIndex;
				while (startRowId <= column1List [i].rowLastElement && startcolumnId <= column1List [i].columnLastElement) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == startRowId && o.columnId == startcolumnId);
					if (data != null) {
						if (!data.isFilled) {
							blocklist.Clear ();
							break;
						} else {
							blocklist.Add (data);
						}
					}
					startRowId += column1List [i].AddtoRow;
					startcolumnId += column1List [i].AddtoColumn;
					}
				if (blocklist.Count > 0) {
					Count++;
					BlocksToDestroy.Add (blocklist);
				}
			}

			//ColumnList2
			for (int i = 0; i < column2List.Count; i++) {
				List<BlockData> blocklist = new List<BlockData> ();
				int startRowId = column2List [i].RowStartIndex;
				int startcolumnId = column2List [i].columnStartIndex;
				while (startRowId <= column2List [i].rowLastElement && startcolumnId <= column2List [i].columnLastElement) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == startRowId && o.columnId == startcolumnId);
					if (data != null) {
						if (!data.isFilled) {
							blocklist.Clear ();
							break;
						} else {
							blocklist.Add (data);
						}
					}
					startRowId += column2List [i].AddtoRow;
					startcolumnId += column2List [i].AddtoColumn;
					//				yield return new WaitForSeconds(0.01f);
				}
				if (blocklist.Count > 0) {
					Count++;
					BlocksToDestroy.Add (blocklist);
				}
			}
		} else {
			for (int i = 0; i < BlockManager.instance.TotalColumns; i++) {
				List<BlockData> blocklist = BlockManager.instance.BlockList.FindAll (o => o.rowId <= BlockManager.instance.TotalRows && o.columnId == i);
				if (blocklist != null && blocklist.Count > 0) {
					if (blocklist.FindAll (o => o.isFilled == false).Count == 0) {
						Count++;
						BlocksToDestroy.Add (blocklist);
						if (GameDataManager.instance.isHelpRunning == 1) {
							CompletedOnhelp ("column");
						}
						SetTimer (5);
					}
				}
			}
		}

		//CheckRows
		for (int i = 0; i < BlockManager.instance.TotalRows; i++) {
			List<BlockData> blocklist = BlockManager.instance.BlockList.FindAll (o => o.rowId == i && o.columnId <= BlockManager.instance.TotalColumns);
			if (blocklist  != null && blocklist.Count > 0) {
				if (blocklist.FindAll (o => o.isFilled == false).Count == 0) {
					Count++;
					BlocksToDestroy.Add(blocklist);
					CompletedOnhelp ("row");
					SetTimer (5);
				}
			}
		}
		
		if(BlocksToDestroy != null && BlocksToDestroy.Count > 0)
		{
			if (Count == 1)
			{
				if (SFX_SingleLine.Length > 0) {
					AudioManager.instance.PlayOneShotClip (SFX_SingleLine[UnityEngine.Random.Range(0,SFX_SingleLine.Length)]);
				}
			} 
			else if (Count == 2)
			{
				if (SFX_DoubleLine.Length > 0) {
					AudioManager.instance.PlayOneShotClip (SFX_DoubleLine[UnityEngine.Random.Range(0,SFX_DoubleLine.Length)]);
				}
			} 
			else if (Count >= 3)
			{
				if (SFX_MultiLine.Length > 0) {
					AudioManager.instance.PlayOneShotClip (SFX_MultiLine[UnityEngine.Random.Range(0,SFX_MultiLine.Length)]);
				}
			}

			for (int i = 0; i < BlocksToDestroy.Count; i++) 
			{
				foreach (BlockData d in BlocksToDestroy[i]) 
				{
					GameObject currentBlock = (GameObject)Instantiate (sampleBlock.gameObject);
					currentBlock.transform.SetParent (sampleBlock.parent);
					currentBlock.transform.localScale = Vector3.one;
					currentBlock.GetComponent<RectTransform> ().position = d.block.GetComponent<RectTransform> ().position;
					currentBlock.GetComponent<Image> ().color = d.block.color;
					currentBlock.gameObject.SetActive (true);
					d.isFilled = false;
					d.block.GetComponent<Image> ().color = currentTheme.boardBlockColor;
					d.block.sprite = flatCircleSprite;

					GameDataManager.instance.createBlockElement (d.rowId, d.columnId, currentTheme.boardBlockColor, false);

//					if (GamePlayMode == GameMode.bomb && bombPlacingDetails != null && bombPlacingDetails.Count > 0) {
//						int index = bombPlacingDetails.FindIndex (o => o.block.rowId == d.rowId && o.block.columnId == d.columnId);
//						if (index != -1) {
//							BlockData objblock = BlockManager.instance.BlockList.Find (o => o.rowId == bombPlacingDetails [index].block.rowId && o.columnId == bombPlacingDetails [index].block.columnId);
//							if (objblock != null) {
//								if (objblock.block.transform.childCount > 0) {
//									objblock.block.sprite = blockImage;
//									objblock.block.color = currentTheme.boardBlockColor;
//									Destroy (objblock.block.transform.GetChild (0).gameObject);
//								}
//							}
//							bombPlacingDetails.RemoveAt (index);
//							GameDataManager.instance.removeBombNode (d.rowId, d.columnId);
//						}
//					}
					yield return new WaitForSeconds (0.01F);
				}
			}
		}
		
		if (Count > 0) 
		{
			int addingScore = (Count *(((Count - 1) * 50) + 100));
			StartCoroutine (UpdateScore (addingScore));
		}
		//return 0;
	}

	void BombCounter()
	{
		bool Gameover = false;
		if (bombPlacingDetails != null && bombPlacingDetails.Count > 0) {
			foreach (BombModedetails s in bombPlacingDetails) {
				s.counter--;
				if (s.block.block.transform.childCount > 0) {
					s.block.block.transform.GetChild (0).GetChild(0).GetComponent<Text> ().text = s.counter.ToString ();
				}

				if (s.counter == 0) {
					Gameover = true;
					foreach(BombModedetails b in bombPlacingDetails)
					{
						BlockData objblock = BlockManager.instance.BlockList.Find (o => o.rowId == b.block.rowId && o.columnId == b.block.columnId);
						if (objblock != null) {
							if (objblock.block.transform.childCount > 0) {
								objblock.block.sprite = blockImage;
								objblock.block.color = currentTheme.boardBlockColor;
								Destroy (objblock.block.transform.GetChild (0).gameObject);
							}
						}
					}
					bombPlacingDetails.Clear ();
					break;
				}
			}	
		}

		if (!Gameover) {
			PlaceBomb ();
		}
		else 
		{
			StartCoroutine (LoadGameOver ("Bomb counter was over!"));
		}
	}

	void PlaceBomb()
	{
		bool DoPlaceBomb = false;
		int NoOfBombToPlace = 1;
		if (TotalMoves % 5 == 0 && bombPlacingDetails != null && bombPlacingDetails.Count < 10) {
			if (TotalMoves > 100) {
				NoOfBombToPlace = bombPlacingDetails.Count+3 > 10 ? 10-bombPlacingDetails.Count : 3;
			} else if (TotalMoves > 50) {
				NoOfBombToPlace = bombPlacingDetails.Count+2 > 10 ? 10-bombPlacingDetails.Count : 2;
			}
			DoPlaceBomb = true;
		}

		if (DoPlaceBomb && bombPlacingDetails != null) 
		{
			if (DoPlaceBomb) {
				for (int i = 0; i < NoOfBombToPlace; i++) {
					List<BlockData> blockList = BlockManager.instance.BlockList.FindAll (o => o.isFilled == true && bombPlacingDetails.FindIndex (b => b.block.rowId == o.rowId && b.block.columnId == o.columnId) == -1);
					BlockData block = blockList [Random.Range (0, blockList.Count)];
					block.block.sprite = dynamiteImage;
					block.block.color = new Color (1, 1, 1, 1);
					GameObject Text = (GameObject)Instantiate (transform.FindChild ("GamePlay-Content/txt-Bomb").gameObject);
					Text.transform.SetParent (block.block.transform);
					Text.transform.localScale = Vector3.one;
					Text.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
					int Counter = Random.Range (6, 10);
					Text.transform.GetChild (0).GetComponent<Text> ().text = Counter.ToString ();
					Text.gameObject.SetActive (true);
					bombPlacingDetails.Add (new BombModedetails (block, Counter));
					GameDataManager.instance.createBombElement (block.rowId, block.columnId, Counter);
				}
			}
		}
	}

	/// <summary>
	/// Updates the score.
	/// </summary>
	/// <returns>The score.</returns>
	/// <param name="ScoreToUpdate">Score to update.</param>
	IEnumerator UpdateScore (int ScoreToUpdate)
	{
		int lastScore = Score;
		Score += ScoreToUpdate;
		if(Score > (scoreAtLastThemeChange + pointsBeforeThemeChange)){
			scoreAtLastThemeChange = Score;
			ChangeBackgroundTheme ();
		}

		PlayerPrefs.SetInt ("lastscore_" + GamePlayMode.ToString (), Score);

		txtScore.rectTransform.localScale = Vector3.one * 1.2F;
		for (int count = lastScore; count <= Score; count += (ScoreToUpdate / 10)) {
			txtScore.text = count.ToString ();
			yield return new WaitForSeconds (0.001F);
		}
		txtScore.text = Score.ToString ();
		txtScore.rectTransform.localScale = Vector3.one;

		if (Score > BestScore) {
			BestScore = Score;
			PlayerPrefs.SetInt ("bestscore_" + GamePlayMode.ToString (), BestScore);
			txtBestScore.text = "Best : " + BestScore.ToString ();
		}
	}

	/// <summary>
	/// Checks for rest object placing.
	/// </summary>
	void CheckForRestObjectPlacing ()
	{
		Transform nextBlocks = transform.FindChild ("GamePlay-Content").FindChild ("NextBlocks-Container");
		List<RectTransform> objectToplace = new List<RectTransform> ();

		foreach (Transform nextBlock in nextBlocks) {
			if (nextBlock.childCount > 0) {
				objectToplace.Add (nextBlock.GetChild (0).GetComponent<RectTransform> ());
			}
		}

		List<bool> CanPlaceObject = new List<bool> ();
		if (objectToplace.Count > 0) {
			foreach (RectTransform s in objectToplace) {
				SelectedObject = s;
				foreach (BlockData d in BlockManager.instance.BlockList) {
					if (!d.isFilled) {
						if (!CheckForEmptyBlocks (d.block.transform)) {
							CanPlaceObject.Add (false);
						} else {
							CanPlaceObject.Add (true);
							break;
						}
					}
				}

				if (CanPlaceObject.FindIndex (o => o == true) != -1) {
					break;
				}
			}

			if(CanPlaceObject.FindIndex(o=>o == true) == -1)
			{
				StartCoroutine(LoadGameOver("No Space to fit any block!"));
			}
		}
	}


	IEnumerator LoadGameOver(string reason)
	{
		txtGameOverReason.text = reason.ToString ();
		txtGameOverReason.transform.parent.gameObject.ScaleTo (EGTween.Hash ("X", 1, "Y", 1, "Z",1, "Time", 0.7F));
		yield return new WaitForSeconds (2F);
		txtGameOverReason.transform.parent.gameObject.ScaleTo (EGTween.Hash ("X", 0, "Y", 0, "Z",0, "Time", 0.7F));
		yield return new WaitForSeconds (0.4F);

		GameController.instance.SpawnUIScreen ("GameOver", true).GetComponent<GameOver> ().setScore (GamePlayMode, Score, BestScore,reason);
		AudioManager.instance.PlayOneShotClip (SFX_GameOver);

	}

	/// <summary>
	/// Checks for empty blocks.
	/// Check object is placed on this block or not
	/// </summary>
	/// <returns><c>true</c>, if empty blocks was checked, <c>false</c> otherwise.</returns>
	/// <param name="BlockToCheck">Block to check.</param>
	bool CheckForEmptyBlocks (Transform BlockToCheck)
	{
		bool canPlaceBlock = false;
		if (BlockToCheck != null) {
			string[] id = BlockToCheck.name.Split ('_');
			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);

				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);
					if (data != null) {
						if (!data.isFilled) {
							List<BlockShapeDetails> ObjectBlocks = SelectedObject.GetComponent<Block> ().ObjectDetails.objectBlocksids;
							bool BlockFilled = false;
							foreach (BlockShapeDetails s  in ObjectBlocks) {
								BlockData chkBlock = BlockManager.instance.BlockList.Find (o => o.rowId == (rowId + s.rowID) && o.columnId == (columnId + s.columnId));
								if (chkBlock != null && !chkBlock.isFilled) {
									
								} else {
									BlockFilled = true;
									break;
								}
							}
							canPlaceBlock = !BlockFilled;
						}
					}
				}
			}
		}

		return canPlaceBlock;
	}

	/// <summary>
	/// Resets the color of the block.
	/// </summary>
	void ResetBlockColor ()
	{
		if (hintColorBlocks != null && hintColorBlocks.Count > 0) {
			foreach (Image i in hintColorBlocks) {
				i.color = currentTheme.boardBlockColor;
			}
			hintColorBlocks.Clear ();
		}
	}

	/// <summary>
	/// Checks for empty blocks.
	/// Check object is placed on this block or not
	/// </summary>
	/// <returns><c>true</c>, if empty blocks was checked, <c>false</c> otherwise.</returns>
	/// <param name="BlockToCheck">Block to check.</param>
	void CheckForHintBlocks (Transform BlockToCheck)
	{
		ResetBlockColor ();
		if (BlockToCheck != null) {
			string[] id = BlockToCheck.name.Split ('_');

			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);

				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);

					if (data != null) {
						if (!data.isFilled) {
							// selected object is the block selected from the tray.
							Block objManager = SelectedObject.GetComponent<Block> ();
							List<BlockShapeDetails> ObjectBlocks = objManager.ObjectDetails.objectBlocksids;

							bool BlockFilled = false;
							// in the blockmanager, loop through the blockdata in the shape of the object selected.
							foreach (BlockShapeDetails s  in ObjectBlocks) {
								BlockData chkBlock = BlockManager.instance.BlockList.Find (o => o.rowId == (rowId + s.rowID) && o.columnId == (columnId + s.columnId));
								if (chkBlock != null && !chkBlock.isFilled) {
									hintColorBlocks.Add (chkBlock.block.GetComponent<Image> ());
								} else {
								// if the user can't place the object there, don't show hints.
									hintColorBlocks.Clear ();
									BlockFilled = true;
									break;
								}
							}

							// if there's a hint to show... 
							if (!BlockFilled && hintColorBlocks != null && hintColorBlocks.Count > 0) {
								foreach (Image i in hintColorBlocks) {
									i.color = new Color (objManager.ObjectDetails.blockColor.r, objManager.ObjectDetails.blockColor.g, objManager.ObjectDetails.blockColor.b, 0.5f);
								}
							}
						}
					}
				}
			}
		}
	}



	/// <summary>
	/// if Help is Running Than This Method Will Trigger next step for Help
	/// </summary>
	/// <param name="Completed">Completed.</param>
	void CompletedOnhelp(string Completed)
	{
		switch(GameDataManager.instance.isHelpRunning)
		{
		case 1:
			if (Completed == "row") {
				GetComponent<ClassicHelp_Gameplay> ().OnRowComplete ();
			} else {
				GetComponent<ClassicHelp_Gameplay> ().OnColumnComplete();
			}
			break;
		}
	}



	void EnableSettingsMenu()
	{
		if (SettingsContent.instance.settings_gameplay != null) {
			SettingsContent.instance.settings_gameplay.gameObject.SetActive(true);
			SettingsContent.instance.transform.SetAsLastSibling();
		}
	}
	
	void DisableSettingsMenu() {
		if (SettingsContent.instance.settings_gameplay != null) {
			SettingsContent.instance.settings_gameplay.gameObject.SetActive(false);
		}
	}

//  Disabled for amazeballs
//	void OnEnable()
//	{
//		ThemeManager.OnThemeChangedEvent += OnThemeChangedEvent;
//	}
//
//	void OnDisable()
//	{
//		ThemeManager.OnThemeChangedEvent -= OnThemeChangedEvent;
//		DisableSettingsMenu ();
//	}
//
//
//	void OnThemeChangedEvent (bool isDarkTheme)
//	{
//		blockColor = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
//	}
}

public class BombModedetails
{
	public BlockData block;
	public int counter;

	public BombModedetails(BlockData _block,int _counter)
	{
		this.block = _block;
		this.counter = _counter;
	}
}


[System.Serializable]
public class RowColumnData
{
	public int RowStartIndex;
	public int AddtoRow;
	[Space]
	public int columnStartIndex;
	public int AddtoColumn;
	[Space]
	public int rowLastElement;
	public int columnLastElement;
}

[System.Serializable]
public class ThemeData{
	public int id;
	public Sprite backgroundImage;
	public Color boardBlockColor;
}