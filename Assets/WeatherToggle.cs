using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class WeatherToggle : MonoBehaviour {
	// Singleton instance of class
	public static WeatherToggle instance;
	public Toggle[] weatherTypes;
	int count;
	// Use this for initialization
	void Start () {
		instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	/// <summary>
	/// Callback for when the toggle is changed, turns on clear if none are currently selected
	/// </summary>
	/// <param name="isTrue">If set to <c>true</c> is true.</param>
	public void OnToggleChange(bool isTrue){
		if(isTrue){
			count++;
		}
		else{
			count--;
			if(count == 0){
				weatherTypes[0].isOn = true;
			}
		}
	}
	/// <summary>
	/// Returns a list of weather types that are currently selected
	/// </summary>
	/// <returns>The selected weather types.</returns>
	public List<WeatherType> GetSelectedWeatherTypes(){
		List<WeatherType> outList = new List<WeatherType>();
		if(weatherTypes[0].isOn){
			outList.Add(WeatherType.Clear);
		}
		if(weatherTypes[1].isOn){
			outList.Add(WeatherType.Snowing);
		}
		if(weatherTypes[2].isOn){
			outList.Add(WeatherType.ThunderStorm);
		}
		if(weatherTypes[3].isOn){
			outList.Add(WeatherType.Dusty);
		}
		if(weatherTypes[4].isOn){
			outList.Add(WeatherType.NuclearFallout);
		}
		return outList;
	}
}
