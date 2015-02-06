using UnityEngine;
using System.Collections;

public class Health {
	int rawHealth = 100;
	public Health(){
		
	}
	public override string ToString(){
		return PrettyHealth().ToString();
	}
	public Health(int startingHealth){
		rawHealth = startingHealth;
	}
	public int PrettyHealth()
	{
		return (Mathf.CeilToInt(((float)rawHealth)/10));
	}
	public int GetRawHealth(){
		return rawHealth;
	}

	public void SetRawHealth (int i)
	{
		rawHealth = i;
		if(rawHealth > 100){
			rawHealth = 100;
		}
	}

	public void AddRawHealth(int change){
		rawHealth += change;
		if(rawHealth > 100)
		{
			rawHealth = 100;
		}
	}
	public Health Clone(){
		return new Health(rawHealth);
	}
	public static bool operator >(Health a, Health b){
		return a.rawHealth > b.rawHealth;
	}
	public static bool operator <(Health a, Health b){
		return a.rawHealth < b.rawHealth;
	}
	public static bool operator <=(Health a, Health b){
		return a.rawHealth <= b.rawHealth;
	}
	public static bool operator ==(Health a, Health b){
		return a.rawHealth == b.rawHealth;
	}
	public static bool operator !=(Health a, Health b){
		return a.rawHealth != b.rawHealth;
	}
	public static bool operator >=(Health a, Health b){
		return a.rawHealth >= b.rawHealth;
	}
	
	public static bool operator >(Health a, int b){
		return a.rawHealth > b;
	}
	public static bool operator <(Health a, int b){
		return a.rawHealth < b;
	}
	public static bool operator >=(Health a, int b){
		return a.rawHealth >= b;
	}
	public static bool operator <=(Health a, int b){
		return a.rawHealth <= b;
	}
	public static bool operator ==(Health a, int b){
		return a.rawHealth == b;
	}
	public static bool operator !=(Health a, int b){
		return a.rawHealth != b;
	}
	
	public static bool operator >(int a, Health b){
		return a > b.rawHealth;
	}
	public static bool operator <(int a, Health b){
		return a < b.rawHealth;
	}
	public static bool operator >=(int a, Health b){
		return a >= b.rawHealth;
	}
	public static bool operator <=(int a, Health b){
		return a <= b.rawHealth;
	}
	public static bool operator ==(int a, Health b){
		return a == b.rawHealth;
	}
	public static bool operator !=(int a, Health b){
		return a != b.rawHealth;
	}
}
