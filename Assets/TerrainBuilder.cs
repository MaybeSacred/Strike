using UnityEngine;
using System.Collections.Generic;

public class TerrainBuilder : MonoBehaviour
{
	TerrainBlock[,] terrain;
	[HideInInspector]
	public float
		lowerXMapBound, upperXMapBound, lowerZMapBound, upperZMapBound;
	[HideInInspector]
	public List<TerrainBlock>
		illuminatedMovementRangeBlocks;
	private List<TerrainBlock> illuminatedMovementRangeBackup;
	public MapData data;
	// Use this for initialization
	void Start()
	{
		illuminatedMovementRangeBlocks = new List<TerrainBlock>();
		BuildAdjacencyLists();
		BuildAIHQDistances();
		data = CreateMapData(GetComponentsInChildren<TerrainBlock>(), Application.loadedLevelName);
	}
	public void SaveIlluminatedBlocks()
	{
		illuminatedMovementRangeBackup = new List<TerrainBlock>(illuminatedMovementRangeBlocks);
	}
	public void LoadIlluminatedBlocks()
	{
		illuminatedMovementRangeBlocks = new List<TerrainBlock>(illuminatedMovementRangeBackup);
	}
	public void BuildAdjacencyLists()
	{
		BuildTerrainArray();
		int i = 0;
		foreach (TerrainBlock t in terrain) {
			t.BuildAdjacencyList(i++);
		}
		SetReachability();
		ClearAllBlocks();
		if (Utilities.fogOfWarEnabled) {
			AllFogOn();
		} else {
			AllFogOff();
		}
	}
	public void ClearDiscovered()
	{
		foreach (TerrainBlock block in terrain) {
			block.discovered = false;
		}
	}
	void SetReachability()
	{
		foreach (TerrainBlock t in terrain) {
			t.SetReachabilityGraph();
		}
	}
	public void BuildAIHQDistances()
	{
		for (int i = 1; i < InGameController.NumberOfActivePlayers(); i++) {
			foreach (MovementType k in System.Enum.GetValues(typeof(MovementType))) {
				MinDistanceToTiles(k, InGameController.GetPlayer(i).hQBlock, 50000);
				foreach (TerrainBlock t in terrain) {
					t.SetDistanceToHQ(InGameController.GetPlayer(i), k);
				}
			}
		}
	}

	public TerrainBlock GetBlockAtPos(int x, int z)
	{
		if (x >= lowerXMapBound && x <= upperXMapBound && z >= lowerZMapBound && z <= upperZMapBound) {
			return terrain [x, z];
		}
		return null;
	}
	public TerrainBlock GetBlockAtPos(Vector3 point)
	{
		if (Mathf.Round(point.x) >= lowerXMapBound && Mathf.Round(point.x) <= upperXMapBound && Mathf.Round(point.z) >= lowerZMapBound && Mathf.Round(point.z) <= upperZMapBound) {
			return terrain [Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.z)];
		}
		return null;
	}
	void BuildTerrainArray()
	{
		TerrainBlock[] blocks = GetComponentsInChildren<TerrainBlock>();
		float largestX = 0;
		float largestZ = 0;
		for (int i = 0; i < blocks.Length; i++) {
			if (blocks [i].transform.position.x > largestX) {
				largestX = blocks [i].transform.position.x;
			}
			if (blocks [i].transform.position.z > largestZ) {
				largestZ = blocks [i].transform.position.z;
			}
		}
		lowerXMapBound = 0;
		upperXMapBound = largestX;
		lowerZMapBound = 0;
		upperZMapBound = largestZ;
		terrain = new TerrainBlock[Mathf.RoundToInt(largestX) + 1, Mathf.RoundToInt(largestZ) + 1];
		for (int i = 0; i < blocks.Length; i++) {
			if (terrain [Mathf.RoundToInt(blocks [i].transform.position.x), Mathf.RoundToInt(blocks [i].transform.position.z)] == null) {
				terrain [Mathf.RoundToInt(blocks [i].transform.position.x), Mathf.RoundToInt(blocks [i].transform.position.z)] = blocks [i];
			} else {
				throw new UnityException("Overlapping terrain blocks at " + Mathf.RoundToInt(blocks [i].transform.position.x) + " ," + Mathf.RoundToInt(blocks [i].transform.position.z) + "  " + terrain [(int)blocks [i].transform.position.x, (int)blocks [i].transform.position.z]);
			}
		}
	}
	// Update is called once per frame
	void Update()
	{
		
	}
	public static int ManhattanDistance(Vector3 v1, Vector3 v2)
	{
		return Mathf.RoundToInt(Mathf.Abs(v1.x - v2.x) + Mathf.Abs(v1.z - v2.z));
	}
	public static int ManhattanDistance(TerrainBlock v1, TerrainBlock v2)
	{
		return Mathf.RoundToInt(Mathf.Abs(v1.transform.position.x - v2.transform.position.x) + Mathf.Abs(v1.transform.position.z - v2.transform.position.z));
	}
	/// <summary>
	/// Returns null if no path OR a shortest path longer than maxDistance is found
	/// </summary>
	/// <returns>The between tiles.</returns>
	/// <param name="startTile">Start tile.</param>
	/// <param name="endTile">End tile.</param>
	/// <param name="maxDistance">Max distance.</param>
	public List<TerrainBlock> PathBetweenTiles(UnitController unit, TerrainBlock startTile, TerrainBlock endTile, float maxDistance, MovementType moveType, out float pathDist)
	{
		HashSet<TerrainBlock> closedSet = new HashSet<TerrainBlock>();
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		startTile.gCost = 0;
		startTile.fCost = ManhattanDistance(startTile.transform.position, endTile.transform.position);
		openSet.Enqueue(startTile);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Peek();
			if (current == endTile) {
				if (current.gCost > maxDistance) {
					pathDist = maxDistance + 1;
					return null;
				}
				pathDist = current.gCost;
				return ReconstructPath(current);
			}
			if (current.fCost > maxDistance) {
				pathDist = maxDistance + 1;
				return null;
			}
			openSet.Dequeue();
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				if (current.adjacentBlocks [i].UnitMovementCost(moveType) > 0) {
					if (!closedSet.Contains(current.adjacentBlocks [i]) && (!(current.adjacentBlocks [i].IsOccupied() && !(current.adjacentBlocks [i].occupyingUnit.GetOwner().IsSameSide(unit.GetOwner()) || current.adjacentBlocks [i].occupyingUnit.GetOwner().IsNeutralSide()) && current.adjacentBlocks [i].occupyingUnit.gameObject.activeSelf))) {
						float tempG = current.gCost + current.adjacentBlocks [i].UnitMovementCost(moveType);
						bool notContained = !openSet.Contains(current.adjacentBlocks [i]);
						if (notContained || tempG < current.adjacentBlocks [i].gCost) {
							current.adjacentBlocks [i].cameFromBlock = current;
							current.adjacentBlocks [i].gCost = tempG;
							current.adjacentBlocks [i].fCost = current.adjacentBlocks [i].gCost + ManhattanDistance(current.adjacentBlocks [i].transform.position, endTile.transform.position);
							if (notContained) {
								openSet.Enqueue(current.adjacentBlocks [i]);
							} else {
								openSet.Remove(current.adjacentBlocks [i]);
								openSet.Enqueue(current.adjacentBlocks [i]);
							}
						}
					}
				}
			}
		}
		pathDist = maxDistance + 1;
		return null;
	}
	public float MinDistanceToTile(TerrainBlock from, TerrainBlock to, UnitController inUnit)
	{
		HashSet<TerrainBlock> closedSet = new HashSet<TerrainBlock>();
		float pathDist;
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		from.gCost = 0;
		from.fCost = ManhattanDistance(from.transform.position, to.transform.position);
		openSet.Enqueue(from);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Peek();
			if (current == to) {
				pathDist = current.gCost;
				return pathDist;
			}
			openSet.Dequeue();
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				if (current.adjacentBlocks [i].UnitMovementCost(inUnit.moveClass) > 0) {
					if (!closedSet.Contains(current.adjacentBlocks [i])) {
						float tempG = current.gCost + current.adjacentBlocks [i].UnitMovementCost(inUnit.moveClass);
						bool notContained = !openSet.Contains(current.adjacentBlocks [i]);
						if (notContained || tempG < current.adjacentBlocks [i].gCost) {
							current.adjacentBlocks [i].cameFromBlock = current;
							current.adjacentBlocks [i].gCost = tempG;
							current.adjacentBlocks [i].fCost = current.adjacentBlocks [i].gCost + ManhattanDistance(current.adjacentBlocks [i].transform.position, to.transform.position);
							if (notContained) {
								openSet.Enqueue(current.adjacentBlocks [i]);
							} else {
								openSet.Remove(current.adjacentBlocks [i]);
								openSet.Enqueue(current.adjacentBlocks [i]);
							}
						}
					}
				}
			}
		}
		return float.PositiveInfinity;
	}
	public float MinDistanceToTileThroughUnMovableBlocks(TerrainBlock from, TerrainBlock to, UnitController inUnit, int movementReplacementCost)
	{
		HashSet<TerrainBlock> closedSet = new HashSet<TerrainBlock>();
		float pathDist;
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		from.gCost = 0;
		from.fCost = ManhattanDistance(from.transform.position, to.transform.position);
		openSet.Enqueue(from);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Peek();
			if (current == to) {
				pathDist = current.gCost;
				return pathDist;
			}
			openSet.Dequeue();
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				if (!closedSet.Contains(current.adjacentBlocks [i])) {
					float tempG = current.gCost + (current.adjacentBlocks [i].UnitMovementCost(inUnit.moveClass) > 0 ? current.adjacentBlocks [i].UnitMovementCost(inUnit.moveClass) : movementReplacementCost);
					bool notContained = !openSet.Contains(current.adjacentBlocks [i]);
					if (notContained || tempG < current.adjacentBlocks [i].gCost) {
						current.adjacentBlocks [i].cameFromBlock = current;
						current.adjacentBlocks [i].gCost = tempG;
						current.adjacentBlocks [i].fCost = current.adjacentBlocks [i].gCost + ManhattanDistance(current.adjacentBlocks [i].transform.position, to.transform.position);
						if (notContained) {
							openSet.Enqueue(current.adjacentBlocks [i]);
						} else {
							openSet.Remove(current.adjacentBlocks [i]);
							openSet.Enqueue(current.adjacentBlocks [i]);
						}
					}
				}
			}
		}
		return float.PositiveInfinity;
	}
	private TerrainBlock ClosestCoMoveableTile(UnitController unit, TerrainBlock startTile, UnitController otherUnit, float maxDistance, MovementType moveType)
	{
		List<TerrainBlock> closedSet = new List<TerrainBlock>();
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		startTile.gCost = 0;
		startTile.fCost = ManhattanDistance(startTile.transform.position, otherUnit.transform.position);
		openSet.Enqueue(startTile);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Peek();
			if (startTile.CanReachBlock(otherUnit, current)) {
				return current;
			}
			if (current.fCost > maxDistance) {
				return null;
			}
			openSet.Dequeue();
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				if (current.adjacentBlocks [i].UnitMovementCost(moveType) > 0) {
					if (!closedSet.Contains(current.adjacentBlocks [i]) && (!(current.adjacentBlocks [i].IsOccupied() && !(current.adjacentBlocks [i].occupyingUnit.GetOwner().IsSameSide(unit.GetOwner()) || current.adjacentBlocks [i].occupyingUnit.GetOwner().IsNeutralSide()) && current.adjacentBlocks [i].occupyingUnit.gameObject.activeSelf))) {
						float tempG = current.gCost + current.adjacentBlocks [i].UnitMovementCost(moveType);
						bool notContained = !openSet.Contains(current.adjacentBlocks [i]);
						if (notContained || tempG < current.adjacentBlocks [i].gCost) {
							current.adjacentBlocks [i].cameFromBlock = current;
							current.adjacentBlocks [i].gCost = tempG;
							current.adjacentBlocks [i].fCost = current.adjacentBlocks [i].gCost + ManhattanDistance(current.adjacentBlocks [i].transform.position, otherUnit.transform.position);
							if (notContained) {
								openSet.Enqueue(current.adjacentBlocks [i]);
							} else {
								openSet.Remove(current.adjacentBlocks [i]);
								openSet.Enqueue(current.adjacentBlocks [i]);
							}
						}
					}
				}
			}
		}
		return null;
	}
	private void MinDistanceToTiles(MovementType moveType, TerrainBlock startTile, float maxDistance)
	{
		List<TerrainBlock> closedSet = new List<TerrainBlock>();
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		startTile.gCost = 0;
		openSet.Enqueue(startTile);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Dequeue();
			if (ManhattanDistance(current.transform.position, startTile.transform.position) > maxDistance) {
				continue;
			}
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				if (current.adjacentBlocks [i].UnitMovementCost(moveType) > 0) {
					float tempG = current.gCost + current.adjacentBlocks [i].UnitMovementCost(moveType);
					if (tempG < current.adjacentBlocks [i].gCost) {
						current.adjacentBlocks [i].cameFromBlock = current;
						current.adjacentBlocks [i].gCost = tempG;
						if (!openSet.Contains(current.adjacentBlocks [i])) {
							openSet.Enqueue(current.adjacentBlocks [i]);
						} else {
							openSet.Remove(current.adjacentBlocks [i]);
							openSet.Enqueue(current.adjacentBlocks [i]);
						}
					}
				}
			}
		}
	}
	/// <summary>
	/// Calculates Minimum paths, while assigning a cost of 1 to blocks that the unit cannot normally move on
	/// </summary>
	/// <param name="moveType">Move type.</param>
	/// <param name="startTile">Start tile.</param>
	/// <param name="maxDistance">Max distance.</param>
	private void MinDistanceToTilesIgnoreIllegalBlocks(MovementType moveType, TerrainBlock startTile, float maxDistance)
	{
		List<TerrainBlock> closedSet = new List<TerrainBlock>();
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		startTile.gCost = 0;
		openSet.Enqueue(startTile);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Dequeue();
			if (ManhattanDistance(current.transform.position, startTile.transform.position) > maxDistance) {
				continue;
			}
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				float tempG = current.gCost + (current.adjacentBlocks [i].UnitMovementCost(moveType) > 0 ? current.adjacentBlocks [i].UnitMovementCost(moveType) : 1);
				if (tempG < current.adjacentBlocks [i].gCost) {
					current.adjacentBlocks [i].cameFromBlock = current;
					current.adjacentBlocks [i].gCost = tempG;
					if (!openSet.Contains(current.adjacentBlocks [i])) {
						openSet.Enqueue(current.adjacentBlocks [i]);
					} else {
						openSet.Remove(current.adjacentBlocks [i]);
						openSet.Enqueue(current.adjacentBlocks [i]);
					}
				}
			}
		}
	}
	public void SetDistancesFromBlockSharedMoveableBlock(UnitController unit, TerrainBlock startTile, AttackableObject aITarget)
	{
		TerrainBlock closestBlock = ClosestCoMoveableTile(unit, unit.currentBlock, (UnitController)aITarget, 50000f, unit.moveClass);
		if (closestBlock != null) {
			MinDistanceToTiles(unit.moveClass, closestBlock, 50000);
		}
		foreach (TerrainBlock t in terrain) {
			t.cachedGCost = t.gCost;
		}
	}

	public void SetDistancesFromBlockIgnoreIllegalBlocks(UnitController unit, TerrainBlock startTile)
	{
		MinDistanceToTilesIgnoreIllegalBlocks(unit.moveClass, startTile, 50000);
		foreach (TerrainBlock t in terrain) {
			t.cachedGCost = t.gCost;
		}
	}

	public void SetDistancesFromBlock(UnitController unit, TerrainBlock startTile)
	{
		MinDistanceToTiles(unit.moveClass, startTile, 50000);
		foreach (TerrainBlock t in terrain) {
			t.cachedGCost = t.gCost;
		}
	}
	/// <summary>
	/// Returns null if no path OR a shortest path longer than maxDistance is found
	/// </summary>
	/// <returns>The distance between tiles.</returns>
	/// <param name="startTile">Start tile.</param>
	/// <param name="endTile">End tile.</param>
	/// <param name="maxDistance">Max distance.</param>
	public List<TerrainBlock> MinDistanceToTiles(UnitController unit, TerrainBlock startTile, float maxDistance)
	{
		List<TerrainBlock> closedSet = new List<TerrainBlock>();
		PriorityQueues.PriorityQueue<TerrainBlock> openSet = new PriorityQueues.PriorityQueue<TerrainBlock>();
		foreach (TerrainBlock block in terrain) {
			block.gCost = block.fCost = float.PositiveInfinity;
			block.cameFromBlock = null;
		}
		startTile.gCost = 0;
		openSet.Enqueue(startTile);
		List<TerrainBlock> outList = new List<TerrainBlock>();
		outList.Add(startTile);
		TerrainBlock current;
		while (openSet.Count() > 0) {
			current = openSet.Dequeue();
			if (ManhattanDistance(current.transform.position, startTile.transform.position) > maxDistance) {
				continue;
			}
			closedSet.Add(current);
			for (int i = 0; i < current.adjacentBlocks.Length; i++) {
				if (current.adjacentBlocks [i].UnitMovementCost(unit.moveClass) > 0) {
					if (!(current.adjacentBlocks [i].IsOccupied() && !(current.adjacentBlocks [i].occupyingUnit.GetOwner().IsSameSide(unit.GetOwner()) || current.adjacentBlocks [i].occupyingUnit.GetOwner().IsNeutralSide()) && current.adjacentBlocks [i].occupyingUnit.gameObject.activeSelf)) {
						float tempG = current.gCost + current.adjacentBlocks [i].UnitMovementCost(unit.moveClass);
						if (tempG < current.adjacentBlocks [i].gCost) {
							current.adjacentBlocks [i].cameFromBlock = current;
							current.adjacentBlocks [i].gCost = tempG;
							if (!openSet.Contains(current.adjacentBlocks [i])) {
								openSet.Enqueue(current.adjacentBlocks [i]);
								outList.Add(current.adjacentBlocks [i]);
							} else {
								openSet.Remove(current.adjacentBlocks [i]);
								openSet.Enqueue(current.adjacentBlocks [i]);
							}
						}
					}
				}
			}
		}
		return outList;
	}
	private List<TerrainBlock> ReconstructPath(TerrainBlock lastBlock)
	{
		List<TerrainBlock> outList = new List<TerrainBlock>();
		while (lastBlock != null) {
			outList.Add(lastBlock);
			lastBlock = lastBlock.cameFromBlock;
		}
		outList.Reverse();
		return outList;
	}
	
	public void IlluminatePossibleMovementBlocks(TerrainBlock startBlock, UnitController unit, float moveRange, int attackRange)
	{
		foreach (TerrainBlock tb in illuminatedMovementRangeBlocks) {
			tb.HideTileColor();
		}
		illuminatedMovementRangeBlocks.Clear();
		TerrainBlock[] tempList = MinDistanceToTiles(unit, startBlock, moveRange + attackRange).ToArray();
		for (int i = 0; i < tempList.Length; i++) {
			if (tempList [i].gCost <= moveRange) {
				tempList [i].DisplayMovementTile();
				for (int k = 0; k < tempList[i].adjacentBlocks.Length; k++) {
					if (tempList [i].adjacentBlocks [k].gCost > moveRange) {
						tempList [i].adjacentBlocks [k].PokeOnAttackGraphic(unit.moveClass, attackRange);
					}
				}
				illuminatedMovementRangeBlocks.Add(tempList [i]);
			}
		}
	}

	public void IlluminatePossibleSupportBlocks(TerrainBlock startBlock, UnitController unit, float moveRange)
	{
		foreach (TerrainBlock tb in illuminatedMovementRangeBlocks) {
			tb.HideTileColor();
		}
		illuminatedMovementRangeBlocks.Clear();
		TerrainBlock[] tempList = MinDistanceToTiles(unit, startBlock, moveRange).ToArray();
		for (int i = 0; i < tempList.Length; i++) {
			if (tempList [i].gCost <= moveRange) {
				tempList [i].DisplaySupportTile();
				illuminatedMovementRangeBlocks.Add(tempList [i]);
			}
		}
	}

	public List<TerrainBlock> MoveableBlocks(TerrainBlock startBlock, UnitController unit, float moveRange)
	{
		TerrainBlock[] tempList = MinDistanceToTiles(unit, startBlock, moveRange).ToArray();
		List<TerrainBlock> outList = new List<TerrainBlock>();
		for (int i = 0; i < tempList.Length; i++) {
			if (tempList [i].gCost <= moveRange) {
				outList.Add(tempList [i]);
			}
		}
		return outList;
	}
	public void IlluminatePossibleAttackBlocksRange(TerrainBlock startBlock, int minRange, int maxRange)
	{
		foreach (TerrainBlock tb in illuminatedMovementRangeBlocks) {
			tb.HideTileColor();
		}
		illuminatedMovementRangeBlocks.Clear();
		RangeTraverser(startBlock, minRange, maxRange, 1 << LayerMask.NameToLayer("Default"), RangeActionAttackMax);
	}
	public void IlluminatePossibleAttackBlocks(TerrainBlock startBlock, UnitController unit, float moveRange, int attackRange)
	{
		foreach (TerrainBlock tb in illuminatedMovementRangeBlocks) {
			tb.HideTileColor();
		}
		illuminatedMovementRangeBlocks.Clear();
		TerrainBlock[] tempList = MinDistanceToTiles(unit, startBlock, moveRange + attackRange).ToArray();
		for (int i = 0; i < tempList.Length; i++) {
			if (tempList [i].gCost <= moveRange) {
				tempList [i].DisplayAttackTile();
				for (int k = 0; k < tempList[i].adjacentBlocks.Length; k++) {
					if (tempList [i].adjacentBlocks [k].gCost > moveRange) {
						tempList [i].adjacentBlocks [k].PokeOnAttackGraphic(unit.moveClass, attackRange);
					}
				}
				illuminatedMovementRangeBlocks.Add(tempList [i]);
			}
		}
	}
	public void ClearMoveBlocks()
	{
		foreach (TerrainBlock tb in illuminatedMovementRangeBlocks) {
			tb.HideTileColor();
		}
		illuminatedMovementRangeBlocks.Clear();
	}
	public void ClearAllBlocks()
	{
		foreach (TerrainBlock tb in terrain) {
			tb.HideTileColor();
		}
	}
	
	delegate void RangeAction(TerrainBlock hit);
	
	void RangeActionFog(TerrainBlock hit)
	{
		hit.HideFog();
	}
	void RangeActionAirMovement(TerrainBlock hit)
	{
		hit.DisplayMovementTile();
	}
	void RangeActionAttackMax(TerrainBlock hit)
	{
		hit.DisplayAttackTile();
	}
	int FogTraverser(TerrainBlock startBlock, int distance, int layer, bool forceClear)
	{
		int startPointX = Mathf.RoundToInt(startBlock.transform.position.x);
		int startPointZ = Mathf.RoundToInt(startBlock.transform.position.z);
		startPointX -= distance;
		int blocksInZDirection = 1;
		int unitsFound = 0;
		for (int i = 0; i < 2*distance + 1; i++) {
			for (int k = 0; k < blocksInZDirection; k++) {
				if (startPointX >= lowerXMapBound && startPointX <= upperXMapBound && startPointZ - k >= lowerZMapBound && startPointZ - k <= upperZMapBound) {
					if (terrain [startPointX, startPointZ - k] != null) {
						TerrainBlock block = terrain [startPointX, startPointZ - k];
						if (!block.hidesInFogOfWar || !block.HasProperty() || ManhattanDistance(block.transform.position, startBlock.transform.position) < 2 || forceClear) {
							if (block.IsOccupied() && !(block.occupyingUnit.isStealthed && ManhattanDistance(block.transform.position, startBlock.transform.position) > 1)) {
								block.occupyingUnit.gameObject.SetActive(true);
								unitsFound++;
							}
							block.HideFog();
						}
					}
				}
			}
			startPointX++;
			if (i >= distance) {
				blocksInZDirection -= 2;
				startPointZ--;
			} else {
				blocksInZDirection += 2;
				startPointZ++;
			}
		}
		return unitsFound--;
	}
	void RangeTraverser(TerrainBlock startBlock, int minRange, int maxRange, int layer, RangeAction ra)
	{
		int startPointX = Mathf.RoundToInt(startBlock.transform.position.x);
		int startPointZ = Mathf.RoundToInt(startBlock.transform.position.z);
		startPointX -= maxRange;
		int blocksInZDirection = 1;
		for (int i = 0; i < 2*maxRange + 1; i++) {
			for (int k = 0; k < blocksInZDirection; k++) {
				if (ManhattanDistance(startBlock.transform.position, new Vector3(startPointX, 0, startPointZ - k)) >= minRange) {
					if (startPointX >= lowerXMapBound && startPointX <= upperXMapBound && startPointZ - k >= lowerZMapBound && startPointZ - k <= upperZMapBound) {
						if (terrain [startPointX, startPointZ - k] != null) {
							ra(terrain [startPointX, startPointZ - k]);
						}
					}
				}
			}
			startPointX++;
			if (i >= maxRange) {
				blocksInZDirection -= 2;
				startPointZ--;
			} else {
				blocksInZDirection += 2;
				startPointZ++;
			}
		}
	}
	public List<TerrainBlock> BlocksWithinRange(TerrainBlock startBlock, int minRange, int maxRange, UnitController querier)
	{
		int startPointX = Mathf.RoundToInt(startBlock.transform.position.x);
		int startPointZ = Mathf.RoundToInt(startBlock.transform.position.z);
		startPointX -= maxRange;
		List<TerrainBlock> outList = new List<TerrainBlock>();
		int blocksInZDirection = 1;
		for (int i = 0; i < 2*maxRange + 1; i++) {
			for (int k = 0; k < blocksInZDirection; k++) {
				if (ManhattanDistance(startBlock.transform.position, new Vector3(startPointX, 0, startPointZ - k)) >= minRange) {
					if (startPointX >= lowerXMapBound && startPointX <= upperXMapBound && startPointZ - k >= lowerZMapBound && startPointZ - k <= upperZMapBound) {
						if (terrain [startPointX, startPointZ - k] != null) {
							outList.Add(terrain [startPointX, startPointZ - k]);
						}
					}
				}
			}
			startPointX++;
			if (i >= maxRange) {
				blocksInZDirection -= 2;
				startPointZ--;
			} else {
				blocksInZDirection += 2;
				startPointZ++;
			}
		}
		return outList;
	}
	public List<AttackableObject> ObjectsWithinRange(TerrainBlock startBlock, int minRange, int maxRange, UnitController querier)
	{
		int startPointX = Mathf.RoundToInt(startBlock.transform.position.x);
		int startPointZ = Mathf.RoundToInt(startBlock.transform.position.z);
		startPointX -= maxRange;
		List<AttackableObject> outList = new List<AttackableObject>();
		int blocksInZDirection = 1;
		for (int i = 0; i < 2*maxRange + 1; i++) {
			for (int k = 0; k < blocksInZDirection; k++) {
				if (ManhattanDistance(startBlock.transform.position, new Vector3(startPointX, 0, startPointZ - k)) >= minRange) {
					if (startPointX >= lowerXMapBound && startPointX <= upperXMapBound && startPointZ - k >= lowerZMapBound && startPointZ - k <= upperZMapBound) {
						if (terrain [startPointX, startPointZ - k] != null) {
							TerrainBlock hitBlock = terrain [startPointX, startPointZ - k];
							if (hitBlock.IsOccupied() && hitBlock.occupyingUnit != querier) {
								outList.Add(hitBlock.occupyingUnit);
							} else if (hitBlock.HasProperty()) {
								outList.Add(hitBlock.occupyingProperty);
							}
						}
					}
				}
			}
			startPointX++;
			if (i >= maxRange) {
				blocksInZDirection -= 2;
				startPointZ--;
			} else {
				blocksInZDirection += 2;
				startPointZ++;
			}
		}
		return outList;
	}
	public int ClearFog(TerrainBlock startBlock, int distance, bool forceClear)
	{
		if (Utilities.fogOfWarEnabled) {
			return FogTraverser(startBlock, distance, 1 << LayerMask.NameToLayer("Default"), forceClear);
		}
		return 0;
	}
	
	public void AllFogOn()
	{
		if (Utilities.fogOfWarEnabled) {
			foreach (TerrainBlock block in terrain) {
				block.DisplayFog();
			}
		}
	}
	
	public void AllFogOff()
	{
		foreach (TerrainBlock block in terrain) {
			block.HideFog();
		}
	}
	public static MapData CreateMapData(TerrainBlock[] blocks, string mapName)
	{
		float largestX = 0;
		float largestZ = 0;
		for (int i = 0; i < blocks.Length; i++) {
			if (blocks [i].transform.position.x > largestX) {
				largestX = blocks [i].transform.position.x;
			}
			if (blocks [i].transform.position.z > largestZ) {
				largestZ = blocks [i].transform.position.z;
			}
		}
		MapData outgoingData = new MapData(mapName, Mathf.RoundToInt(largestX) + 1, Mathf.RoundToInt(largestZ) + 1);
		List<Vector3Serializer> HQLocations = new List<Vector3Serializer>();
		List<Property> properties = new List<Property>();
		List<TerrainObject> units = new List<TerrainObject>();
		RaycastHit hit;
		for (int i = 0; i < blocks.Length; i++) {
			if (outgoingData.mapData [Mathf.RoundToInt(blocks [i].transform.position.x)] [Mathf.RoundToInt(blocks [i].transform.position.z)] == null) {
				outgoingData.mapData [Mathf.RoundToInt(blocks [i].transform.position.x)] [Mathf.RoundToInt(blocks [i].transform.position.z)] = new TerrainObject(blocks [i].name, blocks [i].transform.position, blocks [i].transform.rotation);
				outgoingData.blockStatistics [(int)blocks [i].typeOfTerrain]++;
				if (blocks [i].HasProperty() || Physics.Raycast(new Vector3(Mathf.RoundToInt(blocks [i].transform.position.x), 100f, Mathf.RoundToInt(blocks [i].transform.position.z)), Vector3.down, out hit, 1000f, 1 << LayerMask.NameToLayer("PropertyLayer"))) {
					UnitNames propertyType = UnitNames.Infantry;
					if (hit.collider != null) {
						propertyType = hit.collider.GetComponent<Property>().propertyType;
						properties.Add(hit.collider.GetComponent<Property>());
					} else {
						propertyType = blocks [i].occupyingProperty.propertyType;
						properties.Add(blocks [i].occupyingProperty);
					}
					switch (propertyType) {
						case UnitNames.Headquarters:
							{
								HQLocations.Add(new Vector3Serializer(blocks [i].transform.position));
								outgoingData.maxPlayers++;
								break;
							}
						case UnitNames.City:
							{
								outgoingData.cities++;
								break;
							}
						case UnitNames.Factory:
							{
								outgoingData.factories++;
								break;
							}
						case UnitNames.Airport:
							{
								outgoingData.airports++;
								break;
							}
						case UnitNames.Shipyard:
							{
								outgoingData.shipyards++;
								break;
							}
						case UnitNames.ComTower:
							{
								outgoingData.comTowers++;
								break;
							}
					}
				}
				if (Physics.Raycast(new Vector3(Mathf.RoundToInt(blocks [i].transform.position.x), 100f, Mathf.RoundToInt(blocks [i].transform.position.z)), Vector3.down, out hit, 1000f, LayerMask.NameToLayer("UnitLayer"))) {
					outgoingData.isPreDeploy = true;
					units.Add(new TerrainObject(hit.collider.gameObject));
				}
			} else {
				throw new UnityException("Overlapping terrain blocks at " + Mathf.RoundToInt(blocks [i].transform.position.x) + " ," + Mathf.RoundToInt(blocks [i].transform.position.z) + "  " + outgoingData.mapData [(int)blocks [i].transform.position.x] [(int)blocks [i].transform.position.z]);
			}
		}
		outgoingData.HQLocations = HQLocations.ToArray();
		outgoingData.properties = new TerrainObject[properties.Count];
		for (int i = 0; i < outgoingData.properties.Length; i++) {
			outgoingData.properties [i] = new TerrainObject(properties [i].gameObject);
		}
		outgoingData.units = units.ToArray();
		outgoingData.blockStatistics = NormalizeVector(outgoingData.blockStatistics);
		return outgoingData;
	}
	
	static float[] NormalizeVector(float[] vec)
	{
		float magnitude = 0;
		for (int i = 0; i < vec.Length; i++) {
			magnitude += vec [i] * vec [i];
		}
		magnitude = Mathf.Sqrt(magnitude);
		for (int i = 0; i < vec.Length; i++) {
			vec [i] /= magnitude;
		}
		return vec;
	}
}
