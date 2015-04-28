using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class AIPlayerMedium : AIPlayer
{
	bool producingUnits;
	public enum ProductionTest
	{
		RandomProd,
		HalfNHalf,
		Learned
	}
	;
	public ProductionTest productionType;
	private Clusterer clusterer;
	private List<AttackableObject> targetedObjects;
	private bool makeSupplySea, makeSupplyLand, makeTransport;
	private UnitName transportToMake;
	public bool produceRandom;
	private Dictionary<UnitName, List<UnitController>> supportUnits;
	ProductionEngine productionEngine;
	public GameObject graphicalValueBlock;
	List<GameObject> blockList;
	protected override void Awake ()
	{
		base.Awake ();
		blockList = new List<GameObject> ();
		targetedObjects = new List<AttackableObject> ();
		if (produceRandom) {
			productionType = (ProductionTest)System.Enum.GetValues (typeof(ProductionTest)).GetValue (UnityEngine.Random.Range (0, System.Enum.GetNames (typeof(ProductionTest)).Length));
		}
		clusterer = new Clusterer ();
		supportUnits = new Dictionary<UnitName, List<UnitController>> ();
		Array unitNames = System.Enum.GetValues (typeof(UnitName));
		for (int i = 0; i < unitNames.Length; i++) {
			MonoBehaviour mono = Utilities.GetPrefabFromUnitName ((UnitName)unitNames.GetValue (i));
			if (mono is UnitController) {
				UnitController uc = (UnitController)mono;
				if (uc.canTransport) {
					supportUnits.Add (uc.unitClass, new List<UnitController> ());
				}
			}
		}
	}
	void Start ()
	{
		productionEngine = new ProductionEngine ();
	}
	UnitController GetSupportUnit (UnitController inUnit)
	{
		UnitController closestUnit = null;
		float closestUnitDistance = float.PositiveInfinity;
		foreach (List<UnitController> list in supportUnits.Values) {
			foreach (UnitController checkUnit in list) {
				if (checkUnit.target.HasTarget () && checkUnit.CanCarryUnit (inUnit)) {
					float distance = TerrainBuilder.ManhattanDistance (checkUnit.currentBlock, inUnit.currentBlock);
					if (distance < closestUnitDistance) {
						closestUnit = checkUnit;
						closestUnitDistance = distance;
					}
				}
			}
		}
		return closestUnit;
	}
	void SetTransportToMake (UnitController inUnit)
	{
		int lowestCost = int.MaxValue;
		foreach (UnitName name in supportUnits.Keys) {
			UnitController mono = (UnitController)Utilities.GetPrefabFromUnitName (name);
			if (mono.CanCarryUnit (inUnit)) {
				for (int i = 0; i < properties.Count; i++) {
					if (properties [i].CanProduceUnit (mono.unitClass)) {
						if (mono.baseCost < lowestCost) {
							transportToMake = name;
							lowestCost = mono.baseCost;
						}
						break;
					}
				}
			}
		}
	}
	protected override float EvaluatePosition (TerrainBlock position, out UnitOrderOptions order)
	{
		throw new NotImplementedException ();
	}

	public override void AddUnit (UnitController inUnit)
	{
		if (!units.Contains (inUnit)) {
			units.Add (inUnit);
			inUnit.owner = this;
			countsOfEachUnit [(int)inUnit.unitClass]++;
			if (inUnit.canTransport) {
				List<UnitController> temp = null;
				supportUnits.TryGetValue (inUnit.unitClass, out temp);
				temp.Add (inUnit);
			}
		} else {
			throw new UnityException ("Unit added already");
		}
	}
	public override void DeleteUnitFromGame (UnitController inUnit)
	{
		if (units.Contains (inUnit)) {
			if (inUnit == currentGeneralUnit) {
				currentGeneralUnit = null;
				selectedGeneral.Hide ();
			}
			if (inUnit.canTransport) {
				List<UnitController> temp = null;
				supportUnits.TryGetValue (inUnit.unitClass, out temp);
				if (temp != null) {
					temp.Remove (inUnit);
				}
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
	public override void StartTurn ()
	{
		base.StartTurn ();
		UpdateTargets ();
		ComputeAllUnitPositions ();
	}
	Property GetNextClosestUncapturedProperty (UnitController unit)
	{
		List<Property> enemyProperties = InGameController.instance.GetAllEnemyProperties (this);
		float closestDistance = float.PositiveInfinity;
		Property closestProperty = null;
		float currentPropDistance = 0;
		foreach (Property prop in enemyProperties) {
			if (prop.propertyClass.capturable) {
				if (unit.currentBlock.CanReachBlock (unit, prop.GetOccupyingBlock ())) {
					currentPropDistance = InGameController.instance.currentTerrain.MinDistanceToTile (unit.currentBlock, prop.GetOccupyingBlock (), unit);
					if (currentPropDistance == float.PositiveInfinity) {
						Debug.Log ("Positive infinity returned, probably error with canreachblock functions");
					}
				} else {
					currentPropDistance = InGameController.instance.currentTerrain.MinDistanceToTileThroughUnMovableBlocks (unit.currentBlock, prop.GetOccupyingBlock (), unit, 5);
				}
				var enemyHQTuple = InGameController.instance.ClosestEnemyHQ (prop.GetOccupyingBlock (), unit.moveClass, this);
				float playerHQDistance = InGameController.instance.currentTerrain.MinDistanceToTileThroughUnMovableBlocks (unit.currentBlock, hQBlock, unit, 5);
				var enemyHQDistance = enemyHQTuple.Item1 > 0 ? enemyHQTuple.Item1 : .5f;
				playerHQDistance = playerHQDistance > 0 ? playerHQDistance : .5f;
				currentPropDistance *= (playerHQDistance / enemyHQDistance);
				currentPropDistance /= prop.propertyClass.AICapturePriority;
				if (!targetedObjects.Contains (prop) && currentPropDistance < closestDistance && prop.propertyClass.capturable && !(prop.GetOccupyingBlock ().IsOccupied () && (prop.GetOccupyingBlock ().occupyingUnit.owner.IsSameSide (unit.owner) && prop.occupyingUnit != unit))) {
					closestProperty = prop;
					closestDistance = currentPropDistance;
				}
			}
		}
		return closestProperty;
	}
	UnitController GetClosestResupplyUnit (UnitController unit)
	{
		float closestDistance = float.PositiveInfinity;
		UnitController closestUnit = null;
		float currentUnitDistance = 0;
		foreach (UnitController u in units) {
			currentUnitDistance = TerrainBuilder.ManhattanDistance (u.currentBlock, unit.currentBlock);
			if (currentUnitDistance < closestDistance && u.canSupply) {
				closestDistance = currentUnitDistance;
				closestUnit = u;
			}
		}
		return closestUnit;
	}
	/// <summary>
	/// Should add targets to units' target lists. This way we can push certain units to focus on particular units (interceptors targeting interceptors/t fighters)
	/// </summary>
	void UpdateTargets ()
	{
		targetedObjects.Clear ();
		List<UnitController> unitsNeedingSupply = new List<UnitController> (units);
		unitsNeedingSupply.Sort (UnitController.CompareBySupplyScore);
		List<UnitController> targetedUnitsNeedingSupply = new List<UnitController> ();
		List<UnitController> assignedUnits = new List<UnitController> ();
		//list of clusters of clustered enemy units
		var lists = clusterer.Estimate (InGameController.instance.GetAllEnemyUnits (this));
		//assign infantry to buildings if possible - easy first step
		var infantryList = units.Where ((x) => 
			x.unitClass == UnitName.Infantry || x.unitClass == UnitName.Mortar || x.unitClass == UnitName.Stinger
		);
		foreach (var i in infantryList) {
			if (!i.target.HasTarget ()) {
				var possibleTarget = GetNextClosestUncapturedProperty (i);
				if (possibleTarget != null) {
					assignedUnits.Add (i);
					targetedObjects.Add (possibleTarget);
					i.target.AddTarget (possibleTarget);
				}
			}
		}
		var unassignedUnits = units.Where ((x) => !x.target.HasTarget ());
		foreach (var i in unassignedUnits) {
			// Units needing resupply
			if (i.NeedsResupply ()) {
				var possibleTarget = GetClosestResupplyUnit (i);
				if (possibleTarget) {
					if (i.moveClass == MovementType.Littoral || i.moveClass == MovementType.Sea) {
						makeSupplySea = true;
					} else {
						makeSupplyLand = true;
					}
					i.target.AddTarget (hQBlock.occupyingProperty);
				}
			}
			// combat units
			if (i.minAttackRange > 0) {
				//find best cluster for this unit
				var best = lists.Max ((x) => ClusterAptitude (x, i));
				//find best unit in that cluster
				i.target.AddTarget (BestUnitInCluster (best, i));
			}
			// Supplying units
			if (i.canSupply) {
				UnitController candidate = null;
				float closestDistance = float.PositiveInfinity;
				float currentDistance = 0;
				foreach (UnitController uc in unitsNeedingSupply) {
					if (!targetedUnitsNeedingSupply.Contains (candidate)) {
						currentDistance = (i.transform.position - uc.transform.position).magnitude;
						if (currentDistance < closestDistance) {
							candidate = uc;
							closestDistance = currentDistance;
						}
					}
				}
				targetedUnitsNeedingSupply.Add (candidate);
				i.target.AddTarget (candidate);
			}
		}
	}
	/// <summary>
	/// TODO
	/// </summary>
	void ComputeAllUnitPositions ()
	{
		List<UnitController> temp = new List<UnitController> (unitsToMove);
		temp.Sort (UnitController.CompareByMovePriority);
		unitsToMove = new Stack<UnitController> (temp);
		
	}
	
	/// <summary>
	/// Aptitude of a cluster, determined by the sum of the damage inUnit can do to each unit in the cluster minus what each unit can do to inUnit
	/// </summary>
	/// <returns>The aptitude.</returns>
	/// <param name="enemyUnits">Enemy units.</param>
	/// <param name="inUnit">In unit.</param>
	float ClusterAptitude (List<AttackableObject> enemyUnits, UnitController inUnit)
	{
		float sum = 0;
		Vector3 median = Vector3.zero;
		for (int i = 0; i < enemyUnits.Count; i++) {
			sum += DamageValues.CalculateDamage (inUnit, enemyUnits [i]) * enemyUnits [i].UnitCost () - .33f * DamageValues.CalculateDamage (enemyUnits [i], inUnit) * inUnit.UnitCost ();
			median += enemyUnits [i].GetPosition ();
		}
		median /= (enemyUnits.Count > 0) ? enemyUnits.Count : 1;
		float distance = (inUnit.transform.position - median).magnitude;
		return sum / (distance > 1 ? distance : 1);
	}
	/// <summary>
	/// Returns the best unit to attack for the input unit from the list of enemyUnits
	/// </summary>
	/// <returns>The unit in cluster.</returns>
	/// <param name="enemyUnits">Enemy units.</param>
	/// <param name="inUnit">In unit.</param>
	AttackableObject BestUnitInCluster (List<AttackableObject> enemyUnits, UnitController inUnit)
	{
		float best = 0;
		AttackableObject bestUnit = null;
		for (int i = 0; i < enemyUnits.Count; i++) {
			float current = DamageValues.CalculateDamage (inUnit, enemyUnits [i]) * enemyUnits [i].UnitCost ();
			if (current > best) {
				best = current;
				bestUnit = enemyUnits [i];
			}
		}
		return bestUnit;
	}
	
	void Update ()
	{
		if (canIssueOrders && !InGameController.instance.endingGame) {
			MediumAIUpdate ();
		}
	}
	
	protected override TerrainBlock StateSearch (int numSearchTurns, int statesKept)
	{
		TerrainBlock bestBlockSoFar = null;
		PositionEvaluation bestValueSoFar = new PositionEvaluation (float.NegativeInfinity);
		
		currentUnit.target.SetTerrainBlockStates ();
		blockList = new List<GameObject> ();
		List<TerrainBlock> blocks = InGameController.instance.currentTerrain.MovableBlocks (currentUnit.currentBlock, currentUnit, currentUnit.EffectiveMoveRange ());
		Debug.Log (blocks.Count);
		
		foreach (TerrainBlock block in blocks) {
			PositionEvaluation temp = RecursiveEvaluatePosition (block);
#if DEBUG
			GameObject go = Instantiate (graphicalValueBlock, block.transform.position, Quaternion.identity) as GameObject;
			go.transform.Translate (0, temp.value, 0);
			blockList.Add (go);
#endif
			if (temp.value > bestValueSoFar.value) {
				bestValueSoFar = temp;
				bestBlockSoFar = block;
			}
		}
		//Debug.Break ();
		//Debug.Log(currentUnit.unitClass + " Positions evaluated: " + totalPositionsEvaluated);// + " / positions at depth 0: " + blocks.Count);
		return bestBlockSoFar;
	}
	
	protected PositionEvaluation RecursiveEvaluatePosition (TerrainBlock block)
	{
		PositionEvaluation bestValueSoFar = new PositionEvaluation (float.NegativeInfinity);
		List<TerrainBlock> blocks = InGameController.instance.currentTerrain.MovableBlocks (block, currentUnit, currentUnit.EffectiveMoveRange ());
		for (int i = 0; i < blocks.Count; i++) {
			PositionEvaluation temp = EvaluatePosition (blocks [i]);
			if (temp.value > bestValueSoFar.value) {
				bestValueSoFar = temp;
			}
		}
		bestValueSoFar.value *= .33f;
		PositionEvaluation outEvaluation = EvaluatePosition (block);
		outEvaluation.value += bestValueSoFar.value;
		return outEvaluation;
	}
	
	/*protected PositionEvaluation RecursiveEvaluatePosition(int depth, TerrainBlock block, ref int positionsEvaluated, int maxDepth)
	{
		positionsEvaluated++;
		if(depth < maxDepth)
		{
			PositionEvaluation bestValueSoFar = new PositionEvaluation(float.NegativeInfinity);
			List<TerrainBlock> blocks = InGameController.instance.currentTerrain.MovableBlocks(block, currentUnit, currentUnit.EffectiveMoveRange());
			SortedList<PositionEvaluation, TerrainBlock> rankings = new SortedList<PositionEvaluation, TerrainBlock>(blocks.Count, new PositionEvaluationComparer());
			foreach(TerrainBlock b in blocks)
			{
				rankings.Add(EvaluatePosition(b), b);
			}
			int min = Mathf.Min(rankings.Count, 10);
			for(int i = 0; i < min; i++)
			{
				PositionEvaluation temp = RecursiveEvaluatePosition(depth + 1, rankings.Values[i], ref positionsEvaluated, maxDepth);
				if(temp.value > bestValueSoFar.value)
				{
					bestValueSoFar = temp;
				}
			}
			PositionEvaluation outputEvaluation = EvaluatePosition(block);
			//depth is inverted, incorrect atm
			outputEvaluation.value += (bestValueSoFar.value * Mathf.Pow(.5f, depth));
			return outputEvaluation;
		}
		else
		{
			return EvaluatePosition(block);
		}
	}*/
	
	public class PositionEvaluationComparer : IComparer<PositionEvaluation>
	{
		public int Compare (PositionEvaluation a, PositionEvaluation b)
		{
			if (a.value < b.value) {
				return 1;
			} else {
				return -1;
			}
		}
	}
	
	public class PositionEvaluation
	{
		public UnitOrderOptions bestOrder;
		public float value;
		public TerrainBlock block;
		public PositionEvaluation (float v)
		{
			value = v;
			bestOrder = UnitOrderOptions.EndTurn;
			block = null;
		}
	}
	
	protected PositionEvaluation EvaluatePosition (TerrainBlock position)
	{
		PositionEvaluation bestOptionValue = new PositionEvaluation (0);
		if (position.IsOccupied () && position.occupyingUnit != currentUnit && (!position.occupyingUnit.CanCarryUnit (currentUnit) || !position.occupyingUnit.owner.IsSameSide (currentUnit.owner))) {
			bestOptionValue.value = -100000;
			return bestOptionValue;
		} else {
			bestOptionValue.value = waitModifier;
			bestOptionValue.bestOrder = UnitOrderOptions.EndTurn;
			List<UnitOrderOptions> possibleOrders = currentUnit.CalculateAwaitingOrderOptions (position);
			foreach (UnitOrderOptions currentOrder in possibleOrders) {
				switch (currentOrder) {
				case UnitOrderOptions.AddGeneral:
					{
						if (UtilityOfAddingGeneral () * addGeneralModifier > bestOptionValue.value) {
							bestOptionValue.value = UtilityOfAddingGeneral () * addGeneralModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.AddGeneral;
						}
						break;
					}
				case UnitOrderOptions.Attack:
					{
						if (currentUnit.canMoveAndAttack || currentUnit.currentBlock == position) {
							AttackableObject bestTarget = null;
							float bestTargetedUnitDamage = currentUnit.AISelectBestAttackObject (position, out bestTarget);
							if (bestTargetedUnitDamage * attackModifier > bestOptionValue.value) {
								bestOptionValue.value = bestTargetedUnitDamage * attackModifier;
								bestOptionValue.bestOrder = UnitOrderOptions.Attack;
								bestOptionValue.value += position.defenseBoost * defensiveTerrainDesireModifier;
							}
						}
						break;
					}
				case UnitOrderOptions.Board:
					{
						if (boardModifier > bestOptionValue.value) {
							bestOptionValue.value = boardModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.Board;
						}
						break;
					}
				case UnitOrderOptions.BuildBridge:
					{
						if (buildBridgeModifier > bestOptionValue.value) {
							bestOptionValue.value = buildBridgeModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.BuildBridge;
						}
						break;
					}
				case UnitOrderOptions.Capture:
					{
						float bestCapture = captureModifier * (((float)currentUnit.health.GetRawHealth ()) / 100) + (1 - position.occupyingProperty.NormalizedCaptureCount ()) * position.occupyingProperty.propertyClass.AICapturePriority;
						if (bestCapture > bestOptionValue.value) {
							bestOptionValue.value = bestCapture;
							bestOptionValue.bestOrder = UnitOrderOptions.Capture;
						}
						break;
					}
				case UnitOrderOptions.Load:
					{
						float value = 1 - (currentUnit.EffectiveMoveRange () / 5);
						if (position.occupyingUnit.healsCarriedUnits) {
							if (currentUnit.health.GetRawHealth () < 50) {
								value = (1 - ((float)currentUnit.health.GetRawHealth ()) / 100f);
							}
						}
						if (value * loadModifier > bestOptionValue.value) {
							bestOptionValue.value = value * loadModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.Load;
						}
						break;
					}
				case UnitOrderOptions.ProduceMissile:
					{
						if (produceMissileModifier > bestOptionValue.value) {
							bestOptionValue.value = produceMissileModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.ProduceMissile;
						}
						break;
					}
				case UnitOrderOptions.Repair:
					{
						if (repairModifier > bestOptionValue.value) {
							bestOptionValue.value = repairModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.Repair;
						}
						break;
					}
				case UnitOrderOptions.Stealthify:
					{
						float value = currentUnit.GetNormalizedFuel () * stealthifyModifier;
						if (value > bestOptionValue.value) {
							bestOptionValue.value = value;
							bestOptionValue.bestOrder = UnitOrderOptions.Stealthify;
						}
						break;
					}
				case UnitOrderOptions.Supply:
					{
						float supplyValue = 0;
						for (int i = 0; i < position.adjacentBlocks.Length; i++) {
							if (position.adjacentBlocks [i].IsOccupied () && position.adjacentBlocks [i].occupyingUnit.owner.IsSameSide (currentUnit.owner) && position.adjacentBlocks [i].occupyingUnit != currentUnit) {
								supplyValue += (1 - position.adjacentBlocks [i].occupyingUnit.GetNormalizedFuel ()) + (1 - position.adjacentBlocks [i].occupyingUnit.GetNormalizedAmmo ());
							}
						}
						if (supplyValue * supplyModifier > bestOptionValue.value) {
							bestOptionValue.value = supplyValue * supplyModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.Supply;
						}
						break;
					}
				case UnitOrderOptions.Unload:
					{
						if (unloadModifier > bestOptionValue.value) {
							bestOptionValue.value = unloadModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.Unload;
						}
						break;
					}
				case UnitOrderOptions.UnStealthify:
					{
						float value = (1 - currentUnit.GetNormalizedFuel ()) * unStealthifyModifier;
						if (value > bestOptionValue.value) {
							bestOptionValue.value = value;
							bestOptionValue.bestOrder = UnitOrderOptions.UnStealthify;
						}
						break;
					}
				case UnitOrderOptions.Join:
					{
						float value = 20 - position.occupyingUnit.health.PrettyHealth () - currentUnit.health.PrettyHealth ();
						value *= joinModifier;
						if (value > bestOptionValue.value) {
							bestOptionValue.value = value;
							bestOptionValue.bestOrder = UnitOrderOptions.Join;
						}
						break;
					}
				}
			}
			if (position.HasProperty ()) {
				if (position.occupyingProperty.CanHealUnit (currentUnit) && position.occupyingProperty.owner.IsSameSide (currentUnit.owner)) {
					if (currentUnit.health < 80) {
						float value = (1 - ((float)currentUnit.health.GetRawHealth ()) / 100f);
						if (position.occupyingProperty.CanProduceUnit (currentUnit.unitClass)) {
							value *= .3f;
						}
						if (value * healModifier > bestOptionValue.value) {
							bestOptionValue.value = value * healModifier;
						}
					}
				}
				bestOptionValue.value += position.occupyingProperty.DefenseBonus () * defensiveTerrainDesireModifier;
			}
			bestOptionValue.value += position.defenseBoost * defensiveTerrainDesireModifier;
			//float danger = EnemyDangerAtBlock(position);
			//bestOptionValue.value -= danger;
			bestOptionValue.value += MoveTowardsTarget (position);
			bestOptionValue.value += UnityEngine.Random.Range (0, randomnessModifier);
			return bestOptionValue;
		}
	}
	
	float MoveTowardsTarget (TerrainBlock block)
	{
		return (1000 - block.cachedGCost) * hQMoveTowardsModifier;
	}
	float EnemyDangerAtBlock (TerrainBlock block)
	{
		List<AttackableObject> otherUnits = InGameController.instance.currentTerrain.ObjectsWithinRange (block, 1, 12, currentUnit);
		float alliedUnits = 1;
		float totalEnemyDamage = 0;
		UnitController other = null;
		InGameController.instance.currentTerrain.SaveIlluminatedBlocks ();
		foreach (AttackableObject temp in otherUnits) {
			if (temp is UnitController) {
				other = temp as UnitController;
				if (other.owner.IsSameSide (currentUnit.owner) && other.EffectiveMoveRange () >= TerrainBuilder.ManhattanDistance (other.currentBlock, block)) {
					alliedUnits++;
				} else if (!other.owner.IsSameSide (currentUnit.owner) && !other.owner.IsNeutralSide () && other.EffectiveMoveRange () + other.EffectiveAttackRange () >= TerrainBuilder.ManhattanDistance (other.currentBlock, block) && other.gameObject.activeSelf) {
					if (other.canMoveAndAttack && other.minAttackRange > 0) {
						List<TerrainBlock> otherBlocks = InGameController.instance.currentTerrain.MovableBlocks (other.currentBlock, other, other.EffectiveMoveRange ());
						float otherMaxAttackRange = other.EffectiveAttackRange ();
						foreach (TerrainBlock t in otherBlocks) {
							if (TerrainBuilder.ManhattanDistance (t, block) <= otherMaxAttackRange) {
								totalEnemyDamage += DamageValues.CalculateDamage (other, currentUnit) * currentUnit.baseCost;
								break;
							}
						}
					} else if (TerrainBuilder.ManhattanDistance (other.currentBlock, block) >= other.minAttackRange && TerrainBuilder.ManhattanDistance (other.currentBlock, block) <= other.EffectiveAttackRange ()) {
						totalEnemyDamage += DamageValues.CalculateDamage (other, currentUnit) * currentUnit.baseCost;
					}
				}
			}
		}
		InGameController.instance.currentTerrain.LoadIlluminatedBlocks ();
		return (totalEnemyDamage * currentUnit.AIDefensiveness) / (alliedUnits * 15000f);
	}
	
	protected override void ProduceUnits ()
	{
		unitsToMake.Clear ();
		MediumUnitProductionRandom ();
	}
	
	void MediumUnitProductionRandom ()
	{
		if (producingUnits) {
			UnitName rankedName = productionEngine.Evaluate (this);
			if (makeSupplyLand) {
				rankedName = UnitName.SupplyTank;
				makeSupplyLand = false;
			} else if (makeSupplySea) {
				rankedName = UnitName.SupplyShip;
				makeSupplySea = false;
			} else if (makeTransport) {
				rankedName = transportToMake;
				makeTransport = false;
			}
			if (((UnitController)Utilities.GetPrefabFromUnitName (rankedName)).baseCost <= funds) {
				SortPropertiesByHQDistance (((UnitController)Utilities.GetPrefabFromUnitName (rankedName)).moveClass);
				for (int i = 0; i < properties.Count; i++) {
					if (properties [i].currentState == UnitState.UnMoved && !properties [i].GetOccupyingBlock ().IsOccupied () && properties [i].CanProduceUnit (rankedName)) {
						properties [i].AIProduceUnit (rankedName);
						break;
					}
				}
			}
			productionAttempts++;
			producingUnits = false;
		} else {
			if (productionAttempts < producingProperties) {
				producingUnits = true;
			}
		}
	}
	
	void SortPropertiesByHQDistance (MovementType moveType)
	{
		for (int i = 0; i < properties.Count; i++) {
			properties [i].cachedDistanceFromEnemyHQ = InGameController.instance.ClosestEnemyHQ (properties [i].GetOccupyingBlock (), moveType, this).Item1;
		}
		properties.Sort (Property.CompareByDistanceFromEnemyHQ);
	}
	
	void MediumAIUpdate ()
	{
		if (currentUnit != null) {
			switch (currentUnit.currentState) {
			case UnitState.UnMoved:
				{
					currentUnit.AISelect ();
					break;
				} 
			case UnitState.Selected:
				{
					TerrainBlock block = StateSearch (1, 3);
					currentUnit.AIMoveTo (block);
					break;
				} 
			case UnitState.Moving:
				{
					break;
				} 
			case UnitState.FinishedMove:
				{
					currentUnit = null;
#if DEBUG
					foreach (GameObject go in blockList) {
						Destroy (go);
					}
					blockList.Clear ();
#endif
					break;
				} 
			case UnitState.AwaitingOrder:
				{
					PositionEvaluation p = EvaluatePosition (currentUnit.awaitingOrdersBlock);
					currentUnit.AIDoOrder (p, true);
					break;
				}
			}
		} else {
			if (unitsToMove.Count > 0) {
				do {
					currentUnit = unitsToMove.Pop ();
				} while(currentUnit.isInUnit && unitsToMove.Count > 0);
				if (unitsToMove.Count == 0 && currentUnit.isInUnit) {
					currentUnit = null;
				}
			} else {
				ProduceUnits ();
				if (productionAttempts >= producingProperties) {
					InGameController.instance.AdvanceTurn ();
				}
			}
		}
	}
}
