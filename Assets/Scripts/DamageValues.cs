using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;


public class DamageValues : MonoBehaviour
{
	public static readonly float DEFENSECONSTANT = 10;//How strong a unit of defense counts for
	public static int[][] unitDamageArray;//values of -1 will be unable to attack that unit
	static DamageValues instance;
	// Use this for initialization
	void Awake ()
	{
		// Make sure theres only one instance of damageValues
		if (instance == null || instance == this) {
			if (instance == null) {
				StartCoroutine (LoadASyncValues ());
			}
			instance = this;
			DontDestroyOnLoad (this);
		} else {
			Destroy (this);
		}
		if (unitDamageArray == null) {
			unitDamageArray = new int[Enum.GetValues (typeof(UnitName)).Length][];
			for (int i = 0; i < unitDamageArray.Length; i++) {
				unitDamageArray [i] = new int[Enum.GetValues (typeof(UnitName)).Length];
				for (int j = 0; j < unitDamageArray[i].Length; j++) {
					unitDamageArray [i] [j] = -1;
				}
			}
		}
	}
	IEnumerator LoadASyncValues ()
	{
		#if UNITY_WEBPLAYER
		var names = new WWW (SkirmishMenuViewer.ApplicationServerURL + @"/Maps/unitDamageValues.csv");
		while (!names.isDone) {
			yield return new WaitForSeconds (.001f);
		}
		MemoryStream ms = new MemoryStream (names.bytes);
		StreamReader reader = new StreamReader (ms);
#elif UNITY_STANDALONE
		StreamReader reader = new StreamReader (File.OpenRead (Application.dataPath + @"\Maps\unitDamageValues.csv"));
#endif
		var line = reader.ReadLine ();
		var columnValues = line.Split (new char[]{',', ';'});
		while (!reader.EndOfStream) {
			line = reader.ReadLine ();
			var values = line.Split (new char[]{',', ';'});
			if (!values [0].Equals ("")) {
				UnitName currentName = (UnitName)Enum.Parse (typeof(UnitName), values [0]);
				for (int i = 1; i < values.Length; i++) {
					if (!values [i].Equals ("")) {
						unitDamageArray [(int)currentName] [(int)(UnitName)Enum.Parse (typeof(UnitName), columnValues [i])] = int.Parse (values [i]);
					}
				}
			}
		}
	}
	public static bool CanAttackUnit (UnitController attacker, UnitController defender)
	{
		if (unitDamageArray [(int)attacker.unitClass] [(int)defender.unitClass] >= 0) {
			return true;
		}
		return false;
	}
	public static bool CanAttackUnit (UnitController attacker, Property defender)
	{
		if (unitDamageArray [(int)attacker.unitClass] [(int)defender.propertyType] >= 0 && defender.IsAlive ()) {
			return true;
		}
		return false;
	}
	public static int CalculateLuckDamage (int health)
	{
		return Mathf.RoundToInt ((UnityEngine.Random.Range (0, 5) + UnityEngine.Random.Range (0, 5) * ((float)health / 100)));
	}
	public static int CalculateDamage (AttackableObject attacker, AttackableObject defender)
	{
		float output = unitDamageArray [(int)attacker.GetUnitClass ()] [(int)defender.GetUnitClass ()];
		if (output < 0) {
			return 0;
		}
		output *= (float)(attacker.GetHealth ().PrettyHealth ());
		output *= (100f + attacker.OffenseBonus ());
		output /= (90f + defender.DefenseBonus () + 2 * (defender.GetHealth ().PrettyHealth ()));
		output /= 10f;
		return Mathf.RoundToInt (output);
	}
}
