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
	public TerrainObject[][] mapData;
	public TerrainObject[] properties;
	public string mapName;
	public float[] blockStatistics;
	public MapData(string name, int mapX, int mapY)
	{
		mapName = name;
		blockStatistics = new float[System.Enum.GetValues(typeof(TERRAINTYPE)).Length];
		mapData = new TerrainObject[mapX][];
		for(int i = 0; i < mapData.Length; i++)
		{
			mapData[i] = new TerrainObject[mapY];
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
		if(HQLocations.Length != maxPlayers){
			return false;
		}
		return true;
	}
}
/// <summary>
/// Provides a serializable conversion for terrain blocks and other objects
/// </summary>
[System.Serializable]
public class TerrainObject
{
	public string name;
	public Vector3Serializer position;
	public QuaternionSerializer rotation;
	public TerrainObject(string inName, Vector3 pos, Quaternion rot){
		name = inName;
		position = new Vector3Serializer(pos);
		rotation = new QuaternionSerializer(rot);
	}
	public TerrainObject(GameObject go){
		name = go.name;
		position = new Vector3Serializer(go.transform.position);
		rotation = new QuaternionSerializer(go.transform.rotation);
	}
}
/// <summary>
/// Serializable wrapper struct for Quaternion
/// </summary>
[System.Serializable]
public struct QuaternionSerializer
{
	public float x, y, z, w;
	public QuaternionSerializer(float inX, float inY, float inZ, float inW){
		x = inX;
		y = inY;
		z = inZ;
		w = inW;
	}
	public QuaternionSerializer(Quaternion quatToSerialize){
		x = quatToSerialize.x;
		y = quatToSerialize.y;
		z = quatToSerialize.z;
		w = quatToSerialize.w;
	}
	public Vector3 ToEulerAngles(){
		return (new Quaternion(x, y, z, w)).eulerAngles;
	}
	public Quaternion ToQuaternion(){
		return new Quaternion(x, y, z, w);
	}
}
/// <summary>
/// Serializable wrapper struct for Vector3
/// </summary>
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
	public Vector3 ToVector3(){
		return new Vector3(x, y, z);
	}
}