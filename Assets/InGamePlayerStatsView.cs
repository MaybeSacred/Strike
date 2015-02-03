using UnityEngine;
using System.Collections;

public class InGamePlayerStatsView : MonoBehaviour {
	public UnityEngine.UI.Text playerName, funds, properties, units;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	/// <summary>
	/// Sets the values of the Gui box.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="funds">Funds.</param>
	/// <param name="propertiesCount">Properties count.</param>
	/// <param name="unitsCount">Units count.</param>
	public void SetValues(string name, string funds, string propertiesCount, string unitsCount){
		playerName.text = name;
		this.funds.text = funds;
		properties.text = propertiesCount;
		units.text = unitsCount;
	}
}
