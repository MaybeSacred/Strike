using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class DetailedInfoBoxViewer : MonoBehaviour
{
	public Text nameField, descriptionField;
	public Text strongAgainstField, weakAgainstField;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	/// <summary>
	/// Sets the box info.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="description">Description.</param>
	public void SetBoxInfo (string name, string description)
	{
		SetBoxInfo (name, description, null, null);
	}
	/// <summary>
	/// Sets the box info.
	/// </summary>
	/// <param name="name">Name.</param>
	/// <param name="description">Description.</param>
	/// <param name="bestAgainst">Best against.</param>
	/// <param name="weakAgainst">Weak against.</param>
	public void SetBoxInfo (string name, string description, List<UnitName> strongAgainst, List<UnitName> weakAgainst)
	{
		nameField.text = name;
		descriptionField.text = description;
		if (strongAgainst == null || strongAgainst.Count == 0) {
			strongAgainstField.text = "--";
		} else {
			string temp = "";
			for (int i = 0; i < strongAgainst.Count - 1; i++) {
				temp += MapData.FormatMapName (strongAgainst [i].ToString ()) + ", ";
			}
			if (strongAgainst.Count > 1) {
				temp += MapData.FormatMapName (strongAgainst [strongAgainst.Count - 1].ToString ());
			}
			strongAgainstField.text = temp;
		}
		if (weakAgainst == null || weakAgainst.Count == 0) {
			weakAgainstField.text = "--";
		} else {
			string temp = "";
			for (int i = 0; i < weakAgainst.Count - 1; i++) {
				temp += MapData.FormatMapName (weakAgainst [i].ToString ()) + ", ";
			}
			if (weakAgainst.Count > 1) {
				temp += MapData.FormatMapName (weakAgainst [weakAgainst.Count - 1].ToString ());
			}
			weakAgainstField.text = temp;
		}
	}
}
