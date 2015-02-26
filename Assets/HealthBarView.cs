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
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void SetPosition (Vector3 position)
	{
		Vector3 unitPointOnScreen = Camera.main.WorldToScreenPoint (position);
		float imageWidth = scale / Mathf.Pow (unitPointOnScreen.z, .75f);
		Debug.Log (imageWidth);
		panel.localScale = new Vector3 (imageWidth, imageWidth, imageWidth);
		panel.anchoredPosition = new Vector2 (unitPointOnScreen.x / GetComponentInParent<Canvas> ().scaleFactor, unitPointOnScreen.y / GetComponentInParent<Canvas> ().scaleFactor - yOffset - imageWidth * imageWidthScale);
		//GUI.BeginGroup (new Rect (unitPointOnScreen.x - imageWidth * 5, Screen.height - unitPointOnScreen.y + imageWidth + 8, 10 * imageWidth, imageWidth * 2));
		
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
