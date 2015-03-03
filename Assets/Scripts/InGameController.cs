using UnityEngine;
using System.Collections.Generic;

public class InGameController : MonoBehaviour
{
	// Exposed instance
	public static InGameController instance;
	
	List<Player> players;
	List<PlayerInGameStatistics> collectedStatistics;
	public ParticleSystem mouseOverParticles;
	public ParticleSystem mouseParticles;
	public ParticleSystem possibleTargetParticlePrototypeEditor;
	public ParticleSystem possibleTargetParticlePrototype;
	public int currentPlayer;
	public bool isPaused;
	public TerrainBuilder currentTerrain;
	bool unitSelectedMutex, infoBoxMutex;
	Object selectedUnit;
	int currentUnitMousedOver;
	List<ParticleSystem> possibleTargetParticles;
	public WeatherController weather;
	public int currentTurn = 0;
	public bool endingGame;
	int turnState;
	private bool leftClick, rightClick;
	public bool LeftClick {
		get { return leftClick; }
		protected set {
			leftClick = value;
			simulatedLeftClick = false;
		}
	}
	public bool RightClick {
		get { return rightClick; }
		protected set {
			rightClick = value;
			simulatedRightClick = false;
		}
	}
	bool simulatedLeftClick, simulatedRightClick;
	// Use this for initialization
	void Awake ()
	{
		if (instance == null || instance == this) {
			instance = this;
			DontDestroyOnLoad (this);
			currentTerrain = GameObject.FindObjectOfType<TerrainBuilder> ();
			mouseOverParticles = Instantiate (mouseParticles) as ParticleSystem;
			mouseOverParticles.particleSystem.Stop ();
		} else {
			Destroy (this);
		}
	}
	void Start ()
	{
		possibleTargetParticlePrototype = possibleTargetParticlePrototypeEditor;
		possibleTargetParticles = new List<ParticleSystem> ();
		collectedStatistics = new List<PlayerInGameStatistics> ();
		weather = GetComponent<WeatherController> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!endingGame) {
			if (turnState == 1) {
				turnState = 2;
				players [currentPlayer].StartTurn ();
				if (currentPlayer > 0) {
					InGameGUI.instance.StartTurnChange (players [currentPlayer].playerName);
				}
				if (currentPlayer == 0) {
					AdvanceTurn ();
				}
			} else if (turnState == 0) {
				turnState = 1;
			}
		}
		if (isPaused) {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				isPaused = false;
				InGameGUI.instance.Pause ();
				Utilities.gameCamera.otherMenuActive = false;
			}
		} else {
			if (Input.GetKeyDown (KeyCode.Escape)) {
				if (selectedUnit != null) {
					if (selectedUnit is UnitController) {
						((UnitController)selectedUnit).ResetUnit ();
					}
					if (selectedUnit is Property) {
						((Property)selectedUnit).ResetUnit ();
					}
				}
				isPaused = true;
				InGameGUI.instance.Pause ();
			}
			if (Input.GetKeyDown ("q")) {
				MoveCameraToNextPlayerUnit ();
			}
		}
		if (Input.GetMouseButtonUp (0) || simulatedLeftClick) {
			LeftClick = true;
		} else {
			LeftClick = false;
		}
		if (Input.GetMouseButtonUp (1) || simulatedRightClick) {
			RightClick = true;
		} else {
			RightClick = false;
		}
	}
	public void SimulateLeftClick ()
	{
		simulatedLeftClick = true;
	}
	public void SimulateRightClick ()
	{
		simulatedRightClick = true;
	}
	public int NumberOfActivePlayers ()
	{
		if (players != null) {
			return players.Count;
		}
		return 0;
	}
	public void DisplayPossibleTargetParticles (List<AttackableObject> possibleTargets)
	{
		int i;
		if (possibleTargets.Count > possibleTargetParticles.Count) {
			int difference = possibleTargets.Count - possibleTargetParticles.Count;
			for (i = 0; i < difference; i++) {
				possibleTargetParticles.Add (Instantiate (possibleTargetParticlePrototype) as ParticleSystem);
			}
		}
		i = 0;
		for (; i < possibleTargets.Count; i++) {
			possibleTargetParticles [i].transform.position = possibleTargets [i].GetPosition ();
			possibleTargetParticles [i].gameObject.SetActive (true);
		}
	}
	public void HidePossibleTargetParticles ()
	{
		foreach (ParticleSystem ps in possibleTargetParticles) {
			ps.gameObject.SetActive (false);
		}
	}
	public void MoveCameraToNextPlayerUnit ()
	{
		currentUnitMousedOver++;
		if (currentUnitMousedOver >= players [currentPlayer].units.Count) {
			currentUnitMousedOver = 0;
		}
		int i = 0;
		while (i < players[currentPlayer].units.Count) {
			if (players [currentPlayer].units [currentUnitMousedOver].currentState != UnitState.FinishedMove) {
				Utilities.gameCamera.CenterCameraOnPoint (players [currentPlayer].units [currentUnitMousedOver].transform.position);
				return;
			}
			currentUnitMousedOver++;
			i++;
			if (currentUnitMousedOver >= players [currentPlayer].units.Count) {
				currentUnitMousedOver = 0;
			}
		}
	}

	public bool AcquireUnitSelectedMutex (Object mutexAttempter)
	{
		if (unitSelectedMutex == true) {
			return false;
		} else {
			unitSelectedMutex = true;
			selectedUnit = mutexAttempter;
		}
		return true;
	}
	public bool ReleaseUnitSelectedMutex ()
	{
		unitSelectedMutex = false;
		selectedUnit = null;
		return false;
	}
	public bool UnitSelected ()
	{
		return unitSelectedMutex;
	}
	public void Setup (Player[] play)
	{
		players = new List<Player> (play);
		for (int i = 0; i < players.Count; i++) {
			players [i].SetupGeneral ();
		}
		UnitController[] predeployedUnits = GameObject.FindObjectsOfType<UnitController> ();
		foreach (UnitController uc in predeployedUnits) {
			if (uc.playerNumber > 0) {
				players [uc.playerNumber].AddUnit (uc);
			}
		}
		players [0].loggingProductionData = true;
		currentPlayer = 0;
	}

	public void AdvanceTurn ()
	{
		players [currentPlayer].EndTurn ();
		isPaused = false;
		InGameGUI.instance.Pause ();
		currentTerrain.AllFogOn ();
		currentPlayer++;
		if (currentPlayer >= players.Count) {
			currentPlayer = 0;
		}
		if (currentPlayer == 0) {
			currentTurn++;
			weather.AdvanceWeather (players);
		}
		turnState = 0;
		InGameGUI.instance.SetCurrentPlayerDisplay (players [currentPlayer]);
	}
	/// <summary>
	/// Removes all players, ending the current skirmish
	/// </summary>
	public void QuitSkirmish ()
	{
		for (int i = players.Count - 1; i >= 1; i--) {
			if (players != null) {
				if (players [i] != null) {
					RemovePlayer (players [i]);
				}
			}
		}
	}
	/// <summary>
	/// Removes the player.
	/// </summary>
	/// <param name="toRemove">To remove.</param>
	public void RemovePlayer (Player toRemove)
	{
		collectedStatistics.Add (toRemove.RemovePlayer (false));
		players.Remove (toRemove);
		Destroy (toRemove.gameObject);
		bool allPlayersOnSameSide = true;
		for (int i = 1; i < players.Count-1; i++) {
			if (!players [i].IsSameSide (players [i + 1])) {
				allPlayersOnSameSide = false;
				break;
			}
		}
		if (players.Count <= 2 || allPlayersOnSameSide) {
			endingGame = true;
			for (int i = 1; i < players.Count; i++) {
				collectedStatistics.Add (players [i].RemovePlayer (true));
			}
			Utilities.LoadSkirmishEndScreen (collectedStatistics);
		}
		if (currentPlayer >= players.Count && players.Count > 0) {
			currentPlayer = 0;
			players [currentPlayer].StartTurn ();
		}
	}
	public Player GetCurrentPlayer ()
	{
		if (players.Count > 0 && currentPlayer < players.Count) {
			return players [currentPlayer];
		}
		return null;
	}
	public Player GetPlayer (int playerNum)
	{
		if (players.Count > 0 && playerNum < players.Count) {
			return players [playerNum];
		}
		return null;
	}

	public List<AttackableObject> GetAllEnemyUnits (Player inPlayer)
	{
		List<AttackableObject> outList = new List<AttackableObject> (inPlayer.units.Count);
		for (int i = 0; i < players.Count; i++) {
			if (!players [i].IsSameSide (inPlayer)) {
				for (int k = 0; k < players[i].units.Count; k++) {
					if (!players [i].units [k].isInUnit) {
						outList.Add (players [i].units [k]);
					}
				}
			}
		}
		return outList;
	}

	public List<Property> GetAllEnemyProperties (Player inPlayer)
	{
		List<Property> outList = new List<Property> (inPlayer.properties.Count);
		for (int i = 0; i < players.Count; i++) {
			if (!players [i].IsSameSide (inPlayer)) {
				outList.AddRange (players [i].properties);
			}
		}
		return outList;
	}

	public Player GetTargetablePlayer (Player owner)
	{
		int startNum;
		int i = Random.Range (1, players.Count);
		startNum = i;
		if (!owner.IsSameSide (players [i])) {
			return players [i];
		} else {
			i++;
			while (i != startNum) {
				if (!owner.IsSameSide (players [i])) {
					return players [i];
				}
				i++;
				if (i >= players.Count) {
					i = 1;
				}
			}
		}
		throw new UnityException ("No Valid Players");
	}
	public float ClosestEnemyHQ (TerrainBlock block, MovementType moveType, Player querier, out TerrainBlock hqBlock)
	{
		float bestSoFar = float.MaxValue;
		hqBlock = null;
		for (int i = 1; i < players.Count; i++) {
			if (!players [i].IsSameSide (querier)) {
				if (block.GetDistanceToHQ (players [i], moveType) < bestSoFar) {
					bestSoFar = block.GetDistanceToHQ (players [i], moveType);
					hqBlock = players [i].hQBlock;
				}
			}
		}
		return bestSoFar;
	}
	
	public float ClosestEnemyHQ (TerrainBlock block, MovementType moveType, Player querier)
	{
		float bestSoFar = float.MaxValue;
		for (int i = 1; i < players.Count; i++) {
			if (!players [i].IsSameSide (querier)) {
				if (block.GetDistanceToHQ (players [i], moveType) < bestSoFar) {
					bestSoFar = block.GetDistanceToHQ (players [i], moveType);
				}
			}
		}
		return bestSoFar;
	}
	public int TotalValueRelativeToPlayer (Player relative)
	{
		int totalEnemyValue = 0;
		int totalEnemies = 0;
		for (int i = 1; i < players.Count; i++) {
			if (players [i] != relative) {
				totalEnemyValue += players [i].TotalRelativeValue ();
				totalEnemies++;
			}
		}
		return relative.TotalRelativeValue () - (totalEnemies > 0 ? totalEnemyValue / totalEnemies : 0);
	}
	public Instance CreateInstance (UnitName unitMade, bool reinforcement)
	{
		Instance outInstance;
		if (reinforcement) {
			outInstance = new ReinforcementInstance (System.Enum.GetNames (typeof(UnitName)).Length);
			((ReinforcementInstance)outInstance).accruedReward = InGameController.instance.TotalValueRelativeToPlayer (players [currentPlayer]);
		} else {
			outInstance = new Instance (System.Enum.GetNames (typeof(UnitName)).Length);
		}
		float enemiesOfPlayer = 0;
		for (int i = 0; i < players.Count; i++) {
			if (i == 0) {
				foreach (UnitController u in players[i].units) {
					outInstance.neutralUnitCount [(int)u.unitClass]++;
				}
				foreach (Property u in players[i].properties) {
					outInstance.neutralUnitCount [(int)u.propertyType]++;
				}
			} else if (i == currentPlayer) {
				foreach (UnitController u in players[i].units) {
					outInstance.playerUnitCount [(int)u.unitClass]++;
				}
				foreach (Property u in players[i].properties) {
					outInstance.playerUnitCount [(int)u.propertyType]++;
				}
				outInstance.funds = players [i].funds;
				outInstance.currentTurn = currentTurn;
			} else if (!players [currentPlayer].IsSameSide (players [i])) {
				enemiesOfPlayer++;
				foreach (UnitController u in players[i].units) {
					outInstance.enemyAverageUnitCount [(int)u.unitClass]++;
				}
				foreach (Property u in players[i].properties) {
					outInstance.enemyAverageUnitCount [(int)u.propertyType]++;
				}
			}
		}
		for (int i = 0; i < outInstance.enemyAverageUnitCount.Length; i++) {
			outInstance.enemyAverageUnitCount [i] /= enemiesOfPlayer;
		}
		outInstance.mapData = currentTerrain.data;
		outInstance.classification = unitMade;
		return outInstance;
	}
}
