using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum WeatherType {Clear, Snowing, ThunderStorm, Cloudy, NuclearFallout/*Has decent probability of causing 1 damage to all units on map each day*/};
public class WeatherController : MonoBehaviour {
	private WeatherType currentWeather = WeatherType.Clear;
	public bool randomize;
	public float chanceOfSwitchingWeather;
	public ParticleSystem[] weatherParticles;
	public Material[] weatherSkies;
	public float autoScalingOversizeFactor;
	// Use this for initialization
	void Awake() {
		
	}
	void Start () {
		for(int i = 0; i < weatherParticles.Length; i++)
		{
			weatherParticles[i] = Instantiate(weatherParticles[i]) as ParticleSystem;
			weatherParticles[i].transform.localScale = new Vector3((InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound) * autoScalingOversizeFactor, (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound) * autoScalingOversizeFactor, 1);
			weatherParticles[i].transform.position = new Vector3((InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound)/2, 10, (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound)/2);
			weatherParticles[i].particleSystem.emissionRate *= (InGameController.currentTerrain.upperXMapBound - InGameController.currentTerrain.lowerXMapBound) * (InGameController.currentTerrain.upperZMapBound - InGameController.currentTerrain.lowerZMapBound);
			weatherParticles[i].particleSystem.maxParticles = Mathf.RoundToInt(weatherParticles[i].particleSystem.emissionRate * weatherParticles[i].particleSystem.startLifetime);
			weatherParticles[i].particleSystem.Stop();
			weatherParticles[i].gameObject.SetActive(false);
		}
		weatherParticles[(int)currentWeather].gameObject.SetActive(true);
		weatherParticles[(int)currentWeather].particleSystem.Play();
	}
	
	// Update is called once per frame
	void Update () {
	}
	public void SetWeatherType(WeatherType newWeather, bool random, List<Player> players)
	{
		currentWeather = newWeather;
		AddWeatherEffect(players, currentWeather);
		randomize = random;
	}
	public void AdvanceWeather(List<Player> players)
	{
		if(randomize)
		{
			if(Random.value > 1 - chanceOfSwitchingWeather)
			{
				switch (Random.Range(0, System.Enum.GetValues(typeof(WeatherType)).Length + 2))
				{
					case 0:
					case 1:
					case 2:
					{
						if(currentWeather != WeatherType.Clear)
						{
							ChangeState(WeatherType.Clear, players);
						}
						break;
					}
					case 3:
					{
						if(currentWeather != WeatherType.Cloudy)
						{
							ChangeState(WeatherType.Cloudy, players);
						}
						break;
					}
					case 4:
					{
						if(currentWeather != WeatherType.Snowing)
						{
							ChangeState(WeatherType.Snowing, players);
						}
						break;
					}
					case 6:
					{
						if(currentWeather != WeatherType.NuclearFallout)
						{
							ChangeState(WeatherType.NuclearFallout, players);
						}
						break;
					}
					case 5:
					{
						if(currentWeather != WeatherType.ThunderStorm)
						{
							ChangeState(WeatherType.ThunderStorm, players);
						}
						break;
					}
				}
			}
		}
		if(currentWeather == WeatherType.NuclearFallout)
		{
			foreach(Player p in players)
			{
				foreach(UnitController uc in p.units)
				{
					uc.TakeDamage(FalloutDamage(), true);
				}
			}
		}
	}
	int FalloutDamage()
	{
		return 3 + Random.Range(0, 4);
	}
	void RemoveWeatherEffect(List<Player> players, WeatherType type)
	{
		foreach(Player p in players)
		{
			foreach(UnitController uc in p.units)
			{
				if(uc.moveClass == MovementType.Air)
				{
					uc.modifier.RemoveAllOfModifierType(UnitPropertyModifier.ModifierTypes.Weather);
				}
			}
		}
	}
	void AddWeatherEffect(List<Player> players, WeatherType type)
	{
		if(type == WeatherType.Cloudy)
		{
			foreach(Player p in players)
			{
				foreach(UnitController uc in p.units)
				{
					if(uc.moveClass == MovementType.Air)
					{
						uc.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, -2);
					}
				}
			}
		}
		else if(type == WeatherType.Snowing)
		{
			foreach(Player p in players)
			{
				foreach(UnitController uc in p.units)
				{
					uc.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.Weather, -1);
				}
			}
		}
		else if(type == WeatherType.ThunderStorm)
		{
			foreach(Player p in players)
			{
				foreach(UnitController uc in p.units)
				{
					uc.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, 1, true);
				}
			}
		}
	}
	public void ApplyCurrentWeatherEffect(UnitController inUnit)
	{
		if(currentWeather == WeatherType.Cloudy)
		{
			if(inUnit.moveClass == MovementType.Air)
			{
				inUnit.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, -2);
			}
		}
		else if(currentWeather == WeatherType.Snowing)
		{
			inUnit.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.Weather, -1);
		}
		else if(currentWeather == WeatherType.ThunderStorm)
		{
			inUnit.modifier.AddModifier(UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.Weather, 1, true);
		}
	}
	void ChangeState(WeatherType next, List<Player> players)
	{
		weatherParticles[(int)currentWeather].particleSystem.Stop();
		weatherParticles[(int)currentWeather].gameObject.SetActive(false);
		
		RemoveWeatherEffect(players, currentWeather);
		currentWeather = next;
		weatherParticles[(int)currentWeather].gameObject.SetActive(true);
		weatherParticles[(int)currentWeather].particleSystem.Play();
		AddWeatherEffect(players, currentWeather);
	}
}
