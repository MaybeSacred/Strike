using UnityEngine;
using System.Collections;

public class TerrainGameView : GameView
{
	public UnityEngine.UI.Text nameText, defenseText, totalDefenseText, currentWeather, turnField;
	/// <summary>
	/// Sets the values of the Gui box
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="funds">Funds.</param>
	/// <param name="propertiesCount">Properties count.</param>
	/// <param name="unitsCount">Units count.</param>
	public void SetValues (string name, string defense, string totalDefense)
	{
		nameText.text = name;
		defenseText.text = defense;
		totalDefenseText.text = totalDefense;
		currentWeather.text = InGameController.instance.weather.currentWeather.ToString ();
		turnField.text = "Turn: " + InGameController.instance.currentTurn.ToString ();
	}
}
