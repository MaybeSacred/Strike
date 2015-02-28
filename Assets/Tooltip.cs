using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
public class Tooltip : MonoBehaviour
{
	Text tooltipText;
	RectTransform thisRectTransform;
	public float innerHorizontalMargin, innerVerticalMargin;
	// Use this for initialization
	void Awake ()
	{
		tooltipText = GetComponentInChildren<Text> ();
		thisRectTransform = GetComponent<RectTransform> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	/// <summary>
	/// Sets the position of the tooltip
	/// </summary>
	/// <param name="desiredPosition">Desired position.</param>
	public void SetPosition (Vector3 desiredPosition)
	{
		thisRectTransform.anchoredPosition3D = desiredPosition;
		if (thisRectTransform.rect.center.x + thisRectTransform.rect.width / 2 >= Screen.width) {
			thisRectTransform.anchoredPosition3D -= new Vector3 (Screen.width - thisRectTransform.rect.center.x + thisRectTransform.rect.width / 2, 0, 0);
		} else if (thisRectTransform.rect.center.x - thisRectTransform.rect.width / 2 <= 0) {
			thisRectTransform.anchoredPosition3D += new Vector3 (-thisRectTransform.rect.center.x + thisRectTransform.rect.width / 2, 0, 0);
		}
		if (thisRectTransform.rect.center.y + thisRectTransform.rect.height / 2 >= Screen.height) {
			thisRectTransform.anchoredPosition3D -= new Vector3 (0, Screen.height - thisRectTransform.rect.center.y + thisRectTransform.rect.width / 2, 0);
		} else if (thisRectTransform.rect.center.y - thisRectTransform.rect.height / 2 <= 0) {
			thisRectTransform.anchoredPosition3D += new Vector3 (0, -thisRectTransform.rect.center.y + thisRectTransform.rect.height / 2, 0);
		}
	}
	/// <summary>
	/// Sets the text of the tooltip
	/// </summary>
	/// <param name="textToDisplay">Text to display.</param>
	public void SetText (string textToDisplay)
	{
		tooltipText.text = textToDisplay;
		thisRectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, tooltipText.preferredWidth + innerHorizontalMargin * 2);
		thisRectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, tooltipText.preferredHeight + innerVerticalMargin * 2);
	}
}
