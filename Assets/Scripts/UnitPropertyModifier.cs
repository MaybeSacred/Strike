using System;
using System.Collections.Generic;
public class UnitPropertyModifier
{
	public enum PropertyModifiers {MovementRange, VisionRange, AttackRange};
	public enum ModifierTypes {Weather, GeneralEffect, TerrainMod};
	private List<PropertyModifierKV> modifiers;
	public UnitPropertyModifier ()
	{
		modifiers = new List<UnitPropertyModifier.PropertyModifierKV>();
	}
	public int ApplyModifiers(UnitPropertyModifier.PropertyModifiers modifyingProperty, int initialValue)
	{
		int outvalue = initialValue;
		foreach(PropertyModifierKV pmkv in modifiers)
		{
			if(pmkv.modifiedProperty == modifyingProperty)
			{
				if(pmkv.isAbsolute)
				{
					outvalue = pmkv.value;
				}
				else
				{
					outvalue += pmkv.value;
				}
			}
		}
		return outvalue;
	}
	public void AddModifier(UnitPropertyModifier.PropertyModifiers pm, UnitPropertyModifier.ModifierTypes mt, int size, bool isAbsolute)
	{
		PropertyModifierKV pmkv = new PropertyModifierKV(pm, mt, size, isAbsolute);
		modifiers.Add(pmkv);
		modifiers.Sort();
	}
	public void AddModifier(UnitPropertyModifier.PropertyModifiers pm, UnitPropertyModifier.ModifierTypes mt, int size)
	{
		AddModifier(pm, mt, size, false);
	}
	public bool RemoveModifier(UnitPropertyModifier.PropertyModifiers pm, UnitPropertyModifier.ModifierTypes mt)
	{
		PropertyModifierKV valueToRemove = null;
		foreach(PropertyModifierKV pmkv in modifiers)
		{
			if(pmkv.modifiedProperty == pm && pmkv.modifierType == mt)
			{
				valueToRemove = pmkv;
			}
		}
		if(valueToRemove != null)
		{
			modifiers.Remove(valueToRemove);
			return true;
		}
		else
		{
			return false;
		}
	}
	public bool RemoveAllOfModifier(UnitPropertyModifier.PropertyModifiers pm, UnitPropertyModifier.ModifierTypes mt)
	{
		List<PropertyModifierKV> valuesToRemove = new List<PropertyModifierKV>();
		foreach(PropertyModifierKV pmkv in modifiers)
		{
			if(pmkv.modifiedProperty == pm && pmkv.modifierType == mt)
			{
				valuesToRemove.Add(pmkv);
			}
		}
		if(valuesToRemove.Count > 0)
		{
			foreach(PropertyModifierKV pmkv in valuesToRemove)
			{
				modifiers.Remove(pmkv);
			}
			return true;
		}
		else
		{
			return false;
		}
	}
	public bool RemoveAllOfModifierType(UnitPropertyModifier.ModifierTypes mt)
	{
		List<PropertyModifierKV> valuesToRemove = new List<PropertyModifierKV>();
		foreach(PropertyModifierKV pmkv in modifiers)
		{
			if(pmkv.modifierType == mt)
			{
				valuesToRemove.Add(pmkv);
			}
		}
		if(valuesToRemove.Count > 0)
		{
			foreach(PropertyModifierKV pmkv in valuesToRemove)
			{
				modifiers.Remove(pmkv);
			}
			return true;
		}
		else
		{
			return false;
		}
	}
	public void Clear()
	{
		modifiers.Clear();
	}
	class PropertyModifierKV : IComparable<PropertyModifierKV>{
		public bool isAbsolute;
		public UnitPropertyModifier.PropertyModifiers modifiedProperty;
		public UnitPropertyModifier.ModifierTypes modifierType;
		public int value;
		public PropertyModifierKV(UnitPropertyModifier.PropertyModifiers pm, UnitPropertyModifier.ModifierTypes mt, int size, bool isAbsolute)
		{
			modifiedProperty = pm;
			modifierType = mt;
			value = size;
			this.isAbsolute = isAbsolute;
		}
		public int CompareTo(UnitPropertyModifier.PropertyModifierKV other)
		{
			if(this.isAbsolute && other.isAbsolute)
			{
				return 0;
			}
			else if(this.isAbsolute)
			{
				return -1;
			}
			else if(other.isAbsolute)
			{
				return 1;
			}
			return 0;
		}
	}
}
