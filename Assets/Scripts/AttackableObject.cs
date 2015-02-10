using UnityEngine;
using System.Collections;

public interface AttackableObject
{
	Vector3 GetPosition ();
	Player GetOwner ();
	void SetOwner (Player newOwner);
	Health GetHealth ();
	int OffenseBonus ();
	int DefenseBonus ();
	UnitNames GetUnitClass ();
	int UnitCost ();
	TerrainBlock GetOccupyingBlock ();
}
