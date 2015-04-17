using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

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
		var first = hits.FirstOrDefault (p => {
			if (p.gameObject.GetComponent<TooltipData> () != null) {
				if (lastObjectMouseWasOver != p.gameObject) {
					mouseOverTimer = 0;
					tooltipWasDisplayed = false;
					lastObjectMouseWasOver = p.gameObject;
					tooltip.gameObject.SetActive (false);
				}
				return true;
			}
			return false;
		});
		if (first.isValid) {
			mouseOverTimer += Time.deltaTime;
			if (mouseOverTimer > mouseOverStartTime && !tooltipWasDisplayed) {
				tooltipWasDisplayed = true;
				tooltip.gameObject.SetActive (true);
				tooltip.SetPositionAndText (pe.position, lastObjectMouseWasOver.GetComponent<TooltipData> ().mouseOverText);
			}
		} else {
			lastObjectMouseWasOver = null;
			tooltipWasDisplayed = false;
			tooltip.gameObject.SetActive (false);
		}
	}
}
