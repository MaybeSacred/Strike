using UnityEngine;
using System.Collections;

public class HealthBarView : MonoBehaviour
{
	public GameObject[] hpBars;
	RectTransform panel;
	public int yOffset;
	public float scale;
	public float imageWidthScale;
	void Awake ()
	{
		panel = GetComponent<RectTransform> ();
	}
	
	public void SetPosition (Vector3 position)
	{
		Vector3 unitPointOnScreen = Camera.main.WorldToScreenPoint (position);
		float imageWidth = scale / Mathf.Pow (unitPointOnScreen.z, .75f);
		panel.localScale = new Vector3 (imageWidth, imageWidth, imageWidth);
		panel.anchoredPosition = new Vector2 (unitPointOnScreen.x / GetComponentInParent<Canvas> ().scaleFactor, unitPointOnScreen.y / GetComponentInParent<Canvas> ().scaleFactor - yOffset / unitPointOnScreen.z - imageWidth * imageWidthScale);
	}
	
	public void SetHealthDisplayed (int healthToDisplay)
	{
		for (int i = 0; i < hpBars.Length; i++) {
			if (i < healthToDisplay) {
				hpBars [i].SetActive (true);
			} else {
				hpBars [i].SetActive (false);
			}
		}
	}
}
