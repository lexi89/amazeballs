using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml.Linq;

/// <summary>
/// Block tray manager.
/// This component have all the blocks that will be used during gameplay, it will spawn random blocks based on the probability of the blocks.
/// </summary>
public class BlockTrayManager : MonoBehaviour
{
	public static BlockTrayManager instance;
	public Transform blockContainer;
	public List<Transform> blockList;
	List<int> ProbabilityPool = new List<int> ();
	float blockTransitionTime = 0.5F;
	//bool verticalHelp_classicMode = false;


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
		FillProbabilityPool ();
		startGame ();
	}

	/// <summary>
	/// Starts the game.
	/// </summary>
	public void startGame ()
	{
		int placedBlocks = 0;
		if (GameDataManager.instance.isHelpRunning == 0)
		{
			//OnPlacingBlock ();
//			string suggestedblock1 = GameController.instance.GameDoc.Root.Element("suggestedObject1").Attribute("objectName").Value;
//			string suggestedblock2 = GameController.instance.GameDoc.Root.Element("suggestedObject2").Attribute("objectName").Value;
//			string suggestedblock3 = GameController.instance.GameDoc.Root.Element("suggestedObject3").Attribute("objectName").Value;
//
//			Debug.Log (suggestedblock1 + " : " + suggestedblock2 + " : " + suggestedblock3);

			for (int i = 0; i < 3; i++) 
			{
				GameObject obj = null;
				string ObjectName = GameDataManager.instance.GameDoc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
				ObjectName = ObjectName.Replace ("(Clone)", "");
				int index = blockList.FindIndex (o => o.name == ObjectName);

				if (index != -1) 
				{
					//index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
					obj = (GameObject)Instantiate (blockList [index].gameObject);
					
					obj.transform.SetParent (blockContainer.GetChild (i).transform);
					obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
					obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
					obj.gameObject.SetActive (true);

					placedBlocks++;
				}
			}
			EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
		}

		if (placedBlocks == 0) 
		{
			OnPlacingBlock ();
			StartCoroutine ("UpdateBlocksInCurrentMoveData");
		}
	}

	/// <summary>
	/// Resets the game For Replay level.
	/// </summary>
	public void ResetGame ()
	{
		ProbabilityPool.Clear ();
		FillProbabilityPool ();

		GameDataManager.instance.GameDoc = new XDocument ();
		GameDataManager.instance.GameDoc.Declaration = new XDeclaration ("1.0","UTF-16","no");
		XElement resources = new XElement ("resources");
		XElement totalScore = new XElement ("totalScore", new XAttribute ("score", ""));
		XElement currentScore = new XElement ("currentScore", new XAttribute ("score", "0"));
		XElement timerValue = new XElement ("timerValue", new XAttribute ("time", ""));
		XElement currentMode = new XElement ("currentMode", new XAttribute ("modeId", ""));
		XElement suggestedObject1 = new XElement ("suggestedObject1", new XAttribute ("objectName", ""));
		XElement suggestedObject2 = new XElement ("suggestedObject2", new XAttribute ("objectName", ""));
		XElement suggestedObject3 = new XElement ("suggestedObject3", new XAttribute ("objectName", ""));
		resources.Add (totalScore);
		resources.Add (currentScore);
		resources.Add (timerValue);
		resources.Add (currentMode);
		resources.Add (suggestedObject1);
		resources.Add (suggestedObject2);
		resources.Add (suggestedObject3);
		GameDataManager.instance.GameDoc.Add (resources);

		BlockManager.instance.ReInitializeBlocks ();
		foreach (Transform block in blockContainer) {
			if (block.childCount > 0) {
				Destroy (block.GetChild (0).gameObject);
			}
		}	

		GamePlay.instance.txtScore.text = "0";
		GamePlay.instance.Score = 0;
		GamePlay.instance.TotalMoves = 0;

		//GamePlay.instance.TimerValue = GamePlay.instance.TotalTimerValue;
	
		foreach(BombModedetails b in GamePlay.instance.bombPlacingDetails)
		{
			BlockData objblock = BlockManager.instance.BlockList.Find (o => o.rowId == b.block.rowId && o.columnId == b.block.columnId);
			if (objblock != null) {
				if (objblock.block.transform.childCount > 0) {
					objblock.block.sprite = GamePlay.instance.blockImage;
					objblock.block.color = GamePlay.instance.currentTheme.boardBlockColor;
					Destroy (objblock.block.transform.GetChild (0).gameObject);
				}
			}
		}
		GamePlay.instance.bombPlacingDetails.Clear ();

		Invoke ("OnPlacingBlock", 0.5f);

		if (GamePlay.GamePlayMode == GameMode.timer) {
			GamePlay.instance.Timer.sizeDelta = new Vector2 (555f, GamePlay.instance.Timer.sizeDelta.y);
			GamePlay.instance.SetTimer ();
		}
		PlayerPrefs.DeleteKey ("GameData");
		//PlayerPrefs.DeleteKey ("lastMoveData");
	}


	public void SpawnSuggetedBlocks()
	{
		for (int i = 0; i < 3; i++) 
		{
			if (blockContainer.GetChild (i).childCount > 0) 
			{
				Destroy (blockContainer.GetChild (i).GetChild (0).gameObject);
			}
		}

		int placedBlocks = 0;

		XDocument doc = XDocument.Parse (GameDataManager.instance.lastMoveData);
		for (int i = 0; i < 3; i++)
		{
			GameObject obj = null;

			string ObjectName = doc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
			ObjectName = ObjectName.Replace ("(Clone)", "");
			int index = blockList.FindIndex (o => o.name == ObjectName);

			if (index != -1) 
			{
				//index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
				obj = (GameObject)Instantiate (blockList [index].gameObject);

				obj.transform.SetParent (blockContainer.GetChild (i).transform);
				obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
				obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
				obj.gameObject.SetActive (true);

				placedBlocks++;
			}
		}
		EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));

	}

	/// <summary>
	/// Fills the probability pool. Calculates the probability of the all blocks.
	/// </summary>
	void FillProbabilityPool ()
	{
		if (GamePlay.GamePlayMode == GameMode.hexa) {
			for (int i = 0; i < blockList.Count; i++) {
				for (int index = 0; index < blockList [i].GetComponent<Block> ().blockProbability; index++) {
					ProbabilityPool.Add (i);
				}
			}
		} else {
			for (int i = 0; i < (GameDataManager.instance.isHelpRunning == 0 ? (GamePlay.GamePlayMode == GameMode.plus ? blockList.Count : 19) : blockList.Count); i++) {
				for (int index = 0; index < blockList [i].GetComponent<Block> ().blockProbability; index++) {
					ProbabilityPool.Add (i);
				}
			}
		}

		ShuffleGenericList (ProbabilityPool);
	}

	/// <summary>
	/// Swaps the object.
	/// </summary>
	/// <param name="parentBlock">Parent block.</param>
	/// <param name="blockToTansit">Block to tansit.</param>
	public void swapObject (Transform parentBlock, Transform blockToTansit)
	{
		blockToTansit.SetParent (parentBlock);
		EGTween.MoveTo (blockToTansit.gameObject, EGTween.Hash ("isLocal", true, "x", 0, "time", blockTransitionTime));
	}

	/// <summary>
	/// Raises the placing block event.
	/// </summary>
	public void OnPlacingBlock ()
	{
		int blockRemained = 0;
		foreach (Transform t in blockContainer) 
		{
			if (t.childCount > 0) {
				blockRemained++;
			}
		}	
//		if (blockRemained == 2) 
//		{
//			for (int i = 1; i <= 3; i++) {
//				if (blockContainer.GetChild (i - 1).childCount <= 0) {
//					if (i == 1) {
//						if (blockContainer.GetChild (i).childCount > 0) {
//							swapObject (blockContainer.GetChild (i - 1), blockContainer.GetChild (i).GetChild (0));
//							swapObject (blockContainer.GetChild (i), blockContainer.GetChild (i + 1).GetChild (0));
//						}
//					} else if (i == 2) {
//						if (blockContainer.GetChild (i).childCount >= 0) {
//							swapObject (blockContainer.GetChild (i - 1), blockContainer.GetChild (i).GetChild (0));
//						}
//					} else {
//						GameObject obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
//						obj.transform.SetParent (blockContainer.GetChild (i - 1).transform);
//						obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
//						obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
//						obj.gameObject.SetActive (true);
//						EGTween.MoveFrom (obj, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
//					}
//				}
//			}
//		} 
//		else if (blockRemained == 0) 
//		{
//			for (int i = 0; i < 3; i++) {
//				GameObject obj = null;
//				if (GameController.instance.PlayFromLastStatus) {
//					string ObjectName = GameController.instance.GameDoc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
//					ObjectName = ObjectName.Replace ("(Clone)", "");
//					int index = blockList.FindIndex (o => o.name == ObjectName);
//					if (index == -1) {
//						index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
//					}
//					obj = (GameObject)Instantiate (blockList [index].gameObject);
//				}
//				else
//				{	
//					obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
//				}
//
//				obj.transform.SetParent (blockContainer.GetChild (i).transform);
//				obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
//				obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
//				obj.gameObject.SetActive (true);
//			}
//			EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
//		}
//
//		if (GamePlay.instance != null && GameController.instance.GameDoc != null) {
//			GameController.instance.GameDoc.Root.Element ("suggestedObject1").Attribute ("objectName").SetValue (blockContainer.GetChild (0).GetChild(0).transform.name);
//			GameController.instance.GameDoc.Root.Element ("suggestedObject2").Attribute ("objectName").SetValue (blockContainer.GetChild (1).GetChild(0).transform.name);
//			GameController.instance.GameDoc.Root.Element ("suggestedObject3").Attribute ("objectName").SetValue (blockContainer.GetChild (2).GetChild(0).transform.name);
//		}

//		if (blockRemained == 0) 
//		{
//			for (int i = 0; i < 3; i++) 
//			{
//				GameObject obj = null;
//				if (GameDataManager.instance.PlayFromLastStatus) {
//					string ObjectName = GameDataManager.instance.GameDoc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
//					ObjectName = ObjectName.Replace ("(Clone)", "");
//					int index = blockList.FindIndex (o => o.name == ObjectName);
//					if (index == -1) {
//						index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
//					}
//					obj = (GameObject)Instantiate (blockList [index].gameObject);
//				}
//				else
//				{	
//					obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
//				}
//
//				obj.transform.SetParent (blockContainer.GetChild (i).transform);
//				obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
//				obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
//				obj.gameObject.SetActive (true);
//			}
//			EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
//		}





		string block1Name = "";
		string block2Name = "";
		string block3Name = "";

		if (blockContainer.GetChild (0).childCount > 0) {
			block1Name = blockContainer.GetChild (0).GetChild (0).name;
		}
		if (GamePlay.instance.upcomingBlockFillMode == UpcomingBlockFillMode.AddNewOnAllEmpty) {
			if (blockRemained == 0) 
			{
				for (int i = 0; i < 3; i++) 
				{
					GameObject obj = null;
					if (GameDataManager.instance.PlayFromLastStatus) {
						string ObjectName = GameDataManager.instance.GameDoc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
						ObjectName = ObjectName.Replace ("(Clone)", "");
						int index = blockList.FindIndex (o => o.name == ObjectName);
						if (index == -1) {
							index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
						}
						obj = (GameObject)Instantiate (blockList [index].gameObject);
					}
					else
					{	
						obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
					}

					obj.transform.SetParent (blockContainer.GetChild (i).transform);
					obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
					obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
					obj.gameObject.SetActive (true);
				}
				EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
			}
		} else {
			if (blockRemained == 2) 
			{
				for (int i = 1; i <= 3; i++) {
					if (blockContainer.GetChild (i - 1).childCount <= 0) {
						if (i == 1) {
							if (blockContainer.GetChild (i).childCount > 0) {
								swapObject (blockContainer.GetChild (i - 1), blockContainer.GetChild (i).GetChild (0));
								swapObject (blockContainer.GetChild (i), blockContainer.GetChild (i + 1).GetChild (0));
							}
						} else if (i == 2) {
							if (blockContainer.GetChild (i).childCount >= 0) {
								swapObject (blockContainer.GetChild (i - 1), blockContainer.GetChild (i).GetChild (0));
							}
						} else {
							GameObject obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
							obj.transform.SetParent (blockContainer.GetChild (i - 1).transform);
							obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
							obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
							obj.gameObject.SetActive (true);
							EGTween.MoveFrom (obj, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
						}
					}
				}
			} 
			else if (blockRemained == 0) 
			{
				for (int i = 0; i < 3; i++) {
					GameObject obj = null;
					if (GameDataManager.instance.PlayFromLastStatus) {
						string ObjectName = GameDataManager.instance.GameDoc.Root.Element("suggestedObject"+(i+1)).Attribute("objectName").Value;
						ObjectName = ObjectName.Replace ("(Clone)", "");
						int index = blockList.FindIndex (o => o.name == ObjectName);
						if (index == -1) {
							index = ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)];
						}
						obj = (GameObject)Instantiate (blockList [index].gameObject);
					}
					else
					{	
						obj = (GameObject)Instantiate (blockList [ProbabilityPool [UnityEngine.Random.Range (0, ProbabilityPool.Count)]].gameObject);
					}

					obj.transform.SetParent (blockContainer.GetChild (i).transform);
					obj.GetComponent<RectTransform> ().anchoredPosition3D = Vector3.zero;
					obj.transform.localScale = new Vector3 (0.6f, 0.6f, 1);
					obj.gameObject.SetActive (true);
				}
				EGTween.MoveFrom (blockContainer.gameObject, EGTween.Hash ("isLocal", true, "x", 50, "time", blockTransitionTime));
			}
		}
		if (blockContainer.GetChild (1).childCount > 0) {
			block2Name = blockContainer.GetChild (1).GetChild (0).name;
		}

		if (blockContainer.GetChild (2).childCount > 0) {
			block3Name = blockContainer.GetChild (2).GetChild (0).name;
		}

		GameDataManager.instance.GameDoc.Root.Element ("suggestedObject1").Attribute ("objectName").SetValue (block1Name);
		GameDataManager.instance.GameDoc.Root.Element ("suggestedObject2").Attribute ("objectName").SetValue (block2Name);
		GameDataManager.instance.GameDoc.Root.Element ("suggestedObject3").Attribute ("objectName").SetValue (block3Name);
	
//		Debug.Log ("This is called..");
//
//		GameController.instance.currentMoveData = GameController.instance.GameDoc.ToString ();
	}

	IEnumerator UpdateBlocksInCurrentMoveData()
	{
		yield return new WaitForSeconds (1);

		if (!GameDataManager.instance.currentMoveData.Equals (string.Empty)) {
			XDocument xDoc = XDocument.Parse (GameDataManager.instance.currentMoveData);

			string block1Name = "";
			string block2Name = "";
			string block3Name = "";

			if (blockContainer.GetChild (0).childCount > 0) {
				block1Name = blockContainer.GetChild (0).GetChild (0).name;
			}

			if (blockContainer.GetChild (1).childCount > 0) {
				block2Name = blockContainer.GetChild (1).GetChild (0).name;
			}

			if (blockContainer.GetChild (2).childCount > 0) {
				block3Name = blockContainer.GetChild (2).GetChild (0).name;
			}

			xDoc.Root.Element ("suggestedObject1").Attribute ("objectName").SetValue (block1Name);
			xDoc.Root.Element ("suggestedObject2").Attribute ("objectName").SetValue (block2Name);
			xDoc.Root.Element ("suggestedObject3").Attribute ("objectName").SetValue (block3Name);

			GameDataManager.instance.currentMoveData = xDoc.ToString ();
		}
	}

	/// <summary>
	/// Shuffles the generic list.
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public void ShuffleGenericList<T> (List<T> list)
	{  
		System.Random rng = new System.Random (); 
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next (n + 1);  
			T value = list [k];  
			list [k] = list [n];  
			list [n] = value;  
		}  
	}
}