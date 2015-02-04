using System;
using System.Collections.Generic;
using UnityEngine;
public class AIPlayerMedium : AIPlayer
{
	bool producingUnits;
	public enum ProductionTest {RandomProd, HalfNHalf, Learned};
	public ProductionTest productionType;
	private Clusterer clusterer;
	private List<AttackableObject> targetedObjects;
	private float clusterAssignmentOverflow = 1.25f;
	private bool makeSupplySea, makeSupplyLand, makeTransport;
	private UnitNames transportToMake;
	public bool produceRandom;
	private Dictionary<UnitNames, List<UnitController>> supportUnits;
	ProductionEngine productionEngine;
	protected override void Awake()
	{
		base.Awake();
		targetedObjects = new List<AttackableObject>();
		if(produceRandom)
		{
			productionType = (ProductionTest)System.Enum.GetValues(typeof(ProductionTest)).GetValue(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(ProductionTest)).Length));
		}
		clusterer = new Clusterer();
		supportUnits = new Dictionary<UnitNames, List<UnitController>>();
		Array unitNames = System.Enum.GetValues(typeof(UnitNames));
		for(int i = 0; i < unitNames.Length; i++)
		{
			MonoBehaviour mono = Utilities.GetPrefabFromUnitName((UnitNames)unitNames.GetValue(i));
			if(mono is UnitController)
			{
				UnitController uc = (UnitController) mono;
				if(uc.canTransport)
				{
					supportUnits.Add(uc.unitClass, new List<UnitController>());
				}
			}
		}
		productionEngine = new ProductionEngine();
	}
	UnitController GetSupportUnit(UnitController inUnit)
	{
		UnitController closestUnit = null;
		float closestUnitDistance = float.PositiveInfinity;
		foreach(List<UnitController> list in supportUnits.Values)
		{
			foreach(UnitController checkUnit in list)
			{
				if(checkUnit.AITarget == null && checkUnit.CanCarryUnit(inUnit))
				{
					float distance = TerrainBuilder.ManhattanDistance(checkUnit.currentBlock, inUnit.currentBlock);
					if(distance < closestUnitDistance)
					{
						closestUnit = checkUnit;
						closestUnitDistance = distance;
					}
				}
			}
		}
		return closestUnit;
	}
	void SetTransportToMake(UnitController inUnit)
	{
		int lowestCost = int.MaxValue;
		foreach(UnitNames name in supportUnits.Keys)
		{
			UnitController mono = (UnitController)Utilities.GetPrefabFromUnitName(name);
			if(mono.CanCarryUnit(inUnit))
			{
				for(int i = 0; i < properties.Count; i++)
				{
					if(properties[i].CanProduceUnit(mono.unitClass))
					{
						if(mono.baseCost < lowestCost)
						{
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

	public override void AddUnit(UnitController inUnit)
	{
		if(!units.Contains(inUnit))
		{
			units.Add(inUnit);
			inUnit.SetOwner(this);
			countsOfEachUnit[(int)inUnit.unitClass]++;
			if(inUnit.canTransport)
			{
				List<UnitController> temp = null;
				supportUnits.TryGetValue(inUnit.unitClass, out temp);
				temp.Add(inUnit);
			}
		}
	}
	public override void DeleteUnitFromGame(UnitController inUnit)
	{
		if(units.Contains(inUnit))
		{
			if(inUnit == currentGeneralUnit)
			{
				currentGeneralUnit = null;
				selectedGeneral.Hide();
			}
			if(inUnit.canTransport)
			{
				List<UnitController> temp = null;
				supportUnits.TryGetValue(inUnit.unitClass, out temp);
				if(temp != null){
					temp.Remove(inUnit);
				}
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
	public override void StartTurn()
	{
		base.StartTurn();
		UpdateTargets();
		ComputeAllUnitPositions();
	}
	Property GetNextClosestUncapturedProperty(UnitController unit)
	{
		List<Property> enemyProperties = InGameController.GetAllEnemyProperties(this);
		float closestDistance = float.PositiveInfinity;
		Property closestProperty = null;
		float currentPropDistance = 0;
		foreach(Property prop in enemyProperties)
		{
			if(prop.propertyClass.capturable)
			{
				if(unit.currentBlock.CanReachBlock(unit, prop.GetOccupyingBlock()))
				{
					currentPropDistance = InGameController.currentTerrain.MinDistanceToTile(unit.currentBlock, prop.GetOccupyingBlock(), unit);
					if(currentPropDistance == float.PositiveInfinity)
					{
						Debug.Log("Positive infinity returned, probably error with canreachblock functions");
					}
				}
				else
				{
					currentPropDistance = InGameController.currentTerrain.MinDistanceToTileThroughUnMovableBlocks(unit.currentBlock, prop.GetOccupyingBlock(), unit, 5);
				}
				float enemyHQDistance = InGameController.ClosestEnemyHQ(prop.GetOccupyingBlock(), unit.moveClass, this);
				float playerHQDistance = InGameController.currentTerrain.MinDistanceToTileThroughUnMovableBlocks(unit.currentBlock, hQBlock, unit, 5);
				enemyHQDistance = enemyHQDistance > 0 ? enemyHQDistance : 1;
				playerHQDistance = playerHQDistance > 0 ? playerHQDistance : 1;
				currentPropDistance *= (playerHQDistance / enemyHQDistance);
				currentPropDistance /= prop.AICapturePriority;
				if(!targetedObjects.Contains(prop) && currentPropDistance < closestDistance && prop.propertyClass.capturable && !(prop.GetOccupyingBlock().IsOccupied() && (prop.GetOccupyingBlock().occupyingUnit.GetOwner().IsSameSide(unit.GetOwner()) && prop.occupyingUnit != unit)))
				{
					closestProperty = prop;
					closestDistance = currentPropDistance;
				}
			}
		}
		return closestProperty;
	}
	UnitController GetClosestResupplyUnit(UnitController unit)
	{
		float closestDistance = float.PositiveInfinity;
		UnitController closestUnit = null;
		float currentUnitDistance = 0;
		foreach(UnitController u in units)
		{
			currentUnitDistance = TerrainBuilder.ManhattanDistance(u.currentBlock, unit.currentBlock);
			if(currentUnitDistance < closestDistance && u.canSupply)
			{
				closestDistance = currentUnitDistance;
				closestUnit = u;
			}
		}
		return closestUnit;
	}
	void UpdateTargets()
	{
		targetedObjects.Clear();
		List<UnitController> unitsNeedingSupply = new List<UnitController>(units);
		unitsNeedingSupply.Sort(UnitController.CompareBySupplyScore);
		List<UnitController> targetedUnitsNeedingSupply = new List<UnitController>();
		List<UnitController> assignedUnits = new List<UnitController>();
		//list of clusters of clustered enemy units
		List<List<AttackableObject>> lists = clusterer.Estimate(InGameController.GetAllEnemyUnits(this));
		//assign infantry to buildings if possible - easy first step
		for(int i = 0; i < units.Count; i++)
		{
			//although able to capture, snipers are better used in combat
			if(units[i].canCapture && units[i].unitClass != UnitNames.Sniper)
			{
				units[i].AITarget = GetNextClosestUncapturedProperty(units[i]);
				if(units[i].AITarget != null)
				{
					assignedUnits.Add(units[i]);
					targetedObjects.Add(units[i].AITarget);
				}
			}
		}
		//assign all other units to clusters of enemy units, or our own units if they're support units
		int[] unitsAssignedToCluster = new int[lists.Count];
		for(int i = 0; i < units.Count; i++)
		{
			if(!assignedUnits.Contains(units[i]))
			{
				if(units[i].NeedsResupply())
				{
					units[i].AITarget = GetClosestResupplyUnit(units[i]);
					if(units[i].AITarget == null)
					{
						if(units[i].moveClass == MovementType.Littoral || units[i].moveClass == MovementType.Sea)
						{
							makeSupplySea = true;
						}
						else
						{
							makeSupplyLand = true;
						}
						units[i].AITarget = hQBlock.occupyingProperty;
					}
				}
				//combat units
				else if(units[i].minAttackRange > 0)
				{
					//find best cluster for this unit
					float bestClusterAptitude = float.NegativeInfinity;
					int bestCluster = 0;
					for(int k = 0; k < lists.Count; k++)
					{
						float currentAptitude = ClusterAptitude(lists[k], units[i]);
						if(currentAptitude > bestClusterAptitude && unitsAssignedToCluster[k] < lists[k].Count * clusterAssignmentOverflow)
						{
							bestCluster = k;
							bestClusterAptitude = currentAptitude;
						}
					}
					//find best unit in that cluster
					units[i].AITarget = BestUnitInCluster(lists[bestCluster], units[i]);
					unitsAssignedToCluster[bestCluster]++;
				}
				//supplying units
				else if(units[i].canSupply)
				{
					UnitController candidate = null;
					float closestDistance = float.PositiveInfinity;
					float currentDistance = 0;
					foreach(UnitController uc in unitsNeedingSupply)
					{
						if(!targetedUnitsNeedingSupply.Contains(candidate))
						{
							currentDistance = (units[i].transform.position - uc.transform.position).magnitude;
							if(currentDistance < closestDistance)
							{
								candidate = uc;
								closestDistance = currentDistance;
								if(UnityEngine.Random.value < .75f)
								{
									break;
								}
							}
						}
					}
					targetedUnitsNeedingSupply.Add(candidate);
					units[i].AITarget = candidate;
				}
				assignedUnits.Add(units[i]);
			}
		}
		for(int i = 0; i < units.Count; i++)
		{
			SetTargetBlock(units[i]);
		}
	}
	void ComputeAllUnitPositions(){
		List<UnitController> temp = new List<UnitController>(unitsToMove);
		temp.Sort(UnitController.CompareByMovePriority);
		unitsToMove = new Stack<UnitController>(temp);
		
	}
	void SetTargetBlock(UnitController inUnit)
	{
		if(inUnit.AITarget != null)
		{
			bool canReach = false;
			TerrainBlock reachableBlock = null;
			//Debug.Log(inUnit.AITarget.GetUnitClass());
			//finds a block that the inUnit can reach and attack, or attempts to obtain a taxiing unit to get to the target
			//it sets the inUnit targetblock to either the first, or the occupying block of the target
			if(inUnit.AITarget is UnitController)
			{
				foreach(TerrainBlock tb in InGameController.currentTerrain.BlocksWithinRange(inUnit.AITarget.GetOccupyingBlock(), inUnit.minAttackRange, inUnit.EffectiveAttackRange(), inUnit))
				{
					if(tb.CanReachBlock(inUnit, inUnit.currentBlock))
					{
						canReach = true;
						reachableBlock = tb;
						break;
					}
				}
			}
			else
			{
				if(inUnit.AITarget.GetOccupyingBlock().CanReachBlock(inUnit, inUnit.currentBlock))
				{
					canReach = true;
					reachableBlock = inUnit.AITarget.GetOccupyingBlock();
				}
			}
			inUnit.canReachTarget = canReach;
			if(canReach)
			{
				inUnit.AITargetBlock = reachableBlock;
			}
			else if(inUnit.moveClass == MovementType.Sea || inUnit.moveClass == MovementType.Littoral)
			{
				inUnit.AITargetBlock = inUnit.AITarget.GetOccupyingBlock();
			}
			else
			{
				UnitController target = GetSupportUnit(inUnit);
				if(target == null)
				{
					SetTransportToMake(inUnit);
				}
				else
				{
					target.AITarget = inUnit;
					target.AITargetBlock = inUnit.currentBlock;
				}
				inUnit.AITargetBlock = inUnit.AITarget.GetOccupyingBlock();
			}
		}
		else
		{
			Debug.Log("No Target");
			InGameController.ClosestEnemyHQ(inUnit.currentBlock, inUnit.moveClass, this, out inUnit.AITargetBlock);
			if(inUnit.AITargetBlock.CanReachBlock(inUnit, inUnit.currentBlock))
			{
				inUnit.canReachTarget = true;
			}
			else
			{
				inUnit.canReachTarget = false;
			}
		}
		/*if(inUnit.AITarget != null){
			Debug.Log(inUnit.name + " " + inUnit.AITarget + " " + inUnit.AITarget.GetPosition());
			Debug.Log(inUnit.AITargetBlock.transform.position);
		}
		else{
			Debug.Log("Has no target: " + inUnit.name);
			Debug.Log(inUnit.AITargetBlock.transform.position);
		}*/
	}
	
	/// <summary>
	/// Aptitude of a cluster, determined by the sum of the damage inUnit can do to each unit in the cluster minus what each unit can do to inUnit
	/// </summary>
	/// <returns>The aptitude.</returns>
	/// <param name="enemyUnits">Enemy units.</param>
	/// <param name="inUnit">In unit.</param>
	float ClusterAptitude(List<AttackableObject> enemyUnits, UnitController inUnit)
	{
		float sum = 0;
		Vector3 median = Vector3.zero;
		for(int i = 0; i < enemyUnits.Count; i++)
		{
			sum += DamageValues.CalculateDamage(inUnit,enemyUnits[i]) * enemyUnits[i].UnitCost() - .33f * DamageValues.CalculateDamage(enemyUnits[i], inUnit) * inUnit.UnitCost();
			median += enemyUnits[i].GetPosition();
		}
		median /= (enemyUnits.Count > 0) ? enemyUnits.Count : 1;
		float distance = (inUnit.transform.position - median).magnitude;
		return sum/(distance > 1 ? distance : 1);
	}
	
	AttackableObject BestUnitInCluster(List<AttackableObject> enemyUnits, UnitController inUnit)
	{
		float best = 0;
		AttackableObject bestUnit = null;
		for(int i = 0; i < enemyUnits.Count; i++)
		{
			float current = DamageValues.CalculateDamage(inUnit,enemyUnits[i]) * enemyUnits[i].UnitCost();
			if(current > best)
			{
				best = current;
				bestUnit = enemyUnits[i];
			}
		}
		return bestUnit;
	}
	void Update()
	{
		if(canIssueOrders && !InGameController.endingGame)
		{
			MediumAIUpdate();
		}
	}
	
	protected override TerrainBlock StateSearch(int numSearchTurns, int statesKept)
	{
		TerrainBlock bestBlockSoFar = null;
		PositionEvaluation bestValueSoFar = new PositionEvaluation(float.NegativeInfinity);
		if(currentUnit.canReachTarget && currentUnit.AITarget != null)
		{
			InGameController.currentTerrain.SetDistancesFromBlock(currentUnit, currentUnit.AITargetBlock);
		}
		else if(currentUnit.AITarget != null)
		{
			Debug.Log("cant reach target, has one");
			if(currentUnit.AITarget is UnitController)
			{
				InGameController.currentTerrain.SetDistancesFromBlockSharedMoveableBlock(currentUnit, currentUnit.AITargetBlock, currentUnit.AITarget);
			}
			else
			{
				InGameController.currentTerrain.SetDistancesFromBlockIgnoreIllegalBlocks(currentUnit, currentUnit.AITargetBlock);
			}
		}
		else
		{
			Debug.Log("Has no target");
			InGameController.currentTerrain.SetDistancesFromBlockIgnoreIllegalBlocks(currentUnit, currentUnit.AITargetBlock);
		}
		int totalPositionsEvaluated = 0;
		List<TerrainBlock> blocks = InGameController.currentTerrain.MoveableBlocks(currentUnit.currentBlock, currentUnit, currentUnit.EffectiveMoveRange());
		foreach(TerrainBlock block in blocks){
			PositionEvaluation temp = RecursiveEvaluatePosition(block);
			if(temp.value > bestValueSoFar.value){
				bestValueSoFar = temp;
				bestBlockSoFar = block;
			}
		}
		//Debug.Log(currentUnit.unitClass + " Positions evaluated: " + totalPositionsEvaluated);// + " / positions at depth 0: " + blocks.Count);
		return bestBlockSoFar;
	}
	
	protected PositionEvaluation RecursiveEvaluatePosition(TerrainBlock block)
	{
		PositionEvaluation bestValueSoFar = new PositionEvaluation(float.NegativeInfinity);
		List<TerrainBlock> blocks = InGameController.currentTerrain.MoveableBlocks(block, currentUnit, currentUnit.EffectiveMoveRange());
		for(int i = 0; i < blocks.Count; i++)
		{
			PositionEvaluation temp = EvaluatePosition(blocks[i]);
			if(temp.value > bestValueSoFar.value)
			{
				bestValueSoFar = temp;
			}
		}
		bestValueSoFar.value *= .33f;
		PositionEvaluation outEvaluation = EvaluatePosition(block);
		outEvaluation.value += bestValueSoFar.value;
		return outEvaluation;
	}
	
	/*protected PositionEvaluation RecursiveEvaluatePosition(int depth, TerrainBlock block, ref int positionsEvaluated, int maxDepth)
	{
		positionsEvaluated++;
		if(depth < maxDepth)
		{
			PositionEvaluation bestValueSoFar = new PositionEvaluation(float.NegativeInfinity);
			List<TerrainBlock> blocks = InGameController.currentTerrain.MoveableBlocks(block, currentUnit, currentUnit.EffectiveMoveRange());
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
	
	public class PositionEvaluationComparer : IComparer<PositionEvaluation>{
		public int Compare(PositionEvaluation a, PositionEvaluation b)
		{
			if(a.value < b.value)
			{
				return 1;
			}
			else
			{
				return -1;
			}
		}
	}
	
	public class PositionEvaluation{
		public UnitOrderOptions bestOrder;
		public float value;
		public TerrainBlock block;
		public PositionEvaluation(float v){
			value = v;
			bestOrder = UnitOrderOptions.EndTurn;
			block = null;
		}
	}
	
	protected PositionEvaluation EvaluatePosition(TerrainBlock position)
	{
		PositionEvaluation bestOptionValue = new PositionEvaluation(0);
		if(position.IsOccupied() && position.occupyingUnit != currentUnit && (!position.occupyingUnit.CanCarryUnit(currentUnit) || !position.occupyingUnit.GetOwner().IsSameSide(currentUnit.GetOwner())))
		{
			bestOptionValue.value = -100000;
			return bestOptionValue;
		}
		else
		{
			bestOptionValue.value = waitModifier;
			bestOptionValue.bestOrder = UnitOrderOptions.EndTurn;
			List<UnitOrderOptions> possibleOrders = currentUnit.CalculateAwaitingOrderOptions(position);
			foreach(UnitOrderOptions currentOrder in possibleOrders)
			{
				switch(currentOrder)
				{
				case UnitOrderOptions.AddGeneral:
				{
					if(UtilityOfAddingGeneral() * addGeneralModifier > bestOptionValue.value)
					{
						bestOptionValue.value = UtilityOfAddingGeneral() * addGeneralModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.AddGeneral;
					}
					break;
				}
				case UnitOrderOptions.Attack:
				{
					if(currentUnit.canMoveAndAttack || currentUnit.currentBlock == position)
					{
						AttackableObject bestTarget = null;
						float bestTargetedUnitDamage = currentUnit.AISelectBestAttackObject(position, out bestTarget);
						if(bestTargetedUnitDamage * attackModifier > bestOptionValue.value)
						{
							bestOptionValue.value = bestTargetedUnitDamage * attackModifier;
							bestOptionValue.bestOrder = UnitOrderOptions.Attack;
							bestOptionValue.value += position.defenseBoost * defensiveTerrainDesireModifier;
						}
					}
					break;
				}
				case UnitOrderOptions.Board:
				{
					if(boardModifier > bestOptionValue.value)
					{
						bestOptionValue.value = boardModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.Board;
					}
					break;
				}
				case UnitOrderOptions.BuildBridge:
				{
					if(buildBridgeModifier > bestOptionValue.value)
					{
						bestOptionValue.value = buildBridgeModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.BuildBridge;
					}
					break;
				}
				case UnitOrderOptions.Capture:
				{
					float bestCapture = captureModifier * (((float)currentUnit.health.GetRawHealth())/100) + (1 - position.occupyingProperty.NormalizedCaptureCount()) * position.occupyingProperty.AICapturePriority;
					if(bestCapture > bestOptionValue.value)
					{
						bestOptionValue.value = bestCapture;
						bestOptionValue.bestOrder = UnitOrderOptions.Capture;
					}
					break;
				}
				case UnitOrderOptions.Load:
				{
					float value = 1 - (currentUnit.EffectiveMoveRange()/5);
					if(position.occupyingUnit.healsCarriedUnits)
					{
						if(currentUnit.health.GetRawHealth() < 50)
						{
							value = (1 - ((float)currentUnit.health.GetRawHealth())/100f);
						}
					}
					if(value * loadModifier > bestOptionValue.value)
					{
						bestOptionValue.value = value * loadModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.Load;
					}
					break;
				}
				case UnitOrderOptions.ProduceMissile:
				{
					if(produceMissileModifier > bestOptionValue.value)
					{
						bestOptionValue.value = produceMissileModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.ProduceMissile;
					}
					break;
				}
				case UnitOrderOptions.Repair:
				{
					if(repairModifier > bestOptionValue.value)
					{
						bestOptionValue.value = repairModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.Repair;
					}
					break;
				}
				case UnitOrderOptions.Stealthify:
				{
					float value = currentUnit.GetNormalizedFuel() * stealthifyModifier;
					if(value > bestOptionValue.value)
					{
						bestOptionValue.value = value;
						bestOptionValue.bestOrder = UnitOrderOptions.Stealthify;
					}
					break;
				}
				case UnitOrderOptions.Supply:
				{
					float supplyValue = 0;
					for(int i = 0; i < position.adjacentBlocks.Length; i++)
					{
						if(position.adjacentBlocks[i].IsOccupied() && position.adjacentBlocks[i].occupyingUnit.GetOwner().IsSameSide(currentUnit.GetOwner()) && position.adjacentBlocks[i].occupyingUnit != currentUnit)
						{
							supplyValue += (1 - position.adjacentBlocks[i].occupyingUnit.GetNormalizedFuel()) + (1 - position.adjacentBlocks[i].occupyingUnit.GetNormalizedAmmo());
						}
					}
					if(supplyValue * supplyModifier > bestOptionValue.value)
					{
						bestOptionValue.value = supplyValue * supplyModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.Supply;
					}
					break;
				}
				case UnitOrderOptions.Unload:
				{
					if(unloadModifier > bestOptionValue.value)
					{
						bestOptionValue.value = unloadModifier;
						bestOptionValue.bestOrder = UnitOrderOptions.Unload;
					}
					break;
				}
				case UnitOrderOptions.UnStealthify:
				{
					float value = (1 - currentUnit.GetNormalizedFuel()) * unStealthifyModifier;
					if(value > bestOptionValue.value)
					{
						bestOptionValue.value = value;
						bestOptionValue.bestOrder = UnitOrderOptions.UnStealthify;
					}
					break;
				}
				case UnitOrderOptions.Join:
				{
					float value = 20 - position.occupyingUnit.health.PrettyHealth() - currentUnit.health.PrettyHealth();
					value *= joinModifier;
					if(value > bestOptionValue.value){
						bestOptionValue.value = value;
						bestOptionValue.bestOrder = UnitOrderOptions.Join;
					}
					break;
				}
				}
			}
			if(position.HasProperty())
			{
				if(position.occupyingProperty.CanHealUnit(currentUnit) && position.occupyingProperty.GetOwner().IsSameSide(currentUnit.GetOwner()))
				{
					if(currentUnit.health < 80)
					{
						float value = (1 - ((float)currentUnit.health.GetRawHealth())/100f);
						if(position.occupyingProperty.CanProduceUnit(currentUnit.unitClass))
						{
							value *= .3f;
						}
						if(value * healModifier > bestOptionValue.value)
						{
							bestOptionValue.value = value * healModifier;
						}
					}
				}
				bestOptionValue.value += position.occupyingProperty.DefenseBonus() * defensiveTerrainDesireModifier;
			}
			bestOptionValue.value += position.defenseBoost * defensiveTerrainDesireModifier;
			//float danger = EnemyDangerAtBlock(position);
			//bestOptionValue.value -= danger;
			bestOptionValue.value += MoveTowardsTarget(position);
			bestOptionValue.value += UnityEngine.Random.Range(0, randomnessModifier);
			return bestOptionValue;
		}
	}
	
	public float MoveTowardsTarget(TerrainBlock block)
	{
		return (1000 - block.cachedGCost) * hQMoveTowardsModifier;
	}
	float EnemyDangerAtBlock(TerrainBlock block)
	{
		List<AttackableObject> otherUnits = InGameController.currentTerrain.ObjectsWithinRange(block, 1, 12, currentUnit);
		float alliedUnits = 1;
		float totalEnemyDamage = 0;
		UnitController other = null;
		InGameController.currentTerrain.SaveIlluminatedBlocks();
		foreach(AttackableObject temp in otherUnits)
		{
			if(temp is UnitController)
			{
				other = (UnitController)temp;
				if(other.GetOwner().IsSameSide(currentUnit.GetOwner()) && other.EffectiveMoveRange() >= TerrainBuilder.ManhattanDistance(other.currentBlock, block))
				{
					alliedUnits++;
				}
				else if(!other.GetOwner().IsSameSide(currentUnit.GetOwner()) && !other.GetOwner().IsNeutralSide() && other.EffectiveMoveRange() + other.EffectiveAttackRange() >= TerrainBuilder.ManhattanDistance(other.currentBlock, block) && other.gameObject.activeSelf)
				{
					float distance = 0;
					if(other.canMoveAndAttack && other.minAttackRange > 0)
					{
						List<TerrainBlock> otherBlocks = InGameController.currentTerrain.MoveableBlocks(other.currentBlock, other, other.EffectiveMoveRange());
						float otherMaxAttackRange = other.EffectiveAttackRange();
						foreach(TerrainBlock t in otherBlocks)
						{
							if(TerrainBuilder.ManhattanDistance(t, block) <= otherMaxAttackRange)
							{
								totalEnemyDamage += DamageValues.CalculateDamage(other, currentUnit) * currentUnit.baseCost;
								break;
							}
						}
					}
					else if(TerrainBuilder.ManhattanDistance(other.currentBlock, block) >= other.minAttackRange && TerrainBuilder.ManhattanDistance(other.currentBlock, block) <= other.EffectiveAttackRange())
					{
						totalEnemyDamage += DamageValues.CalculateDamage(other, currentUnit) * currentUnit.baseCost;
					}
				}
			}
		}
		InGameController.currentTerrain.LoadIlluminatedBlocks();
		return (totalEnemyDamage * currentUnit.AIDefensiveness) / (alliedUnits * 15000f);
	}
	
	protected override void ProduceUnits()
	{
		unitsToMake.Clear();
		MediumUnitProductionRandom();
	}
	
	/*void MediumUnitProductionReinforcement()
	{
		if(producingUnits)
		{
			List<UnitNames> rankedNames = GetComponent<MouseEventHandler>().CheckTestInstanceClassificationReinforcement();
			string unitsSelected = "";
			if(rankedNames != null)
			{
				for(int i = 0; i < rankedNames.Count; i++)
				{
					unitsSelected += rankedNames[i].ToString() + ", ";
				}
				//Debug.Log(unitsSelected);
				for(int j = 0; j < 3; j++)
				{
					UnitNames rand = rankedNames[UnityEngine.Random.Range(0, rankedNames.Count)];
					//Debug.Log(rand);
					if(makeSupplyLand)
					{
						rand = UnitNames.SupplyTank;
						makeSupplyLand = false;
					}
					else if(makeSupplySea)
					{
						rand = UnitNames.SupplyShip;
						makeSupplySea = false;
					}
					else if(makeTransport)
					{
						rand = transportToMake;
						makeTransport = false;
					}
					if(((UnitController)Utilities.GetPrefabFromUnitName(rand)).baseCost <= funds)
					{
						for(int i = 0; i < properties.Count; i++)
						{
							if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rand))
							{
								properties[i].AIProduceUnit(rand);
								break;
							}
						}
						break;
					}
				}
				productionAttempts++;
				producingUnits = false;
			}
		}
		else
		{
			if(productionAttempts < producingProperties)
			{
				GetComponent<MouseEventHandler>().StartTestInstanceReinforcement();
				producingUnits = true;
			}
		}
	}*/
	void MediumUnitProductionRandom()
	{
		if(producingUnits)
		{
			switch(productionType)
			{
			case ProductionTest.RandomProd:
			{
				UnitNames rand = RandomUnitName();
				if(((UnitController)Utilities.GetPrefabFromUnitName(rand)).baseCost <= funds)
				{
					for(int i = 0; i < properties.Count; i++)
					{
						if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rand))
						{
							properties[i].AIProduceUnit(rand);
							break;
						}
					}
				}
				productionAttempts++;
				producingUnits = false;
				break;
			}
			case ProductionTest.HalfNHalf:
			{
				if(UnityEngine.Random.value < .5f)
				{
					UnitNames rand = RandomUnitName();
					if(((UnitController)Utilities.GetPrefabFromUnitName(rand)).baseCost <= funds)
					{
						for(int i = 0; i < properties.Count; i++)
						{
							if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rand))
							{
								properties[i].AIProduceUnit(rand);
								break;
							}
						}
					}
					productionAttempts++;
					producingUnits = false;
				}
				else
				{
					List<UnitNames> rankedNames = GetComponent<MouseEventHandler>().CheckTestInstanceClassificationRanked();
					if(rankedNames != null)
					{
						UnitNames rand = rankedNames[UnityEngine.Random.Range(0, rankedNames.Count-1)];
						if(((UnitController)Utilities.GetPrefabFromUnitName(rand)).baseCost <= funds)
						{
							for(int i = 0; i < properties.Count; i++)
							{
								if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rand))
								{
									properties[i].AIProduceUnit(rand);
									break;
								}
							}
						}
						productionAttempts++;
						producingUnits = false;
					}
				}
				break;
			}
			case ProductionTest.Learned:
			{
				UnitNames rankedName = productionEngine.Evaluate(this);
				string unitsSelected = "";
				if(rankedName != null)
				{
					if(makeSupplyLand)
					{
						rankedName = UnitNames.SupplyTank;
						makeSupplyLand = false;
					}
					else if(makeSupplySea)
					{
						rankedName = UnitNames.SupplyShip;
						makeSupplySea = false;
					}
					else if(makeTransport)
					{
						rankedName = transportToMake;
						makeTransport = false;
					}
					if(((UnitController)Utilities.GetPrefabFromUnitName(rankedName)).baseCost <= funds)
					{
						SortPropertiesByHQDistance(((UnitController)Utilities.GetPrefabFromUnitName(rankedName)).moveClass);
						for(int i = 0; i < properties.Count; i++)
						{
							if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rankedName))
							{
								properties[i].AIProduceUnit(rankedName);
								break;
							}
						}
						break;
					}
					productionAttempts++;
					producingUnits = false;
				}
				break;
			}
			}
		}
		else
		{
			if(productionAttempts < producingProperties)
			{
				producingUnits = true;
			}
		}
	}
	void SortPropertiesByHQDistance(MovementType moveType)
	{
		for(int i = 0; i < properties.Count; i++)
		{
			properties[i].cachedDistanceFromEnemyHQ = InGameController.ClosestEnemyHQ(properties[i].GetOccupyingBlock(), moveType, this);
		}
		properties.Sort(Property.CompareByDistanceFromEnemyHQ);
	}
	/*void MediumUnitProduction()
	{
		if(producingUnits)
		{
			/*UnitNames rand = GetComponent<MouseEventHandler>().CheckTestInstanceClassification();
			if(rand != UnitNames.Headquarters)
			{
				if(((UnitController)Utilities.GetPrefabFromUnitName(rand)).baseCost <= funds)
				{
					for(int i = 0; i < properties.Count; i++)
					{
						if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rand))
						{
							properties[i].AIProduceUnit(rand);
							break;
						}
					}
				}
				productionAttempts++;
				producingUnits = false;
			}*/
			/*List<UnitNames> rankedNames = GetComponent<MouseEventHandler>().CheckTestInstanceClassificationRanked();
			if(rankedNames != null)
			{
				UnitNames rand = rankedNames[UnityEngine.Random.Range(0, rankedNames.Count-1)];
				if(((UnitController)Utilities.GetPrefabFromUnitName(rand)).baseCost <= funds)
				{
					for(int i = 0; i < properties.Count; i++)
					{
						if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(rand))
						{
							properties[i].AIProduceUnit(rand);
							break;
						}
					}
				}
				productionAttempts++;
				producingUnits = false;
			}
		}
		else
		{
			if(productionAttempts < producingProperties)
			{
				GetComponent<MouseEventHandler>().StartTestInstance(InGameController.CreateInstance(UnitNames.Infantry,  false));
				producingUnits = true;
			}
		}
	}*/
	void MediumAIUpdate ()
	{
		if(currentUnit != null)
		{
			if(currentUnit.currentState == UnitState.UnMoved)
			{
				currentUnit.AISelect();
			}
			else if(currentUnit.currentState == UnitState.Selected)
			{
				TerrainBlock block = StateSearch(1, 3);
				currentUnit.AIMoveTo(block);
			}
			else if(currentUnit.currentState == UnitState.Moving)
			{
				
			}
			else if(currentUnit.currentState == UnitState.FinishedMove)
			{
				currentUnit = null;
			}
			else if(currentUnit.currentState == UnitState.AwaitingOrder)
			{
				PositionEvaluation p = EvaluatePosition(currentUnit.awaitingOrdersBlock);
				currentUnit.AIDoOrder(p, true);
			}
		}
		else
		{
			if(unitsToMove.Count > 0)
			{
				do
				{
					currentUnit = unitsToMove.Pop();
				}
				while(currentUnit.isInUnit && unitsToMove.Count > 0);
				if(unitsToMove.Count == 0 && currentUnit.isInUnit)
				{
					currentUnit = null;
				}
			}
			else
			{
				ProduceUnits();
				if(productionAttempts >= producingProperties)
				{
					InGameController.instance.AdvanceTurn();
				}
			}
		}
	}
}
