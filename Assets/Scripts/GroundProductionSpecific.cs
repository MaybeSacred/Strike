// //Created by Jon Tyson : jtyson79473@gmail.com
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

class GroundProductionSpecific
{
	public GroundProductionSpecific ()
	{
	}
	
	public List<ProductionEngine.ProductionRule> GetRules ()
	{
		List<ProductionEngine.ProductionRule> rules = new List<ProductionEngine.ProductionRule> ();
		rules.Add (TankGroundRule);
		return rules;
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
		if (InGameController.instance.currentTurn >= 5) {
			// If we're close to building a tank
			if (thisPlayer.funds >= (Utilities.GetPrefabFromUnitName (UnitName.LightTank) as UnitController).baseCost * .75f) {
				if (data.enemyAverageUnitCount [(int)UnitName.Infantry] > 3 || data.enemyAverageUnitCount [(int)UnitName.Mortar] > 2 ||
					data.enemyAverageUnitCount [(int)UnitName.Stinger] > 2) {
					// Build a light tank if we have fewer of them than medium tanks
					if (data.playerUnitCount [(int)UnitName.LightTank] <= data.playerUnitCount [(int)UnitName.MediumTank] - 1) {
						outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, .66f));
					} else {
						// Only build a medium tank if we have some artillery support
						if (data.playerUnitCount [(int)UnitName.Rockets] > 0 || data.playerUnitCount [(int)UnitName.FieldArtillery] > 1) {
							outList.Add (new Tuple<UnitName, float> (UnitName.MediumTank, 1));
						}
					}
					// Build a tank if we have a lot of total infantry enemies
				} else if (data.enemyAverageUnitCount [(int)UnitName.Infantry] + data.enemyAverageUnitCount [(int)UnitName.Mortar] +
					data.enemyAverageUnitCount [(int)UnitName.Stinger] > 5) {
					outList.Add (new Tuple<UnitName, float> (UnitName.LightTank, .66f));
				}
			}
		}
		return outList;
	}
}

