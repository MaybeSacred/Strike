using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Utilities : MonoBehaviour{
	[System.Serializable]
	public class UnitPrefabBindingClass
	{
		public UnitNames name;
		public MonoBehaviour prefab;
	}
	public Texture[] hpPic;
	public Texture2D[] unitRankInsigniasEditor;
	public static Texture2D[] unitRankInsignias;
	public static Texture2D carryingUnitImage;
	public static Texture[] healthPoint;
	public static Color canFireNowRangeColor;	
	public static Color cannotFireNowRangeColor;
	public UnitPrefabBindingClass[] editorUnits;
	public static UnitPrefabBindingClass[] units;
	public static bool fogOfWarEnabled;
	public static TerrainBlock selectedBlock;
	public static General[] generalPrototypes;
	public General[] generals;
	public static WeatherController weather;
	private static Player[] playersToAdd;
	private static GameSettings gameSettings;
	public bool isInMenu;
	public static InGameCamera gameCamera;
	public AIPlayer easyAIPrototype, mediumAIPrototype, hardAIPrototype;
	private static List<PlayerInGameStatistics> statistics;
	void Awake() {
		LearnerUtilities.SetJREPath();
	}
	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this);
		generalPrototypes = generals;
		units = editorUnits;
		healthPoint = hpPic;
		unitRankInsignias = unitRankInsigniasEditor;
		canFireNowRangeColor = new Color((float)113/256, 0, 0, (float)143/256);
		cannotFireNowRangeColor = new Color((float)96/256, (float)75/256, 0, (float)143/256);
		fogOfWarEnabled = true;
		carryingUnitImage = Resources.Load<Texture2D>("UnitBar");
		LearnerUtilities.TerminateNeuralTraining();
	}
	public static General GetGeneral(string id)
	{
		for(int i = 0; i < generalPrototypes.Length; i++)
		{
			if(generalPrototypes[i].name.Equals(id))
			{
				return Instantiate(generalPrototypes[i]) as General;
			}
		}
		throw new UnityException("General not found");
	}
	public static void LoadSkirmishEndScreen(List<PlayerInGameStatistics> pigs)
	{
		statistics = pigs;
		MonoBehaviour[] all = FindObjectsOfType<MonoBehaviour>();
		foreach(MonoBehaviour a in all)
		{
			if(a is InGameController || a is Utilities)
			{
				
			}
			else
			{
				Destroy(a);
			}
		}
		LearnerUtilities.TrainCurrentClassifier(LearnerUtilities.dataFileName);
		Application.LoadLevel("SkirmishEnd");
	}
	public void LoadSkirmishMap(Player[] players, string mapName, GameSettings gs)
	{
		isInMenu = false;
		playersToAdd = players;
		gameSettings = gs;
		Application.LoadLevel(mapName);
	}
	void OnLevelWasLoaded()
	{
		if(!isInMenu)
		{
			isInMenu = true;
			for(int i = 0; i < playersToAdd.Length; i++)
			{
				if(playersToAdd[i].aiLevel != AILevel.Human)
				{
					AIPlayer temp = null;
					switch(playersToAdd[i].aiLevel)
					{
					case AILevel.Easy:
					{
						temp = Instantiate(easyAIPrototype) as AIPlayer;
						break;
					}
					case AILevel.Medium:
					{
						temp = Instantiate(mediumAIPrototype) as AIPlayer;
						break;
					}
					case AILevel.Hard:
					{
						temp = Instantiate(hardAIPrototype) as AIPlayer;
						break;
					}
					}
					temp.Setup(playersToAdd[i]);
					Destroy(playersToAdd[i]);
					playersToAdd[i] = temp;
				}
			}
			InGameController.Setup(playersToAdd);
			fogOfWarEnabled = gameSettings.fogOfWarEnabled;
			if(gameSettings.startingFunds > 0)
			{
				for(int i = 0; i < playersToAdd.Length; i++)
				{
					playersToAdd[i].AddFunds(gameSettings.startingFunds);
				}
			}
			Property[] allProperties = GameObject.FindObjectsOfType<Property>();
			for(int i = 0; i < allProperties.Length; i++)
			{
				if(allProperties[i].propertyClass.baseFunds > 0)
				{
					allProperties[i].propertyClass.baseFunds = gameSettings.propertyBaseFunds;
				}
			}
			GameObject.FindObjectOfType<WeatherController>().SetWeatherType(gameSettings.selectedWeather, gameSettings.randomWeather, new List<Player>(playersToAdd));
			gameCamera = GameObject.FindObjectOfType<InGameCamera>();
		}
		if(Application.loadedLevelName.Equals("SkirmishEnd"))
		{
			GameObject.FindObjectOfType<SkirmishEndMenu>().SetGameStatistics(statistics);
			Destroy(this);
		}
	}
	void Update()
	{
		if(Input.GetKeyDown("g"))
		{
			InGameController.RemovePlayer(InGameController.GetCurrentPlayer());
		}
	}

	public static Texture2D GetRankImage (UnitRanks rank)
	{
		if(rank != UnitRanks.Unranked)
		{
			return unitRankInsignias[(int)rank - 1];
		}
		throw new UnityException("Invalid Rank Image Request " + StackTraceUtility.ExtractStackTrace());
	}

	public static Texture2D GetCarryingImage (UnitNames unitClass)
	{
		return carryingUnitImage;
	}
	
	public static MonoBehaviour GetPrefabFromUnitName(UnitNames inName)
	{
		 if((int)inName < units.Length && units[(int)inName].prefab != null)
		 {
			return units[(int)inName].prefab;
		 }
		 return null;
	}
	public static void LoadTitleScreen()
	{
		Application.LoadLevel("Splash");
	}
	public static string PrettifyVariableName(string inVariable)
	{
		string temp = inVariable;
		string outString = inVariable;
		int numberOfChanges = 0;
		for(int i = 2; i < temp.Length; i++)
		{
			if(char.IsUpper(temp[i]))
			{
				outString = outString.Insert(i+numberOfChanges, " ");
				numberOfChanges++;
			}
		}
		return outString;
	}
}
