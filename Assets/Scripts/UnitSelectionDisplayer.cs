using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnitSelectionDisplayer : MonoBehaviour
{
	public GameObject buttonPrototype;
	Action<UnitName> currentCallback;
	Action unselectedCallback;
	List<RectTransform> currentButtons;
	public float buttonOffset;
	void Awake ()
	{
		currentButtons = new List<RectTransform> ();
	}
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
	public void DisplayUnitList (UnitName[] unitsToDisplay, int maxFunds, Action<UnitName> callback, Action unselectedCallback)
	{
		gameObject.SetActive (true);
		for (int i = 0; i < currentButtons.Count; i++) {
			Destroy (currentButtons [i].gameObject);
		}
		currentButtons = new List<RectTransform> ();
		for (int i = 0; i < unitsToDisplay.Length; i++) {
			var t = SkirmishMenuViewer.InstantiateUIPrefab (buttonPrototype.GetComponent<RectTransform> (), this.GetComponent<RectTransform> ());
			t.GetComponentsInChildren<UnityEngine.UI.Text> (true) [0].text = MapData.FormatMapName (unitsToDisplay [i].ToString ());
			t.GetComponentsInChildren<UnityEngine.UI.Text> (true) [1].text = ((UnitController)Utilities.GetPrefabFromUnitName (unitsToDisplay [i])).baseCost.ToString ();
			currentButtons.Add (t);
			var captured = unitsToDisplay [i];
			//add our delegate to the onClick handler, with appropriate indexing
			Encapsulator (t, captured);
			if (maxFunds < ((UnitController)Utilities.GetPrefabFromUnitName (unitsToDisplay [i])).baseCost) {
				t.GetComponent<UnityEngine.UI.Button> ().interactable = false;
			}
		}
		/*if (unitsToDisplay.Length <= 8) {
			for (int i = 0; i < currentButtons.Count; i++) {
				currentButtons [i].anchorMin = new Vector2 (0.25f, .99f - (i + 1) * buttonOffset);
				currentButtons [i].anchorMax = new Vector2 (.75f, .99f - i * buttonOffset);
				currentButtons [i].offsetMin = new Vector2 (10, 0);
				currentButtons [i].offsetMax = new Vector2 (-10, -4);
			}
		} else {*/
		for (int i = 0; i < currentButtons.Count; i++) {
			if (i < currentButtons.Count / 2 + 1) {
				currentButtons [i].anchorMin = new Vector2 (0, .99f - (i + 1) * buttonOffset);
				currentButtons [i].anchorMax = new Vector2 (.5f, .99f - i * buttonOffset);
				currentButtons [i].offsetMin = new Vector2 (10, 0);
				currentButtons [i].offsetMax = new Vector2 (-4, -4);
			} else {
				currentButtons [i].anchorMin = new Vector2 (0.5f, .99f - (i - currentButtons.Count / 2) * buttonOffset);
				currentButtons [i].anchorMax = new Vector2 (1, .99f - (i - currentButtons.Count / 2 - 1) * buttonOffset);
				currentButtons [i].offsetMin = new Vector2 (4, 0);
				currentButtons [i].offsetMax = new Vector2 (-10, -4);
			}
		}
		currentCallback = callback;
		this.unselectedCallback = unselectedCallback;
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
		unselectedCallback ();
		gameObject.SetActive (false);
	}
}
