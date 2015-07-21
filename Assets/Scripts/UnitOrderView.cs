using UnityEngine;
using System;
using System.Collections.Generic;
public class UnitOrderView : MonoBehaviour
{

	public RectTransform buttonPrototype;
	Action<UnitOrderOptions> currentCallback;
	Action unselectedCallback;
	List<RectTransform> currentButtons;
	public float buttonOffset;
	void Awake ()
	{
		currentButtons = new List<RectTransform> ();
	}
	/// <summary>
	/// Displays a list of unit order options with their selection buttons
	/// </summary>
	/// <param name="unitsToDisplay">Order options to display.</param>
	public void DisplayUnitOrderOptionsList (UnitOrderOptions[] options, Action<UnitOrderOptions> callback, Action unselectedCallback)
	{
		gameObject.SetActive (true);
		for (int i = 0; i < currentButtons.Count; i++) {
			Destroy (currentButtons [i].gameObject);
		}
		currentButtons = new List<RectTransform> ();
		for (int i = 0; i < options.Length; i++) {
			var t = SkirmishMenuViewer.InstantiateUIPrefab (buttonPrototype, this.GetComponent<RectTransform> ());
			t.GetComponentsInChildren<UnityEngine.UI.Text> (true) [0].text = MapData.FormatMapName (options [i].ToString ());
			currentButtons.Add (t);
			var captured = options [i];
			//add our delegate to the onClick handler, with appropriate indexing
			Encapsulator (t, captured);
		}
		for (int i = 0; i < currentButtons.Count; i++) {
			currentButtons [i].anchorMin = new Vector2 (0, 1 - (i + 1) * buttonOffset);
			currentButtons [i].anchorMax = new Vector2 (1, 1 - i * buttonOffset);
			currentButtons [i].offsetMin = new Vector2 (5, 1);
			currentButtons [i].offsetMax = new Vector2 (-5, -1);
		}
		currentCallback = callback;
		this.unselectedCallback = unselectedCallback;
	}
	/// <summary>
	/// Overcomes a possible bug between mono/Unity and lambdas/anonymous functions
	/// </summary>
	/// <param name="t">T.</param>
	/// <param name="input">Input.</param>
	void Encapsulator (RectTransform t, UnitOrderOptions input)
	{
		t.GetComponent<UnityEngine.UI.Button> ().onClick.AddListener (() => {
			OnUnitSelected (input);
		});
	}
	public void OnUnitSelected (UnitOrderOptions inName)
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
