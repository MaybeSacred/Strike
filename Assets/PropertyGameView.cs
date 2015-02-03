using UnityEngine;
using System.Collections;

public class PropertyGameView : MonoBehaviour {
	public UnityEngine.UI.Text[] parameterText;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
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
