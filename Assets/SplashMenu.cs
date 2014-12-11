using UnityEngine;
using System.Collections;

public class SplashMenu : MonoBehaviour {
	public GUIStyle splashMenuGuiStyle;
	public Texture2D backgroundImage;
	public float buttonWidthPct, buttonHeightPct;
	public float buttonHeightOffsetPct;
	public int numberOfButtons;
	// Use this for initialization
	void Start () {
		if(Screen.width != backgroundImage.width)
		{
			TextureScale.Bilinear(backgroundImage, Screen.width, Screen.height);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnGUI()
	{
		GUI.Label(new Rect(0, 0, Screen.width, Screen.height), backgroundImage, splashMenuGuiStyle);
		GUILayout.BeginArea(new Rect(Screen.width/2 - .5f * buttonWidthPct * Screen.width, Screen.height*buttonHeightOffsetPct, buttonWidthPct*Screen.width, buttonHeightPct*Screen.height * numberOfButtons));
		GUILayout.BeginVertical();
		if(GUILayout.Button("Skirmish"))
		{
			Application.LoadLevel("GameMenu");
		}
		if(GUILayout.Button("Options"))
		{
			Application.LoadLevel("Options");
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
