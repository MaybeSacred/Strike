using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class TooltipDisplayer : MonoBehaviour
{
	GameObject lastObjectMouseWasOver;
	float mouseOverTimer;
	// A check to make sure the tooltip is displayed only once per hovered-over object
	bool tooltipWasDisplayed;
	public float mouseOverStartTime;
	public Tooltip tooltip;
	// Use this for initialization
	void Start ()
	{
		tooltip.gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		PointerEventData pe = new PointerEventData (EventSystem.current);
		pe.position = Input.mousePosition;
		List<RaycastResult> hits = new List<RaycastResult> ();
		EventSystem.current.RaycastAll (pe, hits);
		bool gameObjectFound = false;
		foreach (var p in hits) {
			if (p.gameObject.GetComponent<TooltipData> () != null) {
				if (lastObjectMouseWasOver != p.gameObject) {
					mouseOverTimer = 0;
					tooltipWasDisplayed = false;
					lastObjectMouseWasOver = p.gameObject;
					tooltip.gameObject.SetActive (false);
				}
				gameObjectFound = true;
				break;
			}
		}
		if (gameObjectFound) {
			mouseOverTimer += Time.deltaTime;
			if (mouseOverTimer > mouseOverStartTime && !tooltipWasDisplayed) {
				tooltipWasDisplayed = true;
				tooltip.gameObject.SetActive (true);
				tooltip.SetText (lastObjectMouseWasOver.GetComponent<TooltipData> ().mouseOverText);
				tooltip.SetPosition (pe.position);
			}
		} else {
			lastObjectMouseWasOver = null;
			tooltipWasDisplayed = false;
			tooltip.gameObject.SetActive (false);
		}
	}
}
