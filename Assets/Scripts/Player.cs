using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
public enum AILevel {Human, Easy, Medium, Hard};
public class Player : MonoBehaviour{
	public static int TotalUnitsAllowedPPlayer = 50;
	public int currentHue;
	public string generalInStartMenu;
	public General selectedGeneral;
	public string playerName;
	protected int playerNumber = 0;
	public int side = 0;//0 is neutral, first player is 1
	public List<UnitController> units {get; protected set;}
	public bool loggingProductionData = false;
	public int aISelectedInMenu;
	public bool isAISelectedInMenu;
	public bool isSideSelectedInMenu;
	protected bool canIssueOrders;
	public TerrainBlock hQBlock {
		get;
		protected set;
	}

	public List<Property> properties{get; protected set;}
	public int funds {get; protected set;}
	public int menuGeneralNumberSelected;
	public bool selectedInMenu;
	protected static int playerCount;
	public UnitController currentGeneralUnit {get; protected set;}
	public Color mainPlayerColor;
	public Texture2D menuColorTexture;
	protected PlayerInGameStatistics pigs;
	protected Vector3 lastCameraPosition;
	public AILevel aiLevel;
	private List<Property> comTowers;
	/// <summary>
	/// Returns the sum of all units' monetary costs, plus player's funds
	/// </summary>
	/// <returns>The relative value.</returns>
	public int TotalRelativeValue(){
		int value = funds;
		foreach(UnitController u in units){
			value += (u.baseCost / 10) * Utilities.ConvertFixedPointHealth(u.health);
		}
		return value;
	}
	// Use this for initialization
	protected virtual void Awake()
	{
		comTowers = new List<Property>();
	}
	public void Setup(int side, string inGeneral, Color playerColor, string name)
	{
		DontDestroyOnLoad(this);
		playerName = name;
		playerNumber = ++playerCount;
		this.side = side;
		mainPlayerColor = playerColor;
		pigs = new PlayerInGameStatistics();
		pigs.name = playerName;
		units = new List<UnitController>();
		properties = new List<Property>();
		if(loggingProductionData)
		{
			gameObject.AddComponent<MouseEventHandler>();
		}
	}
	public int ComTowersInRange(UnitController checkingUnit, TerrainBlock block)
	{
		int boost = 0;
		foreach(Property com in comTowers)
		{
			if(com.IsInRange(block))
			{
				boost++;
			}
		}
		return boost;
	}
	public int GetNumberOfProperties()
	{
		return properties.Count;
	}
	public void SetupGeneral()
	{
		selectedGeneral = Utilities.GetGeneral(generalInStartMenu);
		selectedGeneral.SetOwner(this);
	}
	public void SendOutGeneral(UnitController unitToAddGeneral)
	{
		currentGeneralUnit = unitToAddGeneral;
		selectedGeneral.ShowGeneral(unitToAddGeneral);
		RemoveFunds(currentGeneralUnit.baseCost/2);
	}
	public void AddFunds(int inMoney)
	{
		funds += inMoney;
		pigs.totalFundsGathered += inMoney;
	}
	public bool RemoveFunds(int inMoney)
	{
		if(funds - inMoney < 0)
		{
			return false;
		}
		funds -= inMoney;
		pigs.totalFundsSpent += inMoney;
		return true;
	}
	public virtual void AddProperty(Property inUnit)
	{
		if(!properties.Contains(inUnit))
		{
			properties.Add(inUnit);
			inUnit.SetOwner(this);
			if(inUnit.propertyType == UnitNames.Headquarters)
			{
				hQBlock = inUnit.GetCurrentBlock();
			}
			else if(inUnit.propertyType == UnitNames.ComTower)
			{
				comTowers.Add(inUnit);
				foreach(UnitController u in inUnit.UnitsInRange())
				{
					u.comTowerEffect++;
				}
			}
		}
	}

	public virtual void RemoveProperty(Property inUnit)
	{
		if(properties.Contains(inUnit))
		{
			if(inUnit.propertyType == UnitNames.Headquarters)
			{
				InGameController.RemovePlayer(this);
			}
			else if(inUnit.propertyType == UnitNames.ComTower)
			{
				comTowers.Remove(inUnit);
				foreach(UnitController u in inUnit.UnitsInRange())
				{
					u.comTowerEffect--;
				}
			}
			properties.Remove(inUnit);
		}
	}
	public virtual void AddUnit(UnitController inUnit)
	{
		if(!units.Contains(inUnit))
		{
			units.Add(inUnit);
			inUnit.SetOwner(this);
		}
	}
	public UnitController ProduceUnit(UnitNames unit)
	{
		UnitController outUnit = (UnitController)MonoBehaviour.Instantiate(Utilities.GetPrefabFromUnitName(unit));
		if(loggingProductionData)
		{
			GetComponent<MouseEventHandler>().AddInstance(outUnit.unitClass);
		}
		AddUnit(outUnit);
		RemoveFunds(outUnit.baseCost);
		pigs.unitsCreated++;
		return outUnit;
	}
	public Property ProduceProperty(UnitNames prop, Vector3 position, Quaternion rotation)
	{
		Property outUnit = (Property)MonoBehaviour.Instantiate(Utilities.GetPrefabFromUnitName(prop), position, rotation);
		outUnit.startingOwner = playerNumber;
		outUnit.SetOwner(this);
		RemoveFunds(outUnit.baseCost);
		return outUnit;
	}
	public virtual void RemoveUnit(UnitController inUnit)
	{
		if(units.Contains(inUnit))
		{
			if(inUnit == currentGeneralUnit)
			{
				currentGeneralUnit = null;
				selectedGeneral.Hide();
			}
			units.Remove(inUnit);
			pigs.unitsLost++;
		}
		if(units.Count == 0)
		{
			InGameController.RemovePlayer(this);
		}
	}
	public virtual void DeleteUnit(UnitController inUnit)
	{
		if(units.Contains(inUnit))
		{
			if(inUnit == currentGeneralUnit)
			{
				currentGeneralUnit = null;
				selectedGeneral.Hide();
			}
			units.Remove(inUnit);
			inUnit.KillUnit();
			pigs.unitsLost++;
		}
		if(units.Count == 0)
		{
			InGameController.RemovePlayer(this);
		}
	}
	public bool CanProduceUnit(UnitNames possibleUnit)
	{
		if(Utilities.GetPrefabFromUnitName(possibleUnit) != null)
		{
			if(funds >= ((UnitController)Utilities.GetPrefabFromUnitName(possibleUnit)).baseCost && units.Count <= TotalUnitsAllowedPPlayer)
			{
				return true;
			}
		}
		return false;
	}
	public bool CanProduceProperty(UnitNames possibleUnit)
	{
		if(Utilities.GetPrefabFromUnitName(possibleUnit) != null)
		{
			if(funds >= ((Property)Utilities.GetPrefabFromUnitName(possibleUnit)).baseCost)
			{
				return true;
			}
		}
		return false;
	}
	public virtual void EndTurn()
	{
		foreach(UnitController unit in units)
		{
			unit.EndTurn();
			if(Utilities.fogOfWarEnabled || unit.isStealthed)
			{
				unit.gameObject.SetActive(false);
			}
			if(!unit.isInUnit && unit.gameObject.activeSelf)
			{
				unit.moveIndicatorParticles.gameObject.SetActive(true);
			}
		}
		foreach(Property prop in properties)
		{
			prop.EndTurn();
		}
		lastCameraPosition = Utilities.gameCamera.lookAtPoint;
		canIssueOrders = false;
	}

	public virtual void StartTurn()
	{
		if(selectedGeneral.powerInEffect)
		{
			selectedGeneral.ExitPower();
		}
		foreach(Property prop in properties)
		{
			prop.StartTurn();
		}
		for(int i = 0; i < units.Count; i++)
		{
			units[i].StartTurn();
		}
		Utilities.gameCamera.CenterCameraOnPoint(lastCameraPosition);
		canIssueOrders = true;
	}
	public void SetPlayerNumber(int newNumber)
	{
		playerNumber = newNumber;
	}
	public int GetPlayerNumber()
	{
		return playerNumber;
	}
	public void SetSide(int newSide)
	{
		side = newSide;
	}
	public bool IsSameSide(Player otherPlayer)
	{
		return otherPlayer.side == side;
	}
	public bool IsNeutralTowards(Player otherPlayer)
	{
		return false;
	}
	public bool IsNeutralSide()
	{
		return side == 0;
	}
	public PlayerInGameStatistics RemovePlayer(bool won)
	{
		if(won)
		{
			GetComponent<MouseEventHandler>().WriteInstances();
		}
		if(aiLevel != AILevel.Human){
			GetComponent<MouseEventHandler>().WriteReinforcementInstances();
		}
		CompileInGameStatistics(won);
		UnitController[] unitArray = units.ToArray();
		for(int i = 0; i < unitArray.Length; i++)
		{
			unitArray[i].KillUnit();
		}
		Property[] propArray = properties.ToArray();
		for(int i = 0; i < propArray.Length; i++)
		{
			if(propArray[i].propertyType == UnitNames.Headquarters)
			{
				propArray[i].DestroyHeadquarters();
			}
			else
			{
				propArray[i].KillUnit();
			}
		}
		pigs.name = playerName;
		return pigs;
	}
	void CompileInGameStatistics (bool won)
	{
		pigs.won = won;
	}

	public void Initialize ()
	{
		EndTurn();
		foreach(Property p in properties)
		{
			if(p.propertyType == UnitNames.Headquarters)
			{
				lastCameraPosition = p.transform.position;
				break;
			}
		}
	}
	public int GetSide()
	{
		return side;
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
	public int CompareTo(System.Object b)
	{
		float weightedCountA = this.Winningness();
		if(b is PlayerInGameStatistics)
		{
			float weightedCountB = ((PlayerInGameStatistics)b).Winningness();
			if(weightedCountA < weightedCountB)
			{
				return -1;
			}
			else if(weightedCountB < weightedCountA)
			{
				return 1;
			}
			else
			{
				return this.name.CompareTo(((PlayerInGameStatistics)b).name);
			}
		}
		return 0;
	}
	private float Winningness()
	{
		return (unitsLost/(unitsCreated > 0?unitsCreated:1) + (1 - totalFundsSpent/(totalFundsGathered > 0?totalFundsGathered:1)))*.5f + (won?1:0);
	}
}
