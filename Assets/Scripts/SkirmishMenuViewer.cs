// //Created by Jon Tyson : jtyson3@gatech.edu
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;


public class SkirmishMenuViewer : MonoBehaviour
{
	public static SkirmishMenuViewer instance;
	List<MapData> maps;
	string[] mapNames;
	string selectedMapName;
	MapData selectedMap;
	
	public PlayerGUIView[] players;
	string[] generalNames;
	GameSettings settings;
	public Player playerPrototype;
	//new GUI stuff							//parent panel to load buttons to
	public RectTransform mapNameLoadButton, mapNamePanel, 
		// Outer panel of button panel
		mapNameOuterPanel,
		scrollBar;
	public UnityEngine.UI.Text mapNameText;
	// Spacing for between button centers
	public int mapNameButtonOffset;
	public RectTransform mapSelect, playerSelect, optionsSelect;
	public GameObject loadingDisplay;
	public UnityEngine.UI.Button mapNamesOpenButton, setPlayersButton;
	//current root folder where map data are located
	public static string ApplicationServerURL = "https://dl.dropboxusercontent.com/u/65011402/strike";
	void Awake ()
	{
		instance = this;
	}
	// Use this for initialization
	void Start ()
	{
		playerSelect.gameObject.SetActive (true);
		settings = new GameSettings ();
		generalNames = System.Enum.GetNames (typeof(Generals));
		mapNameOuterPanel.gameObject.SetActive (false);
		scrollBar.gameObject.SetActive (false);
		StartCoroutine (LoadMapsAsync ());
		StartCoroutine (FinishedLoadingPlayers ());
		optionsSelect.gameObject.SetActive (false);
	}
	/// <summary>
	/// Callback when all player gui's have loaded
	/// </summary>
	/// <returns>The loading player.</returns>
	IEnumerator FinishedLoadingPlayers ()
	{
		foreach (PlayerGUIView pg in players) {
			while (!pg.started) {
				yield return new WaitForSeconds (.01f);
			}
		}
		SwitchToMapSelect ();
		for (int i = 0; i < players.Length; i++) {
			players [i].ChangeSide (i + 1);
		}
		mapNamesOpenButton.interactable = false;
		setPlayersButton.interactable = false;
	}
	/// <summary>
	/// Loads the maps after the map names have been loaded
	/// </summary>
	/// <returns>The maps async.</returns>
	IEnumerator LoadMapsAsync ()
	{
		yield return StartCoroutine (GetMapNames ());
		loadingDisplay.SetActive (false);
		mapNameOuterPanel.gameObject.SetActive (true);
		scrollBar.gameObject.SetActive (true);
		int smallestFontSize = int.MaxValue;
		List<RectTransform> mapButtons = new List<RectTransform> ();
		for (int i = 0; i < mapNames.Length; i++) {
			RectTransform t = InstantiateUIPrefab (mapNameLoadButton, mapNamePanel);
			t.GetComponentsInChildren<UnityEngine.UI.Text> (true) [0].text = MapData.FormatMapName (mapNames [i]);
			mapButtons.Add (t);
			mapButtons [i].gameObject.AddComponent<TooltipData> ().mouseOverText = "Players: " + maps [i].maxPlayers + "\n" + maps [i].mapDescription;
			string captured = mapNames [i].ToString ();
			//add our delegate to the onClick handler, with appropriate indexing
			Encapsulator (t, captured);
			smallestFontSize = Mathf.Min (smallestFontSize, t.GetComponentsInChildren<UnityEngine.UI.Text> (true) [0].fontSize);
		}
		var offset = -mapNameButtonOffset / 2;
		foreach (RectTransform rt in mapButtons) {
			rt.anchoredPosition3D = new Vector3 (0, offset, 0);
			offset -= mapNameButtonOffset;
		}
		offset += mapNameButtonOffset / 2;
		if (Mathf.Abs (offset) > mapNamePanel.rect.height) {
			mapNamePanel.SetInsetAndSizeFromParentEdge (RectTransform.Edge.Bottom, mapNamePanel.rect.height, -offset);
		}
		SetCurrentMap (mapNames [0]);
		mapNameOuterPanel.gameObject.SetActive (false);
		scrollBar.gameObject.SetActive (false);
		mapNamesOpenButton.interactable = true;
		setPlayersButton.interactable = true;
	}
	/// <summary>
	/// Overcomes a possible bug between mono/Unity and lambdas/anonymous functions
	/// </summary>
	/// <param name="t">T.</param>
	/// <param name="input">Input.</param>
	void Encapsulator (RectTransform t, string input)
	{
		t.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener (() => {
			SetCurrentMap (input);
			OnMapSelected ();
		});
	}
	/// <summary>
	/// Sets the current mapData from the provided map name
	/// </summary>
	/// <param name="map">Map.</param>
	void SetCurrentMap (string map)
	{
		selectedMapName = map;
		foreach (MapData mp in maps) {
			if (mp.mapName.Equals (selectedMapName)) {
				selectedMap = mp;
				// Do map loading and viewing here
				GetComponentInChildren<MenuBackgroundMapDisplayer> ().DisplayMap (selectedMap);
				return;
			}
		}
		throw new UnityException ("Could not find map");
	}
	/// <summary>
	/// Opens the map dropdown
	/// </summary>
	public void OnMapDropdownOpened ()
	{
		mapNameOuterPanel.gameObject.SetActive (true);
		scrollBar.gameObject.SetActive (true);
	}
	/// <summary>
	/// Called when a map is selected, sets the name of the map to the map name display
	/// </summary>
	public void OnMapSelected ()
	{
		mapNameOuterPanel.gameObject.SetActive (false);
		scrollBar.gameObject.SetActive (false);
		mapNameText.text = MapData.FormatMapName (selectedMapName);
	}
	/// <summary>
	/// Instantiates a correctly set up UI component from a prefab
	/// </summary>
	/// <returns>The user interface prefab.</returns>
	/// <param name="objectToCopy">Object to copy.</param>
	public static RectTransform InstantiateUIPrefab (RectTransform objectToCopy, RectTransform parent)
	{
		RectTransform temp = Instantiate (objectToCopy, objectToCopy.position, Quaternion.identity) as RectTransform;
		temp.SetParent (parent.transform);
		temp.localPosition = objectToCopy.localPosition;
		temp.localScale = new Vector3 (1, 1, 1);
		temp.localRotation = Quaternion.identity;
		return temp;
	}
	/// <summary>
	/// Gets the map names from file
	/// </summary>
	/// <returns>The map names.</returns>
	public IEnumerator GetMapNames ()
	{
#if UNITY_WEBPLAYER
		var names = new WWW (ApplicationServerURL + @"/Maps/MapNames.bin");
		while (!names.isDone) {
			yield return new WaitForSeconds (.001f);
		}
		MemoryStream ms = new MemoryStream (names.bytes);
		BinaryFormatter deserializer = new BinaryFormatter ();
		mapNames = (string[])deserializer.Deserialize (ms);
		mapNames = ExtractPrettyMapNames (mapNames);
		maps = new List<MapData> ();
		for (int i = 0; i < mapNames.Length; i++) {
			names = new WWW (ApplicationServerURL + @"/Maps/" + mapNames [i] + ".bin");
			while (!names.isDone) {
				yield return new WaitForSeconds (.001f);
			}
			ms = new MemoryStream (names.bytes);
			deserializer = new BinaryFormatter ();
			MapData obj = (MapData)deserializer.Deserialize (ms);
			maps.Add (obj);
		}
#endif
#if UNITY_STANDALONE
		if(File.Exists(Application.dataPath + @"/Maps/MapNames.bin"))
		{
			Stream TestFileStream = File.OpenRead(Application.dataPath + @"/Maps/MapNames.bin");
			BinaryFormatter deserializer = new BinaryFormatter();
			mapNames = (string[])deserializer.Deserialize(TestFileStream);
			TestFileStream.Close();
			mapNames = ExtractPrettyMapNames(mapNames);
			maps = new List<MapData>();
			for(int i = 0; i < mapNames.Length; i++)
			{
				if(File.Exists(Application.dataPath + @"/Maps/" + mapNames[i] + ".bin"))
				{
					TestFileStream = File.OpenRead(Application.dataPath + @"/Maps/" + mapNames[i] + ".bin");
					deserializer = new BinaryFormatter();
					MapData obj = (MapData)deserializer.Deserialize(TestFileStream);
					maps.Add(obj);
					TestFileStream.Close();
				}
				else
				{
					Debug.Log("No data found for map: " + mapNames[i]);
				}
			}
		}
		else
		{
			Debug.Log("Could not open MapNames data");
		}
		//Added to maintain consistency with web deployment code
		yield return new WaitForSeconds(.001f);
#endif
	}
	
	public void SetGameSettingsPropertyIncome (int input)
	{
		settings.propertyBaseFunds = input;
	}
	
	public void SetGameSettingsStartingIncome (int input)
	{
		settings.startingFunds = input;
	}
	/// <summary>
	/// Loads the map meta data for provided names
	/// </summary>
	/// <param name="namesToLoad">Names to load.</param>
	IEnumerator LoadMapMetaData ()
	{
		maps = new List<MapData> ();
#if UNITY_WEBPLAYER
		for (int i = 0; i < mapNames.Length; i++) {
			var names = new WWW (ApplicationServerURL + @"/Maps/" + mapNames [i] + ".bin");
			while (!names.isDone) {
				yield return new WaitForSeconds (.01f);
			}
			MemoryStream ms = new MemoryStream (names.bytes);
			BinaryFormatter deserializer = new BinaryFormatter ();
			MapData obj = (MapData)deserializer.Deserialize (ms);
			maps.Add (obj);
		}
#endif
#if UNITY_STANDALONE
		for(int i = 0; i < mapNames.Length; i++)
		{
			if(File.Exists(Application.dataPath + @"/Maps/" + mapNames[i] + ".bin"))
			{
				Stream TestFileStream = File.OpenRead(Application.dataPath + @"/Maps/" + mapNames[i] + ".bin");
				BinaryFormatter deserializer = new BinaryFormatter();
				MapData obj = (MapData)deserializer.Deserialize(TestFileStream);
				maps.Add(obj);
				TestFileStream.Close();
			}
			else
			{
				Debug.Log("No data found for map: " + mapNames[i]);
			}
		}
		return null;
#endif
	}
	/// <summary>
	/// Processes an array of strings to remove extra file junk
	/// </summary>
	/// <returns>The pretty map names.</returns>
	/// <param name="inNames">In names.</param>
	string[] ExtractPrettyMapNames (string[] inNames)
	{
		for (int i = 0; i < inNames.Length; i++) {
			string[] temp = inNames [i].Split (new string[]{"\\", "/", "."}, System.StringSplitOptions.RemoveEmptyEntries);
			if (temp.Length > 1) {
				inNames [i] = temp [temp.Length - 2];
			}
		}
		return inNames;
	}
	
	/// <summary>
	/// Switches panels to player setup
	/// </summary>
	public void SwitchToPlayerSelect ()
	{
		mapSelect.gameObject.SetActive (false);
		optionsSelect.gameObject.SetActive (false);
		SetPlayersActive (selectedMap.maxPlayers);
		playerSelect.gameObject.SetActive (true);
	}
	/// <summary>
	/// Switchs to options select.
	/// </summary>
	public void SwitchToOptionsSelect ()
	{
		playerSelect.gameObject.SetActive (false);
		optionsSelect.gameObject.SetActive (true);
	}
	/// <summary>
	/// Switches panels to map setup
	/// </summary>
	public void SwitchToMapSelect ()
	{
		mapSelect.gameObject.SetActive (true);
		playerSelect.gameObject.SetActive (false);
	}
	public void ToggleFogOfWar (bool inBool)
	{
		settings.fogOfWarEnabled = inBool;
	}
	/// <summary>
	/// Sets active player configuration panels and hides inactive ones
	/// </summary>
	/// <param name="activePlayers">Number of panels to set active</param>
	void SetPlayersActive (int activePlayers)
	{
		for (int i = 0; i < 8; i++) {
			players [i].playerSideSlider.maxValue = activePlayers;
			if (i < activePlayers) {
				players [i].gameObject.SetActive (true);
			} else {
				players [i].gameObject.SetActive (false);
			}
		}
	}
	/// <summary>
	/// Starts the game.
	/// </summary>
	public void StartGame ()
	{
		settings.startingFunds = GameObject.Find ("Input StartingFunds").GetComponent<IncrementButton> ().GetValue ();
		settings.propertyBaseFunds = GameObject.Find ("Input PropertyIncome").GetComponent<IncrementButton> ().GetValue ();
		settings.selectedWeather = WeatherToggle.instance.GetSelectedWeatherTypes ();
		Player[] temp = new Player[selectedMap.maxPlayers + 1];
		temp [0] = Instantiate (playerPrototype) as Player;
		temp [0].Setup (0, Generals.Taron, new Color (.8f, .8f, .8f), "--Neutral--");
		temp [0].SetPlayerNumber (0);
		for (int i = 1; i < players.Length + 1; i++) {
			if (i < temp.Length) {
				temp [i] = players [i - 1].thisPlayer;
				temp [i].SetPlayerNumber (i);
			} else {
				if (players [i - 1].started) {
					Destroy (players [i - 1].thisPlayer.gameObject);
				}
				Destroy (players [i - 1].gameObject);
			}
		}
		GameObject.FindObjectOfType<Utilities> ().LoadSkirmishMap (temp, selectedMapName, settings);
		Destroy (this.gameObject);
	}
}