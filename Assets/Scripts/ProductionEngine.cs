// //Created by Jon Tyson : jtyson79473@gmail.com
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

public class ProductionEngine
{
	/// <summary>
	/// A collection of rules for the engine to evaluate
	/// </summary>
	List<ProductionRule> rules;
	/// <summary>
	/// A list of unitNames and their associated frequencies
	/// </summary>
	Dictionary<UnitName, float> frequencyList;
	public ProductionEngine ()
	{
		rules = new List<ProductionRule> ();
		frequencyList = new Dictionary<UnitName, float> ();
		foreach (UnitName u in System.Enum.GetValues(typeof(UnitName))) {
			frequencyList.Add (u, 0);
		}
		// Initialize rules with some common to all maps
		rules.Add (EarlyGroundRule);
		rules.Add (MidGroundRule);
		rules.Add (LateGroundRule);
		rules.Add (TankGroundRule);
		rules.Add (ResupplyTankRule);
	}
	/// <summary>
	/// A rule for the production engine. Returns a list of possible units to make given the counts of enemy units
	/// </summary>
	/// <param name="player">Player.</param>
	public delegate List<UnitName> ProductionRule (Instance data, Player thisPlayer);
	
	/// <summary>
	/// Evaluates the currently stored rules, selecting a best unit and returning it
	/// </summary>
	/// <param name="player">Player.</param>
	public UnitName Evaluate (Player player)
	{
		frequencyList = ZeroOut (frequencyList);
		// Apply rules
		foreach (ProductionRule pr in rules) {
			List<UnitName> temp = pr.Invoke (InGameController.CreateInstance (UnitName.Infantry, false), player);
			// Increase returned units in frequency list
			foreach (UnitName u in temp) {
				frequencyList [u]++;
			}
		}
		frequencyList = Normalize (frequencyList);
		UnitName one = SelectUnit (frequencyList);
		return one;
	}
	/// <summary>
	/// Returns a UnitNames which is selected from the provided Dictionary, 
	/// with higher probability given to units with higher frequencies
	/// </summary>
	/// <returns>The unit.</returns>
	/// <param name="dic">Dic.</param>
	UnitName SelectUnit (Dictionary<UnitName, float> dic)
	{
		float randomValue = UnityEngine.Random.value;
		float currentMinimum = 0;
		foreach (UnitName u in dic.Keys) {
			if (currentMinimum + dic [u] >= randomValue) {
				return u;
			} else {
				currentMinimum += dic [u];
			}
		}
		return UnitName.Headquarters;
	}
	/// <summary>
	/// Normalizes the values of a dictionary
	/// </summary>
	/// <param name="dic">Dic.</param>
	Dictionary<UnitName, float> Normalize (Dictionary<UnitName, float> dic)
	{
		float sum = 0;
		foreach (float value in dic.Values) {
			sum += value;
		}
		UnitName[] copy = new UnitName[dic.Count];
		dic.Keys.CopyTo (copy, 0);
		for (int i = 0; i < dic.Count; i++) {
			dic [copy [i]] /= sum;
		}
		return dic;
	}
	/// <summary>
	/// Zeros out the values stored in the frequency list
	/// </summary>
	/// <returns>The out.</returns>
	/// <param name="dic">Dic.</param>
	Dictionary<UnitName, float> ZeroOut (Dictionary<UnitName, float> dic)
	{
		UnitName[] copy = new UnitName[dic.Count];
		dic.Keys.CopyTo (copy, 0);
		for (int i = 0; i < dic.Count; i++) {
			dic [copy [i]] = 0;
		}
		return dic;
	}
	/// <summary>
	/// A rule for the early ground game
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	public List<UnitName> EarlyGroundRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		// only activates if turn is less than 4
		if (InGameController.currentTurn <= 4) {
			// If we have decent amount of money, build some light vehicles
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.Stryker) as UnitController).baseCost + 
				(Utilities.GetPrefabFromUnitName (UnitName.Infantry) as UnitController).baseCost) {
				outList.Add (UnitName.Stryker);
				if (data.playerUnitCount [(int)UnitName.Infantry] > 1) {
					outList.Add (UnitName.Mortar);
					outList.Add (UnitName.Stinger);
					outList.Add (UnitName.Humvee);
				}
				// Produce some early power units if we have the funds
				if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.FieldArtillery) as UnitController).baseCost +
					(Utilities.GetPrefabFromUnitName (UnitName.Infantry) as UnitController).baseCost) {
					outList.Add (UnitName.LightTank);
					outList.Add (UnitName.FieldArtillery);
				}
			} else if (thisPlayer.funds >= 3000) {
				outList.Add (UnitName.Mortar);
			}
			outList.Add (UnitName.Infantry);
		}
		return outList;
	}
	/// <summary>
	/// Adds specific cases for building tanks, effective more for ground-only maps
	/// </summary>
	/// <returns>The ground rule.</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	public List<UnitName> TankGroundRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		if (InGameController.currentTurn >= 5) {
			// If we're close to building a tank
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.LightTank) as UnitController).baseCost * .75f) {
				if (data.enemyAverageUnitCounts [(int)UnitName.Infantry] > 3 || data.enemyAverageUnitCounts [(int)UnitName.Mortar] > 2 ||
					data.enemyAverageUnitCounts [(int)UnitName.Stinger] > 2) {
					// Build a light tank if we have fewer of them than medium tanks
					if (data.playerUnitCount [(int)UnitName.LightTank] <= data.playerUnitCount [(int)UnitName.MediumTank] - 1) {
						outList.Add (UnitName.LightTank);
					} else {
						// Only build a medium tank if we have some artillery support
						if (data.playerUnitCount [(int)UnitName.Rockets] > 0 || data.playerUnitCount [(int)UnitName.FieldArtillery] > 1) {
							outList.Add (UnitName.MediumTank);
						}
					}
					// Build a tank if we have a lot of total infantry enemies
				} else if (data.enemyAverageUnitCounts [(int)UnitName.Infantry] + data.enemyAverageUnitCounts [(int)UnitName.Mortar] +
					data.enemyAverageUnitCounts [(int)UnitName.Stinger] > 4) {
					outList.Add (UnitName.LightTank);
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
	public List<UnitName> SniperGroundRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		if (data.enemyAverageUnitCounts [(int)UnitName.Rockets] > 0 || data.enemyAverageUnitCounts [(int)UnitName.Missiles] > 0 
			|| data.enemyAverageUnitCounts [(int)UnitName.MediumTank] > 0) {
			if (data.playerUnitCount [(int)UnitName.Sniper] < 2) {
				outList.Add (UnitName.Sniper);
			}
			// If theres alot of high-priced units, build more snipers
			if (data.enemyAverageUnitCounts [(int)UnitName.Rockets] + data.enemyAverageUnitCounts [(int)UnitName.MediumTank]
				+ data.enemyAverageUnitCounts [(int)UnitName.Missiles] > 3) {
				outList.Add (UnitName.Sniper);
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
	public List<UnitName> ResupplyTankRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		if (data.playerUnitCount [(int)UnitName.SupplyTank] < thisPlayer.units.Count / 10) {
			outList.Add (UnitName.SupplyTank);
		}
		return outList;
	}
	/// <summary>
	/// A rule for radar production in fog-of-war
	/// </summary>
	/// <returns>A list of unitNames</returns>
	/// <param name="data">Data.</param>
	/// <param name="thisPlayer">This player.</param>
	public List<UnitName> FOWRadarRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		if (Utilities.fogOfWarEnabled) {
			if (InGameController.currentTurn > 2 && thisPlayer.units.Count / 8 >= 
				data.playerUnitCount [(int)UnitName.MobileRadar]) {
				outList.Add (UnitName.MobileRadar);
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
	public List<UnitName> MidGroundRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		// only activates between 2 and 10 turns in
		if (InGameController.currentTurn > 2 && InGameController.currentTurn < 10) {
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.LightTank) as UnitController).baseCost) {
				outList.Add (UnitName.LightTank);
			}
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.FieldArtillery) as UnitController).baseCost) {
				outList.Add (UnitName.FieldArtillery);
			}
			
			// Build stronger infantry if there are several strong artillery units
			if (data.enemyAverageUnitCounts [(int)UnitName.Rockets] + data.enemyAverageUnitCounts [(int)UnitName.FieldArtillery] > 1.5f) {
				outList.Add (UnitName.Mortar);
			}
			if (data.enemyAverageUnitCounts [(int)UnitName.Rockets] + data.enemyAverageUnitCounts [(int)UnitName.FieldArtillery] > 1.5f) {
				outList.Add (UnitName.Stinger);
			}

			// Build more infantry if theres a lot of buildings left to capture and move them out
			if (data.mapData.cities + data.mapData.factories + data.mapData.airports + data.mapData.shipyards > 
				.7f * (data.enemyAverageUnitCounts [(int)UnitName.City] + data.enemyAverageUnitCounts [(int)UnitName.Airport] + 
				data.enemyAverageUnitCounts [(int)UnitName.Factory] + data.enemyAverageUnitCounts [(int)UnitName.Shipyard] + 
				data.playerUnitCount [(int)UnitName.City] + data.playerUnitCount [(int)UnitName.Airport] + 
				data.playerUnitCount [(int)UnitName.Factory] + data.playerUnitCount [(int)UnitName.Shipyard])) {
				if (data.playerUnitCount [(int)UnitName.Infantry] <= 4) {
					outList.Add (UnitName.Infantry);
				}
				if (data.playerUnitCount [(int)UnitName.Stinger] <= 3) {
					outList.Add (UnitName.Stinger);
				}
				if (data.playerUnitCount [(int)UnitName.Mortar] <= 3) {
					outList.Add (UnitName.Mortar);
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
	public List<UnitName> LateGroundRule (Instance data, Player thisPlayer)
	{
		List<UnitName> outList = new List<UnitName> ();
		// only activates between 2 and 10 turns in
		if (InGameController.currentTurn > 6) {
			if (thisPlayer.funds >= 10000) {
				outList.Add (UnitName.Rockets);
				outList.Add (UnitName.MediumTank);
			}
			// Build units to counter triangle
			if (data.enemyAverageUnitCounts [(int)UnitName.Rockets] + data.enemyAverageUnitCounts [(int)UnitName.FieldArtillery] >= 
				data.enemyAverageUnitCounts [(int)UnitName.MediumTank] + data.enemyAverageUnitCounts [(int)UnitName.LightTank] - 1) {
				outList.Add (UnitName.Stinger);
				if (data.enemyAverageUnitCounts [(int)UnitName.FieldArtillery] > data.enemyAverageUnitCounts [(int)UnitName.Rockets]) {
					outList.Add (UnitName.Mortar);
				}
			} else if (data.enemyAverageUnitCounts [(int)UnitName.MediumTank] + data.enemyAverageUnitCounts [(int)UnitName.LightTank] >= 
				data.enemyAverageUnitCounts [(int)UnitName.Mortar] + data.enemyAverageUnitCounts [(int)UnitName.Stinger] - 2) {
				outList.Add (UnitName.FieldArtillery);
			} else {
				outList.Add (UnitName.LightTank);
			}
		}
		return outList;
	}
	/// <summary>
	/// Adds a production rule to the engine for evaluation. See <see cref="ProductionEngine.ProductionRule" />
	/// </summary>
	/// <param name="pr">Pr.</param>
	public void AddProductionRule (ProductionRule pr)
	{
		rules.Add (pr);
	}
}
