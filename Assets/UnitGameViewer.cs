using UnityEngine;
using System.Collections;

public class UnitGameViewer : MonoBehaviour {
	public UnityEngine.UI.Text nameText, healthText, ammoText, fuelText;
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
		UpdateMultipleGraphics();
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
		if(rank != UnitRanks.UnRanked && carriedUnitCount > 0){
			UpdateMultipleGraphics();
		}
		else if(carriedUnitCount > 0){
			DisplayUnitsCarried();
			HideRank();
		}
		else if(rank != UnitRanks.UnRanked){
			DisplayRank();
			HideCarriedUnits();
		}
		else{
			HideCarriedUnits();
			HideRank();
		}
	}
	/// <summary>
	/// Updates when there are multiple unit info graphics to display
	/// </summary>
	void UpdateMultipleGraphics(){
		if(currentDisplayedUnitRank != UnitRanks.UnRanked && currentDisplayedCarriedCount > 0){
			if((Mathf.FloorToInt(animationTimer)/2) % 2 == 0){
				DisplayRank();
				HideCarriedUnits();
			}
			else {
				DisplayUnitsCarried();
				HideRank();
			}
		}
	}
	/// <summary>
	/// Displays the number of units carried
	/// </summary>
	void DisplayUnitsCarried(){
		for(int i = 0; i < carriedUnitImages.Length; i++){
			if(i < currentDisplayedCarriedCount){
				carriedUnitImages[i].gameObject.SetActive(true);
			}
			else{
				carriedUnitImages[i].gameObject.SetActive(false);
			}
		}
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
		for(int i = 0; i < carriedUnitImages.Length; i++){
			carriedUnitImages[i].gameObject.SetActive(false);
		}
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
