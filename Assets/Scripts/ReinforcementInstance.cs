// //Created by Jon Tyson : jtyson3@gatech.edu
#define EXPORT_INSTANCE_COMMENTS
using UnityEngine;
using System.Text;
using System;
[Serializable]
public class ReinforcementInstance : Instance
{
	public float accruedReward;
	public ReinforcementInstance(int numUnits) : base(numUnits)
	{
		prettyName = "Instance " + Instance.currentNumber++ + " " + InGameController.currentTurn;
	}
	public override string CreateARFFDataString(bool moreReadable)
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
		instanceString.Append(this.classification.ToString() + ",");
		instanceString.Append(accruedReward);
		instanceString.Append("\n");
		return instanceString.ToString();
	}
	public override string AttributeString()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append(MapData.AttributeString());
		sb.Append("@attribute 'Turn' numeric\n");
		sb.Append("@attribute 'Funds' numeric\n");
		foreach(string value in Enum.GetNames(typeof(UnitNames)))
		{
			sb.Append("@attribute 'Player " + value + "' real\n");
		}
		foreach(string value in Enum.GetNames(typeof(UnitNames)))
		{
			sb.Append("@attribute 'EnemyAverage " + value + "' real\n");
		}
		sb.Append("@attribute 'class' {");
		foreach(string strokeS in Enum.GetNames(typeof(UnitNames)))
		{
			sb.Append(strokeS+",");
		}
		sb.Remove(sb.Length-1, 1);
		sb.Append("}\n");
		sb.Append("@attribute 'Reward' numeric\n");
		return sb.ToString();
	}
}
