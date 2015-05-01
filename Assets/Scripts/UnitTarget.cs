// //Created by Jon Tyson : jtyson3@gatech.edu
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class UnitTarget
{
	List<AttackableObject> _targets;
	/// <summary>
	/// The primary target.
	/// </summary>
	/// <value>The primary target.</value>
	public AttackableObject primaryTarget {
		get { return _targets.FirstOrDefault (); }
	}
	/// <summary>
	/// The block that the unit is currently moving towards, which is also the primaryTarget's block, if the unit can reach that block
	/// </summary>
	/// <value>The target block.</value>
	public TerrainBlock targetBlock{ get; private set; }
	/// <summary>
	/// Gets a value indicating whether this <see cref="UnitTarget"/>can reach the primary target's block.
	/// </summary>
	/// <value><c>true</c> if can reach target; otherwise, <c>false</c>.</value>
	public bool canReachTarget { get; private set; }
	/// <summary>
	/// Gets a value indicating whether this <see cref="UnitTarget"/>can attack the primary target's block.
	/// </summary>
	/// <value><c>true</c> if can attack target; otherwise, <c>false</c>.</value>
	public bool canAttackTarget { get; private set; }
	UnitController parent{ get; set; }
	public UnitTarget (UnitController parent)
	{
		this.parent = parent;
		_targets = new List<AttackableObject> ();
	}
	public void AddTarget (AttackableObject newTarget)
	{
		if (newTarget == null) {
			throw new ArgumentNullException ("newTarget");
		}
		if (!_targets.Contains (newTarget)) {
			_targets.Add (newTarget);
		}
	}
	/// <summary>
	/// Determines whether this instance has a target.
	/// </summary>
	/// <returns><c>true</c> if this instance has target; otherwise, <c>false</c>.</returns>
	public bool HasTarget ()
	{
		return primaryTarget != default(AttackableObject);
	}
	public override string ToString ()
	{
		return string.Format ("[UnitTarget: primaryTarget={0}, targetBlock={1}, canReachBlock={2}]", primaryTarget, targetBlock, canReachTarget);
	}
	/// <summary>
	/// Sets the terrain block states for the parent's target
	/// </summary>
	public void SetTerrainBlockStates ()
	{
		SetTargetBlock ();
		if (HasTarget ()) {
			if (canReachTarget) {
				InGameController.instance.currentTerrain.SetDistancesFromBlock (parent, targetBlock);
			} else {
				Debug.Log ("Can't reach target, has one");
				if (primaryTarget is UnitController) {
					InGameController.instance.currentTerrain.SetDistancesFromBlockSharedMovableBlock (parent, targetBlock, primaryTarget);
				} else {
					InGameController.instance.currentTerrain.SetDistancesFromBlockIgnoreIllegalBlocks (parent, targetBlock);
				}
			}
		} else {
			Debug.Log ("Has no target");
			InGameController.instance.currentTerrain.SetDistancesFromBlockIgnoreIllegalBlocks (parent, targetBlock);
		}
	}
	
	void HasNoReachableTarget ()
	{
		
	}
	
	bool CheckForReachableTarget (AttackableObject input)
	{
		// First check if we can reach the target directly
		if (primaryTarget.GetOccupyingBlock ().CanReachBlock (parent.moveClass, parent.currentBlock)) {
			canReachTarget = true;
			targetBlock = primaryTarget.GetOccupyingBlock ();
		}
		// Next check if we can attack the target. Note that its possible we may not be able to attack, but can reach a target
		foreach (TerrainBlock tb in InGameController.instance.currentTerrain.BlocksWithinRange(primaryTarget.GetOccupyingBlock(), parent.minAttackRange, parent.EffectiveAttackRange())) {
			if (tb.CanReachBlock (parent.moveClass, parent.currentBlock)) {
				canAttackTarget = true;
				targetBlock = tb;
				break;
			}
		}
		return canReachTarget || canAttackTarget;
	}
	/// <summary>
	/// Sets the target block, using targeted units
	/// </summary>
	void SetTargetBlock ()
	{
		canReachTarget = false;
		canAttackTarget = false;
		if (HasTarget ()) {
			_targets.First (CheckForReachableTarget);
			// if we cant attack or reach the target
			if (!canAttackTarget && !canReachTarget) {
				HasNoReachableTarget ();
			}
		} else {
			Debug.Log ("No Target");
			var tuple = InGameController.instance.ClosestEnemyHQ (parent.currentBlock, parent.moveClass, parent.owner);
			targetBlock = tuple.Item2;
			if (targetBlock.CanReachBlock (parent.moveClass, parent.currentBlock)) {
				canReachTarget = true;
			} else {
				canReachTarget = false;
			}
		}
		//Debug.Log(inUnit.AITarget.GetUnitClass());
		//finds a block that the inUnit can reach and attack, or attempts to obtain a taxiing unit to get to the target
		//it sets the inUnit targetblock to either the first, or the occupying block of the target
			
		//			
		//			inUnit.canReachTarget = canReach;
		//			if (canReach) {
		//				inUnit.AITargetBlock = reachableBlock;
		//			} else if (inUnit.moveClass == MovementType.Sea || inUnit.moveClass == MovementType.Littoral) {
		//				inUnit.AITargetBlock = inUnit.AITarget.GetOccupyingBlock ();
		//			} else {
		//				UnitController target = GetSupportUnit (inUnit);
		//				if (target == null) {
		//					SetTransportToMake (inUnit);
		//				} else {
		//					target.AITarget = inUnit;
		//					target.AITargetBlock = inUnit.currentBlock;
		//				}
		//				inUnit.AITargetBlock = inUnit.AITarget.GetOccupyingBlock ();
		//			}
		//		} 
		/*if(inUnit.AITarget != null){
			Debug.Log(inUnit.name + " " + inUnit.AITarget + " " + inUnit.AITarget.GetPosition());
			Debug.Log(inUnit.AITargetBlock.transform.position);
		}
		else{
			Debug.Log("Has no target: " + inUnit.name);
			Debug.Log(inUnit.AITargetBlock.transform.position);
		}*/
	}
}

