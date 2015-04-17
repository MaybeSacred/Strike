using UnityEngine;
using System.Collections;

public interface AttackableObject
{
	Vector3 GetPosition ();
	Health GetHealth ();
	int OffenseBonus ();
	int DefenseBonus ();
	UnitName GetUnitClass ();
	int UnitCost ();
	TerrainBlock GetOccupyingBlock ();
}
