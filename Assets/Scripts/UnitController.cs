﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum UnitNames {Infantry, Stinger, Stryker, CarpetBomber, TacticalFighter, Interceptor, AttackCopter, LiftCopter, LightTank,
	 MediumTank, Rockets, Missiles, FieldArtillery, Mortar, SupplyTank, UAV, Humvee, Sniper, MobileRadar, Corvette, Destroyer,
	 Submarine, Carrier, Amphibious, SupplyShip, AATank, Boomer, Headquarters, City, Factory, Airport, Shipyard, ComTower, Bridge, Bunker};
public enum UnitState {UnMoved, BuildingBridge, Selected, Moving, AwaitingOrder, TargetingUnit, Unloading, FinishedMove, Dying};
public enum UnitOrderOptions {ProduceMissile, BuildBridge, Attack, Supply, Repair, Capture, Load, Unload, Board, AddGeneral, GeneralPower, BuildUnit, UnStealthify, Stealthify, EndTurn};
public enum MovementType {Air, Sea, Littoral, LightVehicle, HeavyVehicle, Tank, Amphibious, Sniper, Infantry};
//Story idea: in the middle of and right after nuclear war
public enum UnitRanks {Unranked, Private, Corporal, Sergeant, Elite};
public enum UnitAttackType {Direct, Indirect, Both};//Used for determining counterattack ability

public class UnitController : MonoBehaviour, AttackableObject, IComparable{
	public static float actionDisplayXOffset = -50, actionDisplayYOffset = 0, actionDisplayWidth = 100, actionDisplayHeight = 25,
						infoBoxXOffset = -80, infoBoxYOffset = 40, infoBoxWidth = 80, infoBoxHeight = 80;
	public static Texture2D healthPoint;
	public UnitNames unitClass;
	public TextMesh targetedDamageDisplay, targetedDamageOutline;
	public MovementType moveClass;
	public bool canMoveAndAttack;
	public UnitAttackType attackType;
	private Player owner;
	public int playerNumber; //Used for predeployed units
	public UnitState currentState{get; private set;}
	public int baseCost;
	public int movementRange;
	public Health health;
	public float startFuel;
	private float currentFuel;
	public int fogOfWarRange;
	public int primaryAmmo;
	private int primaryAmmoRemaining;
	private float infoBoxTimeoutCounter;
	private bool unitJustSelected;//Used to separate mouse clicks when unit is first selected
	public int maxAttackRange, minAttackRange;
	private bool didNotMoveThisTurn;
	private List<UnitOrderOptions> possibleOrders;
	private List<AttackableObject> possibleTargets;
	public bool canCapture, canSupply, canTransport, canStealth, canBuild;
	public bool isInUnit {get; private set;}
	public bool isStealthed {get; private set;}
	private List<UnitController> carriedUnits;
	public int transportCapacity;
	public int captureRate;
	private bool showingHealth, showingHitDamage;
	private bool wasTargeted;
	private bool hasUnitSelectedMutex;
	public GUIStyle infoBoxOptions;
	private bool showingNextTurnAttackRange, showingAttackRange;
	public TerrainBlock currentBlock {get; private set;}
	public TerrainBlock awaitingOrdersBlock {get; private set;}
	private List<TerrainBlock> currentMoveBlocks;
	private float currentPathDistance;
	private bool hitFogOfWarUnit;
	[HideInInspector]
	public Property occupiedProperty;
	public ParticleSystem destroyedParticles;
	private List<UnitController> partiallyUnloadedUnits;
	private UnitController currentlyUnloadingUnit;
	public bool healsCarriedUnits;
	private int movingCounter;
	private static int inverseMovingSpeed = 10;
	private UnitRanks veteranStatus = UnitRanks.Unranked;
	private int damageReceived;
	public string prettyName;
	public string description;
	private bool displayUnitInfo;
	public UnitPropertyModifier modifier;
	public ParticleSystem moveIndicatorParticles {get; private set;}
	public AttackableObject AITarget;
	public TerrainBlock AITargetBlock;
	public bool canReachTarget;
	public float AIDefensiveness;
	public int comTowerEffect;
	public ReinforcementInstance reinforcement;
	public TerrainBlock AICachedCurrentBlock;
	public int AIMovePriority;
	// Use this for initialization
	void Awake()
	{
		health = new Health();
		moveIndicatorParticles = (ParticleSystem)Instantiate(GameObject.FindObjectOfType<InGameController>().mouseParticles);
		moveIndicatorParticles.gameObject.SetActive(false);
		currentMoveBlocks = new List<TerrainBlock>();
		primaryAmmoRemaining = primaryAmmo;
		currentFuel = startFuel;
		currentState = UnitState.FinishedMove;
		possibleOrders = new List<UnitOrderOptions>();
		possibleTargets = new List<AttackableObject>();
		carriedUnits = new List<UnitController>();
		partiallyUnloadedUnits = new List<UnitController>();
		if(canStealth)
		{
			isStealthed = true;
		}
		modifier = new UnitPropertyModifier();
		try
		{
			currentBlock = awaitingOrdersBlock = InGameController.currentTerrain.GetBlockAtPos(transform.position);
		}
		catch
		{
			
		}
	}
	void Start () {
		RaycastHit hit;
		Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 3, transform.position.z), Vector3.down, out hit, 10f, 1);
		currentBlock = awaitingOrdersBlock = hit.collider.gameObject.GetComponent<TerrainBlock>();
		if(currentBlock == null)
		{
			Debug.Log("No block");
		}
		else
		{
			currentBlock.Occupy(this);
		}
		if(name.Contains("(Clone)"))
		{
			name = name.Replace("(Clone)", "");
		}
		targetedDamageDisplay.gameObject.SetActive(true);
		targetedDamageOutline.gameObject.SetActive(true);
		targetedDamageDisplay.text = "";
		targetedDamageOutline.text = "";
		InGameController.weather.ApplyCurrentWeatherEffect(this);
		moveIndicatorParticles.transform.position = transform.position + .5f*Vector3.down;
		moveIndicatorParticles.particleSystem.startColor = owner.mainPlayerColor;
		moveIndicatorParticles.transform.parent = transform;
		comTowerEffect = owner.ComTowersInRange(this, currentBlock);
		reinforcement = (ReinforcementInstance)InGameController.CreateInstance(unitClass, true);
	}
	void OnMouseOver()
	{
		OnMouseOverExtra();
	}
	void OnMouseEnter()
	{
		OnMouseEnterExtra();
	}

	public TerrainBlock GetOccupyingBlock ()
	{
		return currentBlock;
	}
	public float GetNormalizedFuel ()
	{
		return currentFuel/startFuel;
	}
	public int UnitCost ()
	{
		return baseCost;
	}

	public float GetNormalizedAmmo ()
	{
		return ((float)primaryAmmoRemaining)/(primaryAmmo > 0 ? primaryAmmo : 1);
	}
	public void OnMouseEnterExtra()
	{
		if(currentState == UnitState.UnMoved || !InGameController.GetCurrentPlayer().Equals(owner) && gameObject.activeSelf)
		{
			
		}
	}
	public void OnMouseOverExtra()
	{
		infoBoxTimeoutCounter = 0;
		if(Input.GetMouseButton(1) && (currentState == UnitState.UnMoved || currentState == UnitState.FinishedMove) && gameObject.activeSelf)
		{
			ShowNextTurnAttackRange();
		}
	}
	void OnMouseExit()
	{
		OnMouseExitExtra();
	}
	public void OnMouseExitExtra()
	{
		InGameController.mouseOverParticles.gameObject.SetActive(false);
	}
	void OnMouseUp()
	{
		OnMouseUpExtra();
	}
	public void OnMouseUpExtra()
	{
		if(InGameController.GetCurrentPlayer() == owner && !InGameController.isPaused && currentState != UnitState.FinishedMove)
		{
			if(!hasUnitSelectedMutex)
			{
				hasUnitSelectedMutex = InGameController.AcquireUnitSelectedMutex(this);
			}
			if(hasUnitSelectedMutex)
			{
				switch(currentState)
				{
					case UnitState.UnMoved:
					{
						unitJustSelected = true;
						break;
					}
					case UnitState.Selected:
					{
						ChangeState(UnitState.Selected, UnitState.AwaitingOrder);
						break;
					}
					case UnitState.AwaitingOrder:
					{
						break;
					}
				}
			}
		}
	}
	public List<UnitOrderOptions> CalculateAwaitingOrderOptions(TerrainBlock block)
	{
		possibleOrders.Clear();
		if(block.IsOccupied() && block.occupyingUnit.CanCarryUnit(this) && block.occupyingUnit.GetOwner().IsSameSide(owner))
		{
			possibleOrders.Add(UnitOrderOptions.Load);
		}
		else if(block.IsOccupied() && block.occupyingUnit.GetOwner().IsNeutralSide() && canCapture)
		{
			{
				possibleOrders.Add(UnitOrderOptions.Board);
			}
		}
		else
		{
			possibleTargets.Clear();
			if(owner.currentGeneralUnit == this && owner.selectedGeneral.CanUsePower())
			{
				possibleOrders.Add(UnitOrderOptions.GeneralPower);
			}
			if(((!canMoveAndAttack && didNotMoveThisTurn) || canMoveAndAttack) && primaryAmmoRemaining > 0)
			{
				possibleTargets = CalculatePossibleTargets(block);
				if(possibleTargets.Count > 0)
				{
					possibleOrders.Add(UnitOrderOptions.Attack);
				}
			}
			if(canSupply)
			{
				for(int i = 0; i < block.adjacentBlocks.Length; i++)
				{
					if(block.adjacentBlocks[i].HasProperty() && block.adjacentBlocks[i].occupyingProperty.GetOwner().IsSameSide(owner) && block.adjacentBlocks[i].occupyingProperty.health < 100)
					{
						possibleOrders.Add(UnitOrderOptions.Repair);
						break;
					}
				}
			}
			if(canSupply)
			{
				for(int i = 0; i < block.adjacentBlocks.Length; i++)
				{
					if(block.adjacentBlocks[i].IsOccupied() && block.adjacentBlocks[i].occupyingUnit.GetOwner().IsSameSide(owner) && block.adjacentBlocks[i].occupyingUnit != this)
					{
						possibleOrders.Add(UnitOrderOptions.Supply);
						break;
					}
				}
			}
			if(canCapture && block.HasProperty() && !block.occupyingProperty.GetOwner().IsSameSide(owner) && block.occupyingProperty.propertyClass.capturable)
			{
				if(block.IsOccupied() && block.occupyingUnit != this)
				{
					
				}
				else
				{
					possibleOrders.Add(UnitOrderOptions.Capture);
				}
			}
			if(didNotMoveThisTurn && block.HasProperty() && block.occupyingProperty.CanProduceUnit(unitClass) && block.occupyingProperty.GetOwner().IsSameSide(owner))
			{
				if(owner.currentGeneralUnit == null && owner.funds >= baseCost/2)
				{
					possibleOrders.Add(UnitOrderOptions.AddGeneral);
				}
			}
			if(canBuild)
			{
				for(int i = 0; i < block.adjacentBlocks.Length; i++)
				{
					if(!block.adjacentBlocks[i].IsOccupied() && !block.adjacentBlocks[i].HasProperty())
					{
						if(block.adjacentBlocks[i].typeOfTerrain == TERRAINTYPE.River && owner.CanProduceProperty(UnitNames.Bridge) && block.adjacentBlocks[i].adjacentBlocks[i].UnitMovementCost(moveClass) > 0)
						{
							possibleOrders.Add(UnitOrderOptions.BuildBridge);
							break;
						}
					}
				}
			}
			if(carriedUnits.Count > 0)
			{
				for(int i = 0; i < block.adjacentBlocks.Length; i++)
				{
					if(!block.adjacentBlocks[i].IsOccupied())
					{
						foreach(UnitController uc in carriedUnits)
						{
							if(block.UnitMovementCost(uc.moveClass) > 0 && awaitingOrdersBlock.adjacentBlocks[i].UnitMovementCost(uc.moveClass) > 0 && !possibleOrders.Contains(UnitOrderOptions.Unload))
							{
								possibleOrders.Add(UnitOrderOptions.Unload);
								break;
							}
						}
					}
				}
			}
			if(canStealth)
			{
				if(isStealthed)
				{
					possibleOrders.Add(UnitOrderOptions.UnStealthify);
				}
				else
				{
					possibleOrders.Add(UnitOrderOptions.Stealthify);
				}
			}
			possibleOrders.Add(UnitOrderOptions.EndTurn);
		}
		return possibleOrders;
	}
	public List<AttackableObject> CalculatePossibleTargets(TerrainBlock block)
	{
		List<AttackableObject> list = new List<AttackableObject>();
		List<AttackableObject> allUnitsInRange = InGameController.currentTerrain.ObjectsWithinRange(block, minAttackRange, modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange), this);
		foreach(AttackableObject ao in allUnitsInRange)
		{
			if(ao is UnitController && ((UnitController)ao) != this)
			{
				if(!((UnitController)ao).GetOwner().IsSameSide(owner) && DamageValues.CanAttackUnit(this, (UnitController)ao) && ((UnitController)ao).gameObject.activeSelf)
				{
					list.Add(ao);
				}
			}
			else if(ao is Property)
			{
				if(!((Property)ao).GetOwner().IsSameSide(owner) && DamageValues.CanAttackUnit(this, ((Property)ao)))
				{
					list.Add(ao);
				}
			}
		}
		return list;
	}
	void Update () {
		if(currentState == UnitState.FinishedMove)
		{
			if(wasTargeted)
			{
				wasTargeted = false;
			}
			else
			{
				targetedDamageDisplay.text = "";
				targetedDamageOutline.text = "";
			}
			if(showingNextTurnAttackRange && Input.GetMouseButtonUp(1))
			{
				HideMoveRange();
			}
		}
		else if(currentState == UnitState.UnMoved)
		{
			if(showingNextTurnAttackRange && Input.GetMouseButtonUp(1))
			{
				HideMoveRange();
			}
		}
		else if(currentState == UnitState.Selected)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit, float.PositiveInfinity, 1))
			{
				TerrainBlock block = hit.collider.GetComponent<TerrainBlock>();
				GeneratePath(block);
				if(Input.GetMouseButtonDown(0))
				{
					if(!block.IsOccupied() || (block.IsOccupied() && (block.occupyingUnit.GetOwner().IsSameSide(owner) || block.occupyingUnit.GetOwner().IsNeutralSide()) && block.occupyingUnit.CanCarryUnit(this))
					   || (!block.occupyingUnit.gameObject.activeSelf))
					{
						ChangeState(UnitState.Selected, UnitState.Moving);
					}
				}
			}
			if(Input.GetMouseButtonDown(1))
			{
				ChangeState(UnitState.Selected, UnitState.UnMoved);
			}
		}
		else if(currentState == UnitState.BuildingBridge)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit, float.PositiveInfinity, 1))
			{
				for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
				{
					if(awaitingOrdersBlock.adjacentBlocks[i] == hit.collider.GetComponent<TerrainBlock>())
					{
						if(!awaitingOrdersBlock.adjacentBlocks[i].IsOccupied() && awaitingOrdersBlock.adjacentBlocks[i].typeOfTerrain == TERRAINTYPE.River)
						{
							awaitingOrdersBlock.adjacentBlocks[i].DisplaySupportTile();
							if(Input.GetMouseButtonDown(0))
							{
								Property prop = owner.ProduceProperty(UnitNames.Bridge, awaitingOrdersBlock.adjacentBlocks[i].transform.position + Vector3.up*.5f, (Mathf.RoundToInt(awaitingOrdersBlock.adjacentBlocks[i].transform.position.x) != Mathf.RoundToInt(awaitingOrdersBlock.transform.position.x) ? Quaternion.AngleAxis(90, Vector3.up): Quaternion.identity));
								prop.StartConstruction();
								ChangeState(UnitState.BuildingBridge, UnitState.FinishedMove);
							}
						}
						break;
					}
					else
					{
						awaitingOrdersBlock.adjacentBlocks[i].HideTileColor();
					}
				}
			}
			if(Input.GetMouseButtonDown(1))
			{
				ChangeState(UnitState.BuildingBridge, UnitState.AwaitingOrder);
			}
		}
		else if(currentState == UnitState.Unloading)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit, float.PositiveInfinity, 1))
			{
				for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
				{
					if(awaitingOrdersBlock.adjacentBlocks[i] == hit.collider.GetComponent<TerrainBlock>())
					{
						if(currentlyUnloadingUnit != null && !awaitingOrdersBlock.adjacentBlocks[i].IsOccupied() && awaitingOrdersBlock.adjacentBlocks[i].UnitMovementCost(currentlyUnloadingUnit.moveClass) > 0)
						{
							currentlyUnloadingUnit.transform.position = awaitingOrdersBlock.adjacentBlocks[i].transform.position + .5f * Vector3.up;
							currentlyUnloadingUnit.gameObject.SetActive(true);
							if(Input.GetMouseButtonDown(0))
							{
								partiallyUnloadedUnits.Add(currentlyUnloadingUnit);
								currentlyUnloadingUnit.currentBlock = currentlyUnloadingUnit.awaitingOrdersBlock = awaitingOrdersBlock.adjacentBlocks[i];
								currentlyUnloadingUnit.isInUnit = false;
								awaitingOrdersBlock.adjacentBlocks[i].Occupy(currentlyUnloadingUnit);
								currentlyUnloadingUnit = null;
								if(partiallyUnloadedUnits.Count == carriedUnits.Count)
								{
									ChangeState(UnitState.Unloading, UnitState.FinishedMove);
								}
							}
						}
						break;
					}
				}
			}
			if(Input.GetMouseButtonDown(1))
			{
				if(partiallyUnloadedUnits.Count == 0)
				{
					foreach(UnitController uc in carriedUnits)
					{
						uc.gameObject.SetActive(false);
					}
					ChangeState(UnitState.Unloading, UnitState.AwaitingOrder);
				}
				else
				{
					ChangeState(UnitState.Unloading, UnitState.FinishedMove);
				}
			}
		}
		else if(currentState == UnitState.Moving)
		{
			if(movingCounter < inverseMovingSpeed*currentMoveBlocks.Count)
			{
				if(currentMoveBlocks[movingCounter/inverseMovingSpeed].IsOccupied() && !currentMoveBlocks[movingCounter/inverseMovingSpeed].occupyingUnit.gameObject.activeSelf)
				{
					hitFogOfWarUnit = true;
					awaitingOrdersBlock = currentMoveBlocks[movingCounter/inverseMovingSpeed - 1];
					infoBoxTimeoutCounter = 0;
					ChangeState(UnitState.Moving, UnitState.FinishedMove);
				}
				else
				{
					transform.position = new Vector3(currentMoveBlocks[movingCounter/inverseMovingSpeed].transform.position.x, transform.position.y, currentMoveBlocks[movingCounter/inverseMovingSpeed].transform.position.z);
				}
				movingCounter++;
			}
			else
			{
				ChangeState(UnitState.Moving, UnitState.AwaitingOrder);
			}
		}
		else if(currentState == UnitState.TargetingUnit)
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
			{
				if(hit.collider.GetComponent<TerrainBlock>().IsOccupied() && hit.collider.GetComponent<TerrainBlock>().occupyingUnit != this)
				{
					UnitController other = hit.collider.GetComponent<TerrainBlock>().occupyingUnit;
					if(possibleTargets.Contains(other))
					{
						other.DisplayTargetDamage(DamageValues.CalculateDamage(this, other));
						if(Input.GetMouseButtonDown(0))
						{
							ExchangeFire(other);
							ChangeState(UnitState.TargetingUnit, UnitState.FinishedMove);
						}
					}
				}
				else if(hit.collider.GetComponent<TerrainBlock>().HasProperty())
				{
					Property other = hit.collider.GetComponent<TerrainBlock>().occupyingProperty;
					if(possibleTargets.Contains(other))
					{
						other.DisplayTargetDamage(DamageValues.CalculateDamage(this, other));
						if(Input.GetMouseButtonDown(0))
						{
							ExchangeFire(other);
							ChangeState(UnitState.TargetingUnit, UnitState.FinishedMove);
						}
					}
				}
			}
			if(Input.GetMouseButtonDown(1))
			{
				ChangeState(UnitState.TargetingUnit, UnitState.AwaitingOrder);
			}
		}
		else if(currentState == UnitState.AwaitingOrder)
		{
			if(Input.GetMouseButtonDown(1))
			{
				ChangeState(UnitState.AwaitingOrder, UnitState.Selected);
			}
		}
		if(unitJustSelected)
		{
			ChangeState(UnitState.UnMoved, UnitState.Selected);
			unitJustSelected = false;
		}
	}

	void CheckPathConsistency()
	{
		for(int i = 0; i < currentMoveBlocks.Count; i++)
		{
			for(int j = i+1; j < currentMoveBlocks.Count; j++)
			{
				if(currentMoveBlocks[i]==currentMoveBlocks[j])
				{
					currentMoveBlocks.RemoveRange(i+1, j-i);
					return;
				}
			}
		}
	}

	public int EffectiveAttackRange ()
	{
		return modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange);
	}

	public int EffectiveMoveRange()
	{
		int owt = modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.MovementRange, movementRange);
		return owt > Mathf.FloorToInt(currentFuel) ? Mathf.FloorToInt(currentFuel):owt;
	}
	void GeneratePath(TerrainBlock block)
	{
		if(!currentMoveBlocks.Contains(block))
		{
			if(block.IsOccupied() && !(block.occupyingUnit.GetOwner().IsSameSide(owner) || block.occupyingUnit.GetOwner().IsNeutralSide()) && block.occupyingUnit.gameObject.activeSelf)
			{
				
			}
			else
			{
				float pathDistance;
				List<TerrainBlock> possiblePath = InGameController.currentTerrain.PathBetweenTiles(this, currentMoveBlocks[currentMoveBlocks.Count-1], block, EffectiveMoveRange(), moveClass, out pathDistance);
				if(possiblePath != null && pathDistance + currentPathDistance <= EffectiveMoveRange())
				{
					TerrainBlock.HideMovementPath(currentMoveBlocks);
					for(int i = 1; i < possiblePath.Count; i++)
					{
						currentMoveBlocks.Add(possiblePath[i]);
					}
					CheckPathConsistency();
					currentPathDistance = RecalculatePathCost();
					currentMoveBlocks[0].DisplayMovementPath(currentMoveBlocks);
				}
				else
				{
					possiblePath = InGameController.currentTerrain.PathBetweenTiles(this, currentBlock, block, EffectiveMoveRange(), moveClass, out pathDistance);
					if(possiblePath != null)
					{
						TerrainBlock.HideMovementPath(currentMoveBlocks);
						currentMoveBlocks = possiblePath;
						currentPathDistance = pathDistance;
						currentMoveBlocks[0].DisplayMovementPath(currentMoveBlocks);
					}
				}
			}
		}
		else
		{
			int blockIndex = currentMoveBlocks.IndexOf(block);
			if(blockIndex < currentMoveBlocks.Count -1)
			{
				TerrainBlock.HideMovementPath(currentMoveBlocks);
				currentMoveBlocks.RemoveRange(blockIndex + 1, currentMoveBlocks.Count - blockIndex - 1);
				currentPathDistance = RecalculatePathCost();
				currentMoveBlocks[0].DisplayMovementPath(currentMoveBlocks);
			}
		}
	}
	void ChangeState(UnitState current, UnitState next)
	{
		switch(current)
		{
		case UnitState.AwaitingOrder:
		{
			switch(next)
			{
					case UnitState.Selected:
					{
						transform.position = new Vector3(currentBlock.transform.position.x, transform.position.y, currentBlock.transform.position.z);
						currentBlock.DisplayMovementPath(currentMoveBlocks);
						comTowerEffect = owner.ComTowersInRange(this,currentBlock);
						didNotMoveThisTurn = true;
						currentState = UnitState.Selected;
						movingCounter = 0;
						if(owner.currentGeneralUnit == this)
						{
							owner.selectedGeneral.ShowZone(currentBlock);
						}
						if(canMoveAndAttack)
						{
							InGameController.currentTerrain.IlluminatePossibleMovementBlocks(currentBlock, this, EffectiveMoveRange(), (primaryAmmoRemaining > 0 ? modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange): 0));
						}
						else
						{
							InGameController.currentTerrain.IlluminatePossibleMovementBlocks(currentBlock, this, EffectiveMoveRange(), 0);
						}
						break;
					}
					case UnitState.Unloading:
					{
						currentState = UnitState.Unloading;
						break;
					}
					case UnitState.BuildingBridge:
					{
						currentState = UnitState.BuildingBridge;
						break;
					}
					case UnitState.TargetingUnit:
					{
						InGameController.DisplayPossibleTargetParticles(possibleTargets);
						currentState = UnitState.TargetingUnit;
						break;
					}
					case UnitState.FinishedMove:
					{
						InternalEndTurn();
						break;
					}
				}
				break;
			}
			case UnitState.BuildingBridge:
			{
				switch(next)
				{
					case UnitState.AwaitingOrder:
					{
						for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
						{
							awaitingOrdersBlock.adjacentBlocks[i].HideTileColor();
						}
						currentState = UnitState.AwaitingOrder;
						break;
					}
					case UnitState.FinishedMove:
					{
						for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
						{
							awaitingOrdersBlock.adjacentBlocks[i].HideTileColor();
						}
						InternalEndTurn();
						break;
					}
				}
				break;
			}
			case UnitState.FinishedMove:
			{
				switch(next)
				{
					case UnitState.UnMoved:
					{
						currentPathDistance = 0;
						currentMoveBlocks.Clear();
						currentMoveBlocks.Add(currentBlock);
						awaitingOrdersBlock = currentBlock;
						comTowerEffect = owner.ComTowersInRange(this,currentBlock);
						didNotMoveThisTurn = true;
						currentState = UnitState.UnMoved;
						moveIndicatorParticles.gameObject.SetActive(true);
						foreach(TerrainBlock tb in currentBlock.adjacentBlocks)
						{
							if(tb.IsOccupied() && tb.occupyingUnit.isStealthed && !tb.occupyingUnit.owner.IsSameSide(owner))
							{
								tb.occupyingUnit.SetActive(true);
							}
						}
						if(!isInUnit)
						{
							InGameController.currentTerrain.ClearFog(currentBlock, modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.VisionRange, fogOfWarRange), unitClass == UnitNames.UAV?true:false);
						}
						break;
					}
				}
				break;
			}
			case UnitState.Moving:
			{
				switch(next)
				{
					case UnitState.AwaitingOrder:
					{
						if(owner.currentGeneralUnit == this)
						{
							owner.selectedGeneral.ShowZone(awaitingOrdersBlock);
						}
						currentState = UnitState.AwaitingOrder;
						TerrainBlock.HideMovementPath(currentMoveBlocks);
						CalculateAwaitingOrderOptions(awaitingOrdersBlock);
						comTowerEffect = owner.ComTowersInRange(this,awaitingOrdersBlock);
						movingCounter = 0;
						if(!possibleOrders.Contains(UnitOrderOptions.Load))
						{
							ShowAttackRange();
						}
						break;
					}
					case UnitState.Selected:
					{
						movingCounter = 0;
						transform.position = new Vector3(currentBlock.transform.position.x, transform.position.y, currentBlock.transform.position.z);
						TerrainBlock.HideMovementPath(currentMoveBlocks);
						currentPathDistance = 0;
						currentState = UnitState.Selected;
						didNotMoveThisTurn = true;
						break;
					}
					case UnitState.FinishedMove:
					{
						hitFogOfWarUnit = false;
						movingCounter = 0;
						TerrainBlock.HideMovementPath(currentMoveBlocks);
						InternalEndTurn();
						break;
					}
				}
				break;
			}
			case UnitState.Selected:
			{
				switch(next)
				{
					case UnitState.AwaitingOrder:
					{
						didNotMoveThisTurn = true;
						awaitingOrdersBlock = currentMoveBlocks[currentMoveBlocks.Count-1];
						if(owner.currentGeneralUnit == this)
						{
							owner.selectedGeneral.ShowZone(awaitingOrdersBlock);
						}
						currentState = UnitState.AwaitingOrder;
						CalculateAwaitingOrderOptions(awaitingOrdersBlock);
						if(!possibleOrders.Contains(UnitOrderOptions.Load))
						{
							ShowAttackRange();
						}
						break;
					}
					case UnitState.Moving:
					{
						didNotMoveThisTurn = false;
						awaitingOrdersBlock = currentMoveBlocks[currentMoveBlocks.Count-1];
						HideMoveRange();
						currentState = UnitState.Moving;
						break;
					}
					case UnitState.UnMoved:
					{
						didNotMoveThisTurn = true;
						TerrainBlock.HideMovementPath(currentMoveBlocks);
						currentPathDistance = 0;
						HideMoveRange();
						moveIndicatorParticles.gameObject.SetActive(true);
						hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
						currentState = UnitState.UnMoved;
						break;
					}
				}
				break;
			}
			case UnitState.TargetingUnit:
			{
				switch(next)
				{
					case UnitState.FinishedMove:
					{
						InGameController.HidePossibleTargetParticles();
						if(health > 0)
						{
							InternalEndTurn();
						}
						else
						{
							EndTurn();
						}
						break;
					}
					case UnitState.AwaitingOrder:
					{
						currentState = UnitState.AwaitingOrder;
						InGameController.HidePossibleTargetParticles();
						break;
					}
				}
				break;
			}
			case UnitState.UnMoved:
			{
				switch(next)
				{
					case(UnitState.Selected):
					{
						currentState = UnitState.Selected;
						currentPathDistance = 0;
						movingCounter = 0;
						moveIndicatorParticles.gameObject.SetActive(false);
						if(canMoveAndAttack)
						{
							InGameController.currentTerrain.IlluminatePossibleMovementBlocks(currentBlock, this, EffectiveMoveRange(), (primaryAmmoRemaining > 0 ? modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange): 0));
						}
						else
						{
							InGameController.currentTerrain.IlluminatePossibleMovementBlocks(currentBlock, this, EffectiveMoveRange(), 0);
						}
						break;
					}
				}
				break;
			}
			case UnitState.Unloading:
			{
				switch(next)
				{
					case(UnitState.AwaitingOrder):
					{
						currentState = UnitState.AwaitingOrder;
						break;
					}
					case(UnitState.FinishedMove):
					{
						foreach(UnitController uc in partiallyUnloadedUnits)
						{
							carriedUnits.Remove(uc);
						}
						partiallyUnloadedUnits.Clear();
						InternalEndTurn();
						break;
					}
				}
				break;
			}
		}
	}
	
	public void DisplayTargetDamage(int damage)
	{
		wasTargeted = true;
		targetedDamageDisplay.text = damage + "%";
		targetedDamageDisplay.transform.rotation = Camera.main.transform.rotation;
		targetedDamageOutline.text = damage + "%";
		targetedDamageOutline.transform.rotation = Camera.main.transform.rotation;
	}
	public void SetOwner(Player newOwner)
	{
		owner = newOwner;
		moveIndicatorParticles.particleSystem.startColor = owner.mainPlayerColor;
	}
	public Player GetOwner()
	{
		return owner;
	}
	public void Heal(int attemptedHealAmount, bool useFunds)
	{
		if(health < 100)
		{
			if(useFunds)
			{
				for(int i = 0; i < attemptedHealAmount; i++)
				{
					if(owner.RemoveFunds((int)(baseCost/10)))
					{
						health.AddRawHealth(10);
					}
				}
			}
			else
			{
				health.AddRawHealth(attemptedHealAmount*10);
				
			}
		}
	}
	void CalculateFuelUsage()
	{
		if(!isInUnit)
		{
			if(moveClass == MovementType.Air)
			{
				if(isStealthed)
				{
					currentFuel -= 5;
				}
				else
				{
					currentFuel -= 3;
				}
			}
			else if(moveClass == MovementType.Littoral || moveClass == MovementType.Sea)
			{
				if(isStealthed)
				{	
					currentFuel -= 5;
				}
				else
				{
					currentFuel -= 2;
				}
			}
			else if(moveClass == MovementType.Sniper && isStealthed)
			{
				currentFuel -= 3;
			}
			else
			{
				currentFuel--;
			}
		}
		if(currentFuel < 0)
		{
			if(moveClass == MovementType.Air)
			{
				owner.DeleteUnit(this);
			}
			currentFuel = 0;
		}
	}
	public void StartTurn()
	{
		if(!isInUnit)
		{
			gameObject.SetActive(true);
		}
		if(veteranStatus == UnitRanks.Elite && !isInUnit)
		{
			Heal(1, false);
		}
		if(currentBlock == null)
		{
			currentBlock = InGameController.currentTerrain.GetBlockAtPos(transform.position);
		}
		CalculateFuelUsage();
		if(currentBlock.HasProperty())
		{
			if(currentBlock.occupyingProperty.GetOwner().IsSameSide(owner) && currentBlock.occupyingProperty.CanHealUnit(this))
			{
				Heal(2, true);
				Resupply();
			}
		}
		if(healsCarriedUnits)
		{
			foreach(UnitController uc in carriedUnits)
			{
				uc.Heal(2, true);
				uc.Resupply();
			}
		}
		if(canSupply)
		{
			foreach(TerrainBlock t in currentBlock.adjacentBlocks)
			{
				if(t.IsOccupied() && t.occupyingUnit.GetOwner().IsSameSide(owner) && t.occupyingUnit != this)
				{
					t.occupyingUnit.Resupply();
				}
			}
		}
		if(owner.currentGeneralUnit == this && !isInUnit)
		{
			owner.selectedGeneral.ShowZone(awaitingOrdersBlock);
		}
		if(!isInUnit)
		{
			ChangeState(UnitState.FinishedMove, UnitState.UnMoved);
		}
	}
	void InternalEndTurn()
	{
		if(!isInUnit)
		{
			if(currentBlock != awaitingOrdersBlock)
			{
				currentBlock.UnOccupy(this);
				currentBlock = awaitingOrdersBlock;
				currentBlock.Occupy(this);
			}
			foreach(TerrainBlock tb in currentBlock.adjacentBlocks)
			{
				if(tb.IsOccupied() && tb.occupyingUnit.isStealthed && !tb.occupyingUnit.owner.IsSameSide(owner))
				{
					tb.occupyingUnit.SetActive(true);
				}
			}
			InGameController.currentTerrain.ClearFog(currentBlock, modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.VisionRange, fogOfWarRange), unitClass == UnitNames.UAV?true:false);
			if(owner.currentGeneralUnit == this)
			{
				owner.selectedGeneral.ShowZone(awaitingOrdersBlock);
			}
		}
		EndTurn();
	}
	public void EndTurn ()
	{
		hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
		comTowerEffect = owner.ComTowersInRange(this,currentBlock);
		currentState = UnitState.FinishedMove;
		HideMoveRange();
		currentFuel -= CalculateFuelCost();
		currentPathDistance = 0;
	}
	public void ShowAttackRange()
	{
		showingAttackRange = true;
		if((canMoveAndAttack || didNotMoveThisTurn) && primaryAmmoRemaining > 0)
		{
			InGameController.currentTerrain.IlluminatePossibleAttackBlocksRange(awaitingOrdersBlock, minAttackRange, (primaryAmmoRemaining > 0 ? modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange): 0));
		}
	}
	public Vector3 GetPosition()
	{
		return transform.position;
	}
	public void ShowNextTurnAttackRange()
	{
		if(InGameController.currentTerrain.illuminatedMovementRangeBlocks.Count == 0)
		{
			showingNextTurnAttackRange = true;
			if(primaryAmmoRemaining > 0)
			{
				if(canMoveAndAttack)
				{
					InGameController.currentTerrain.IlluminatePossibleAttackBlocks(currentBlock, this, EffectiveMoveRange(), (primaryAmmoRemaining > 0 ? modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange): 0));
				}
				else
				{
					InGameController.currentTerrain.IlluminatePossibleAttackBlocksRange(currentBlock, minAttackRange, (primaryAmmoRemaining > 0 ? modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, maxAttackRange): 0));
				}
			}
		}
	}
	public void SetActive(bool activate)
	{
		gameObject.SetActive(activate);
		moveIndicatorParticles.gameObject.SetActive(activate);
	}
	public void HideMoveRange()
	{
		InGameController.currentTerrain.ClearMoveBlocks();
		showingNextTurnAttackRange = false;
	}
	public void HideAttackRange()
	{
		InGameController.currentTerrain.ClearMoveBlocks();
		showingAttackRange = false;
	}
	public void OnGUI()
	{
		if(currentState == UnitState.AwaitingOrder)
		{
			Vector3 unitPointOnScreen = Camera.main.WorldToScreenPoint(transform.position);
			GUI.BeginGroup(new Rect(unitPointOnScreen.x + actionDisplayXOffset, Screen.height - unitPointOnScreen.y + actionDisplayYOffset, actionDisplayWidth, actionDisplayHeight*possibleOrders.Count));
			for(int i = 0; i < possibleOrders.Count; i++)
			{
				if(GUI.Button(new Rect(0, i*actionDisplayHeight, actionDisplayWidth, actionDisplayHeight), Utilities.PrettifyVariableName(possibleOrders[i].ToString())))
				{
					EvaluateOrder(possibleOrders[i]);
				}
			}
			GUI.EndGroup();
		}
		if(currentState == UnitState.Unloading)
		{
			if(currentlyUnloadingUnit == null)
			{
				Vector3 unitPointOnScreen = Camera.main.WorldToScreenPoint(transform.position);
				GUI.BeginGroup(new Rect(unitPointOnScreen.x + actionDisplayXOffset, Screen.height - unitPointOnScreen.y + actionDisplayYOffset, actionDisplayWidth, actionDisplayHeight*(carriedUnits.Count)));
				for(int i = 0; i < carriedUnits.Count; i++)
				{
					if(!partiallyUnloadedUnits.Contains(carriedUnits[i]))
					{
						if(GUI.Button(new Rect(0, i*actionDisplayHeight, actionDisplayWidth, actionDisplayHeight), carriedUnits[i].name))
						{
							currentlyUnloadingUnit = carriedUnits[i];
						}
					}
				}
				GUI.EndGroup();
			}
		}
		if(infoBoxTimeoutCounter < .5f)
		{
			if(hitFogOfWarUnit)
			{
				ShowFOWSurprise();
			}
		}
	}
	public void ShowUnitControllerInfo()
	{
		GUI.BeginGroup(new Rect(Screen.width - 3*infoBoxWidth - 40, 0, infoBoxWidth + 40, infoBoxHeight));
		GUI.Box(new Rect(0, 0, infoBoxWidth + 40, infoBoxHeight), prettyName);
		GUI.Label(new Rect(0, 20, infoBoxWidth, 20), new GUIContent("HP " + health.PrettyHealth().ToString(), "Current Health"));
		GUI.Label(new Rect(0, 40, infoBoxWidth, 20), new GUIContent("Fuel: " + Mathf.FloorToInt(currentFuel) + "/" + startFuel, "Current Fuel"));
		if(primaryAmmo > 0)
		{
			GUI.Label(new Rect(0, 60, infoBoxWidth, 20), new GUIContent("Ammo: " + primaryAmmoRemaining + "/" + primaryAmmo, "Ammunition Remaining"));
		}
		else
		{
			GUI.Label(new Rect(0, 60, infoBoxWidth, 20), new GUIContent("Ammo: --/--", "Ammunition Remaining"));
		}
		if(veteranStatus != UnitRanks.Unranked)
		{
			GUI.Label(new Rect(infoBoxWidth+4, 20, 32, infoBoxHeight), Utilities.GetRankImage(veteranStatus));
		}
		GUI.EndGroup();
	}
	public void ShowDetailedInfo()
	{
		GUI.BeginGroup(new Rect(Screen.width/2 - infoBoxWidth, Screen.height/2 - infoBoxHeight, 2 * infoBoxWidth, 2 * infoBoxHeight));
		GUI.TextArea(new Rect(0, 0, 2 * infoBoxWidth, 2 * infoBoxHeight), description);
		GUI.EndGroup();
	}
	public static bool CanRetaliate(UnitAttackType retaliator, UnitAttackType defender)
	{
		if(defender == UnitAttackType.Indirect || retaliator == UnitAttackType.Indirect)
		{
			return false;
		}
		return true;
	}
	public bool TakeDamage(int inDamage, bool leaveAlive)
	{
		health.AddRawHealth(-inDamage);
		if(leaveAlive)
		{
			if(health < 10)
			{
				health.SetRawHealth(10);
			}
		}
		if(health <= 0)
		{
			owner.DeleteUnit(this);
			return true;
		}
		damageReceived += inDamage;
		if(veteranStatus == UnitRanks.Sergeant && damageReceived >= 70)
		{
			veteranStatus = UnitRanks.Elite;
		}
		return false;
	}
	void Snipe(UnitController other, int damage)
	{
		primaryAmmoRemaining--;
		if(other.TakeDamage(damage, false))
		{
			RankUp();
		}
		else if(UnityEngine.Random.Range(1, 200) > other.health)
		{
			foreach(UnitController carriedUnit in other.carriedUnits)
			{
				carriedUnit.KillUnit();
			}
			other.owner.RemoveUnit(other);
			InGameController.GetPlayer(0).AddUnit(other);
		}
	}
	public void ExchangeFire(UnitController other)
	{
		int damageOut = DamageValues.CalculateDamage(this, other) + DamageValues.CalculateLuckDamage(health.GetRawHealth());
		if(unitClass == UnitNames.Sniper && (other.moveClass == MovementType.Amphibious
		                                     || other.moveClass == MovementType.HeavyVehicle 
		                                     || other.moveClass == MovementType.LightVehicle
		                                     || other.moveClass == MovementType.Tank))
		{
			Snipe(other, damageOut);
		}
		else
		{
			primaryAmmoRemaining--;
			if(owner.selectedGeneral.IsInZoneRange(transform) && !other.GetOwner().IsNeutralSide())
			{
				owner.selectedGeneral.UpdateGeneral(damageOut > other.health.GetRawHealth()?other.health.GetRawHealth():damageOut);
			}
			if(other.TakeDamage(damageOut, false))
			{
				RankUp();
				return;
			}
			if(TerrainBuilder.ManhattanDistance(transform.position, other.transform.position) >= other.minAttackRange &&
			   TerrainBuilder.ManhattanDistance(transform.position, other.transform.position) <= other.modifier.ApplyModifiers(UnitPropertyModifier.PropertyModifiers.AttackRange, other.maxAttackRange) &&
			   DamageValues.CanAttackUnit(other, this) && CanRetaliate(other.attackType, attackType) &&
			   other.primaryAmmoRemaining > 0)
			{
				damageOut = DamageValues.CalculateDamage(other, this) + DamageValues.CalculateLuckDamage(other.health.GetRawHealth());
				if(TakeDamage(damageOut, false))
				{
					other.RankUp();
				}
				if(other.GetOwner().selectedGeneral.IsInZoneRange(other.transform) && !owner.IsNeutralSide())
				{
					other.GetOwner().selectedGeneral.UpdateGeneral(damageOut > other.health.GetRawHealth()?other.health.GetRawHealth():damageOut);
				}
				other.primaryAmmoRemaining--;
			}
		}
	}
	public void RankUp()
	{
		if(veteranStatus == UnitRanks.Elite)
		{
			
		}
		else if(veteranStatus == UnitRanks.Sergeant)
		{
			if(damageReceived >= 70)
			{
				veteranStatus = UnitRanks.Elite;
			}
		}
		else if(veteranStatus == UnitRanks.Corporal)
		{
			veteranStatus = UnitRanks.Sergeant;
		}
		else if(veteranStatus == UnitRanks.Private)
		{
			veteranStatus = UnitRanks.Corporal;
		}
		else
		{
			veteranStatus = UnitRanks.Private;
		}
	}
	public void ExchangeFire(Property other)
	{
		int damageOut = DamageValues.CalculateDamage(this, other) + DamageValues.CalculateLuckDamage(health.GetRawHealth());
		other.TakeDamage(damageOut);
		if(owner.selectedGeneral.IsInZoneRange(transform) && !other.GetOwner().IsNeutralSide())
		{
			owner.selectedGeneral.UpdateGeneral(damageOut > other.health.GetRawHealth()?other.health.GetRawHealth():damageOut);
		}
		primaryAmmoRemaining--;
	}
	public void KillUnit()
	{
		if(occupiedProperty != null)
		{
			occupiedProperty.UnOccupy();
		}
		if(carriedUnits.Count > 0)
		{
			foreach(UnitController unit in carriedUnits)
			{
				if(awaitingOrdersBlock.UnitMovementCost(unit.moveClass) > 0)
				{
					unit.transform.position = awaitingOrdersBlock.transform.position + .5f * Vector3.up;
					unit.SetActive(true);
					unit.currentBlock = unit.awaitingOrdersBlock = awaitingOrdersBlock;
					unit.isInUnit = false;
					awaitingOrdersBlock.UnOccupy(this);
					awaitingOrdersBlock.Occupy(unit);
					break;
				}
			}
		}
		if(currentBlock != null && currentBlock.occupyingUnit == this)
		{
			currentBlock.UnOccupy(this);
		}
		else if(awaitingOrdersBlock != null && awaitingOrdersBlock.occupyingUnit == this)
		{
			awaitingOrdersBlock.UnOccupy(this);
		}
		InGameController.HidePossibleTargetParticles();
		EndTurn();
		if(owner.aiLevel != AILevel.Human)
		{
			reinforcement.accruedReward = InGameController.TotalValueRelativeToPlayer(owner) - reinforcement.accruedReward;
			owner.GetComponent<MouseEventHandler>().AddReinforcementInstance(reinforcement);
		}
		Destroy(moveIndicatorParticles);
		Destroy(gameObject);
	}
	private void EvaluateOrder(UnitOrderOptions inOrder)
	{
		switch(inOrder)
		{
			case UnitOrderOptions.GeneralPower:
			{
				owner.selectedGeneral.EnterPower();
				ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
				break;
			}
			case UnitOrderOptions.Attack:
			{
				ChangeState(currentState, UnitState.TargetingUnit);
				break;
			}
			case UnitOrderOptions.Capture:
			{
				if(awaitingOrdersBlock.HasProperty())
				{
					awaitingOrdersBlock.occupyingProperty.Capture(Mathf.RoundToInt((float)health.PrettyHealth()*captureRate), this);
				}
				ChangeState(currentState, UnitState.FinishedMove);
				break;
			}
			case UnitOrderOptions.EndTurn:
			{
				ChangeState(currentState, UnitState.FinishedMove);
				break;
			}
			case UnitOrderOptions.BuildBridge:
			{
				ChangeState(UnitState.AwaitingOrder, UnitState.BuildingBridge);
				break;
			}
			case UnitOrderOptions.Board:
			{
				InGameController.GetPlayer(0).DeleteUnit(awaitingOrdersBlock.occupyingUnit);
				owner.AddUnit(awaitingOrdersBlock.occupyingUnit);
				owner.DeleteUnit(this);
				break;
			}
			case UnitOrderOptions.Stealthify:
			{
				isStealthed = true;
				InternalEndTurn();
				break;
			}
			case UnitOrderOptions.UnStealthify:
			{
				isStealthed = false;
				InternalEndTurn();
				break;
			}
			case UnitOrderOptions.Load:
			{
				awaitingOrdersBlock.occupyingUnit.carriedUnits.Add(this);
				isInUnit = true;
				currentBlock.UnOccupy(this);
				currentBlock = awaitingOrdersBlock = null;
				EndTurn();
				if(owner.currentGeneralUnit == this)
				{
					owner.selectedGeneral.HideZone();
				}
				SetActive(false);
				break;
			}
			case UnitOrderOptions.Repair:
			{
				for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
				{
					if(awaitingOrdersBlock.adjacentBlocks[i].HasProperty() && awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.GetOwner().IsSameSide(owner))
					{
						if(awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.justBuilt)
						{
							
						}
						else if(awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.isUnderConstruction)
						{
							awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.FinishConstruction();
						}
						else
						{
							awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.HealBase(5);
						}
					}
				}
				ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
				break;
			}
			case UnitOrderOptions.Supply:
			{
				for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
				{
					if(awaitingOrdersBlock.adjacentBlocks[i].IsOccupied() && awaitingOrdersBlock.adjacentBlocks[i].occupyingUnit.GetOwner().IsSameSide(owner))
					{
						awaitingOrdersBlock.adjacentBlocks[i].occupyingUnit.Resupply();
					}
				}
				ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
				break;
			}
			case UnitOrderOptions.Unload:
			{
				ChangeState(UnitState.AwaitingOrder, UnitState.Unloading);
				break;
			}
			case UnitOrderOptions.AddGeneral:
			{
				owner.SendOutGeneral(this);
				if(veteranStatus != UnitRanks.Elite)
				{
					veteranStatus = UnitRanks.Sergeant;
				}
				ChangeState(UnitState.AwaitingOrder, UnitState.Selected);
				break;
			}
		}
	}
	public void Resupply()
	{
		currentFuel = startFuel;
		primaryAmmoRemaining = primaryAmmo;
	}
	public void ShowFOWSurprise()
	{
		targetedDamageDisplay.text = "!";
		targetedDamageOutline.text = "!";
		targetedDamageDisplay.transform.rotation = targetedDamageOutline.transform.rotation = Utilities.gameCamera.transform.rotation;
	}
	public bool CanCarryUnit(UnitController unitToCarry)
	{
		if(carriedUnits.Count < transportCapacity)
		{
			if((moveClass == MovementType.Littoral || moveClass == MovementType.Amphibious)
			   && (unitToCarry.moveClass == MovementType.LightVehicle || unitToCarry.moveClass == MovementType.HeavyVehicle ||
			    unitToCarry.moveClass == MovementType.Infantry || unitToCarry.moveClass == MovementType.Sniper ||
			    unitToCarry.moveClass == MovementType.Tank))
			{
			    return true;
			}
			if((unitClass == UnitNames.Stryker || unitClass == UnitNames.LiftCopter) && (unitToCarry.moveClass == MovementType.Infantry || unitToCarry.moveClass == MovementType.Sniper))
			{
				return true;
			}
			if(unitClass == UnitNames.LiftCopter && unitToCarry.moveClass == MovementType.LightVehicle)
			{
				return true;
			}
			if(unitClass == UnitNames.Carrier && unitToCarry.moveClass == MovementType.Air)
			{
				return true;
			}
			if(unitClass == UnitNames.Corvette && (unitToCarry.unitClass == UnitNames.LiftCopter || unitToCarry.unitClass == UnitNames.AttackCopter))
			{
				return true;
			}
		}
		return false;
	}
	public void ResetUnit()
	{
		if(owner == InGameController.GetPlayer(InGameController.currentPlayer))
		{
			didNotMoveThisTurn = true;
			TerrainBlock.HideMovementPath(currentMoveBlocks);
			currentPathDistance = 0;
			InGameController.currentTerrain.ClearMoveBlocks();
			HideAttackRange();
			hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
			awaitingOrdersBlock = currentBlock;
			transform.position = new Vector3(currentBlock.transform.position.x, transform.position.y, currentBlock.transform.position.z);
			currentState = UnitState.UnMoved;
		}
		else
		{
			HideAttackRange();
		}
	}
	public float CalculateFuelCost()
	{
		return currentPathDistance;
	}
	public int DefenseBonus()
	{
		double outBoost = 0;
		if(veteranStatus == UnitRanks.Elite)
		{
			outBoost += 1.5f;
		}
		if(owner.selectedGeneral.IsInZoneRange(transform))
		{
			outBoost += owner.selectedGeneral.UnitDefensiveBoost(unitClass);
		}
		if(awaitingOrdersBlock.HasProperty())
		{
			outBoost += awaitingOrdersBlock.occupyingProperty.TerrainBonus();
		}
		outBoost += awaitingOrdersBlock.defenseBoost;
		outBoost += comTowerEffect;
		return (int)(outBoost * DamageValues.DEFENSECONSTANT);
	}
	public int OffenseBonus()
	{
		float outBoost = 0;
		if(veteranStatus == UnitRanks.Elite)
		{
			outBoost += 1.5f;
		}
		else if(veteranStatus == UnitRanks.Sergeant)
		{
			outBoost += 1.5f;
		}
		else if(veteranStatus == UnitRanks.Corporal)
		{
			outBoost += 1;
		}
		else if(veteranStatus == UnitRanks.Private)
		{
			outBoost += .5f;
		}
		if(owner.selectedGeneral.IsInZoneRange(transform))
		{
			outBoost += owner.selectedGeneral.UnitOffensiveBoost(unitClass);
		}
		outBoost += comTowerEffect;
		return Mathf.RoundToInt(outBoost * DamageValues.DEFENSECONSTANT);
	}
	float RecalculatePathCost()
	{
		float pathCost = 0;
		for(int i = 1; i < currentMoveBlocks.Count; i++)
		{
			pathCost += currentMoveBlocks[i].UnitMovementCost(moveClass);
		}
		return pathCost;
	}
	public float AISelectBestAttackObject(TerrainBlock block, out AttackableObject ao)
	{
		List<AttackableObject> possibleTargets = CalculatePossibleTargets(block);
		float bestTargetedUnitDamage = 0;
		ao = null;
		for(int i = 0; i < possibleTargets.Count; i++)
		{
			float damageToUnit = DamageValues.CalculateDamage(this, possibleTargets[i]);
			damageToUnit = damageToUnit > possibleTargets[i].GetHealth().GetRawHealth() ? possibleTargets[i].GetHealth().GetRawHealth() : damageToUnit;
			if(possibleTargets[i].GetOwner().IsNeutralSide())
			{
				if(possibleTargets[i] is Property)
				{
					damageToUnit *= Mathf.Pow((100 - InGameController.ClosestEnemyHQ(block, moveClass, owner))/145f, 5)*.5f;
				}
			}
			if(possibleTargets[i].GetOccupyingBlock().HasProperty() && !possibleTargets[i].GetOccupyingBlock().occupyingProperty.GetOwner().IsSameSide(owner))
			{
				damageToUnit *= (6 - 5 * possibleTargets[i].GetOccupyingBlock().occupyingProperty.NormalizedCaptureCount());
			}
			if(damageToUnit * possibleTargets[i].UnitCost()/100000f > bestTargetedUnitDamage)
			{
				bestTargetedUnitDamage = damageToUnit * possibleTargets[i].UnitCost()/100000f;
				ao = possibleTargets[i];
			}
		}
		return bestTargetedUnitDamage;
	}
	public void AIDoOrder (AIPlayerMedium.PositionEvaluation order)
	{
		switch (order.bestOrder)
		{
		case UnitOrderOptions.Attack:
		{
			ChangeState(UnitState.AwaitingOrder, UnitState.TargetingUnit);
			AttackableObject temp;
			AISelectBestAttackObject(awaitingOrdersBlock, out temp);
			if(temp is UnitController)
			{
				ExchangeFire((UnitController)temp);
			}
			else
			{
				ExchangeFire((Property)temp);
			}
			if(health > 0)
			{
				ChangeState(UnitState.TargetingUnit, UnitState.FinishedMove);
			}
			break;
		}
		case UnitOrderOptions.GeneralPower:
		{
			owner.selectedGeneral.EnterPower();
			ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.Capture:
		{
			if(awaitingOrdersBlock.HasProperty())
			{
				awaitingOrdersBlock.occupyingProperty.Capture(Mathf.RoundToInt((float)health.PrettyHealth()*captureRate), this);
			}
			ChangeState(currentState, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.EndTurn:
		{
			ChangeState(currentState, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.BuildBridge:
		{
			ChangeState(UnitState.AwaitingOrder, UnitState.BuildingBridge);
			AIBuildBridge();
			ChangeState(UnitState.BuildingBridge, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.Board:
		{
			InGameController.GetPlayer(0).DeleteUnit(awaitingOrdersBlock.occupyingUnit);
			owner.AddUnit(awaitingOrdersBlock.occupyingUnit);
			owner.DeleteUnit(this);
			break;
		}
		case UnitOrderOptions.Stealthify:
		{
			isStealthed = true;
			InternalEndTurn();
			break;
		}
		case UnitOrderOptions.UnStealthify:
		{
			isStealthed = false;
			InternalEndTurn();
			break;
		}
		case UnitOrderOptions.Load:
		{
			awaitingOrdersBlock.occupyingUnit.carriedUnits.Add(this);
			isInUnit = true;
			currentBlock.UnOccupy(this);
			currentBlock = awaitingOrdersBlock = null;
			if(owner.currentGeneralUnit == this)
			{
				owner.selectedGeneral.HideZone();
			}
			EndTurn();
			SetActive(false);
			break;
		}
		case UnitOrderOptions.Repair:
		{
			for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
			{
				if(awaitingOrdersBlock.adjacentBlocks[i].HasProperty() && awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.GetOwner().IsSameSide(owner))
				{
					if(awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.justBuilt)
					{
						
					}
					else if(awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.isUnderConstruction)
					{
						awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.FinishConstruction();
					}
					else
					{
						awaitingOrdersBlock.adjacentBlocks[i].occupyingProperty.HealBase(5);
					}
				}
			}
			ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.Supply:
		{
			for(int i = 0; i < awaitingOrdersBlock.adjacentBlocks.Length; i++)
			{
				if(awaitingOrdersBlock.adjacentBlocks[i].IsOccupied() && awaitingOrdersBlock.adjacentBlocks[i].occupyingUnit.GetOwner().IsSameSide(owner))
				{
					awaitingOrdersBlock.adjacentBlocks[i].occupyingUnit.Resupply();
				}
			}
			ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.Unload:
		{
			ChangeState(UnitState.AwaitingOrder, UnitState.Unloading);
			AIUnload();
			ChangeState(UnitState.Unloading, UnitState.FinishedMove);
			break;
		}
		case UnitOrderOptions.AddGeneral:
		{
			owner.SendOutGeneral(this);
			if(veteranStatus != UnitRanks.Elite)
			{
				veteranStatus = UnitRanks.Sergeant;
			}
			ChangeState(UnitState.AwaitingOrder, UnitState.FinishedMove);
			break;
		}
		}
	}
	void AIBuildBridge()
	{
		
	}
	void AIUnload()
	{
		
	}
	public void AISelect(){
		ChangeState(UnitState.UnMoved, UnitState.Selected);
	}
	public void AIMoveTo (TerrainBlock block)
	{
		GeneratePath(block);
		if(currentMoveBlocks != null)
		{
			if(currentMoveBlocks.Count <= 1)
			{
				ChangeState(UnitState.Selected, UnitState.AwaitingOrder);
			}
			else
			{
				ChangeState(UnitState.Selected, UnitState.Moving);
			}
		}
	}
	
	public Health GetHealth ()
	{
		return health;
	}

	public UnitNames GetUnitClass ()
	{
		return unitClass;
	}

	public bool NeedsResupply ()
	{
		if(GetNormalizedAmmo() < .25f || GetNormalizedFuel() < .25f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public static int CompareBySupplyScore(UnitController a, UnitController b)
	{
		if(a.GetNormalizedAmmo() + a.GetNormalizedFuel() < b.GetNormalizedAmmo() + b.GetNormalizedFuel())
		{
			return -1;
		}
		else
		{
			return 1;
		}
	}
	public static int CompareByMovePriority(UnitController a, UnitController b)
	{
		if(a.AIMovePriority < b.AIMovePriority)
		{
			return -1;
		}
		else if(a.AIMovePriority > b.AIMovePriority)
		{
			return 1;
		}
		return 0;
	}
	/// <summary>
	/// Compares on random Index
	/// </summary>
	public int CompareTo(System.Object other)
	{
		float a = UnityEngine.Random.value;
		float b = UnityEngine.Random.value;
		if(a < b)
		{
			return -1;
		}
		else if(a > b)
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}
}
