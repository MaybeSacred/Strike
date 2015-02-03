using UnityEngine;
using System.Collections.Generic;
public enum BLOCKDIRECTIONS{West, East, North, South};
public enum TERRAINTYPE{River, Mountain, Sea, Shoal, Road, Forest, Hill, Plain};
public class TerrainBlock : MonoBehaviour, System.IComparable<TerrainBlock> {
	//Stuff used for pathfinding
	[HideInInspector]
	public TerrainBlock cameFromBlock;
	[HideInInspector]
	public float gCost, fCost, cachedGCost;
	[HideInInspector]
	public TerrainBlock[] adjacentBlocks;
	[HideInInspector]
	public float minDistanceSoFar;
	private float[,] distancesToPlayerHQ;
	public MovementFuelCost[] terrainCosts;
	public int defenseBoost;
	public bool showingMoveTile = true, showingAttackTile, showingSupportTile, discovered;
	public static Color attackTileColor, moveTileColor, supportTileColor, offColor,
						fogOnColor, fogOffColor;
	public Transform beginningMoveSection, lastMoveSection, middleMoveStraightSection, middleMoveCornerSection;
	public bool hidesInFogOfWar;
	public int fogVisionBoost;
	public TERRAINTYPE typeOfTerrain;
	[System.Serializable]
	public class MovementFuelCost{
		public MovementType type;
		/// <summary>
		/// -1 if a move type cannot move on this terrain
		/// </summary>
		public float cost;
	}
	public bool randomizeOnStartup;
	public UnitController occupyingUnit{get; private set;}
	public Property occupyingProperty{get; private set;}
	private bool displayingFog = true;
	private Material fogMaterial, graphicOverlayMaterial;
	public string prettyName;
	bool displayUnitInfo;
	public string description;
	int[] reachability;
	void Awake()
	{
		reachability = new int[System.Enum.GetNames(typeof(MovementType)).Length];
	}
	// Use this for initialization
	void Start () {
		distancesToPlayerHQ = new float[InGameController.NumberOfActivePlayers(),System.Enum.GetValues(typeof(MovementType)).Length];
		minDistanceSoFar = int.MaxValue;
		beginningMoveSection.gameObject.SetActive(false);
		lastMoveSection.gameObject.SetActive(false);
		middleMoveCornerSection.gameObject.SetActive(false);
		middleMoveCornerSection.gameObject.SetActive(false);
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for(int i = 0; i < renderers.Length; i++)
		{
			if(renderers[i].name.Equals("Fog"))
			{
				fogMaterial = renderers[i].material;
				renderers[i].material = fogMaterial;
			}
			if(renderers[i].name.Equals(this.name))
			{
				graphicOverlayMaterial = renderers[i].material;
				renderers[i].material = graphicOverlayMaterial;
			}
		}
		if(randomizeOnStartup)
		{
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, Random.Range(0, 3)*90, transform.eulerAngles.z);
		}
		attackTileColor = new Color(1, 0.55f, 0.55f, 1);
		moveTileColor = new Color(0.55f, .55f, 1, 1);
		supportTileColor = new Color(.55f, 1, .55f, 1);
		offColor = new Color(0.8f, 0.8f, 0.8f, 1);
		fogOnColor = new Color(0.5f, 0.5f, 0.5f, .5f);
		fogOffColor = new Color(0, 0, 0, 0);
	}
	public void SetDistanceToHQ(Player player, MovementType moveType)
	{
		distancesToPlayerHQ[player.GetPlayerNumber(), (int)moveType] = gCost;
	}
	public float GetDistanceToHQ(Player player, MovementType moveType)
	{
		return distancesToPlayerHQ[player.GetPlayerNumber(), (int)moveType];
	}
	public void AttachProperty(Property prop)
	{
		occupyingProperty = prop;
		if(!prop.isUnderConstruction)
		{
			if(occupyingProperty.propertyType == UnitNames.Shipyard)
			{
				for(int i = 0; i < terrainCosts.Length; i++)
				{
					terrainCosts[i].cost = 1;
				}
			}
			else if(occupyingProperty.propertyType == UnitNames.Bridge)
			{
				hidesInFogOfWar = false;
				for(int i = 0; i < terrainCosts.Length; i++)
				{
					if(terrainCosts[i].type == MovementType.Sea || terrainCosts[i].type == MovementType.Littoral)
					{
						
					}
					else
					{
						terrainCosts[i].cost = 1;
					}
				}
			}
		}
	}
	public void DetachProperty(Property prop, bool temporaryDetach)
	{
		if(!temporaryDetach)
		{
			occupyingProperty = null;
		}
		if(prop.propertyType == UnitNames.Bridge)
		{
			if(name.Contains("Sea"))
			{
				SeaCosts();
			}
			else if(name.Contains("River"))
			{
				RiverCosts();
			}
		}
	}
	private void SeaCosts()
	{
		terrainCosts[(int)MovementType.Air].cost = 1;
		terrainCosts[(int)MovementType.Sea].cost = 1;
		terrainCosts[(int)MovementType.Littoral].cost = 1;
		terrainCosts[(int)MovementType.LightVehicle].cost = -1;
		terrainCosts[(int)MovementType.HeavyVehicle].cost = -1;
		terrainCosts[(int)MovementType.Tank].cost = -1;
		terrainCosts[(int)MovementType.Amphibious].cost = -1;
		terrainCosts[(int)MovementType.Sniper].cost = -1;
		terrainCosts[(int)MovementType.Infantry].cost = -1;
	}
	private void RiverCosts()
	{
		terrainCosts[(int)MovementType.Air].cost = 1;
		terrainCosts[(int)MovementType.Sea].cost = -1;
		terrainCosts[(int)MovementType.Littoral].cost = 3;
		terrainCosts[(int)MovementType.LightVehicle].cost = -1;
		terrainCosts[(int)MovementType.HeavyVehicle].cost = -1;
		terrainCosts[(int)MovementType.Tank].cost = -1;
		terrainCosts[(int)MovementType.Amphibious].cost = 2;
		terrainCosts[(int)MovementType.Sniper].cost = 2;
		terrainCosts[(int)MovementType.Infantry].cost = 2;
	}
	public void BuildAdjacencyList(int inReachability)
	{
		for(int i = 0; i < reachability.Length; i++)
		{
			reachability[i] = inReachability;
		}
		RaycastHit hit;
		List<TerrainBlock> temp = new List<TerrainBlock>();
		Vector3 offset = Vector3.zero;
		offset.y = 1;
		offset.x = 1;
		if(Physics.Raycast(transform.position + offset, Vector3.down, out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
		{
			temp.Add(hit.collider.GetComponent<TerrainBlock>());
		}
		offset.x = -1;
		if(Physics.Raycast(transform.position + offset, Vector3.down, out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
		{
			temp.Add(hit.collider.GetComponent<TerrainBlock>());
		}
		offset.x = 0;
		offset.z = 1;
		if(Physics.Raycast(transform.position + offset, Vector3.down, out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
		{
			temp.Add(hit.collider.GetComponent<TerrainBlock>());
		}
		offset.z = -1;
		if(Physics.Raycast(transform.position + offset, Vector3.down, out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
		{
			temp.Add(hit.collider.GetComponent<TerrainBlock>());;
		}
		adjacentBlocks = temp.ToArray();
	}
	public void SetReachabilityGraph()
	{
		for(int i = 0; i < reachability.Length; i++)
		{
			Stack<TerrainBlock> stack = new Stack<TerrainBlock>();
			InGameController.currentTerrain.ClearDiscovered();
			stack.Push(this);
			TerrainBlock current = null;
			while(stack.Count > 0)
			{
				current = stack.Pop();
				if(!current.discovered)
				{
					current.discovered = true;
					foreach(TerrainBlock other in current.adjacentBlocks)
					{
						if(current.terrainCosts[i].cost > 0 && other.terrainCosts[i].cost > 0)
						{
							current.reachability[i] = other.reachability[i] = Mathf.Min(current.reachability[i], other.reachability[i]);
						}
						stack.Push(other);
					}
				}
			}
		}
	}
	public bool CanReachBlock(UnitController querier, TerrainBlock other)
	{
		if(reachability[(int)querier.moveClass] == other.reachability[(int)querier.moveClass])
		{
			return true;
		}
		return false;
	}
	public void ShowTerrainBlockInfo()
	{
		GUI.BeginGroup(new Rect(Screen.width - UnitController.infoBoxWidth, 0, UnitController.infoBoxWidth, UnitController.infoBoxHeight));
		GUI.Box(new Rect(0, 0, UnitController.infoBoxWidth, UnitController.infoBoxHeight), "");
		GUI.Label(new Rect(0, 0, UnitController.infoBoxWidth, 20), new GUIContent(prettyName));
		GUI.Label(new Rect(0, 20, UnitController.infoBoxWidth, 20), new GUIContent("Defense: " + defenseBoost, "Defense Boost"));
		GUI.EndGroup();
	}
	public void ShowDetailedInfo()
	{
		GUI.BeginGroup(new Rect(Screen.width/2 - UnitController.infoBoxWidth, Screen.height/2 - UnitController.infoBoxHeight, 2 * UnitController.infoBoxWidth, 2 * UnitController.infoBoxHeight));
		GUI.Box(new Rect(0, 0, 2 * UnitController.infoBoxWidth, 2 * UnitController.infoBoxHeight), "");
		GUI.Label(new Rect(2, 2, 2 * UnitController.infoBoxWidth, 2 * UnitController.infoBoxHeight), description);
		GUI.EndGroup();
	}
	void OnMouseEnter()
	{
		if(IsOccupied())
		{
			occupyingUnit.OnMouseEnterExtra();
		}
	}
	void OnMouseOver()
	{
		if(IsOccupied())
		{
			occupyingUnit.OnMouseOverExtra();
		}
		else if(HasProperty())
		{
			occupyingProperty.OnMouseOverExtra();
		}
	}
	void OnMouseUp()
	{
		if(IsOccupied())
		{
			occupyingUnit.OnMouseUpExtra();
		}
		else if(HasProperty())
		{
			occupyingProperty.OnMouseUpExtra();
		}
		Utilities.selectedBlock = this;
	}
	void OnMouseExit()
	{
		if(occupyingUnit != null)
		{
			occupyingUnit.OnMouseExitExtra();
		}
		if(occupyingProperty != null)
		{
			occupyingProperty.OnMouseExitExtra();
		}
	}
	public bool HasProperty()
	{
		if(occupyingProperty != null)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	public float UnitMovementCost(MovementType moveType)
	{
		return terrainCosts[(int)moveType].cost;
	}
	public void Occupy(UnitController occupier)
	{
		if(occupyingUnit == null || occupyingUnit == occupier)
		{
			occupyingUnit = occupier;
			if(fogVisionBoost != 0)
			{
				occupyingUnit.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.TerrainMod, fogVisionBoost);
			}
			if(occupyingProperty != null)
			{
				occupyingProperty.occupyingUnit = occupier;
			}
		}
		else
		{
			Debug.Break();
			throw new UnityException("Already occupied block");
		}
	}
	public bool IsOccupied()
	{
		return occupyingUnit != null;
	}
	public void UnOccupy(UnitController occupier)
	{
		if(occupier != occupyingUnit)
		{
			Debug.Log("Error with occupying blocks " + occupier.name);
		}
		occupier.modifier.RemoveAllOfModifierType(UnitPropertyModifier.ModifierTypes.TerrainMod);
		occupyingUnit = null;
		if(occupyingProperty != null)
		{
			occupyingProperty.UnOccupy();
		}
	}
	public void DisplayMovementTile()
	{
		graphicOverlayMaterial.color = moveTileColor;
		showingMoveTile = true;
	}
	public void DisplayAttackTile()
	{
		showingAttackTile = true;
		InGameController.currentTerrain.illuminatedMovementRangeBlocks.Add(this);
		graphicOverlayMaterial.color = attackTileColor;
	}
	public void DisplaySupportTile()
	{
		graphicOverlayMaterial.color = supportTileColor;
		showingSupportTile = true;
	}
	public void HideTileColor()
	{
		showingAttackTile = showingMoveTile = showingSupportTile = false;
		graphicOverlayMaterial.color = offColor;
		minDistanceSoFar = 10000;
	}
	public void DisplayFog()
	{
		displayingFog = true;
		fogMaterial.color = fogOnColor;
	}
	public void HideFog()
	{
		displayingFog = false;
		fogMaterial.color = fogOffColor;
		if(IsOccupied())
		{
			occupyingUnit.gameObject.SetActive(true);
			occupyingUnit.moveIndicatorParticles.gameObject.SetActive(true);
		}
	}
	public void PokeFogOn()
	{
		if(!displayingFog)
		{
			DisplayFog();
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				adjacentBlocks[i].PokeFogOn();
			}
		}
	}
	public static void HideMovementPath(List<TerrainBlock> list)
	{
		foreach(TerrainBlock tb in list)
		{
			tb.lastMoveSection.gameObject.SetActive(false);
			tb.middleMoveCornerSection.gameObject.SetActive(false);
			tb.middleMoveStraightSection.gameObject.SetActive(false);
			tb.beginningMoveSection.gameObject.SetActive(false);
		}
	}
	public void DisplayMovementPath(List<TerrainBlock> list)
	{
		if(list.Count > 1)
		{
			this.DisplayBeginningMoveSection(list[1]);
			for(int i = 1; i < list.Count -1; i++)
			{
				list[i].DisplayMiddleMoveSection(list[i-1], list[i+1]);
			}
			list[list.Count -1].DisplayLastMoveSection(list[list.Count - 2]);
		}
		else
		{
			list[0].beginningMoveSection.gameObject.SetActive(false);
		}
	}
	void DisplayBeginningMoveSection(TerrainBlock next)
	{
		beginningMoveSection.gameObject.SetActive(true);
		if((int)next.transform.position.x > (int)transform.position.x)
		{
			beginningMoveSection.eulerAngles = new Vector3(270, 0, 0);
		}
		else if((int)next.transform.position.x < (int)transform.position.x)
		{
			beginningMoveSection.eulerAngles = new Vector3(270, 180, 0);
		}
		else if((int)next.transform.position.z > (int)transform.position.z)
		{
			beginningMoveSection.eulerAngles = new Vector3(270, 270, 0);
		}
		else if((int)next.transform.position.z < (int)transform.position.z)
		{
			beginningMoveSection.eulerAngles = new Vector3(270, 90, 0);
		}
	}
	void DisplayMiddleMoveSection(TerrainBlock before, TerrainBlock next)
	{
		if((int)next.transform.position.x == (int)before.transform.position.x)
		{
			middleMoveStraightSection.gameObject.SetActive(true);
			middleMoveStraightSection.eulerAngles = new Vector3(0, 90, 0);
		}
		else if((int)next.transform.position.z == (int)before.transform.position.z)
		{
			middleMoveStraightSection.gameObject.SetActive(true);
			middleMoveStraightSection.eulerAngles = new Vector3(0, 0, 0);
		}
		else if((int)next.transform.position.z > (int)transform.position.z)
		{
			if((int)next.transform.position.x > (int)before.transform.position.x)
			{	
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 0, 90);
			}
			else
			{
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 90, 90);
			}
		}
		else if((int)next.transform.position.z < (int)transform.position.z)
		{
			if((int)next.transform.position.x < (int)before.transform.position.x)
			{	
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 180, 90);
			}
			else
			{
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 270, 90);
			}
		}
		else if((int)next.transform.position.x < (int)transform.position.x)
		{
			if((int)next.transform.position.z < (int)before.transform.position.z)
			{	
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 0, 90);
			}
			else
			{
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 270, 90);
			}
		}
		else if((int)next.transform.position.x > (int)transform.position.x)
		{
			if((int)next.transform.position.z > (int)before.transform.position.z)
			{	
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 180, 90);
			}
			else
			{
				middleMoveCornerSection.gameObject.SetActive(true);
				middleMoveCornerSection.eulerAngles = new Vector3(270, 90, 90);
			}
		}
	}
	void DisplayLastMoveSection(TerrainBlock before)
	{
		lastMoveSection.gameObject.SetActive(true);
		if((int)before.transform.position.x > (int)transform.position.x)
		{
			lastMoveSection.eulerAngles = new Vector3(270, 180, 0);
		}
		else if((int)before.transform.position.x < (int)transform.position.x)
		{
			lastMoveSection.eulerAngles = new Vector3(270, 0, 0);
		}
		else if((int)before.transform.position.z > (int)transform.position.z)
		{
			lastMoveSection.eulerAngles = new Vector3(270, 90, 0);
		}
		else if((int)before.transform.position.z < (int)transform.position.z)
		{
			lastMoveSection.eulerAngles = new Vector3(270, 270, 0);
		}
	}
	public void PokeFogOn(int distance)
	{
		if(distance >= 0)
		{
			distance--;
			DisplayFog();
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				adjacentBlocks[i].PokeFogOn(distance);
			}
		}
	}
	public void PokeFogOff(int distance)
	{
		if(distance >= 0)
		{
			distance--;
			HideFog();
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				adjacentBlocks[i].PokeFogOff(distance);
			}
		}
	}
	public void PokeOnAttackGraphic(MovementType moveType, int attackRange)
	{
		if(attackRange > 0)
		{
			DisplayAttackTile();
			--attackRange;
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				if(!adjacentBlocks[i].showingMoveTile)
				{
					adjacentBlocks[i].PokeOnAttackGraphic(moveType, attackRange);
				}
			}
		}
	}
	public void PokeOnMovementAttackGraphic(UnitController unit, int moveRange, int attackRange)
	{
		if(!showingMoveTile && TerrainBuilder.ManhattanDistance(transform.position, unit.transform.position) <= moveRange)
		{
			showingMoveTile = true;
			InGameController.currentTerrain.illuminatedMovementRangeBlocks.Add(this);
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				if(adjacentBlocks[i].UnitMovementCost(unit.moveClass) > 0)
				{
					if(adjacentBlocks[i].UnitMovementCost(unit.moveClass) + minDistanceSoFar < adjacentBlocks[i].minDistanceSoFar)
					{
						adjacentBlocks[i].minDistanceSoFar = adjacentBlocks[i].UnitMovementCost(unit.moveClass) + minDistanceSoFar;
					}
				}
			}
			float bestSoFar = 10000;
			int bestIndexSoFar = 0;
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				if(adjacentBlocks[i].UnitMovementCost(unit.moveClass) > 0 && adjacentBlocks[i].UnitMovementCost(unit.moveClass) < bestSoFar && !adjacentBlocks[i].showingMoveTile)
				{
					if(adjacentBlocks[i].IsOccupied() && adjacentBlocks[i].occupyingUnit.gameObject.activeSelf && !adjacentBlocks[i].occupyingUnit.GetOwner().IsSameSide(unit.GetOwner()) && DamageValues.CanAttackUnit(unit, adjacentBlocks[i].occupyingUnit))
					{
						
					}
					else
					{
						bestSoFar = adjacentBlocks[i].UnitMovementCost(unit.moveClass);
						bestIndexSoFar = i;
					}
				}
			}
			adjacentBlocks[bestIndexSoFar].PokeOnMovementAttackGraphic(unit, moveRange, attackRange);
		}
		/*for(int i = 0; i < 4; i++)
		{
			if(adjacentBlocks[i] != null && !adjacentBlocks[i].showingMoveTile)
			{
				if(attackRange >= 0)
				{
					adjacentBlocks[i].PokeOnAttackGraphic(unit.moveClass, attackRange);
				}
			}
		}*/
	}
	public void PokeOnAllAttackGraphic(UnitController unit, float moveRange, int attackRange)
	{
		if(!showingAttackTile)
		{
			//DisplayAttackTile();
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				if(adjacentBlocks[i].UnitMovementCost(unit.moveClass) > 0)
				{
					if(moveRange - adjacentBlocks[i].UnitMovementCost(unit.moveClass) >= 0)
					{
						adjacentBlocks[i].PokeOnAllAttackGraphic(unit, moveRange - adjacentBlocks[i].UnitMovementCost(unit.moveClass), attackRange);
					}
				}
			}
		}
		/*for(int i = 0; i < 4; i++)
		{
			if(adjacentBlocks[i] != null)
			{
				if(attackRange >= 0 && !adjacentBlocks[i].showingAttackTile)
				{
					adjacentBlocks[i].PokeOnAttackGraphic(unit.moveClass, attackRange);
				}
			}
		}*/
	}
	public void PokeOffMovementAttackGraphic()
	{
		if(showingAttackTile || showingMoveTile)
		{
			showingAttackTile = false;
			showingMoveTile = false;
			graphicOverlayMaterial.color = offColor;
			for(int i = 0; i < adjacentBlocks.Length; i++)
			{
				adjacentBlocks[i].PokeOffMovementAttackGraphic();
			}
		}
	}
	// Update is called once per frame
	void Update () {
		
	}
	public int CompareTo(TerrainBlock other)
	{
		if (this.fCost < other.fCost) return -1;
		else if (this.fCost > other.fCost) return 1;
		else return 0;
	}

	public void SetTerrainView (TerrainGameViewer terrainView)
	{
		terrainView.SetValues(prettyName, defenseBoost.ToString());
	}

}
