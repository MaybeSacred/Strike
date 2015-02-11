using UnityEngine;
using System.Collections;

public class SkirmishEndMenuPlayer : MonoBehaviour
{
	public UnityEngine.UI.Text nameText, fundsGathered, fundsSpent, unitsCreated, unitsLost;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	public void SetText (PlayerInGameStatistics pig)
	{
		SetText (pig.name,
		        pig.totalFundsGathered.ToString (),
		        pig.totalFundsSpent.ToString (),
		        pig.unitsCreated.ToString (),
		        pig.unitsLost.ToString ());
	}
	public void SetText (string name, string fundsGathered, string fundsSpent, string unitsCreated, string unitsLost)
	{
		this.nameText.text = name;
		this.fundsGathered.text = fundsGathered;
		this.fundsSpent.text = fundsSpent;
		this.unitsCreated.text = unitsCreated;
		this.unitsLost.text = unitsLost;
	}
}
