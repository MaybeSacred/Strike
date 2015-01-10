using UnityEngine;
using System.Collections;

public class SplashMenu : MonoBehaviour {
	public GUIStyle splashMenuGuiStyle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void OnLoadGameMenu(){
		Application.LoadLevel("GameMenu");
	}
	public void OnLoadOptions(){
		Application.LoadLevel("Options");
	}
}
