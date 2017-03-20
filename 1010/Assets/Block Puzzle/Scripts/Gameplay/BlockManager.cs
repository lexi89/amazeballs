using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

public class BlockManager : MonoBehaviour 
{
	public int TotalRows = 10;
	public int TotalColumns = 10;
	public List<BlockData> BlockList = new List<BlockData>();
	public static BlockManager instance;
	public Sprite defaultBlockImage;
	public Sprite bubbleSprite;
	public Color blockColor;
	public Color[] ThemeColors;

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
	}

	void Start()
	{
		if (GamePlay.GamePlayMode == GameMode.hexa) {
			TotalRows = 9;
			TotalColumns = 9;
		} else {
			TotalRows = 10;
			TotalColumns = 10;
		}
		blockColor = GamePlay.instance.currentTheme.boardBlockColor;
		InitializeBlocks ();

	}

	/// <summary>
	/// Initializes the blockList.
	/// </summary>
	public void InitializeBlocks()
	{
		Transform obj;
		int blockId = 0;

		for(int i = 0;i<TotalRows;i++)
		{
			for(int j = 0;j<TotalColumns;j++)
			{
				obj = transform.FindChild ("Block_" + i + "_" + j);
	
				if (obj != null) 
				{
					obj.GetComponent<Image>().color = blockColor;

					if (GameDataManager.instance.PlayFromLastStatus) 
					{
						XElement rootElemnt = GameDataManager.instance.GameDoc.Root;
						XElement BlockElement = rootElemnt.Elements ("block").Where (o => o.Attribute ("row").Value == i.ToString () && o.Attribute ("col").Value == j.ToString ()).FirstOrDefault ();
						XElement BombElement = rootElemnt.Elements ("bomb").Where (o => o.Attribute ("row").Value == i.ToString () && o.Attribute ("col").Value == j.ToString ()).FirstOrDefault ();
						bool isFilled = false;

						if (BlockElement != null) 
						{
							Color _blockColor = new Color (
								                   float.Parse (BlockElement.Element ("color").Attribute ("r").Value),
								                   float.Parse (BlockElement.Element ("color").Attribute ("g").Value),
								                   float.Parse (BlockElement.Element ("color").Attribute ("b").Value)
							                   );
							if(BlockElement.Attribute ("isFilled").Value.ToString () == "true"){
								isFilled = true;
								obj.GetComponent<Image> ().sprite = bubbleSprite;
								obj.GetComponent<Image> ().color = _blockColor;
							}

//							if (BombElement != null) 
//							{
//								obj.GetComponent<Image> ().sprite = GamePlay.instance.dynamiteImage;
//								obj.GetComponent<Image> ().color = new Color (1, 1, 1, 1);
//								GameObject Text = (GameObject)Instantiate (GamePlay.instance.transform.FindChild ("GamePlay-Content/txt-Bomb").gameObject);
//								Text.transform.SetParent (obj.transform);
//								Text.transform.localScale = Vector3.one;
//								Text.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
//								Text.transform.GetChild (0).GetComponent<Text> ().text = BombElement.Attribute ("number").Value;
//								Text.gameObject.SetActive (true);
//								GamePlay.instance.bombPlacingDetails.Add (new BombModedetails (new BlockData (blockId, i, j, true, obj.GetComponent<Image> ()), int.Parse (BombElement.Attribute ("number").Value)));
//							}
						}
						BlockList.Add (new BlockData (blockId, i, j, isFilled, obj.GetComponent<Image> ()));
					}
					else 
					{
						BlockList.Add (new BlockData (blockId, i, j, false, obj.GetComponent<Image> ()));
						GameDataManager.instance.createBlockElement (i, j, GamePlay.instance.currentTheme.boardBlockColor, false);
					}
				}
				blockId++;
			}
		}

		// if continuing... load the score and timer.
		if (GameDataManager.instance.GameDoc != null && GameDataManager.instance.PlayFromLastStatus) {
			GamePlay.instance.Score = int.Parse (GameDataManager.instance.GameDoc.Root.Element ("currentScore").Attribute ("score").Value);
			GamePlay.instance.txtScore.text = GamePlay.instance.Score.ToString ();

			PlayerPrefs.SetString ("GameData", string.Empty);

			if (GamePlay.GamePlayMode == GameMode.timer) {

				if (GameDataManager.instance.GameDoc.Root.Element ("timerValue") != null) 
				{
					int savedTimer = int.Parse (GameDataManager.instance.GameDoc.Root.Element ("timerValue").Attribute ("time").Value);
					GamePlay.instance.RestartTimer (savedTimer);
				}
			}
		}
		else 
		{
			GameDataManager.instance.currentMoveData = GameDataManager.instance.GameDoc.ToString ();
		}
		GameDataManager.instance.PlayFromLastStatus = false;
	}

	/// <summary>
	/// Processes the undo.
	/// </summary>
	public void ProcessUndo()
	{
		// reset the blocklist.
		BlockList = new List<BlockData> ();

		if (GameDataManager.instance.lastMoveData != string.Empty) 
		{
			// go back one move.
			GameDataManager.instance.currentMoveData = GameDataManager.instance.lastMoveData;
			// parse the last move data
			XDocument doc = XDocument.Parse (GameDataManager.instance.lastMoveData);
			// work from this new parse.
			GameDataManager.instance.GameDoc = doc;
			print ("currentmovedata: " + GameDataManager.instance.currentMoveData);
			Transform obj;
			int blockId = 0;
						
			for (int i = 0; i<TotalRows; i++) 
			{
				for (int j = 0; j<TotalColumns; j++) 
				{
					obj = transform.FindChild ("Block_" + i + "_" + j);

					if (obj != null) 
					{
						obj.GetComponent<Image> ().color = GamePlay.instance.currentTheme.boardBlockColor;
						XElement rootElemnt = doc.Root;
						XElement BlockElement = rootElemnt.Elements ("block").Where (o => o.Attribute ("row").Value == i.ToString () && o.Attribute ("col").Value == j.ToString ()).FirstOrDefault ();
//						XElement BombElement = rootElemnt.Elements ("bomb").Where (o => o.Attribute ("row").Value == i.ToString () && o.Attribute ("col").Value == j.ToString ()).FirstOrDefault ();
						bool isFilled = false;
						obj.GetComponent<Image> ().sprite = defaultBlockImage;
						if (BlockElement != null) {
							
							Color _blockColor = new Color (
									float.Parse (BlockElement.Element ("color").Attribute ("r").Value),
									float.Parse (BlockElement.Element ("color").Attribute ("g").Value),
									float.Parse (BlockElement.Element ("color").Attribute ("b").Value));
							if(BlockElement.Attribute ("isFilled").Value.ToString () == "true"){
								isFilled = true;
								obj.GetComponent<Image> ().sprite = bubbleSprite;
								obj.GetComponent<Image> ().color = _blockColor;
							}

//							if (_blockColor != ThemeColors [0] && _blockColor != ThemeColors [1]) {
//								print("in not theme..");
//								isFilled = true;
//								obj.GetComponent<Image> ().color = _blockColor;
//							}

//							if (BombElement != null) 
//							{
//								obj.GetComponent<Image> ().sprite = GamePlay.instance.dynamiteImage;
//								obj.GetComponent<Image> ().color = new Color (1, 1, 1, 1);
//								GameObject Text = (GameObject)Instantiate (GamePlay.instance.transform.FindChild ("GamePlay-Content/txt-Bomb").gameObject);
//								Text.transform.SetParent (obj.transform);
//								Text.transform.localScale = Vector3.one;
//								Text.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
//								Text.transform.GetChild (0).GetComponent<Text> ().text = BombElement.Attribute ("number").Value;
//								Text.gameObject.SetActive (true);
//								GamePlay.instance.bombPlacingDetails.Add (new BombModedetails (new BlockData (blockId, i, j, true, obj.GetComponent<Image> ()), int.Parse (BombElement.Attribute ("number").Value)));
//							}
						}
						BlockList.Add (new BlockData (blockId, i, j, isFilled, obj.GetComponent<Image> ()));
					}
					blockId++;
				}
			}


			GamePlay.instance.Score = int.Parse (doc.Root.Element ("currentScore").Attribute ("score").Value);
			GamePlay.instance.txtScore.text = GamePlay.instance.Score.ToString();

			if (GamePlay.GamePlayMode == GameMode.timer) 
			{
				if (doc.Root.Element ("timerValue") != null) 
				{
					GamePlay.instance.TimerValue = int.Parse (doc.Root.Element ("timerValue").Attribute ("time").Value);
//					float PercentageValue = (GamePlay.instance.TotalTimerValue * 100) / GamePlay.instance.TotalTimerValue;
//					GamePlay.instance.Timer.sizeDelta = new Vector2 ((PercentageValue * 555) / 100, GamePlay.instance.Timer.sizeDelta.y);
				}
			}
			// get the blocks back in the tray.
			BlockTrayManager.instance.SpawnSuggetedBlocks ();
		}
		else 
		{
			Transform obj;
			int blockId = 0;

			for(int i = 0;i<TotalRows;i++)
			{
				for(int j = 0;j<TotalColumns;j++)
				{
					obj = transform.FindChild ("Block_" + i + "_" + j);
					
					if (obj != null) 
					{
						obj.GetComponent<Image>().color = blockColor;
						BlockList.Add (new BlockData (blockId, i, j, false, obj.GetComponent<Image> ()));
						GameDataManager.instance.createBlockElement (i, j, GamePlay.instance.currentTheme.boardBlockColor, false);
					}
				}
			}

			GamePlay.instance.Score = 0;
			GamePlay.instance.txtScore.text = GamePlay.instance.Score.ToString();

			GameDataManager.instance.GameDoc.Root.Element ("currentScore").Attribute ("score").SetValue (GamePlay.instance.Score.ToString ());
		}
			
		GamePlay.instance.btnUndo.gameObject.SetActive (false);
		GameDataManager.instance.lastMoveData = string.Empty;
		//GameController.instance.ResetGameData ();
//		updateUnfilledBlockColor ();
	}

	void updateUnfilledBlockColor(){
		print ("updating the unfilled blocks");
		foreach (var blockData in BlockList) {
			if(!blockData.isFilled){
				print("found one");
				blockData.block.color = GamePlay.instance.currentTheme.boardBlockColor;
			}
		}
	}
	/// <summary>
	/// ReInitializes the blockList.
	/// </summary>
	public void ReInitializeBlocks()
	{
		Transform obj;
		int blockId = 0;
		BlockList.Clear ();
		for(int i = 0;i<TotalRows;i++)
		{
			for(int j = 0;j<TotalColumns;j++)
			{
				obj = transform.FindChild ("Block_" + i + "_" + j);
				if (obj != null) {
					BlockList.Add (new BlockData (blockId, i, j, false, obj.GetComponent<Image> ()));
					obj.GetComponent<Image> ().color = blockColor;
					obj.GetComponent<Image> ().sprite = defaultBlockImage;
					GameDataManager.instance.createBlockElement (i, j, blockColor, false);
					blockId++;
				}
			}
		}
		GameDataManager.instance.GameDoc.Root.Descendants ().Where (e => e.Name == "bomb").Remove ();
	}

//  Disabled for Amazeballs.
	void OnEnable()
	{
		print ("gameplay in enable: " + GamePlay.instance);
		GamePlay.instance.OnBackgroundChange += OnBackgroundChange;
//		ThemeManager.OnThemeChangedEvent += OnThemeChangedEvent;
	}

	void OnDisable()
	{
		print ("gameplay in disable: " + GamePlay.instance);

		GamePlay.instance.OnBackgroundChange -= OnBackgroundChange;

//		ThemeManager.OnThemeChangedEvent -= OnThemeChangedEvent;
	}

	void OnBackgroundChange ()
	{
		for (int i = 0; i < transform.childCount; i++) {
			// change each block that isn't filled to the new theme block color.
			blockColor = GamePlay.instance.currentTheme.boardBlockColor;
			Transform boardBlockToCheck = transform.GetChild (i);
			string[] id = boardBlockToCheck.name.Split ('_');
			if (id.Length >= 3) {
				int rowId = id [1].TryParseInt (-1);
				int columnId = id [2].TryParseInt (-1);
				if (rowId != -1 && columnId != -1) {
					BlockData data = BlockManager.instance.BlockList.Find (o => o.rowId == rowId && o.columnId == columnId);
					if(data != null){
						if(!data.isFilled){
							boardBlockToCheck.GetComponent<Image> ().color = GamePlay.instance.currentTheme.boardBlockColor;
						}
					}
				}
			}
		}
	}
//
//	void OnThemeChangedEvent (bool isDarkTheme)
//	{
//		blockColor = (isDarkTheme) ? ThemeColors [0] : ThemeColors [1];
//		foreach (Transform t in transform) {
//			Image img = t.GetComponent<Image> ();
//			if (img.color == ThemeColors [0] || img.color == ThemeColors [1]) {
//				t.GetComponent<Image> ().color = blockColor;
//			}
//		}
//	}
}

public class BlockData
{
	public int blockId;
	public int rowId;
	public int columnId;
	public bool isFilled;
	public Image block;

	public BlockData (int blockId, int rowId, int columnId, bool isFilled,Image block)
	{
		this.blockId = blockId;
		this.rowId = rowId;
		this.columnId = columnId;
		this.isFilled = isFilled;
		this.block = block;
	}	
}
