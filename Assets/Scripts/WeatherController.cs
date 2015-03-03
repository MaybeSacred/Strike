using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum WeatherType
{
	Clear,
	Snowing,
	ThunderStorm,
	Dusty,
	NuclearFallout/*Has decent probability of causing 1 damage to all units on map each day*/}
;
public class WeatherController : MonoBehaviour
{
	public WeatherType currentWeather{ get; protected set; }
	List<WeatherType> possibleWeathers;
	public float chanceOfSwitchingWeather;
	public ParticleSystem[] weatherParticles;
	public Material[] weatherSkies;
	public float autoScalingOversizeFactor;
	// Use this for initialization
	void Awake ()
	{
		currentWeather = WeatherType.Clear;
	}
	void Start ()
	{
		Vector3 sunEulerAngles = GameObject.Find ("Sun").transform.eulerAngles;
		for (int i = 0; i < weatherParticles.Length; i++) {
			weatherParticles [i] = Instantiate (weatherParticles [i]) as ParticleSystem;
			weatherParticles [i].transform.localScale = new Vector3 ((InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound) * autoScalingOversizeFactor, (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound) * autoScalingOversizeFactor, 1);
			// If clear skies system
			if (i == 0) {
				weatherParticles [i].transform.position = new Vector3 ((InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound) / 2, 7, (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound) / 2);
				weatherParticles [i].transform.eulerAngles = sunEulerAngles;
			} else {
				weatherParticles [i].transform.position = new Vector3 ((InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound) / 2, 10, (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound) / 2);
			}
			weatherParticles [i].particleSystem.emissionRate *= (InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound) * (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound);
			weatherParticles [i].particleSystem.maxParticles = Mathf.RoundToInt (weatherParticles [i].particleSystem.emissionRate * weatherParticles [i].particleSystem.startLifetime);
			weatherParticles [i].particleSystem.Stop ();
			weatherParticles [i].gameObject.SetActive (false);
		}
		weatherParticles [(int)currentWeather].gameObject.SetActive (true);
		weatherParticles [(int)currentWeather].particleSystem.Play ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	/// <summary>
	/// Sets up additional properties and initializes weather state
	/// </summary>
	/// <param name="newWeather">New weather.</param>
	/// <param name="players">Players.</param>
	public void SetWeatherType (List<WeatherType> inWeathers, List<Player> players)
	{
		possibleWeathers = inWeathers;
		currentWeather = possibleWeathers [0];
		AddWeatherEffect (players, currentWeather);
	}
	/// <summary>
	/// Advances the weather, switching if multiple weather types are defined and applies any turn modifiers
	/// </summary>
	/// <param name="players">Players.</param>
	public void AdvanceWeather (List<Player> players)
	{
		if (Random.value > 1 - chanceOfSwitchingWeather) {
			int random = Random.Range (0, possibleWeathers.Count);
			if (currentWeather != possibleWeathers [random]) {
				ChangeState (possibleWeathers [random], players);
			}
		}
		if (currentWeather == WeatherType.NuclearFallout) {
			foreach (Player p in players) {
				foreach (UnitController uc in p.units) {
					uc.TakeDamage (FalloutDamage (), true);
				}
			}
		}
	}
	/// <summary>
	/// Returns fallout damage, 5 +- 2
	/// </summary>
	/// <returns>The damage.</returns>
	int FalloutDamage ()
	{
		return 3 + Random.Range (0, 5);
	}
	void RemoveWeatherEffect (List<Player> players, WeatherType type)
	{
		foreach (Player p in players) {
			foreach (UnitController uc in p.units) {
				uc.modifier.RemoveAllOfModifierType (UnitPropertyModifier.ModifierTypes.Weather);
			}
		}
	}
	void AddWeatherEffect (List<Player> players, WeatherType type)
	{
		if (type == WeatherType.Dusty) {
			foreach (Player p in players) {
				foreach (UnitController uc in p.units) {
					if (uc.moveClass == MovementType.Air) {
						uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, -1);
					}
				}
			}
		} else if (type == WeatherType.Snowing) {
			foreach (Player p in players) {
				foreach (UnitController uc in p.units) {
					uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.Weather, -1);
				}
			}
		} else if (type == WeatherType.ThunderStorm) {
			foreach (Player p in players) {
				foreach (UnitController uc in p.units) {
					uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, 1, true);
				}
			}
		}
	}
	public void ApplyCurrentWeatherEffect (UnitController inUnit)
	{
		if (currentWeather == WeatherType.Dusty) {
			if (inUnit.moveClass == MovementType.Air) {
				inUnit.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, -1);
			}
		} else if (currentWeather == WeatherType.Snowing) {
			inUnit.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.Weather, -1);
		} else if (currentWeather == WeatherType.ThunderStorm) {
			inUnit.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, 1, true);
		}
	}
	void ChangeState (WeatherType next, List<Player> players)
	{
		weatherParticles [(int)currentWeather].particleSystem.Stop ();
		weatherParticles [(int)currentWeather].gameObject.SetActive (false);
		RemoveWeatherEffect (players, currentWeather);
		currentWeather = next;
		weatherParticles [(int)currentWeather].gameObject.SetActive (true);
		weatherParticles [(int)currentWeather].particleSystem.Play ();
		AddWeatherEffect (players, currentWeather);
	}
}
