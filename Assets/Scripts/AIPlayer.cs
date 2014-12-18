using UnityEngine;
using System;
using System.Collections.Generic;
public abstract class AIPlayer : Player
{
	public float attackModifier, captureModifier, 
		produceMissileModifier, buildBridgeModifier, supplyModifier, 
		repairModifier, loadModifier, unloadModifier, 
		boardModifier, addGeneralModifier, unStealthifyModifier, 
		stealthifyModifier, hQMoveTowardsModifier, buildingMoveTowardsModifier,
		randomnessModifier, defensiveTerrainDesireModifier, healModifier,
		joinModifier, waitModifier;
	protected Stack<UnitController> unitsToMove;
	protected Queue<UnitNames> unitsToMake;
	protected List<UnitController> sightedEnemyUnits;
	protected UnitController currentUnit;
	protected int[] countsOfEachUnit;
	protected int producingProperties = 0;
	protected int productionAttempts;
	protected override void Awake()
	{
		base.Awake();
		unitsToMove = new Stack<UnitController>();
		unitsToMake = new Queue<UnitNames>();
		countsOfEachUnit = new int[System.Enum.GetValues(typeof(UnitNames)).Length];
		for(int i = 0; i < countsOfEachUnit.Length; i++)
		{
			countsOfEachUnit[i] = 0;
		}
	}
	void Start()
	{
		
	}
	public void Setup(Player oldPlayer)
	{
		DontDestroyOnLoad(this);
		playerName = name;
		playerNumber = oldPlayer.GetPlayerNumber();
		this.side = oldPlayer.GetSide();
		mainPlayerColor = oldPlayer.mainPlayerColor;
		pigs = new PlayerInGameStatistics();
		pigs.name = playerName;
		units = new List<UnitController>();
		properties = new List<Property>();
		aiLevel = oldPlayer.aiLevel;
		generalInStartMenu = oldPlayer.generalInStartMenu;
		loggingProductionData = oldPlayer.loggingProductionData;
		if(loggingProductionData)
		{
			gameObject.AddComponent<MouseEventHandler>();
		}
	}
	public override void StartTurn()
	{
		base.StartTurn();
		PushUnits();
		productionAttempts = 0;
	}
	public override void EndTurn()
	{
		base.EndTurn();
	}
	public override void AddUnit(UnitController inUnit)
	{
		if(!units.Contains(inUnit))
		{
			units.Add(inUnit);
			inUnit.SetOwner(this);
			countsOfEachUnit[(int)inUnit.unitClass]++;
		}
	}
	public override void DeleteUnit(UnitController inUnit)
	{
		if(units.Contains(inUnit))
		{
			if(inUnit == currentGeneralUnit)
			{
				currentGeneralUnit = null;
				selectedGeneral.Hide();
			}
			countsOfEachUnit[(int)inUnit.unitClass]--;
			inUnit.KillUnit();
			units.Remove(inUnit);
			pigs.unitsLost++;
		}
		if(units.Count == 0)
		{
			InGameController.RemovePlayer(this);
		}
	}
	public override void AddProperty(Property inUnit)
	{
		if(!properties.Contains(inUnit))
		{
			properties.Add(inUnit);
			countsOfEachUnit[(int)inUnit.propertyType]++;
			if(inUnit.propertyClass.producableUnits.Length > 0)
			{
				producingProperties++;
			}
			inUnit.SetOwner(this);
			if(inUnit.propertyType == UnitNames.Headquarters)
			{
				hQBlock = inUnit.GetCurrentBlock();
			}
		}
	}
	public override void RemoveProperty(Property inUnit)
	{
		if(properties.Contains(inUnit))
		{
			countsOfEachUnit[(int)inUnit.propertyType]--;
			if(inUnit.propertyClass.producableUnits.Length > 0)
			{
				producingProperties--;
			}
			if(inUnit.propertyType == UnitNames.Headquarters)
			{
				InGameController.RemovePlayer(this);
			}
			properties.Remove(inUnit);
		}
	}
	protected abstract TerrainBlock StateSearch(int numSearchTurns, int statesKept);

	protected abstract float EvaluatePosition(TerrainBlock position, out UnitOrderOptions order);
	public float MoveTowardsEnemyHQ(TerrainBlock block)
	{
		float closest = InGameController.ClosestEnemyHQ(block, currentUnit.moveClass, currentUnit.GetOwner());
		if(closest < 1 && !currentUnit.canCapture)
		{
			return -10;
		}
		return (100 - closest) * hQMoveTowardsModifier;
	}
	protected virtual float UtilityOfAddingGeneral()
	{
		return ((float)currentUnit.baseCost)/30000f;
	}
	protected abstract void ProduceUnits();
	
	[System.Serializable]
	public class UnitNameIntBinder
	{
		public UnitNames name;
		public int value;
	}
	protected bool HasBuildingToProduceUnit (UnitNames name)
	{
		for(int i = 0; i < properties.Count; i++)
		{
			if(properties[i].CanProduceUnit(name))
			{
				return true;
			}
		}
		return false;
	}

	protected UnitNames RandomUnitName()
	{
		return (UnitNames)System.Enum.GetValues(typeof(UnitNames)).GetValue(UnityEngine.Random.Range(0, 27));
	}
	protected void PushUnits()
	{
		unitsToMove.Clear();
		for(int i = 0; i < units.Count; i++)
		{
			unitsToMove.Push(units[i]);
		}
	}
}

