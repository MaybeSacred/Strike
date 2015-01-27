using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public class SkirmishMenu : MonoBehaviour {
	public float menuXPct, menuYPct;
	public GUIStyle style;
	private List<MapData> maps;
	private string[] mapNames;
	private string selectedMapName;
	private Popup.ListState mapSelectionDropdown;
	private bool weatherSelectionDropDownActive;
	private int weatherSelectionDropDownIndex;
	private Player[] players;
	private Popup.ListState[] generalDropdownStates, aiDropdownStates, playerSideDropdownStates;
	private string[] generalNames;
	public Texture2D colorTexture;
	private GameSettings settings;
	public Player playerPrototype;
	// Use this for initialization
	/*void Start () {
		players = new Player[8];
		mapSelectionDropdown = new Popup.ListState();
		generalDropdownStates = new Popup.ListState[8];
		aiDropdownStates = new Popup.ListState[8];
		playerSideDropdownStates = new Popup.ListState[8];
		for(int i = 0; i < generalDropdownStates.Length; i++)
		{
			generalDropdownStates[i] = new Popup.ListState();
			aiDropdownStates[i] = new Popup.ListState();
			playerSideDropdownStates[i] = new Popup.ListState();
		}
		settings = new GameSettings();
		generalNames = System.Enum.GetNames(typeof(Generals));
		for(int i = 0; i < players.Length;i++)
		{
			players[i] = Instantiate(playerPrototype) as Player;
			players[i].loggingProductionData = true;
			players[i].Setup(i+1, "", Color.white, "-Enter Name-");
			players[i].menuColorTexture = Instantiate(colorTexture) as Texture2D;
			players[i].currentHue = Random.Range(0, 256);
			players[i].side = i+1;
		}
		mapNames = Directory.GetFiles(Application.dataPath + @"\Maps\", "*.unity");
		mapNames = ExtractPrettyMapNames(mapNames);
		LoadMapMetaData();
	}
	void LoadMapMetaData()
	{
		maps = new List<MapData>();
		for(int i = 0; i < mapNames.Length; i++)
		{
			if(File.Exists(Application.dataPath + @"\Maps\" + mapNames[i] + ".bin"))
			{
				Stream TestFileStream = File.OpenRead(Application.dataPath + @"\Maps\" + mapNames[i] + ".bin");
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
	}
	string[] ExtractPrettyMapNames(string[] inNames)
	{
		for(int i = 0; i < inNames.Length; i++)
		{
			string[] temp = inNames[i].Split(new string[]{"\\", "/", "."}, System.StringSplitOptions.RemoveEmptyEntries);
			if(temp.Length > 1)
			{
				inNames[i] = temp[temp.Length-2];
			}
		}
		return inNames;
	}
	void Update () {
		transform.rotation *= Quaternion.AngleAxis(1f*Time.deltaTime, new Vector3(0, 1, 0));
	}
	private float playerItemWidthPct = .1f;
	int scrollPosition;
	void OnGUI()
	{
		GUI.BeginGroup(new Rect(Screen.width * (.5f * (1-menuXPct)), Screen.height * (.5f * (1-menuYPct)), Screen.width * menuXPct, Screen.height * menuYPct));
		//Player Selection Section
		GUILayout.BeginArea(new Rect(0, 0, .5f*Screen.width*menuXPct, Screen.height*menuYPct));
		GUILayout.Box("Players");
		GUILayout.BeginVertical();
		for(int i = 0; i < maps[mapSelectionDropdown.listEntry].maxPlayers; i++)
		{
			players[i].playerName = GUI.TextField(new Rect(0, ((i+1)/10f) * Screen.height*menuYPct, playerItemWidthPct * Screen.width * menuXPct, (1/10f) * Screen.height*menuYPct - 1), players[i].playerName);
			Popup.List(new Rect(playerItemWidthPct * Screen.width*menuXPct, ((i+1)/10f) * Screen.height*menuYPct, playerItemWidthPct * Screen.width * menuXPct, (1/10f) * Screen.height*menuYPct - 1), ref generalDropdownStates[i], new GUIContent(generalNames[generalDropdownStates[i].listEntry]), generalNames, "button", "box", GUIStyle.none, ListCallBackFunc, 5);
			players[i].menuGeneralNumberSelected = generalDropdownStates[i].listEntry;
			Popup.List(new Rect(2 * playerItemWidthPct * Screen.width*menuXPct, ((i+1)/10f) * Screen.height*menuYPct, playerItemWidthPct * Screen.width * menuXPct, (1/10f) * Screen.height*menuYPct - 1), ref aiDropdownStates[i], new GUIContent(System.Enum.GetNames(typeof(AILevel))[aiDropdownStates[i].listEntry]), System.Enum.GetNames(typeof(AILevel)),"button", "box", GUIStyle.none, ListCallBackFunc, 4);
			players[i].currentHue = (int)GUI.HorizontalSlider(new Rect(3 * playerItemWidthPct * Screen.width*menuXPct, ((i+1)/10f) * Screen.height * menuYPct, playerItemWidthPct * Screen.width*menuXPct, (1/10f) * Screen.height*menuYPct - 1), players[i].currentHue, 0, 359);
			players[i].mainPlayerColor = HSVtoRGB(players[i].currentHue, 1, 1);
			SetColor(players[i].menuColorTexture, players[i].mainPlayerColor);
			GUI.Label(new Rect(4 * playerItemWidthPct * Screen.width*menuXPct, ((i+1)/10f) * Screen.height*menuYPct, .05f * Screen.width*menuXPct, (1/10f) * Screen.height*menuYPct - 1), players[i].menuColorTexture);
			if(Popup.List(new Rect(4.5f * playerItemWidthPct * Screen.width*menuXPct, ((i+1)/10f) * Screen.height*menuYPct, .05f * Screen.width * menuXPct, (1/10f) * Screen.height*menuYPct - 1), ref players[i].isSideSelectedInMenu, ref players[i].side, new GUIContent(players[i].side.ToString()), new string[]{"1", "2", "3", "4"}, GUIStyle.none, ListCallBackFunc))
			{
				players[i].side++;
			}
		}
		if(GUI.Button(new Rect(0, (9/10f) * Screen.height*menuYPct, .25f*Screen.width*menuXPct, (1/10f) * Screen.height*menuYPct - 1), "Start Game"))
		{
			StartGame();
		}
		if(GUI.Button(new Rect(.25f*Screen.width*menuXPct, (9/10f) * Screen.height*menuYPct, .25f*Screen.width*menuXPct, (1/10f) * Screen.height*menuYPct - 1), "Back"))
		{
			Utilities.LoadTitleScreen();
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
		//Map selection
		GUI.BeginGroup(new Rect(.5f*Screen.width*menuXPct, 0, .5f*Screen.width*menuXPct, .5f*Screen.height*menuYPct));
		GUI.Box(new Rect(0, 0, .5f*Screen.width*menuXPct, .08f*Screen.height*menuYPct), "Map Select");
		Popup.List(new Rect(0, .081f*Screen.height*menuYPct, .5f*Screen.width*menuXPct, .08f*Screen.height*menuYPct), ref mapSelectionDropdown, new GUIContent(mapNames[mapSelectionDropdown.listEntry]), mapNames,"button", "box", GUIStyle.none, ListCallBackFunc, 6);
		GUI.EndGroup();
		//Options
		GUILayout.BeginArea(new Rect(.5f*Screen.width*menuXPct, .5f*Screen.height*menuYPct, .5f*Screen.width*menuXPct, .5f*Screen.height*menuYPct));
		GUILayout.BeginVertical();
		GUILayout.Box("Options");
		settings.fogOfWarEnabled = GUILayout.Toggle(settings.fogOfWarEnabled, "Fog Enabled");
		GUILayout.BeginHorizontal();
		int possibleValue = 0;
		GUILayout.Label("Starting Funds");
		if(int.TryParse(GUILayout.TextField(settings.startingFunds.ToString(), 5), out possibleValue))
		{
			settings.startingFunds = possibleValue;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		possibleValue = 0;
		GUILayout.Label("Funds Per Base");
		if(int.TryParse(GUILayout.TextField(settings.propertyBaseFunds.ToString(), 5), out possibleValue))
		{
			settings.propertyBaseFunds = possibleValue;
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		Popup.List(ref weatherSelectionDropDownActive, ref weatherSelectionDropDownIndex, new GUIContent(System.Enum.GetNames(typeof(WeatherType))[weatherSelectionDropDownIndex]), System.Enum.GetNames(typeof(WeatherType)), GUIStyle.none, ListCallBackFunc);
		settings.selectedWeather = (WeatherType)System.Enum.GetValues(typeof(WeatherType)).GetValue(weatherSelectionDropDownIndex);
		settings.randomWeather = GUILayout.Toggle(settings.randomWeather, "Random Weather");
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
		GUILayout.EndArea();
		GUI.EndGroup();
	}
	void ListCallBackFunc()
	{
		
	}
	void StartGame()
	{
		Player[] temp = new Player[maps[mapSelectionDropdown.listEntry].maxPlayers + 1];
		temp[0] = Instantiate(playerPrototype) as Player;
		temp[0].Setup(0, "Taron", new Color(.8f, .8f, .8f), "--Neutral--");
		temp[0].generalInStartMenu = "Taron";
		for(int i = 1; i < players.Length + 1; i++)
		{
			if(i < temp.Length)
			{
				temp[i] = players[i-1];
				temp[i].generalInStartMenu = generalNames[temp[i].menuGeneralNumberSelected];
				temp[i].aiLevel = (AILevel)System.Enum.GetValues(typeof(AILevel)).GetValue(aiDropdownStates[i-1].listEntry);
			}
			else
			{
				Destroy(players[i-1]);
			}
		}
		GameObject.FindObjectOfType<Utilities>().LoadSkirmishMap(temp, mapNames[mapSelectionDropdown.listEntry], settings);
		Destroy(this.gameObject);
	}*/
	void SetColor(Texture2D tex, Color col)
	{
		Color[] colors = new Color[Mathf.RoundToInt(tex.width * tex.height)];
		for(int i = 0; i < colors.Length; i++)
		{
			colors[i] = col;
		}
		tex.SetPixels(colors);
		tex.Apply();
	}
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
