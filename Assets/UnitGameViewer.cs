using UnityEngine;
using System.Collections;

public class UnitGameViewer : MonoBehaviour {
	public UnityEngine.UI.Text nameText, healthText, ammoText, fuelText, carriedUnitText;
	public UnityEngine.UI.Image[] rankImages, 
		// Array of carried unit images
		carriedUnitImages;
	UnitRanks currentDisplayedUnitRank;
	int currentDisplayedCarriedCount;
	float animationTimer = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		animationTimer += Time.deltaTime;
		UpdateMultipleModifiers();
	}
	/// <summary>
	/// Sets the values of the Gui box
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="funds">Funds.</param>
	/// <param name="propertiesCount">Properties count.</param>
	/// <param name="unitsCount">Units count.</param>
	public void SetValues(string name, string health, string ammo, string fuel, UnitRanks rank, int carriedUnitCount){
		nameText.text = name;
		healthText.text = health;
		ammoText.text = ammo;
		fuelText.text = fuel;
		currentDisplayedUnitRank = rank;
		currentDisplayedCarriedCount = carriedUnitCount;
		if(rank == UnitRanks.UnRanked){
			HideRank();
		}
		if(carriedUnitCount == 0){
			HideCarriedUnits();
		}
	}
	void UpdateMultipleModifiers(){
		if(currentDisplayedUnitRank != UnitRanks.UnRanked && currentDisplayedCarriedCount > 0){
			if((Mathf.FloorToInt(animationTimer)/2) % 2 == 0){
				DisplayRank();
			}
			else {
				
			}
		}
		else if(currentDisplayedUnitRank != UnitRanks.UnRanked){
			DisplayRank();
		}
		else if(currentDisplayedCarriedCount > 0){
			
		}
	}
	/// <summary>
	/// Displays the number of units carried
	/// </summary>
	void DisplayUnitsCarried(){
		carriedUnitText.text = currentDisplayedCarriedCount.ToString();
	}
	/// <summary>
	/// Displays the proper rank image
	/// </summary>
	void DisplayRank(){
		for(int i = 1; i < System.Enum.GetValues(typeof(UnitRanks)).Length; i++){
			if((int)currentDisplayedUnitRank == i){
				rankImages[i - 1].gameObject.SetActive(true);
			}
			else {
				rankImages[i - 1].gameObject.SetActive(false);
			}
		}
	}
	/// <summary>
	/// Hides the carried units display
	/// </summary>
	void HideCarriedUnits(){
		
	}
	/// <summary>
	/// Hides all rank images
	/// </summary>
	void HideRank(){
		for(int i = 1; i < System.Enum.GetValues(typeof(UnitRanks)).Length; i++){
			rankImages[i - 1].gameObject.SetActive(false);
		}
	}
	public static string FormatSlashedString(string current, string max){
		return current + "/" + max;
	}
}
