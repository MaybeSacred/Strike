using UnityEngine;
using System.Collections;

public class GeneralBarView : MonoBehaviour
{
	public RectTransform mask, powerImage;
	// Use this for initialization
	void Start ()
	{
	
	}
	/// <summary>
	/// Sets the graphics to display the appropriate power level
	/// </summary>
	/// <param name="level">Level.</param>
	public void SetPowerLevel (float level)
	{
		// If theres something to actually display, turn gameobject on
		if (level > 0) {
			gameObject.SetActive (true);
			float width = GetComponent<RectTransform> ().rect.width;
			mask.offsetMax = new Vector2 ((1 - level) * width, mask.offsetMax.y);
			powerImage.offsetMax = new Vector2 (-mask.offsetMax.x, powerImage.offsetMax.y);
		} else {
			gameObject.SetActive (false);
		}
	}
}
