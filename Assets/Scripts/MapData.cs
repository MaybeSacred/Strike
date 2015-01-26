using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;


[System.Serializable]
public class MapData{
	public int maxPlayers;
	public Vector3Serializer[] HQLocations;
	public int cities;
	public int factories;
	public int airports;
	public int shipyards;
	public int comTowers;
	public bool isPreDeploy;
	public string[][] mapData;
	public float[] blockStatistics;
	public MapData(int mapX, int mapY)
	{
		blockStatistics = new float[System.Enum.GetValues(typeof(TERRAINTYPE)).Length];
		mapData = new string[mapX][];
		for(int i = 0; i < mapData.Length; i++)
		{
			mapData[i] = new string[mapY];
		}
	}
	public string InstanceData()
	{
		StringBuilder instanceString = new StringBuilder();
		instanceString.Append(maxPlayers + ",");
		instanceString.Append(cities + ",");
		instanceString.Append(factories + ",");
		instanceString.Append(airports + ",");
		instanceString.Append(shipyards + ",");
		instanceString.Append(comTowers + ",");
		instanceString.Append((Utilities.fogOfWarEnabled?1:0).ToString() + ",");
		for(int i = 0; i < blockStatistics.Length; i++)
		{
			instanceString.Append(blockStatistics[i].ToString() + ",");
		}
		return instanceString.ToString();
	}
	public static string AttributeString()
	{
		StringBuilder instanceString = new StringBuilder();
		instanceString.Append("@attribute 'MaxPlayers' numeric\n");
		instanceString.Append("@attribute 'Cities' numeric\n");
		instanceString.Append("@attribute 'Factories' numeric\n");
		instanceString.Append("@attribute 'Airports' numeric\n");
		instanceString.Append("@attribute 'Shipyards' numeric\n");
		instanceString.Append("@attribute 'ComTowers' numeric\n");
		instanceString.Append("@attribute 'FogOfWar' numeric\n");
		foreach(string s in System.Enum.GetNames(typeof(TERRAINTYPE)))
		{
			instanceString.Append("@attribute 'Terrain " + s + "' real\n");
		}
		return instanceString.ToString();
	}
	/// <summary>
	/// Provides checks on the playibility of a map
	/// </summary>
	/// <returns><c>true</c> if this instance is valid; otherwise, <c>false</c>.</returns>
	public bool IsPlayable(){
		if(maxPlayers < 1 || maxPlayers > 8){
			return false;
		}
		return true;
	}
}
[System.Serializable]
public struct Vector3Serializer
{
	public float x;
	public float y;
	public float z;
	public Vector3Serializer(Vector3 v3)
	{
		x = v3.x;
		y = v3.y;
		z = v3.z;
	}
	public void SetVector3(Vector3 v3)
	{
		x = v3.x;
		y = v3.y;
		z = v3.z;
	}
}