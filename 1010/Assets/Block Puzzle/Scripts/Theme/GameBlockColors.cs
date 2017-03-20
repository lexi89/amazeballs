using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BlockColorName
{
	PINK,
	TEAL,
	LIGHT_BLUE,
	YELLOW,
	ORANGE,
	PURPLE,
//
//	RED,
//	GREEN,
//	BLUE,
//	PURPLE,
//	YELLOW,
//	ORANGE,
//	BROWN
}

public class GameBlockColors : MonoBehaviour 
{
	public List <colorData> DarkThemeBlockColorData  = new List<colorData>();
	public List <colorData> LightThemeBlockColorData  = new List<colorData>();

	public static GameBlockColors instance;

	void Awake()
	{
		if (instance == null) {
			instance = this;
			return;
		}
		Destroy (gameObject);
	}
}

[System.Serializable]
public class colorData
{
	public BlockColorName blockColorName;
	public Color color;
}
