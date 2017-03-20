using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Xml.Linq;
using System.Linq;

public class GameDataManager : MonoBehaviour {

	public static GameDataManager instance;

	/// <summary>
	/// The game document.
	/// GameDoc :- xml for BlockDetails if Left Game InBetween Gaameplay...
	/// </summary>
	public XDocument GameDoc;


	/// <summary>
	/// isHelpRunning Is To mention that help is running on screen or not
	/// isHelpRunning = 0 : Means No Help isRunning
	/// isHelpRunning = 1 : Means Help isRunning for Mode Classic
	/// isHelpRunning = 2 : Means Help isRunning for Mode Bomb
	/// isHelpRunning = 3 : Means Help isRunning for Mode Plus
	/// </summary>
	public int isHelpRunning = 0;

	/// <summary>
	/// Wheather to apply data from which game was left or not
	/// </summary>
	public bool PlayFromLastStatus = false;
	//public List<string> undoData = new List<string> ();

	public string lastMoveData = string.Empty;
	public string currentMoveData = string.Empty;
	public bool ActivingLastSession = false;

	void Awake()
	{
		if (instance == null) {
			instance = this;
			return;
		}
		Destroy (gameObject);
	}

	void Start()
	{
		Invoke ("CheckForLastStatus",0.2f);
	}

	public void OnEnable()
	{
		GameDoc = new XDocument ();
		GameDoc.Declaration = new XDeclaration ("1.0","UTF-16","no");
		XElement resources = new XElement ("resources");
		XElement totalScore = new XElement ("totalScore", new XAttribute ("score", ""));
		XElement currentScore = new XElement ("currentScore", new XAttribute ("score", "0"));
		XElement currentTheme = new XElement ("currentTheme", new XAttribute ("id", "0"));
		XElement timerValue = new XElement ("timerValue", new XAttribute ("time", ""));
		XElement currentMode = new XElement ("currentMode", new XAttribute ("modeId", ""));
		XElement suggestedObject1 = new XElement ("suggestedObject1", new XAttribute ("objectName", ""));
		XElement suggestedObject2 = new XElement ("suggestedObject2", new XAttribute ("objectName", ""));
		XElement suggestedObject3 = new XElement ("suggestedObject3", new XAttribute ("objectName", ""));
		resources.Add (totalScore);
		resources.Add (currentScore);
		resources.Add (timerValue);
		resources.Add (currentMode);
		resources.Add (currentTheme);
		resources.Add (suggestedObject1);
		resources.Add (suggestedObject2);
		resources.Add (suggestedObject3);
		GameDoc.Add (resources);
	}


	void CheckForLastStatus()
	{
		if (PlayerPrefs.GetString ("GameData", string.Empty) != string.Empty) 
		{
			GameDoc = XDocument.Parse (PlayerPrefs.GetString ("GameData", string.Empty));
			XElement root = GameDoc.Root;

			int lastScore = 0;
			int.TryParse (GameDoc.Root.Element ("currentScore").Attribute ("score").Value, out lastScore);

			if (lastScore > 0 && root.Elements ("block").Count () > 0) 
			{
				PlayFromLastStatus = true;
				GameMode mode = (GameMode)(int.Parse (GameDoc.Root.Element ("currentMode").Attribute ("modeId").Value));
				if (mode == GameMode.classic) {
					GamePlay.GamePlayMode = GameMode.classic;
				} else if (mode == GameMode.bomb) {
					GamePlay.GamePlayMode = GameMode.bomb;
				} else if (mode == GameMode.plus) {
					GamePlay.GamePlayMode = GameMode.plus;
				} else if (mode == GameMode.hexa) {
					GamePlay.GamePlayMode = GameMode.hexa;
				} else {
					GamePlay.GamePlayMode = GameMode.timer;
				}

				if (GamePlay.GamePlayMode == GameMode.hexa) {
					GameObject Gameplay =  GameController.instance.SpawnUIScreen ("GamePlay_hex", true);
					Gameplay.name = "GamePlay";
				} else {
					GameObject Gameplay = GameController.instance.SpawnUIScreen ("GamePlay", true);
					Gameplay.SetActive (true);
				}

				lastMoveData = PlayerPrefs.GetString ("lastMoveData", string.Empty);

				currentMoveData = GameDoc.ToString ();
				SettingsContent.instance.settings_main.gameObject.SetActive (false);
				ActivingLastSession = true;
			} 
			else 
			{
				ResetGameData ();
				lastMoveData = string.Empty;
				ActivingLastSession = false;
			}
		} 

		ShowInitialContent ();
	}

	void ShowInitialContent()
	{
		bool ShowingReview = false;

		if (!ActivingLastSession) 
		{			
			if ( (PlayerPrefs.GetInt ("HasReviewedApp", 0) == 0) && DateTimeManager.instance.lastOpenedDays >= 1) 
			{
				ShowingReview = true;
			}

			if (ShowingReview) {
				GameController.instance.SpawnUIScreen ("ReviewScreen", true);
			} 
		}
	}

	public void ResetGameData()
	{
		GameDoc = new XDocument ();
		GameDoc.Declaration = new XDeclaration ("1.0","UTF-16","no");
		XElement resources = new XElement ("resources");
		XElement totalScore = new XElement ("totalScore", new XAttribute ("score", ""));
		XElement currentScore = new XElement ("currentScore", new XAttribute ("score", "0"));
		XElement timerValue = new XElement ("timerValue", new XAttribute ("time", ""));
		XElement currentMode = new XElement ("currentMode", new XAttribute ("modeId", ""));
		XElement currentTheme = new XElement ("currentTheme", new XAttribute ("id", "0"));

		XElement suggestedObject1 = new XElement ("suggestedObject1", new XAttribute ("objectName", ""));
		XElement suggestedObject2 = new XElement ("suggestedObject2", new XAttribute ("objectName", ""));
		XElement suggestedObject3 = new XElement ("suggestedObject3", new XAttribute ("objectName", ""));
		resources.Add (currentTheme);
		resources.Add (totalScore);
		resources.Add (currentScore);
		resources.Add (timerValue);
		resources.Add (currentMode);
		resources.Add (suggestedObject1);
		resources.Add (suggestedObject2);
		resources.Add (suggestedObject3);
		GameDoc.Add (resources);

		PlayerPrefs.DeleteKey ("GameData");
		PlayerPrefs.DeleteKey ("lastMoveData");
		lastMoveData = string.Empty;
		currentMoveData = string.Empty;
	}

	/// <summary>
	/// Creates the bomb element in Xml.
	/// </summary>
	/// <returns>The bomb element.</returns>
	/// <param name="rowId">Row identifier.</param>
	/// <param name="columnId">Column identifier.</param>
	/// <param name="bomb">Bomb Counter.</param>
	public XElement createBombElement(int rowId,int columnId,int bomb)
	{
		XElement bombelement = (from e in GameDoc.Root.Elements ("bomb")
			where (e.Attribute ("row").Value == rowId.ToString () && e.Attribute ("col").Value == columnId.ToString ())
			select e).FirstOrDefault ();

		if (bombelement == null) 
		{
			bombelement = new XElement ("bomb");
			XAttribute row1 = new XAttribute ("row",rowId.ToString());
			XAttribute col1 = new XAttribute ("col",columnId.ToString());
			XAttribute number1 = new XAttribute ("number",bomb.ToString());
			bombelement.Add (row1);
			bombelement.Add (col1);
			bombelement.Add (number1);
			GameDoc.Root.Add (bombelement);	
		} else {
			bombelement.Attribute ("number").SetValue (bomb.ToString ());
		}

		return bombelement;
	}

	/// <summary>
	/// Creates the block element.
	/// </summary>
	/// <returns>The block element.</returns>
	/// <param name="rowId">Row identifier.</param>
	/// <param name="columnId">Column identifier.</param>
	/// <param name="blockColor">Block color.</param>
	/// <param name="isFilled">Is it filled?.</param>
	public XElement createBlockElement(int rowId,int columnId,Color blockColor, bool isFilled)
	{
		XElement element = (from e in GameDoc.Root.Elements ("block")
			where (e.Attribute ("row").Value == rowId.ToString () && e.Attribute ("col").Value == columnId.ToString ())
			select e).FirstOrDefault ();

		if (element == null) {
			element = new XElement ("block");
			XAttribute row = new XAttribute ("row", rowId.ToString ());
			XAttribute col = new XAttribute ("col", columnId.ToString ());
			XAttribute newIsFilled = new XAttribute("isFilled", isFilled);
			element.Add(newIsFilled);
			element.Add (row);
			element.Add (col);

			XElement colorelement = new XElement ("color");
			XAttribute r = new XAttribute ("r", blockColor.r.ToString ());
			XAttribute g = new XAttribute ("g", blockColor.g.ToString ());
			XAttribute b = new XAttribute ("b", blockColor.b.ToString ());
			colorelement.Add (r);
			colorelement.Add (g);
			colorelement.Add (b);

			element.Add (colorelement);

			GameDoc.Root.Add (element);
		} else {
			XElement colorelement = element.Element("color");
			colorelement.Attribute("r").SetValue(blockColor.r.ToString ());
			colorelement.Attribute("g").SetValue(blockColor.g.ToString ());
			colorelement.Attribute("b").SetValue(blockColor.b.ToString ());
			element.Attribute ("isFilled").SetValue (isFilled);
		}
		return element;
	}

	public void UpdateSuggestedBlocksInGameDoc()
	{
		XElement element = GameDoc.Root.Element ("suggestedObject1");
		Debug.Log(element.ToString());
	}

	/// <summary>
	/// Removes the bomb node From Xml.
	/// </summary>
	/// <param name="rowId">Row identifier.</param>
	/// <param name="columnId">Column identifier.</param>
	public void removeBombNode(int rowId,int columnId)
	{
		GameDoc.Root.Elements ("bomb").Where (e=>  e.Attribute ("row").Value == rowId.ToString () && e.Attribute ("col").Value == columnId.ToString ()).Remove();
	}

	//	if game goes in background the gamedata will be saved if game is suspended from background
	void OnApplicationFocus(bool Focus)
	{
		if (!Focus && GameController.instance.WindowStack.Count > 0 )
		{
			GameObject PeekWiondow = GameController.instance.WindowStack.Peek ();
			if (GamePlay.instance != null && (PeekWiondow != null && (PeekWiondow.name == "GamePlay" || PeekWiondow.name == "GamePlay_hex" || PeekWiondow.name == "Settings-Screen-GamePlay" || PeekWiondow.name == "Quit-Confirm-Play"))) {
				GameDoc.Root.Element ("totalScore").Attribute ("score").SetValue (GamePlay.instance.BestScore.ToString ());
				GameDoc.Root.Element ("currentMode").Attribute ("modeId").SetValue (((int)GamePlay.GamePlayMode).ToString ());

//				float percentageValue = (GamePlay.instance.Timer.sizeDelta.x * 100) / 555f;
//				int timer = (int)(percentageValue * 120) / 100;
				GameDoc.Root.Element ("timerValue").Attribute ("time").SetValue (GamePlay.instance.TimerValue.ToString ());

				PlayerPrefs.SetString ("GameData", GameDoc.ToString ());
				PlayerPrefs.SetString ("lastMoveData", lastMoveData);
			} 
		}
	}
}
