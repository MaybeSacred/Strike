// //Created by Jon Tyson : jtyson3@gatech.edu
#define EXPORT_INSTANCE_COMMENTS
using UnityEngine;
using System.Text;
using System;
[Serializable]
public class Instance
{
	public float[] playerUnitCount, enemyAverageUnitCounts;
	public int currentTurn;
	public int funds;
	public MapData mapData;
	public UnitName classification;
	public string prettyName;
	protected static long currentNumber;
	public Instance(int numUnits)
	{
		playerUnitCount = new float[numUnits];
		enemyAverageUnitCounts = new float[numUnits];
		prettyName = "Instance " + Instance.currentNumber++ + " " + InGameController.currentTurn;
	}
	public virtual string CreateARFFDataString(bool moreReadable)
	{
		StringBuilder instanceString = new StringBuilder();
		#if EXPORT_INSTANCE_COMMENTS
		instanceString.Append("%" + this.prettyName + "\n");
		#endif
		instanceString.Append(mapData.InstanceData());
		instanceString.Append(currentTurn + ",");
		instanceString.Append(funds + ",");
		for(int i = 0; i < playerUnitCount.Length; i++)
		{
			instanceString.Append(playerUnitCount[i] + ",");
		}
		for(int i = 0; i < enemyAverageUnitCounts.Length; i++)
		{
			instanceString.Append(enemyAverageUnitCounts[i] + ",");
		}
		instanceString.Append(this.classification.ToString());
		instanceString.Append("\n");
		return instanceString.ToString();
	}
	public virtual string AttributeString()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(MapData.AttributeString());
		sb.Append("@attribute 'Turn' numeric\n");
		sb.Append("@attribute 'Funds' numeric\n");
		foreach(string value in Enum.GetNames(typeof(UnitName)))
		{
			sb.Append("@attribute 'Player " + value + "' real\n");
		}
		foreach(string value in Enum.GetNames(typeof(UnitName)))
		{
			sb.Append("@attribute 'EnemyAverage " + value + "' real\n");
		}
		sb.Append("@attribute 'class' {");
		foreach(string strokeS in Enum.GetNames(typeof(UnitName)))
		{
			sb.Append(strokeS+",");
		}
		sb.Remove(sb.Length-1, 1);
		sb.Append("}\n");
		return sb.ToString();
	}
	public void ApplyRandomization(float randomness)
	{
		for(int i = 0; i < playerUnitCount.Length; i++)
		{
			playerUnitCount[i] += UnityEngine.Random.Range(-randomness, randomness);
		}
		for(int i = 0; i < enemyAverageUnitCounts.Length; i++)
		{
			enemyAverageUnitCounts[i] += UnityEngine.Random.Range(-randomness, randomness);
		}
	}
}
