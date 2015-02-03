using UnityEngine;
using System.Collections;

public class TerrainGameViewer : MonoBehaviour {
	public UnityEngine.UI.Text nameText, defenseText;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	/// <summary>
	/// Sets the values of the Gui box. Params can be anything with a tostring method
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="funds">Funds.</param>
	/// <param name="propertiesCount">Properties count.</param>
	/// <param name="unitsCount">Units count.</param>
	public void SetValues(string name, string defense){
		nameText.text = name;
		defenseText.text = defense;
	}
}
