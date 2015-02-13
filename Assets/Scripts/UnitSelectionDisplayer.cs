using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnitSelectionDisplayer : MonoBehaviour
{
	public GameObject buttonPrototype;
	Action<UnitName> currentCallback;
	public float buttonOffset;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	/// <summary>
	/// Displays a list of unit names with their buttons
	/// </summary>
	/// <param name="unitsToDisplay">Units to display.</param>
	public void DisplayUnitList (UnitName[] unitsToDisplay, Action<UnitName> callback)
	{
		gameObject.SetActive (true);
		var unitButtons = new List<RectTransform> ();
		for (int i = 0; i < unitsToDisplay.Length; i++) {
			var t = SkirmishMenuViewer.InstantiateUIPrefab (buttonPrototype.GetComponent<RectTransform> (), this.GetComponent<RectTransform> ());
			t.GetComponentsInChildren<UnityEngine.UI.Text> (true) [0].text = MapData.FormatMapName (unitsToDisplay [i].ToString ());
			unitButtons.Add (t);
			var captured = unitsToDisplay [i];
			//add our delegate to the onClick handler, with appropriate indexing
			Encapsulator (t, captured);
		}
		if (unitsToDisplay.Length <= 8) {
			for (int i = 0; i < unitButtons.Count; i++) {
				unitButtons [i].anchorMin = new Vector2 (0.25f, .99f - (i + 1) * buttonOffset);
				unitButtons [i].anchorMax = new Vector2 (.75f, .99f - i * buttonOffset);
				unitButtons [i].offsetMin = new Vector2 (14, 3);
				unitButtons [i].offsetMax = new Vector2 (14, 0);
			}
		} else {
			for (int i = 0; i < unitButtons.Count; i++) {
				if (i < unitButtons.Count / 2 + 1) {
					unitButtons [i].anchorMin = new Vector2 (0, .99f - (i + 1) * buttonOffset);
					unitButtons [i].anchorMax = new Vector2 (.5f, .99f - i * buttonOffset);
					unitButtons [i].offsetMin = new Vector2 (14, 3);
					unitButtons [i].offsetMax = new Vector2 (6, 0);
				} else {
					unitButtons [i].anchorMin = new Vector2 (0.5f, .99f - (i - unitButtons.Count / 2) * buttonOffset);
					unitButtons [i].anchorMax = new Vector2 (1, .99f - (i - unitButtons.Count / 2 - 1) * buttonOffset);
					unitButtons [i].offsetMin = new Vector2 (6, 3);
					unitButtons [i].offsetMax = new Vector2 (14, 0);
				}
			}
		}
		currentCallback = callback;
	}
	/// <summary>
	/// Overcomes a possible bug between mono/Unity and lambdas/anonymous functions
	/// </summary>
	/// <param name="t">T.</param>
	/// <param name="input">Input.</param>
	void Encapsulator (RectTransform t, UnitName input)
	{
		t.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener (() => {
			OnUnitSelected (input);
		});
	}
	public void OnUnitSelected (UnitName inName)
	{
		currentCallback (inName);
		OnClose ();
	}
	/// <summary>
	/// Raises the close event when the back button is pressed or a unit is selected
	/// </summary>
	public void OnClose ()
	{
		gameObject.SetActive (false);
	}
}
