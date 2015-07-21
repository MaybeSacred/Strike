using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Linq;
public class PlayerGUIView : MonoBehaviour
{
	public Player thisPlayer;
	// Displays and selects player colour
	public Slider colorSelectSlider;
	// Used to select player side
	public Slider playerSideSlider;
	// Player name text
	public Text nameText;
	// Player side text
	public Text sideText;
	// ButtonPrototype is used for all dropdowns
	public RectTransform buttonPrototype;
	// Buttons that display general and AI
	public RectTransform generalTopButton, AITopButton;
	// Containers for static dropdowns
	public RectTransform generalSelectDropdown, AISelectDropdown;
	// Buttons within each dropdown
	List<RectTransform> generalDropdownButtons, AIDropdownButtons;
	// Offset for the spacing of dropdown buttons
	public float buttonOffset;
	
	static PlayerGUIView playerSelected;
	
	public bool started = false;
	static HashSet<string> naughtyWords;
	// Use this for initialization
	void Awake ()
	{
		if (naughtyWords == null) {
			naughtyWords = new HashSet<string> ();
			StartCoroutine (GetNaughtyWords ());
		}
	}
	IEnumerator GetNaughtyWords ()
	{
		var names = new WWW (SkirmishMenuViewer.ApplicationServerURL + @"/Data/naughty.txt");
		while (!names.isDone) {
			yield return new WaitForEndOfFrame ();
		}
		MemoryStream ms = new MemoryStream (names.bytes);
		StreamReader sr = new StreamReader (ms);
		while (!sr.EndOfStream) {
			naughtyWords.Add (sr.ReadLine ());
		}
	}
	void Start ()
	{
		thisPlayer = Instantiate (thisPlayer) as Player;
		thisPlayer.Setup (UnityEngine.Random.Range (1, 4), Generals.Taron, Color.red, "");
		// Initialize sliders
		playerSideSlider.value = thisPlayer.side;
		ChangeSide (thisPlayer.side);
		int tempRandom = UnityEngine.Random.Range (0, 359);
		ChangeSliderHue (tempRandom);
		colorSelectSlider.value = tempRandom;
		// Set up dropdowns
		generalSelectDropdown = SkirmishMenuViewer.InstantiateUIPrefab (generalSelectDropdown, generalTopButton);
		var generalEnumList = System.Enum.GetValues (typeof(Generals)).Cast<Generals> ().ToList ();
		generalSelectDropdown = SkirmishMenuViewer.InstantiateDropdown<Generals> (generalSelectDropdown, buttonPrototype, generalEnumList, buttonOffset,
			SetGeneral, x => Utilities.GetGeneral (x).GetComponent<TooltipData> ().mouseOverText);
		if (GetComponent<RectTransform> ().localPosition.y > 0) {
			generalSelectDropdown.offsetMin = new Vector2 (0, -generalTopButton.rect.height - generalEnumList.Count * buttonOffset);
			generalSelectDropdown.offsetMax = new Vector2 (0, -generalTopButton.rect.height);
		}
		generalSelectDropdown.SetParent (GetComponent<RectTransform> ().parent);
		generalSelectDropdown.gameObject.SetActive (false);
		
		AISelectDropdown = SkirmishMenuViewer.InstantiateUIPrefab (AISelectDropdown, AITopButton);
		var AIEnumList = System.Enum.GetValues (typeof(AILevel)).Cast<AILevel> ().ToList ();
		AISelectDropdown = SkirmishMenuViewer.InstantiateDropdown<AILevel> (AISelectDropdown, buttonPrototype, AIEnumList, buttonOffset, SetAILevel);
		if (GetComponent<RectTransform> ().localPosition.y > 0) {
			AISelectDropdown.offsetMin = new Vector2 (0, -AITopButton.rect.height - AIEnumList.Count * buttonOffset);
			AISelectDropdown.offsetMax = new Vector2 (0, -AITopButton.rect.height);
		}
		AISelectDropdown.SetParent (GetComponent<RectTransform> ().parent);
		AISelectDropdown.gameObject.SetActive (false);
		started = true;
	}
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButtonUp (1)) {
			generalSelectDropdown.gameObject.SetActive (false);
			AISelectDropdown.gameObject.SetActive (false);
			playerSelected = null;
		}
	}
	/// <summary>
	/// Opens the general select dropdown.
	/// </summary>
	public void OpenGeneralSelectDropdown ()
	{
		if (playerSelected == null || playerSelected == this) {
			AISelectDropdown.gameObject.SetActive (false);
			generalSelectDropdown.gameObject.SetActive (true);
			playerSelected = this;
		}
	}
	/// <summary>
	/// Sets the general from dropdown menu
	/// <param name="input">Input.</param>
	public void SetGeneral (Generals input)
	{
		thisPlayer.generalSelectedInGUI = input;
		generalTopButton.GetComponentInChildren<Text> ().text = input.ToString ();
		generalSelectDropdown.gameObject.SetActive (false);
		playerSelected = null;
	}
	/// <summary>
	/// Opens the AI select dropdown.
	/// </summary>
	public void OpenAISelectDropdown ()
	{
		if (playerSelected == null || playerSelected == this) {
			AISelectDropdown.gameObject.SetActive (true);
			generalSelectDropdown.gameObject.SetActive (false);
			playerSelected = this;
		}
	}
	/// <summary>
	/// Sets the AI level.
	/// </summary>
	/// <param name="input">Input.</param>
	public void SetAILevel (AILevel input)
	{
		thisPlayer.aiLevel = input;
		AITopButton.GetComponentInChildren<Text> ().text = input.ToString ();
		AISelectDropdown.gameObject.SetActive (false);
		playerSelected = null;
	}
	
	/// <summary>
	/// Changes the hue of a slider
	/// </summary>
	public void ChangeSliderHue (float hue)
	{
		Color col = HSVtoRGB (hue, 1, 1);
		colorSelectSlider.GetComponentInChildren<UnityEngine.UI.Image> ().color = col;
		thisPlayer.mainPlayerColor = col;
	}
	/// <summary>
	/// Sets the maximum number of sides based on the current map
	/// </summary>
	/// <param name="maxSides">Max sides.</param>
	public void SetMaxSides (int maxSides)
	{
		playerSideSlider.maxValue = maxSides;
	}
	/// <summary>
	/// Sets the side of the player and displays this back to the user
	/// </summary>
	/// <param name="newSide">New side.</param>
	public void ChangeSide (float newSide)
	{
		thisPlayer.side = Mathf.RoundToInt (newSide);
		sideText.text = "Side: " + thisPlayer.side.ToString ();
		playerSideSlider.value = newSide;
	}
	/// <summary>
	/// Sets the name of the player, if legal
	/// </summary>
	/// <param name="newName">New name.</param>
	public void SetPlayerName (string newName)
	{
		if (NameIsValid (newName)) {
			thisPlayer.playerName = newName;
		}
	}
	/// <summary>
	/// Checks whether a player name is legal
	/// </summary>
	/// <returns><c>true</c>, if is valid was named, <c>false</c> otherwise.</returns>
	/// <param name="input">Input.</param>
	bool NameIsValid (string input)
	{
		if (input.Contains ("--Neutral--")) {
			return false;
		}
		if (naughtyWords.Contains (input)) {
			return false;
		}
		if (input.Trim ().Equals ("")) {
			return false;
		}
		return true;
	}
	/// <summary>
	/// Displays a drop down menu selecter in a legal place
	/// </summary>
	public void DisplayDropDownMenu ()
	{
		
	}
	/// <summary>
	/// Validate the contained player's properties
	/// Returns false if any of the values were not set properly
	/// </summary>
	public bool IsValid ()
	{
		if (isActiveAndEnabled) {
			return thisPlayer.IsValid ();
		}
		return true;
	}
	/// <summary>
	/// Converts a color in hsv to rgb colour space
	/// </summary>
	/// <returns>A Color object.</returns>
	/// <param name="h">hue</param>
	/// <param name="s">saturation</param>
	/// <param name="v">value</param>
	Color HSVtoRGB (float h, float s, float v)
	{
		int i;
		float f, p, q, t;
		Color outColor = new Color ();
		outColor.a = 1;
		if (s == 0) {
			// achromatic (grey)
			outColor.r = outColor.g = outColor.b = v;
			return outColor;
		}
		h /= 60;			// sector 0 to 5
		i = Mathf.FloorToInt (h);
		f = h - i;			// factorial part of h
		p = v * (1 - s);
		q = v * (1 - s * f);
		t = v * (1 - s * (1 - f));
		
		switch (i) {
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
