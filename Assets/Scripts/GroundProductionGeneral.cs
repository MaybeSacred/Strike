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

class GroundProductionGeneral
{
	public GroundProductionGeneral ()
	{
			
	}
	public List<ProductionEngine.ProductionRule> GetRules ()
	{
		List<ProductionEngine.ProductionRule> rules = new List<ProductionEngine.ProductionRule> ();
		rules.Add (EarlyGroundRule);
		rules.Add (MidGroundRule);
		rules.Add (LateGroundRule);
		rules.Add (TankGroundRule);
		rules.Add (ResupplyTankRule);
		return rules;
	}
	/// <summary>
	/// A rule for the early ground game
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> EarlyGroundRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		// only activates if turn is less than 4
		if (InGameController.currentTurn <= 4) {
			// If we have decent amount of money, build some light vehicles
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.Stryker) as UnitController).baseCost + 
				(Utilities.GetPrefabFromUnitName (UnitName.Infantry) as UnitController).baseCost) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Stryker, 1));
				if (data.playerUnitCount [(int)UnitName.Infantry] > 1) {
					outList.Add (new Tuple<UnitName, float> (UnitName.Mortar, 1));
					outList.Add (new Tuple<UnitName, float> (UnitName.Stinger, 1));
					outList.Add (new Tuple<UnitName, float> (UnitName.Humvee, 1));
				}
				// Produce some early power units if we have the funds
				if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.FieldArtillery) as UnitController).baseCost +
					(Utilities.GetPrefabFromUnitName (UnitName.Infantry) as UnitController).baseCost) {
					outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, 1));
					outList.Add (new Tuple<UnitName, float> (UnitName.FieldArtillery, 1));
				}
			} else if (thisPlayer.funds >= 3000) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Mortar, 1));
			}
			outList.Add (new Tuple<UnitName, float> (UnitName.Infantry, 1));
		}
		return outList;
	}
	/// <summary>
	/// Adds specific cases for building tanks, effective more for ground-only maps
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> TankGroundRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (InGameController.currentTurn >= 5) {
			// If we're close to building a tank
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.LightTank) as UnitController).baseCost * .75f) {
				if (data.enemyAverageUnitCount [(int)UnitName.Infantry] > 3 || data.enemyAverageUnitCount [(int)UnitName.Mortar] > 2 ||
					data.enemyAverageUnitCount [(int)UnitName.Stinger] > 2) {
					// Build a light tank if we have fewer of them than medium tanks
					if (data.playerUnitCount [(int)UnitName.LightTank] <= data.playerUnitCount [(int)UnitName.MediumTank] - 1) {
						outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, 1));
					} else {
						// Only build a medium tank if we have some artillery support
						if (data.playerUnitCount [(int)UnitName.Rockets] > 0 || data.playerUnitCount [(int)UnitName.FieldArtillery] > 1) {
							outList.Add (new Tuple<UnitName, float> (UnitName.MediumTank, 1));
						}
					}
					// Build a tank if we have a lot of total infantry enemies
				} else if (data.enemyAverageUnitCount [(int)UnitName.Infantry] + data.enemyAverageUnitCount [(int)UnitName.Mortar] +
					data.enemyAverageUnitCount [(int)UnitName.Stinger] > 4) {
					outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, 1));
				}
			}
		}
		return outList;
	}
	/// <summary>
	/// Defines heuristics for building sniper units
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> SniperGroundRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		float averageHighPricedUnits = data.enemyAverageUnitCount [(int)UnitName.Rockets] + data.enemyAverageUnitCount [(int)UnitName.MediumTank]
			+ data.enemyAverageUnitCount [(int)UnitName.Missiles];
		
		if (averageHighPricedUnits > 0) {
			if (data.playerUnitCount [(int)UnitName.Sniper] < 2) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Sniper, .75f));
			}
			
			// If theres a lot of high-priced units, build more snipers
			if (averageHighPricedUnits > 3) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Sniper, .5f));
				if (averageHighPricedUnits > 5) {
					outList.Add (new Tuple<UnitName, float> (UnitName.Sniper, .33f));
				}
			}
		}
		return outList;
	}
	/// <summary>
	/// Builds a supply tank if there are fewer than 1 per 10 units
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> ResupplyTankRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (data.playerUnitCount [(int)UnitName.SupplyTank] < thisPlayer.units.Count / 10) {
			outList.Add (new Tuple<UnitName, float> (UnitName.SupplyTank, 1));
		}
		return outList;
	}
	/// <summary>
	/// A rule for radar production in fog-of-war
	/// </summary>
	/// <returns>A list of unitNames</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> FOWRadarRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		if (Utilities.fogOfWarEnabled) {
			if (InGameController.currentTurn > 2 && thisPlayer.units.Count / 8 >= 
				data.playerUnitCount [(int)UnitName.MobileRadar]) {
				outList.Add (new Tuple<UnitName, float> (UnitName.MobileRadar, 1));
			}
		}
		return outList;
	}
	/// <summary>
	/// A rule for early-mid ground game
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> MidGroundRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		// only activates between 2 and 10 turns in
		if (InGameController.currentTurn > 2 && InGameController.currentTurn < 10) {
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.LightTank) as UnitController).baseCost) {
				outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, 1));
			}
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.FieldArtillery) as UnitController).baseCost) {
				outList.Add (new Tuple<UnitName, float> (UnitName.FieldArtillery, 1));
			}
				
			// Build stronger infantry if there are several strong artillery units
			if (data.enemyAverageUnitCount [(int)UnitName.Rockets] + data.enemyAverageUnitCount [(int)UnitName.FieldArtillery] >= 1.5f) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Mortar, 1));
				outList.Add (new Tuple<UnitName, float> (UnitName.Stinger, 1));
			}
				
			// Build more infantry if theres a lot of buildings left to capture and move them out
			if (data.mapData.cities + data.mapData.factories + data.mapData.airports + data.mapData.shipyards > 
				1.25f * (data.enemyAverageUnitCount [(int)UnitName.City] + data.enemyAverageUnitCount [(int)UnitName.Airport] + 
				data.enemyAverageUnitCount [(int)UnitName.Factory] + data.enemyAverageUnitCount [(int)UnitName.Shipyard] + 
				data.playerUnitCount [(int)UnitName.City] + data.playerUnitCount [(int)UnitName.Airport] + 
				data.playerUnitCount [(int)UnitName.Factory] + data.playerUnitCount [(int)UnitName.Shipyard])) {
				if (data.playerUnitCount [(int)UnitName.Infantry] <= 4) {
					outList.Add (new Tuple<UnitName, float> (UnitName.Infantry, .4f));
				}
				if (data.playerUnitCount [(int)UnitName.Stinger] <= 3) {
					outList.Add (new Tuple<UnitName, float> (UnitName.Stinger, .6f));
				}
				if (data.playerUnitCount [(int)UnitName.Mortar] <= 3) {
					outList.Add (new Tuple<UnitName, float> (UnitName.Mortar, .5f));
				}
			}
		}
		return outList;
	}
	/// <summary>
	/// A rule for late ground game
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	List<Tuple<UnitName, float>> LateGroundRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		// only activates after 6 turns in
		if (InGameController.currentTurn > 6) {
			if (thisPlayer.funds >= 10000) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Rockets, 1));
				outList.Add (new Tuple<UnitName, float> (UnitName.MediumTank, 1));
			}
			// Build units to counter triangle
			if (data.enemyAverageUnitCount [(int)UnitName.Rockets] + data.enemyAverageUnitCount [(int)UnitName.FieldArtillery] >= 
				data.enemyAverageUnitCount [(int)UnitName.MediumTank] + data.enemyAverageUnitCount [(int)UnitName.LightTank] - 1) {
				outList.Add (new Tuple<UnitName, float> (UnitName.Stinger, 1));
				if (data.enemyAverageUnitCount [(int)UnitName.FieldArtillery] > data.enemyAverageUnitCount [(int)UnitName.Rockets]) {
					outList.Add (new Tuple<UnitName, float> (UnitName.Mortar, 1));
				}
			} else if (data.enemyAverageUnitCount [(int)UnitName.MediumTank] + data.enemyAverageUnitCount [(int)UnitName.LightTank] >= 
				data.enemyAverageUnitCount [(int)UnitName.Mortar] + data.enemyAverageUnitCount [(int)UnitName.Stinger] - 2) {
				outList.Add (new Tuple<UnitName, float> (UnitName.FieldArtillery, 1));
			} else {
				outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, 1));
			}
		}
		return outList;
	}
}
