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
		rules.Add (TankRule);
		return rules;
	}
	List<Tuple<UnitName, float>> TankRule (Instance data, Player thisPlayer)
	{
		List<Tuple<UnitName, float>> outList = new List<Tuple<UnitName, float>> ();
		return outList;
	}
}

