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

class AirProduction
{
	public AirProduction ()
	{
	}
	public List<ProductionEngine.ProductionRule> GetRules ()
	{
		List<ProductionEngine.ProductionRule> rules = new List<ProductionEngine.ProductionRule> ();
		rules.Add (BomberRule);
		rules.Add (InterceptorRule);
		rules.Add (TacticalFighterRule);
		rules.Add (LiftCopterRule);
		rules.Add (AttackCopterRule);
		rules.Add (UAVRule);
		rules.Add (AerialInteractionRule);
		return rules;
	}
	List<Tuple<UnitName, float>> BomberRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (data.enemyAverageUnitCount [(int)UnitName.Interceptor] < data.playerUnitCount [(int)UnitName.Interceptor]) {
			outList.Add (new Tuple<UnitName, float> (UnitName.CarpetBomber, .16f));
		}
		if (data.playerUnitCount [(int)UnitName.TacticalFighter] + data.playerUnitCount [(int)UnitName.Interceptor] > 0) {
			outList.Add (new Tuple<UnitName, float> (UnitName.CarpetBomber, .2f));
		}
		if (data.enemyAverageUnitCount [(int)UnitName.MediumTank] + data.enemyAverageUnitCount [(int)UnitName.Missiles]
			+ data.enemyAverageUnitCount [(int)UnitName.Rockets] > 2) {
			outList.Add (new Tuple<UnitName, float> (UnitName.CarpetBomber, .45f));
		}
		return outList;
	}
	/// <summary>
	/// Rule for the production of interceptors
	/// </summary>
	/// <returns>The rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> InterceptorRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (data.enemyAverageUnitCount [(int)UnitName.CarpetBomber] >= 1.25f || 
			data.enemyAverageUnitCount [(int)UnitName.Interceptor] >= .5f || 
			data.enemyAverageUnitCount [(int)UnitName.TacticalFighter] >= 2f) {
			// If theres more enemy interceptors by alot
			if (data.enemyAverageUnitCount [(int)UnitName.Interceptor] > data.playerUnitCount [(int)UnitName.Interceptor] + 2) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Interceptor, .75f));
			} else if (data.playerUnitCount [(int)UnitName.Interceptor] < 1) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Interceptor, 1));
			} else {
				outList.Add (new Tuple<UnitName, float> (UnitName.Interceptor, .33f));
			}
		}
		return outList;
	}
	/// <summary>
	/// Rules for producing tactical fighters
	/// </summary>
	/// <returns>The fighter rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> TacticalFighterRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		// If theres alot of enemy interceptors, make interceptors instead
		if (data.enemyAverageUnitCount [(int)UnitName.Interceptor] >= 1.6f * data.playerUnitCount [(int)UnitName.Interceptor]) {
			outList.Add (new Tuple<UnitName, float> (UnitName.Interceptor, .33f));
		} else if (data.playerUnitCount [(int)UnitName.TacticalFighter] < 2) {
			outList.Add (new Tuple<UnitName, float> (UnitName.TacticalFighter, 1f));
		} else if (data.playerUnitCount [(int)UnitName.TacticalFighter] < 5) {
			outList.Add (new Tuple<UnitName, float> (UnitName.TacticalFighter, .33f));
		}
		return outList;
	}
	/// <summary>
	/// Production rule for lift copters
	/// </summary>
	/// <returns>The fighter rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> LiftCopterRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (InGameController.currentTurn < 5) {
			if (data.playerUnitCount [(int)UnitName.LiftCopter] < 2) {
				outList.Add (new Tuple<UnitName, float> (UnitName.LiftCopter, 1));
			}
		} else {
			if (data.neutralUnitCount [(int)UnitName.City] + data.neutralUnitCount [(int)UnitName.Airport] + 
				data.neutralUnitCount [(int)UnitName.Factory] + data.neutralUnitCount [(int)UnitName.Shipyard] > 4) {
				outList.Add (new Tuple<UnitName, float> (UnitName.LiftCopter, .33f));
			}
		}
		return outList;
	}
	/// <summary>
	/// Attack copter production rule
	/// </summary>
	/// <returns>The copter rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> AttackCopterRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (data.playerUnitCount [(int)UnitName.AttackCopter] < 2) {
			outList.Add (new Tuple<UnitName, float> (UnitName.AttackCopter, 1f));
		}
		return outList;
	}
	/// <summary>
	/// Rule for production of UAV
	/// </summary>
	/// <returns>The rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> UAVRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (Utilities.fogOfWarEnabled) {
			if (data.playerUnitCount [(int)UnitName.UAV] < 2) {
				outList.Add (new Tuple<UnitName, float> (UnitName.UAV, 1));
			}
		}
		return outList;
	}
	/// <summary>
	/// Defines interaction between aerial unit production
	/// </summary>
	/// <returns>The interaction rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> AerialInteractionRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		float averageEnemyAAUnits = data.enemyAverageUnitCount [(int)UnitName.AATank] + data.enemyAverageUnitCount [(int)UnitName.Missiles];
		if (averageEnemyAAUnits > 4) {
			outList.Add (new Tuple<UnitName, float> (UnitName.TacticalFighter, .07f * averageEnemyAAUnits));
			outList.Add (new Tuple<UnitName, float> (UnitName.CarpetBomber, -.03f * averageEnemyAAUnits));
		}
		float enemyInfantry = data.enemyAverageUnitCount [(int)UnitName.Infantry] + data.enemyAverageUnitCount [(int)UnitName.Mortar] +
			data.enemyAverageUnitCount [(int)UnitName.Stinger] + data.enemyAverageUnitCount [(int)UnitName.Sniper];
		float enemyVehicles = data.enemyAverageUnitCount [(int)UnitName.LightTank] + data.enemyAverageUnitCount [(int)UnitName.MediumTank] +
			data.enemyAverageUnitCount [(int)UnitName.Rockets];
		if (enemyInfantry > enemyVehicles + averageEnemyAAUnits + 5) {
			outList.Add (new Tuple<UnitName, float> (UnitName.AttackCopter, .05f * (enemyInfantry - enemyVehicles - averageEnemyAAUnits)));
			outList.Add (new Tuple<UnitName, float> (UnitName.TacticalFighter, -.05f * (enemyInfantry - enemyVehicles - averageEnemyAAUnits)));
		} else if (enemyInfantry < enemyVehicles + averageEnemyAAUnits - 3) {
			outList.Add (new Tuple<UnitName, float> (UnitName.AttackCopter, -.05f * (enemyInfantry - enemyVehicles - averageEnemyAAUnits)));
			outList.Add (new Tuple<UnitName, float> (UnitName.TacticalFighter, .05f * (enemyInfantry - enemyVehicles - averageEnemyAAUnits)));
		}
		return outList;
	}
}

