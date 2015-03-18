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
public class CombinedForcesProduction
{
	public CombinedForcesProduction ()
	{
	}
	public List<ProductionEngine.ProductionRule> GetRules ()
	{
		List<ProductionEngine.ProductionRule> rules = new List<ProductionEngine.ProductionRule> ();
		rules.Add (CarrierRule);
		return rules;
	}
	List<Tuple<UnitName, float>> CarrierRule (Instance data, Player thisPlayer)
	{
		var outList = new List<Tuple<UnitName, float>> ();
		if (thisPlayer.funds > 15000) {
			outList.Add (new Tuple<UnitName, float> (UnitName.Carrier, .2f));
		}
		// A blocking to save up for at least one carrier when there are air units to be carried
		if (data.GetPlayerAirUnitCount () > 1 &&
			data.playerUnitCount [(int)UnitName.Carrier] < 1) {
			outList.Add (new Tuple<UnitName, float> (UnitName.Carrier, 1.25f));
		}
		return outList;
	}
}

