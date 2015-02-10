using UnityEngine;
using System.Collections;

public class PropertyGameView : GameView {
	public UnityEngine.UI.Text[] parameterText;
	/// <summary>
	/// Sets the values of the Gui box
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="funds">Funds.</param>
	/// <param name="propertiesCount">Properties count.</param>
	/// <param name="unitsCount">Units count.</param>
	public void SetValues(params string[] input){
		if(input.Length != parameterText.Length){
			throw new System.ArgumentException();
		}
		for(int i = 0; i < input.Length; i++){
			parameterText[i].text = input[i];
		}
	}
}
