using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public enum AILevel
{
	Human,
	Easy,
	Medium,
	Hard}
;
public class Player : MonoBehaviour
{
	public static int TotalUnitsAllowedPPlayer = 50;
	public int currentHue;
	public General selectedGeneral;
	public string playerName {
		get{ return name;}
		set {
			name = value;
			pigs.name = value;
		}
	}
	protected int playerNumber = 0;
	public int side = 0;//0 is neutral, first player is 1
	public List<UnitController> units { get; protected set; }
	public bool loggingProductionData = false;
	public int aISelectedInMenu;
	public bool isAISelectedInMenu;
	public bool isSideSelectedInMenu;
	protected bool canIssueOrders;
	public TerrainBlock hQBlock {
		get;
		protected set;
	}
	public List<Property> properties{ get; protected set; }
	public int funds { get; protected set; }
	public int menuGeneralNumberSelected;
	protected static int playerCount;
	public UnitController currentGeneralUnit { get; protected set; }
	public Color mainPlayerColor;
	protected PlayerInGameStatistics pigs;
	protected Vector3 lastCameraPosition;
	public Generals generalSelectedInGUI;
	public AILevel aiLevel;
	private List<Property> comTowers;
	/// <summary>
	/// Returns the sum of all units' monetary costs, plus player's funds
	/// </summary>
	/// <returns>The relative value.</returns>
	public int TotalRelativeValue ()
	{
		int value = funds;
		foreach (UnitController u in units) {
			value += (u.baseCost / 10) * u.health.PrettyHealth ();
		}
		return value;
	}
	// Use this for initialization
	protected virtual void Awake ()
	{
		comTowers = new List<Property> ();
		pigs = new PlayerInGameStatistics ();
	}
	/// <summary>
	/// Setup the specified side, general, playerColor and name.
	/// </summary>
	/// <param name="side">Side.</param>
	/// <param name="inGeneral">In general.</param>
	/// <param name="playerColor">Player color.</param>
	/// <param name="name">Name.</param>
	public void Setup (int side, Generals inGeneral, Color playerColor, string name)
	{
		DontDestroyOnLoad (this);
		playerName = name;
		generalSelectedInGUI = inGeneral;
		playerNumber = ++playerCount;
		this.side = side;
		mainPlayerColor = playerColor;
		units = new List<UnitController> ();
		properties = new List<Property> ();
		if (loggingProductionData) {
			gameObject.AddComponent<MouseEventHandler> ();
		}
	}
	/// <summary>
	/// Returns the number of player's com towers within range of the checking unit
	/// </summary>
	/// <returns>The towers in range.</returns>
	/// <param name="checkingUnit">Checking unit.</param>
	/// <param name="block">Block.</param>
	public int ComTowersInRange (UnitController checkingUnit, TerrainBlock block)
	{
		int boost = 0;
		foreach (Property com in comTowers) {
			if (com.IsInComTowerRange (block)) {
				boost++;
			}
		}
		return boost;
	}
	public int GetNumberOfProperties ()
	{
		return properties.Count;
	}
	/// <summary>
	/// Sets the general from a prefab
	/// </summary>
	public void SetupGeneral ()
	{
		selectedGeneral = Utilities.GetGeneral (generalSelectedInGUI);
		selectedGeneral.SetOwner (this);
	}
	/// <summary>
	/// Sends out the general.
	/// </summary>
	/// <param name="unitToAddGeneral">Unit to add general.</param>
	public void SendOutGeneral (UnitController unitToAddGeneral)
	{
		currentGeneralUnit = unitToAddGeneral;
		selectedGeneral.ShowGeneral (unitToAddGeneral);
		RemoveFunds (currentGeneralUnit.baseCost / 2);
	}
	/// <summary>
	/// Adds funds.
	/// </summary>
	/// <param name="inMoney">In money.</param>
	public void AddFunds (int inMoney)
	{
		funds += inMoney;
		pigs.totalFundsGathered += inMoney;
		InGameGUI.instance.SetPlayerDisplay (this);
	}
	/// <summary>
	/// Removes funds.
	/// </summary>
	/// <returns><c>true</c>, if funds was removed, <c>false</c> otherwise.</returns>
	/// <param name="inMoney">In money.</param>
	public bool RemoveFunds (int inMoney)
	{
		if (funds - inMoney < 0) {
			return false;
		}
		funds -= inMoney;
		pigs.totalFundsSpent += inMoney;
		InGameGUI.instance.SetPlayerDisplay (this);
		return true;
	}
	/// <summary>
	/// Binds a property to the player
	/// </summary>
	/// <param name="inUnit">In unit.</param>
	public virtual void AddProperty (Property inUnit)
	{
		if (!properties.Contains (inUnit)) {
			properties.Add (inUnit);
			inUnit.SetOwner (this);
			if (inUnit.propertyType == UnitName.Headquarters) {
				hQBlock = inUnit.GetCurrentBlock ();
			} else if (inUnit.propertyType == UnitName.ComTower) {
				comTowers.Add (inUnit);
				foreach (UnitController u in inUnit.UnitsInRange()) {
					u.comTowerEffect++;
				}
			}
			InGameGUI.instance.SetPlayerDisplay (this);
		}
	}
	/// <summary>
	/// Removes the property from the player
	/// </summary>
	/// <param name="inUnit">In unit.</param>
	public virtual void RemoveProperty (Property inUnit)
	{
		if (properties.Contains (inUnit)) {
			if (inUnit.propertyType == UnitName.Headquarters) {
				InGameController.RemovePlayer (this);
			} else if (inUnit.propertyType == UnitName.ComTower) {
				comTowers.Remove (inUnit);
				foreach (UnitController u in inUnit.UnitsInRange()) {
					u.comTowerEffect--;
				}
			}
			properties.Remove (inUnit);
			InGameGUI.instance.SetPlayerDisplay (this);
		}
	}
	/// <summary>
	/// Binds a unit to the player
	/// </summary>
	/// <param name="inUnit">In unit.</param>
	public virtual void AddUnit (UnitController inUnit)
	{
		if (!units.Contains (inUnit)) {
			units.Add (inUnit);
			inUnit.SetOwner (this);
			InGameGUI.instance.SetPlayerDisplay (this);
		}
	}
	/// <summary>
	/// Produces the unit and binds it to the player
	/// </summary>
	/// <returns>The unit.</returns>
	/// <param name="unit">Unit.</param>
	public UnitController ProduceUnit (UnitName unit)
	{
		UnitController outUnit = (UnitController)MonoBehaviour.Instantiate (Utilities.GetPrefabFromUnitName (unit));
#if UNITY_STANDALONE
		if(loggingProductionData)
		{
			GetComponent<MouseEventHandler>().AddInstance(outUnit.unitClass);
		}
#endif
		AddUnit (outUnit);
		RemoveFunds (outUnit.baseCost);
		pigs.unitsCreated++;
		return outUnit;
	}
	/// <summary>
	/// Actually produces a property in the specified location and binds it to the player
	/// </summary>
	/// <returns>The property.</returns>
	/// <param name="prop">Property.</param>
	/// <param name="position">Position.</param>
	/// <param name="rotation">Rotation.</param>
	public Property ProduceProperty (UnitName prop, Vector3 position, Quaternion rotation)
	{
		Property outUnit = (Property)MonoBehaviour.Instantiate (Utilities.GetPrefabFromUnitName (prop), position, rotation);
		outUnit.startingOwner = playerNumber;
		outUnit.SetOwner (this);
		RemoveFunds (outUnit.baseCost);
		return outUnit;
	}
	/// <summary>
	/// Removes the unit from player, setting its side to neutral
	/// </summary>
	/// <param name="inUnit">In unit.</param>
	public virtual void RemoveUnitFromPlayer (UnitController inUnit)
	{
		if (units.Contains (inUnit)) {
			if (inUnit == currentGeneralUnit) {
				currentGeneralUnit = null;
				selectedGeneral.Hide ();
			}
			units.Remove (inUnit);
			InGameGUI.instance.SetPlayerDisplay (this);
			pigs.unitsLost++;
		}
		if (units.Count == 0) {
			InGameController.RemovePlayer (this);
		}
	}
	/// <summary>
	/// Removes a unit from this player and from the game
	/// </summary>
	/// <param name="inUnit">In unit.</param>
	public virtual void DeleteUnitFromGame (UnitController inUnit)
	{
		if (units.Contains (inUnit)) {
			if (inUnit == currentGeneralUnit) {
				currentGeneralUnit = null;
				selectedGeneral.Hide ();
			}
			units.Remove (inUnit);
			InGameGUI.instance.SetPlayerDisplay (this);
			inUnit.KillUnit ();
			pigs.unitsLost++;
		}
		if (units.Count == 0) {
			InGameController.RemovePlayer (this);
		}
	}
	/// <summary>
	/// Determines whether this player can produce the input Unit
	/// </summary>
	/// <returns><c>true</c> if this instance can produce unit the specified possibleUnit; otherwise, <c>false</c>.</returns>
	/// <param name="possibleUnit">Possible unit.</param>
	public bool CanProduceUnit (UnitName possibleUnit)
	{
		if (Utilities.GetPrefabFromUnitName (possibleUnit) != null) {
			if (funds >= ((UnitController)Utilities.GetPrefabFromUnitName (possibleUnit)).baseCost && units.Count <= TotalUnitsAllowedPPlayer) {
				return true;
			}
		}
		return false;
	}
	/// <summary>
	/// Determines whether this player can produce the input Property.
	/// </summary>
	/// <returns><c>true</c> if this instance can produce property the specified possibleUnit; otherwise, <c>false</c>.</returns>
	/// <param name="possibleUnit">Possible unit.</param>
	public bool CanProduceProperty (UnitName possibleUnit)
	{
		if (Utilities.GetPrefabFromUnitName (possibleUnit) != null) {
			if (funds >= ((Property)Utilities.GetPrefabFromUnitName (possibleUnit)).baseCost) {
				return true;
			}
		}
		return false;
	}
	/// <summary>
	/// Ends the player's turn.
	/// </summary>
	public virtual void EndTurn ()
	{
		foreach (UnitController unit in units) {
			unit.EndTurn ();
			if (Utilities.fogOfWarEnabled || unit.isStealthed) {
				unit.gameObject.SetActive (false);
			}
			if (!unit.isInUnit && unit.gameObject.activeSelf) {
				unit.moveIndicatorParticles.gameObject.SetActive (true);
			}
		}
		foreach (Property prop in properties) {
			prop.EndTurn ();
		}
		lastCameraPosition = Utilities.gameCamera.lookAtPoint;
		canIssueOrders = false;
	}
	/// <summary>
	/// Starts the turn.
	/// </summary>
	public virtual void StartTurn ()
	{
		if (selectedGeneral.powerInEffect) {
			selectedGeneral.ExitPower ();
		}
		foreach (Property prop in properties) {
			prop.StartTurn ();
		}
		for (int i = 0; i < units.Count; i++) {
			units [i].StartTurn ();
		}
		Utilities.gameCamera.CenterCameraOnPoint (lastCameraPosition);
		canIssueOrders = true;
	}
	/// <summary>
	/// Sets the player number.
	/// </summary>
	/// <param name="newNumber">New number.</param>
	public void SetPlayerNumber (int newNumber)
	{
		playerNumber = newNumber;
	}
	/// <summary>
	/// Gets the player number.
	/// </summary>
	/// <returns>The player number.</returns>
	public int GetPlayerNumber ()
	{
		return playerNumber;
	}
	/// <summary>
	/// Sets the side.
	/// </summary>
	/// <param name="newSide">New side.</param>
	public void SetSide (int newSide)
	{
		side = newSide;
	}
	/// <summary>
	/// Determines whether this instance is same side as otherPlayer.
	/// </summary>
	/// <returns><c>true</c> if this instance is same side the specified otherPlayer; otherwise, <c>false</c>.</returns>
	/// <param name="otherPlayer">Other player.</param>
	public bool IsSameSide (Player otherPlayer)
	{
		return otherPlayer.side == side;
	}
	/// <summary>
	/// Determines whether this instance is neutral towards the specified otherPlayer. Feature not implemented yet
	/// </summary>
	/// <returns><c>true</c> if this instance is neutral towards the specified otherPlayer; otherwise, <c>false</c>.</returns>
	/// <param name="otherPlayer">Other player.</param>
	public bool IsNeutralTowards (Player otherPlayer)
	{
		return false;
	}
	/// <summary>
	/// Determines whether this player is neutral side.
	/// </summary>
	/// <returns><c>true</c> if this instance is neutral side; otherwise, <c>false</c>.</returns>
	public bool IsNeutralSide ()
	{
		return side == 0;
	}
	/// <summary>
	/// Removes the player, compiles and returns inGamestatistics
	/// </summary>
	/// <returns>The player.</returns>
	/// <param name="won">If set to <c>true</c> won.</param>
	public PlayerInGameStatistics RemovePlayer (bool won)
	{
#if UNITY_STANDALONE
		if(won)
		{
			GetComponent<MouseEventHandler>().WriteInstances();
		}
		if(aiLevel != AILevel.Human){
			GetComponent<MouseEventHandler>().WriteReinforcementInstances();
		}
#endif
		CompileInGameStatistics (won);
		UnitController[] unitArray = units.ToArray ();
		for (int i = 0; i < unitArray.Length; i++) {
			unitArray [i].KillUnit ();
		}
		Property[] propArray = properties.ToArray ();
		for (int i = 0; i < propArray.Length; i++) {
			if (propArray [i].propertyType == UnitName.Headquarters) {
				propArray [i].DestroyHeadquarters ();
			} else {
				propArray [i].KillUnit ();
			}
		}
		pigs.name = playerName;
		return pigs;
	}
	/// <summary>
	/// Compiles the in game statistics.
	/// </summary>
	/// <param name="won">If set to <c>true</c> won.</param>
	void CompileInGameStatistics (bool won)
	{
		pigs.won = won;
	}
	
	public void Initialize ()
	{
		EndTurn ();
		foreach (Property p in properties) {
			if (p.propertyType == UnitName.Headquarters) {
				lastCameraPosition = p.transform.position;
				break;
			}
		}
	}
	/// <summary>
	/// Gets the side.
	/// </summary>
	/// <returns>The side.</returns>
	public int GetSide ()
	{
		return side;
	}
	/// <summary>
	/// Sets the information displayed in the game gui
	/// </summary>
	/// <param name="view">View.</param>
	public void SetPlayerGUIView (InGamePlayerStatsView view)
	{
		if (IsNeutralSide ()) {
			view.SetValues ("Neutral", "--", properties.Count.ToString (), units.Count.ToString ());
		} else {
			view.SetValues (playerName, funds.ToString (), properties.Count.ToString (), units.Count.ToString ());
		}
	}
}

/// <summary>
/// Container class for various statistics about the player's game
/// </summary>
public class PlayerInGameStatistics : IComparable
{
	public int totalFundsGathered, totalFundsSpent, unitsCreated, unitsLost;
	public string name;
	public bool won;
	public int CompareTo (System.Object b)
	{
		float weightedCountA = this.Winningness ();
		if (b is PlayerInGameStatistics) {
			float weightedCountB = ((PlayerInGameStatistics)b).Winningness ();
			if (weightedCountA < weightedCountB) {
				return -1;
			} else if (weightedCountB < weightedCountA) {
				return 1;
			} else {
				return this.name.CompareTo (((PlayerInGameStatistics)b).name);
			}
		}
		return 0;
	}
	private float Winningness ()
	{
		return (unitsLost / (unitsCreated > 0 ? unitsCreated : 1) + (1 - totalFundsSpent / (totalFundsGathered > 0 ? totalFundsGathered : 1))) * .5f + (won ? 1 : 0);
	}
}
