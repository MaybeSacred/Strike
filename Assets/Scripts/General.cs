using UnityEngine;
using System.Collections.Generic;
public enum Generals
{
	Lucy, //General boost, after lucille ball
	Navya, //Boosted ground units, female Indian
	Desmond, //Boosted naval units
	Taron, //Boosted air units
	Kiera //Heals units in zone, ex nurse
}
public class General : MonoBehaviour
{
	public int zoneSize;
	public Generals generalName;
	public List<UnitName> boostedUnits;
	public int offensiveBoost, defensiveBoost;
	public int damageNeededForPower;
	private int currentStoredPower;
	private int zoneSizeBoost;
	public bool powerInEffect { get; private set; }
	private List<UnitController> unitsInPowerEffect;
	private List<ParticleSystem> zoneBlocks;
	public ParticleSystem particlePrototype;
	public Player owner { get; private set; }
	private bool isActive;
	void Awake ()
	{
	
	}
	public static General CreateGeneral (General prototype)
	{
		General g = Instantiate (prototype) as General;
		g.transform.position = Vector3.zero;
		g.zoneBlocks = new List<ParticleSystem> (g.GetBlockCount (g.zoneSize + g.zoneSizeBoost));
		for (int i = 0; i < g.GetBlockCount(g.zoneSize + 2); i++) {
			g.zoneBlocks.Add (Instantiate (g.particlePrototype) as ParticleSystem);
			g.zoneBlocks [i].transform.SetParent (g.transform);
			g.zoneBlocks [i].gameObject.SetActive (false);
		}
		return g;
	}
	public void ShowZone (TerrainBlock centerBlock)
	{
		if (!powerInEffect) {
			transform.position = centerBlock.transform.position;
			int startPointX = Mathf.RoundToInt (centerBlock.transform.position.x) - zoneSize - zoneSizeBoost;
			int startPointZ = Mathf.RoundToInt (centerBlock.transform.position.z);
			int blocksInZDirection = 1;
			int blocksUsedInZoneBlocks = 0;
			for (int i = 0; i < 2*(zoneSize + zoneSizeBoost) + 1; i++) {
				for (int k = 0; k < blocksInZDirection; k++) {
					if (InGameController.instance.currentTerrain.GetBlockAtPos (i + startPointX, startPointZ - k) != null) {
						zoneBlocks [blocksUsedInZoneBlocks].transform.position = InGameController.instance.currentTerrain.GetBlockAtPos (i + startPointX, startPointZ - k).transform.position + Vector3.up * .51f;
						zoneBlocks [blocksUsedInZoneBlocks].gameObject.SetActive (true);
						blocksUsedInZoneBlocks++;
					}
				}
				if (i >= zoneSize + zoneSizeBoost) {
					blocksInZDirection -= 2;
					startPointZ -= 1;
				} else {
					blocksInZDirection += 2;
					startPointZ += 1;
				}
			}
			for (int i = blocksUsedInZoneBlocks; i < zoneBlocks.Count; i++) {
				zoneBlocks [i].gameObject.SetActive (false);
			}
		}
	}
	public void HideZone ()
	{
		for (int i = 0; i < zoneBlocks.Count; i++) {
			zoneBlocks [i].gameObject.SetActive (false);
		}
	}
	public bool CanUsePower ()
	{
		if (currentStoredPower >= damageNeededForPower) {
			return true;
		}
		return false;
	}
	private int GetBlockCount (int zoneRadius)
	{
		zoneRadius = 2 * zoneRadius + 1;
		return Mathf.CeilToInt ((float)zoneRadius * (float)zoneRadius / 2);
	}
	public void SetOwner (Player newOwner)
	{
		owner = newOwner;
		foreach (ParticleSystem ps in zoneBlocks) {
			ps.GetComponent<ParticleSystem>().startColor = new Color (owner.mainPlayerColor.r, owner.mainPlayerColor.g, owner.mainPlayerColor.b, .167f);
		}
	}
	public void UpdateGeneral (int inDamage)
	{
		if (!powerInEffect) {
			currentStoredPower += inDamage;
			if (currentStoredPower >= damageNeededForPower) {
				zoneSizeBoost = 2;
				ShowZone (InGameController.instance.currentTerrain.GetBlockAtPos (transform.position));
			} else if (currentStoredPower >= damageNeededForPower / 2) {
				zoneSizeBoost = 1;
				ShowZone (InGameController.instance.currentTerrain.GetBlockAtPos (transform.position));
			}
			InGameGUI.instance.ShowGeneralPower (((float)currentStoredPower / (float)damageNeededForPower), owner);
		}
	}
	public void ShowGeneral (UnitController ridingUnit)
	{
		ResetGeneral ();
		isActive = true;
	}
	public void Hide ()
	{
		ResetGeneral ();
		foreach (ParticleSystem gp in zoneBlocks) {
			gp.gameObject.SetActive (false);
		}
		isActive = false;
	}
	public void ResetGeneral ()
	{
		currentStoredPower = 0;
		zoneSizeBoost = 0;
	}
	public int UnitOffensiveBoost (UnitName inUnit)
	{
		if (boostedUnits.Contains (inUnit)) {
			if (powerInEffect) {
				switch (generalName) {
				case Generals.Desmond:
					{
						return offensiveBoost + 2;
					}
				case Generals.Lucy:
					{
					
						break;
					}
				case Generals.Navya:
					{
						return offensiveBoost + 4;
					}
				case Generals.Taron:
					{
						return offensiveBoost + 2;
					}
				case Generals.Kiera:
					{
					
						break;
					}
				}
			}
			return offensiveBoost + 1;
		}
		return 1;
	}
	public int UnitDefensiveBoost (UnitName inUnit)
	{
		if (boostedUnits.Contains (inUnit)) {
			if (powerInEffect) {
				switch (generalName) {
				case Generals.Desmond:
					{
					
						break;
					}
				case Generals.Lucy:
					{
					
						break;
					}
				case Generals.Navya:
					{
						return defensiveBoost + 4;
					}
				case Generals.Taron:
					{
						return defensiveBoost + 2;
					}
				case Generals.Kiera:
					{
						return defensiveBoost + 3;
					}
				}
			}
			return defensiveBoost + 1;
		}
		return 1;
	}
	public bool IsInZoneRange (Transform checkingObject)
	{
		if (isActive) {
			if (TerrainBuilder.ManhattanDistance (checkingObject.position, transform.position) <= zoneSize + zoneSizeBoost || powerInEffect) {
				return true;
			}
		}
		return false;
	}
	public void EnterPower ()
	{
		powerInEffect = true;
		zoneSizeBoost = 0;
		currentStoredPower = 0;
		unitsInPowerEffect = new List<UnitController> (owner.units);
		HideZone ();
		switch (generalName) {
		case Generals.Desmond:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					if (uc.moveClass == MovementType.Sea || uc.moveClass == MovementType.Littoral) {
						uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.GeneralEffect, 1);
						if (uc.maxAttackRange > 1) {
							uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.AttackRange, UnitPropertyModifier.ModifierTypes.GeneralEffect, 2);
						}
					}
				}
				break;
			}
		case Generals.Lucy:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.GeneralEffect, 1);
					if (uc.maxAttackRange > 1) {
						uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.AttackRange, UnitPropertyModifier.ModifierTypes.GeneralEffect, 1);
					}
				}
				break;
			}
		case Generals.Navya:
			{
				
				break;
			}
		case Generals.Taron:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					if (uc.moveClass == MovementType.Air) {
						uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.GeneralEffect, 2);
					}
					uc.modifier.AddModifier (UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.GeneralEffect, 2);
				}
				break;
			}
		case Generals.Kiera:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					uc.Heal (2, false);
				}
				break;
			}
		}
	}
	public void ExitPower ()
	{
		powerInEffect = false;
		switch (generalName) {
		case Generals.Desmond:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					if (uc.moveClass == MovementType.Sea || uc.moveClass == MovementType.Littoral) {
						uc.modifier.RemoveModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.GeneralEffect);
						if (uc.maxAttackRange > 1) {
							uc.modifier.RemoveModifier (UnitPropertyModifier.PropertyModifiers.AttackRange, UnitPropertyModifier.ModifierTypes.GeneralEffect);
						}
					}
				}
				break;
			}
		case Generals.Lucy:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					if (uc != null) {
						uc.modifier.RemoveModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.GeneralEffect);
						if (uc.maxAttackRange > 1) {
							uc.modifier.RemoveModifier (UnitPropertyModifier.PropertyModifiers.AttackRange, UnitPropertyModifier.ModifierTypes.GeneralEffect);
						}
					}
				}
				break;
			}
		case Generals.Navya:
			{
				
				break;
			}
		case Generals.Taron:
			{
				foreach (UnitController uc in unitsInPowerEffect) {
					if (uc.moveClass == MovementType.Air) {
						uc.modifier.RemoveModifier (UnitPropertyModifier.PropertyModifiers.MovementRange, UnitPropertyModifier.ModifierTypes.GeneralEffect);
					}
					uc.modifier.RemoveModifier (UnitPropertyModifier.PropertyModifiers.VisionRange, UnitPropertyModifier.ModifierTypes.GeneralEffect);
				}
				break;
			}
		case Generals.Kiera:
			{
				
				break;
			}
		}
	}
}

