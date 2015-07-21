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
	protected Queue<UnitName> unitsToMake;
	protected List<UnitController> sightedEnemyUnits;
	protected UnitController currentUnit;
	protected int[] countsOfEachUnit;
	protected int producingProperties = 0;
	protected int productionAttempts;
	protected Dictionary<UnitName, BayesianNetwork> bayesNets;
	protected override void Awake ()
	{
		base.Awake ();
		unitsToMove = new Stack<UnitController> ();
		unitsToMake = new Queue<UnitName> ();
		countsOfEachUnit = new int[System.Enum.GetValues (typeof(UnitName)).Length];
		for (int i = 0; i < countsOfEachUnit.Length; i++) {
			countsOfEachUnit [i] = 0;
		}
	}
	protected void Start ()
	{
		bayesNets = new Dictionary<UnitName, BayesianNetwork> ();
		foreach (UnitName unit in System.Enum.GetValues(typeof(UnitName))) {
			bayesNets.Add (unit, new BayesianNetwork (Mathf.RoundToInt (InGameController.instance.currentTerrain.upperXMapBound + 1), Mathf.RoundToInt (InGameController.instance.currentTerrain.upperZMapBound + 1)));
		}
		Debug.Log (Mathf.RoundToInt (InGameController.instance.currentTerrain.upperXMapBound));
	}
	public void Setup (Player oldPlayer)
	{
		DontDestroyOnLoad (this);
		playerName = name;
		playerNumber = oldPlayer.GetPlayerNumber ();
		this.side = oldPlayer.GetSide ();
		mainPlayerColor = oldPlayer.mainPlayerColor;
		pigs = new PlayerInGameStatistics ();
		pigs.name = playerName;
		units = new List<UnitController> ();
		properties = new List<Property> ();
		aiLevel = oldPlayer.aiLevel;
		generalSelectedInGUI = oldPlayer.generalSelectedInGUI;
		loggingProductionData = oldPlayer.loggingProductionData;
		if (loggingProductionData) {
			gameObject.AddComponent<MouseEventHandler> ();
		}
	}
	public override void StartTurn ()
	{
		base.StartTurn ();
		PushUnits ();
		productionAttempts = 0;
	}
	public override void EndTurn ()
	{
		base.EndTurn ();
	}
	public override void AddUnit (UnitController inUnit)
	{
		if (!units.Contains (inUnit)) {
			units.Add (inUnit);
			inUnit.owner = this;
			countsOfEachUnit [(int)inUnit.unitClass]++;
		}
	}
	public override void DeleteUnitFromGame (UnitController inUnit)
	{
		if (units.Contains (inUnit)) {
			if (inUnit == currentGeneralUnit) {
				currentGeneralUnit = null;
				selectedGeneral.Hide ();
			}
			countsOfEachUnit [(int)inUnit.unitClass]--;
			inUnit.KillUnit ();
			units.Remove (inUnit);
			pigs.unitsLost++;
		}
		if (units.Count == 0) {
			InGameController.instance.RemovePlayer (this);
		}
	}
	public override void AddProperty (Property inUnit)
	{
		if (!properties.Contains (inUnit)) {
			properties.Add (inUnit);
			countsOfEachUnit [(int)inUnit.propertyType]++;
			if (inUnit.propertyClass.producableUnits.Length > 0) {
				producingProperties++;
			}
			inUnit.SetOwner (this);
			if (inUnit.propertyType == UnitName.Headquarters) {
				hQBlock = inUnit.GetCurrentBlock ();
			}
		}
	}
	public override void RemoveProperty (Property inUnit)
	{
		if (properties.Contains (inUnit)) {
			countsOfEachUnit [(int)inUnit.propertyType]--;
			if (inUnit.propertyClass.producableUnits.Length > 0) {
				producingProperties--;
			}
			if (inUnit.propertyType == UnitName.Headquarters) {
				InGameController.instance.RemovePlayer (this);
			}
			properties.Remove (inUnit);
		}
	}
	protected abstract Tuple<TerrainBlock, PositionEvaluation> StateSearch (int numSearchTurns, int statesKept);

	protected abstract float EvaluatePosition (TerrainBlock position, out UnitOrderOptions order);
	public float MoveTowardsEnemyHQ (TerrainBlock block)
	{
		var closest = InGameController.instance.ClosestEnemyHQ (block, currentUnit.moveClass, currentUnit.owner);
		if (closest.Item1 < 1 && !currentUnit.canCapture) {
			return -10;
		}
		return (100 - closest.Item1) * hQMoveTowardsModifier;
	}
	protected virtual float UtilityOfAddingGeneral ()
	{
		return ((float)currentUnit.baseCost) / 30000f;
	}
	protected abstract void ProduceUnits ();
	
	[System.Serializable]
	public class UnitNameIntBinder
	{
		public UnitName name;
		public int value;
	}
	protected bool HasBuildingToProduceUnit (UnitName name)
	{
		for (int i = 0; i < properties.Count; i++) {
			if (properties [i].CanProduceUnit (name)) {
				return true;
			}
		}
		return false;
	}

	protected UnitName RandomUnitName ()
	{
		return (UnitName)System.Enum.GetValues (typeof(UnitName)).GetValue (UnityEngine.Random.Range (0, 27));
	}
	protected void PushUnits ()
	{
		unitsToMove.Clear ();
		for (int i = 0; i < units.Count; i++) {
			unitsToMove.Push (units [i]);
		}
	}
	public void UpdateBayesNet (UnitName unitClass, Vector3 position, float value)
	{
		bayesNets [unitClass].UpdateNetwork (position.x, position.z, value);
	}
}

