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
		Instance instance = InGameController.CreateInstance (UnitName.Infantry, false);
		// Add naval rules if there are shipyards
		if (instance.neutralUnitCount [(int)UnitName.Shipyard] + instance.playerUnitCount [(int)UnitName.Shipyard] 
			+ instance.enemyAverageUnitCount [(int)UnitName.Shipyard] > 0) {
			NavalProduction np = new NavalProduction ();
			foreach (ProductionRule r in np.GetRules()) {
				rules.Add (r);
			}
		}
		// Add air rules if there are airports
		else if (instance.neutralUnitCount [(int)UnitName.Airport] + instance.playerUnitCount [(int)UnitName.Airport] 
			+ instance.enemyAverageUnitCount [(int)UnitName.Airport] > 0) {
			AirProduction ap = new AirProduction ();
			foreach (ProductionRule r in ap.GetRules()) {
				rules.Add (r);
			}
		}
		// Add ground rules if its a ground-only map
		else {
			GroundProductionSpecific gps = new GroundProductionSpecific ();
			foreach (ProductionRule r in gps.GetRules()) {
				rules.Add (r);
			}
		}
		// Initialize rules with some ground rules common to all maps
		GroundProductionGeneral gpg = new GroundProductionGeneral ();
		foreach (ProductionRule r in gpg.GetRules()) {
			rules.Add (r);
		}
	}
	/// <summary>
	/// A rule for the production engine. Returns a list of possible units to make given the counts of enemy units
	/// </summary>
	/// <param name="player">Player.</param>
	public delegate List<Tuple<UnitName, float>> ProductionRule (Instance data, Player thisPlayer);
	
	/// <summary>
	/// Evaluates the currently stored rules, selecting a best unit and returning it
	/// </summary>
	/// <param name="player">Player.</param>
	public UnitName Evaluate (Player player)
	{
		frequencyList = ZeroOut (frequencyList);
		// Compile a game state instance
		Instance instance = InGameController.CreateInstance (UnitName.Infantry, false);
		// Apply rules
		foreach (ProductionRule pr in rules) {
			List<Tuple<UnitName, float>> temp = pr.Invoke (instance, player);
			// Increase returned units in frequency list
			foreach (Tuple<UnitName, float> u in temp) {
				frequencyList [u.Item1] += u.Item2;
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
	/// Adds a production rule to the engine for evaluation. See <see cref="ProductionEngine.ProductionRule" />
	/// </summary>
	/// <param name="pr">Pr.</param>
	public void AddProductionRule (ProductionRule pr)
	{
		rules.Add (pr);
	}
}
