using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
	[SerializeField]
	public BlockShape ObjectDetails;
	public int blockProbability;
	Vector3 OrigionalScale;

	public BlockColorName thisBlockColorName;

	colorData DarkColorData;
	colorData LightColorData;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	void Awake ()
	{
		OrigionalScale = transform.FindChild ("blocksContainer").localScale;

		//MyClass result = list.Find(x => x.GetId() == "xy");

//		DarkColorData = GameBlockColors.instance.DarkThemeBlockColorData.Find (x => x.blockColorName.ToString () == thisBlockColorName.ToString ());
//		LightColorData = GameBlockColors.instance.DarkThemeBlockColorData.Find (x => x.blockColorName.ToString () == thisBlockColorName.ToString ());
	}

	void Start()
	{
		SetColorOfItsParts ();
//		ThemeManager_OnThemeChangedEvent (ThemeManager.instance.isDarkTheme);
	}

	void OnEnable()
	{
//		ThemeManager.OnThemeChangedEvent += ThemeManager_OnThemeChangedEvent;
	}


	void OnDisable()
	{
//		ThemeManager.OnThemeChangedEvent -= ThemeManager_OnThemeChangedEvent;
	}

	void SetColorOfItsParts(){
		Image[] imagesOfParts = GetComponentsInChildren<Image> ();
		foreach (var image in imagesOfParts) {
			image.color = ObjectDetails.blockColor;
		}
	}

	void ThemeManager_OnThemeChangedEvent (bool isDarkTheme)
	{
//		foreach (Transform t in transform.GetChild(0)) 
//		{
//			t.GetComponent<Image> ().color = (isDarkTheme) ? DarkColorData.color : LightColorData.color;
//			ObjectDetails.blockColor = (isDarkTheme) ? DarkColorData.color : LightColorData.color;
//		}
	}

	/// <summary>
	/// Resets the scaling of the block to original scale.
	/// </summary>
	public void ResetScaling ()
	{
		transform.FindChild ("blocksContainer").localScale = OrigionalScale;
	}
}

[System.Serializable]
public class BlockShapeDetails
{
	public int rowID;
	public int columnId;
}

/// <summary>
/// This class contains all the property related to block.
/// </summary>
[System.Serializable]
public class BlockShape
{
	public int blockID;
	public int totalBlocks;
	public int totalRows;
	public int totalColumns;
	[SerializeField]
	public List<BlockShapeDetails> objectBlocksids;
	public Color blockColor;
	public RectTransform ColliderObject;

	/// <summary>
	/// Initializes a new instance of the <see cref="BlockShape"/> class.
	/// </summary>
	/// <param name="objectId">Object identifier.</param>
	/// <param name="totalBlocks">Total blocks.</param>
	/// <param name="totalRows">Total rows.</param>
	/// <param name="totalColumns">Total columns.</param>
	/// <param name="objectBlocksids">Object blocksids.</param>
	/// <param name="colliderObject">Collider object.</param>
	/// <param name="blockColor">Block color.</param>
	public BlockShape (int objectId, int totalBlocks, int totalRows, int totalColumns, List<BlockShapeDetails> objectBlocksids, RectTransform colliderObject, Color blockColor)
	{
		this.blockID = objectId;
		this.totalBlocks = totalBlocks;
		this.totalRows = totalRows;
		this.totalColumns = totalColumns;
		this.objectBlocksids = objectBlocksids;
		this.ColliderObject = colliderObject;
		this.blockColor = blockColor;
		this.blockColor = colliderObject.GetComponent<Image> ().color;
	}
}
