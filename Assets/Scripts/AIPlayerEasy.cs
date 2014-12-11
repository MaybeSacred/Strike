using System;
using System.Collections.Generic;
using UnityEngine;
public class AIPlayerEasy : AIPlayer
{
	public UnitNameIntBinder[] easyUnitQuotas;
	protected override void Awake()
	{
		base.Awake();
		countsOfEachUnit = new int[System.Enum.GetNames(typeof(UnitNames)).Length];
	}
	void Update()
	{
		if(InGameController.GetPlayer(InGameController.currentPlayer) == this && !InGameController.endingGame)
		{
			EasyAIUpdate();
		}
	}
	void EasyAIUpdate()
	{
		if(currentUnit != null)
		{
			if(currentUnit.currentState == UnitState.Moving)
			{
				
			}
			else if(currentUnit.currentState == UnitState.FinishedMove)
			{
				currentUnit = null;
			}
			else if(currentUnit.currentState == UnitState.AwaitingOrder)
			{
				UnitOrderOptions order;
				EvaluatePosition(currentUnit.awaitingOrdersBlock, out order);
				//currentUnit.AIDoOrder(order);
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
				else
				{
					TerrainBlock block = StateSearch(1, 3);
					currentUnit.AIMoveTo(block);
				}
			}
			else
			{
				ProduceUnits();
				InGameController.AdvanceTurn();
			}
		}
	}
	protected override TerrainBlock StateSearch(int numSearchTurns, int statesKept)
	{
		TerrainBlock bestBlockSoFar = null;
		float bestValueSoFar = 0;
		UnitOrderOptions nullOrder = UnitOrderOptions.EndTurn;
		for(int turn = 0; turn < numSearchTurns; turn++)
		{
			List<TerrainBlock> blocks = InGameController.currentTerrain.MoveableBlocks(currentUnit.currentBlock, currentUnit, currentUnit.modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.MovementRange, currentUnit.movementRange));
			for(int i = 0; i < blocks.Count; i++)
			{
				float temp = EvaluatePosition(blocks[i], out nullOrder);
				if(temp > bestValueSoFar)
				{
					bestValueSoFar = temp;
					bestBlockSoFar = blocks[i];
				}
			}
		}
		return bestBlockSoFar;
	}

	protected override float EvaluatePosition(TerrainBlock position, out UnitOrderOptions order)
	{
		if(position.IsOccupied() && position.occupyingUnit != currentUnit && (!position.occupyingUnit.CanCarryUnit(currentUnit) || !position.occupyingUnit.GetOwner().IsSameSide(currentUnit.GetOwner())))
		{
			order = UnitOrderOptions.EndTurn;
			return -1;
		}
		else
		{
			float bestOptionValue = waitModifier;
			order = UnitOrderOptions.EndTurn;
			List<UnitOrderOptions> possibleOrders = currentUnit.CalculateAwaitingOrderOptions(position);
			foreach(UnitOrderOptions currentOrder in possibleOrders)
			{
				switch(currentOrder)
				{
					case UnitOrderOptions.AddGeneral:
					{
						if(UtilityOfAddingGeneral() * addGeneralModifier > bestOptionValue)
						{
							bestOptionValue = UtilityOfAddingGeneral() * addGeneralModifier;
							order = UnitOrderOptions.AddGeneral;
						}
						break;
					}
					case UnitOrderOptions.Attack:
					{
						if(currentUnit.canMoveAndAttack || currentUnit.currentBlock == position)
						{
							AttackableObject bestTarget = null;
							float bestTargetedUnitDamage = currentUnit.AISelectBestAttackObject(position, out bestTarget);
							if(bestTargetedUnitDamage * attackModifier > bestOptionValue)
							{
								bestOptionValue = bestTargetedUnitDamage * attackModifier;
								order = UnitOrderOptions.Attack;
								bestOptionValue += position.defenseBoost * defensiveTerrainDesireModifier;
							}
						}
						break;
					}
					case UnitOrderOptions.Board:
					{
						if(boardModifier > bestOptionValue)
						{
							bestOptionValue = boardModifier;
							order = UnitOrderOptions.Board;
						}
						break;
					}
					case UnitOrderOptions.BuildBridge:
					{
						if(buildBridgeModifier > bestOptionValue)
						{
							bestOptionValue = buildBridgeModifier;
							order = UnitOrderOptions.BuildBridge;
						}
						break;
					}
					case UnitOrderOptions.Capture:
					{
						float bestCapture = captureModifier * (((float)currentUnit.health)/100) + (1 - position.occupyingProperty.NormalizedCaptureCount());
						if(bestCapture > bestOptionValue)
						{
							bestOptionValue = bestCapture;
							order = UnitOrderOptions.Capture;
						}
						break;
					}
					case UnitOrderOptions.Load:
					{
						float value = 1 - (((float)currentUnit.modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.MovementRange, currentUnit.movementRange))/5);
						if(value * loadModifier > bestOptionValue)
						{
							bestOptionValue = value * loadModifier;
							order = UnitOrderOptions.Load;
						}
						break;
					}
					case UnitOrderOptions.ProduceMissile:
					{
						if(produceMissileModifier > bestOptionValue)
						{
							bestOptionValue = produceMissileModifier;
							order = UnitOrderOptions.ProduceMissile;
						}
						break;
					}
					case UnitOrderOptions.Repair:
					{
						if(repairModifier > bestOptionValue)
						{
							bestOptionValue = repairModifier;
							order = UnitOrderOptions.Repair;
						}
						break;
					}
					case UnitOrderOptions.Stealthify:
					{
						float value = currentUnit.GetNormalizedFuel() * stealthifyModifier;
						if(value > bestOptionValue)
						{
							bestOptionValue = value;
							order = UnitOrderOptions.Stealthify;
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
						if(supplyValue * supplyModifier > bestOptionValue)
						{
							bestOptionValue = supplyValue * supplyModifier;
							order = UnitOrderOptions.Supply;
						}
						break;
					}
					case UnitOrderOptions.Unload:
					{
						if(unloadModifier > bestOptionValue)
						{
							bestOptionValue = unloadModifier;
							order = UnitOrderOptions.Unload;
						}
						break;
					}
					case UnitOrderOptions.UnStealthify:
					{
						float value = (1 - currentUnit.GetNormalizedFuel()) * unStealthifyModifier;
						if(value > bestOptionValue)
						{
							bestOptionValue = value;
							order = UnitOrderOptions.UnStealthify;
						}
						break;
					}
				}
			}
			if(position.HasProperty())
			{
				if(position.occupyingProperty.CanHealUnit(currentUnit) && position.occupyingProperty.GetOwner().IsSameSide(currentUnit.GetOwner()))
				{
					float value = (1 - (float)currentUnit.health/100f);
					if(position.occupyingProperty.CanProduceUnit(currentUnit.unitClass))
					{
						value *= .8f;
					}
					if(value * healModifier > bestOptionValue)
					{
						bestOptionValue = value * healModifier;
						order = UnitOrderOptions.EndTurn;
					}
					else
					{
						bestOptionValue += value * healModifier;
					}
					//Debug.Log("Heal " + value + " " + bestOptionValue);
				}
				bestOptionValue += position.occupyingProperty.DefenseBonus() * defensiveTerrainDesireModifier;
			}
			bestOptionValue += MoveTowardsEnemyHQ(position);
			bestOptionValue += UnityEngine.Random.Range(0, randomnessModifier);
			return bestOptionValue;
		}
	}
	protected override float UtilityOfAddingGeneral()
	{
		return ((float)currentUnit.baseCost)/30000f;
	}
	protected override void ProduceUnits()
	{
		unitsToMake.Clear();
		EasyUnitProduction();
	}
	void EasyUnitProduction()
	{
		if(EasyProductionQuotaFilled())
		{	
			for(int i = 0; i < 10; i++)
			{
				UnitNames rand = RandomUnitName();
				if(HasBuildingToProduceUnit(rand))
				{
					unitsToMake.Enqueue(rand);
				}
			}
		}
		else
		{
			FillEasyQuota();
		}
		int count = unitsToMake.Count;
		for(; count > 0; count--)
		{
			if(((UnitController)Utilities.GetPrefabFromUnitName(unitsToMake.Peek())).baseCost <= funds)
			{
				for(int i = 0; i < properties.Count; i++)
				{
					if(properties[i].currentState == UnitState.UnMoved && !properties[i].GetOccupyingBlock().IsOccupied() && properties[i].CanProduceUnit(unitsToMake.Peek()))
					{
						properties[i].AIProduceUnit(unitsToMake.Dequeue());
						break;
					}
				}
			}
		}
	}
	void FillEasyQuota()
	{
		if(aiLevel == AILevel.Easy)
		{
			int enqueuedUnits = 0;
			for(int i = 0; i < countsOfEachUnit.Length; i++)
			{
				if(countsOfEachUnit[i] < easyUnitQuotas[i].value && HasBuildingToProduceUnit(easyUnitQuotas[i].name))
				{
					for(int k = countsOfEachUnit[i]; k < easyUnitQuotas[i].value /*&& enqueuedUnits < 5*/; k++)
					{
						unitsToMake.Enqueue(easyUnitQuotas[i].name);
						enqueuedUnits++;
					}
					if(enqueuedUnits >=5)
					{
						break;
					}
				}
			}
		}
	}
	bool EasyProductionQuotaFilled()
	{
		for(int i = 0; i < countsOfEachUnit.Length; i++)
		{
			if(countsOfEachUnit[i] < easyUnitQuotas[i].value && HasBuildingToProduceUnit(easyUnitQuotas[i].name))
			{
				return false;
			}
		}
		return true;
	}
}

