// //Created by Jon Tyson : jtyson3@gatech.edu
#define EXPORT_INSTANCE_COMMENTS
using UnityEngine;
using System.Text;
using System;
[Serializable]
public class Instance
{
	public float[] playerUnitCount, enemyAverageUnitCount, neutralUnitCount;
	public int currentTurn;
	public int funds;
	public MapData mapData;
	public UnitName classification;
	public string prettyName;
	protected static long currentNumber;
	public Instance (int numUnits)
	{
		playerUnitCount = new float[numUnits];
		enemyAverageUnitCount = new float[numUnits];
		neutralUnitCount = new float[numUnits];
		prettyName = "Instance " + Instance.currentNumber++ + " " + InGameController.instance.currentTurn;
	}
	public virtual string CreateARFFDataString (bool moreReadable)
	{
		StringBuilder instanceString = new StringBuilder ();
		#if EXPORT_INSTANCE_COMMENTS
		instanceString.Append ("%" + this.prettyName + "\n");
		#endif
		instanceString.Append (mapData.InstanceData ());
		instanceString.Append (currentTurn + ",");
		instanceString.Append (funds + ",");
		for (int i = 0; i < playerUnitCount.Length; i++) {
			instanceString.Append (playerUnitCount [i] + ",");
		}
		for (int i = 0; i < enemyAverageUnitCount.Length; i++) {
			instanceString.Append (enemyAverageUnitCount [i] + ",");
		}
		instanceString.Append (this.classification.ToString ());
		instanceString.Append ("\n");
		return instanceString.ToString ();
	}
	public virtual string AttributeString ()
	{
		StringBuilder sb = new StringBuilder ();
		sb.Append (MapData.AttributeString ());
		sb.Append ("@attribute 'Turn' numeric\n");
		sb.Append ("@attribute 'Funds' numeric\n");
		foreach (string value in Enum.GetNames(typeof(UnitName))) {
			sb.Append ("@attribute 'Player " + value + "' real\n");
		}
		foreach (string value in Enum.GetNames(typeof(UnitName))) {
			sb.Append ("@attribute 'EnemyAverage " + value + "' real\n");
		}
		sb.Append ("@attribute 'class' {");
		foreach (string strokeS in Enum.GetNames(typeof(UnitName))) {
			sb.Append (strokeS + ",");
		}
		sb.Remove (sb.Length - 1, 1);
		sb.Append ("}\n");
		return sb.ToString ();
	}
	public void ApplyRandomization (float randomness)
	{
		for (int i = 0; i < playerUnitCount.Length; i++) {
			playerUnitCount [i] += UnityEngine.Random.Range (-randomness, randomness);
		}
		for (int i = 0; i < enemyAverageUnitCount.Length; i++) {
			enemyAverageUnitCount [i] += UnityEngine.Random.Range (-randomness, randomness);
		}
	}
	/// <summary>
	/// Gets the enemy air unit count.
	/// </summary>
	/// <returns>The enemy air unit count.</returns>
	public float GetEnemyAirUnitCount ()
	{
		return enemyAverageUnitCount [(int)UnitName.AttackCopter] + 
			enemyAverageUnitCount [(int)UnitName.CarpetBomber] + 
			enemyAverageUnitCount [(int)UnitName.TacticalFighter] + 
			enemyAverageUnitCount [(int)UnitName.Interceptor];
	}
	/// <summary>
	/// Gets the player air unit count.
	/// </summary>
	/// <returns>The player air unit count.</returns>
	public float GetPlayerAirUnitCount ()
	{
		return playerUnitCount [(int)UnitName.AttackCopter] + 
			playerUnitCount [(int)UnitName.CarpetBomber] + 
			playerUnitCount [(int)UnitName.TacticalFighter] + 
			playerUnitCount [(int)UnitName.Interceptor];
	}
	/// <summary>
	/// Gets the enemy naval unit count.
	/// </summary>
	/// <returns>The enemy naval unit count.</returns>
	public float GetEnemyNavalUnitCount ()
	{
		return enemyAverageUnitCount [(int)UnitName.Corvette] + 
			enemyAverageUnitCount [(int)UnitName.Destroyer] + 
			enemyAverageUnitCount [(int)UnitName.Carrier] + 
			enemyAverageUnitCount [(int)UnitName.Submarine] + 
			enemyAverageUnitCount [(int)UnitName.Boomer];
	}
	/// <summary>
	/// Gets the player naval unit count.
	/// </summary>
	/// <returns>The player naval unit count.</returns>
	public float GetPlayerNavalUnitCount ()
	{
		return playerUnitCount [(int)UnitName.Corvette] + 
			playerUnitCount [(int)UnitName.Destroyer] + 
			playerUnitCount [(int)UnitName.Carrier] + 
			playerUnitCount [(int)UnitName.Submarine] + 
			playerUnitCount [(int)UnitName.Boomer];
	}
	/// <summary>
	/// Gets the enemy vehicle unit count.
	/// </summary>
	/// <returns>The enemy vehicle unit count.</returns>
	public float GetEnemyVehicleUnitCount ()
	{
		return GetEnemyOffensiveVehicleUnitCount () + GetEnemySupportVehicleUnitCount ();
	}
	/// <summary>
	/// Gets the enemy support vehicle unit count.
	/// </summary>
	/// <returns>The enemy support vehicle unit count.</returns>
	public float GetEnemySupportVehicleUnitCount ()
	{
		return enemyAverageUnitCount [(int)UnitName.Humvee] + 
			enemyAverageUnitCount [(int)UnitName.MobileRadar] + 
			enemyAverageUnitCount [(int)UnitName.Stryker] + 
			enemyAverageUnitCount [(int)UnitName.SupplyTank];
	}
	/// <summary>
	/// Gets the enemy offensive vehicle unit count.
	/// </summary>
	/// <returns>The enemy offensive vehicle unit count.</returns>
	public float GetEnemyOffensiveVehicleUnitCount ()
	{
		return enemyAverageUnitCount [(int)UnitName.FieldArtillery] + 
			enemyAverageUnitCount [(int)UnitName.Rockets] + 
			enemyAverageUnitCount [(int)UnitName.LightTank] + 
			enemyAverageUnitCount [(int)UnitName.MediumTank];
	}
	/// <summary>
	/// Gets the player support vehicle unit count.
	/// </summary>
	/// <returns>The player support vehicle unit count.</returns>
	public float GetPlayerSupportVehicleUnitCount ()
	{
		return playerUnitCount [(int)UnitName.Humvee] + 
			playerUnitCount [(int)UnitName.MobileRadar] + 
			playerUnitCount [(int)UnitName.Stryker] + 
			playerUnitCount [(int)UnitName.SupplyTank];
	}
	/// <summary>
	/// Gets the player offensive vehicle unit count.
	/// </summary>
	/// <returns>The player offensive vehicle unit count.</returns>
	public float GetPlayerOffensiveVehicleUnitCount ()
	{
		return playerUnitCount [(int)UnitName.FieldArtillery] + 
			playerUnitCount [(int)UnitName.Rockets] + 
			playerUnitCount [(int)UnitName.LightTank] + 
			playerUnitCount [(int)UnitName.MediumTank];
	}
	/// <summary>
	/// Gets the player vehicle unit count.
	/// </summary>
	/// <returns>The player vehicle unit count.</returns>
	public float GetPlayerVehicleUnitCount ()
	{
		return GetPlayerOffensiveVehicleUnitCount () + GetPlayerSupportVehicleUnitCount ();
	}
	/// <summary>
	/// Gets the enemy infantry unit count.
	/// </summary>
	/// <returns>The enemy infantry unit count.</returns>
	public float GetEnemyInfantryUnitCount ()
	{
		return enemyAverageUnitCount [(int)UnitName.Infantry] + 
			enemyAverageUnitCount [(int)UnitName.Stinger] + 
			enemyAverageUnitCount [(int)UnitName.Mortar];
	}
	/// <summary>
	/// Gets the player infantry unit count.
	/// </summary>
	/// <returns>The player infantry unit count.</returns>
	public float GetPlayerInfantryUnitCount ()
	{
		return playerUnitCount [(int)UnitName.Infantry] + 
			playerUnitCount [(int)UnitName.Stinger] + 
			playerUnitCount [(int)UnitName.Mortar];
	}
}
