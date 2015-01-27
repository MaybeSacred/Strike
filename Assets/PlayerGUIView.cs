using UnityEngine;
using System.Collections;

public class PlayerGUIView : MonoBehaviour {
	public Player thisPlayer;
	//Displays and selects player colour
	public UnityEngine.UI.Slider colorSelectSlider;
	//Used to select player side
	public UnityEngine.UI.Slider playerSideSlider;
	//Player name text
	public UnityEngine.UI.Text nameText;
	//Player side text
	public UnityEngine.UI.Text sideText;
	
	// Use this for initialization
	void Awake() {
		thisPlayer = Instantiate(thisPlayer) as Player;
		ChangeSliderHue(Random.Range(0, 359));
	}
	void Start(){
		/*List<RectTransform> mapButtons = new List<RectTransform>();
		int count = 0;
		foreach(string name in mapNames){
			RectTransform t = InstantiateUIPrefab(mapNameLoadButton, mapNamePanel);
			t.GetComponentsInChildren<UnityEngine.UI.Text>(true)[0].text = name;
			mapButtons.Add(t);
			int captured = count;
			//add our delegate to the onClick handler, with appropriate indexing
			t.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => {SetCurrentMap(captured);});
			count++;
			smallestFontSize = Mathf.Min(smallestFontSize, t.GetComponentsInChildren<UnityEngine.UI.Text>(true)[0].fontSize);
		}*/
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	/// <summary>
	/// Returns the player associated with this gui block
	/// </summary>
	/// <returns>The player.</returns>
	public Player GetPlayer(){
		return thisPlayer;
	}
	
	/// <summary>
	/// Changes the hue of a slider
	/// </summary>
	public void ChangeSliderHue(float hue){
		Color col = HSVtoRGB(hue, 1, 1);
		colorSelectSlider.GetComponentInChildren<UnityEngine.UI.Image>().color = col;
		thisPlayer.mainPlayerColor = col;
	}
	/// <summary>
	/// Sets the maximum number of sides based on the current map
	/// </summary>
	/// <param name="maxSides">Max sides.</param>
	public void SetMaxSides(int maxSides){
		playerSideSlider.maxValue = maxSides;
	}
	/// <summary>
	/// Sets the side of the player and displays this back to the user
	/// </summary>
	/// <param name="newSide">New side.</param>
	public void ChangeSide(float newSide){
		thisPlayer.side = Mathf.RoundToInt(newSide);
		sideText.text = "Side: " + thisPlayer.side.ToString();
		
	}
	/// <summary>
	/// Sets the name of the player, if legal
	/// </summary>
	/// <param name="newName">New name.</param>
	public void SetPlayerName(string newName){
		if(NameIsValid(newName)){
			thisPlayer.playerName = thisPlayer.name = newName;
		}
	}
	/// <summary>
	/// Checks whether a player name is legal
	/// </summary>
	/// <returns><c>true</c>, if is valid was named, <c>false</c> otherwise.</returns>
	/// <param name="input">Input.</param>
	bool NameIsValid(string input){
		if(input.Contains("fuck")){
			return false;
		}
		return true;
	}
	/// <summary>
	/// Displays a drop down menu selecter in a legal place
	/// </summary>
	public void DisplayDropDownMenu(){
		
	}
	/// <summary>
	/// Converts a color in hsv to rgb colour space
	/// </summary>
	/// <returns>A Color object.</returns>
	/// <param name="h">hue</param>
	/// <param name="s">saturation</param>
	/// <param name="v">value</param>
	Color HSVtoRGB(float h, float s, float v )
	{
		int i;
		float f, p, q, t;
		Color outColor = new Color();
		outColor.a = 1;
		if( s == 0 ) {
			// achromatic (grey)
			outColor.r = outColor.g = outColor.b = v;
			return outColor;
		}
		h /= 60;			// sector 0 to 5
		i = Mathf.FloorToInt(h);
		f = h - i;			// factorial part of h
		p = v * ( 1 - s );
		q = v * ( 1 - s * f );
		t = v * ( 1 - s * ( 1 - f ) );
		
		switch( i ) {
		case 0:
			outColor.r = v;
			outColor.g = t;
			outColor.b = p;
			break;
		case 1:
			outColor.r = q;
			outColor.g = v;
			outColor.b = p;
			break;
		case 2:
			outColor.r = p;
			outColor.g = v;
			outColor.b = t;
			break;
		case 3:
			outColor.r = p;
			outColor.g = q;
			outColor.b = v;
			break;
		case 4:
			outColor.r = t;
			outColor.g = p;
			outColor.b = v;
			break;
		default:		// case 5:
			outColor.r = v;
			outColor.g = p;
			outColor.b = q;
			break;
		}
		return outColor;
	}
}
