using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
public class Tooltip : MonoBehaviour
{
	Text tooltipText;
	RectTransform thisRectTransform;
	public float innerHorizontalMargin, innerVerticalMargin;
	public float maxWidth;
	public float whatever { get; set; }
	// Use this for initialization
	void Awake ()
	{
		tooltipText = GetComponentInChildren<Text> ();
		thisRectTransform = GetComponent<RectTransform> ();
	}
	/// <summary>
	/// Sets the position of the tooltip
	/// </summary>
	/// <param name="desiredPosition">Desired position.</param>
	public void SetPosition (Vector3 desiredPosition)
	{
		thisRectTransform.anchoredPosition3D = desiredPosition / GetComponentInParent<Canvas> ().scaleFactor;
		Debug.Log (thisRectTransform.anchoredPosition3D);
		if (thisRectTransform.anchoredPosition3D.x + thisRectTransform.rect.width / 2 >= Screen.width) {
			Debug.Log (">width");
			thisRectTransform.anchoredPosition3D -= new Vector3 (Screen.width - thisRectTransform.anchoredPosition3D.x + thisRectTransform.rect.width / 2, 0, 0);
		} else if (thisRectTransform.anchoredPosition3D.x - thisRectTransform.rect.width / 2 <= 0) {
			Debug.Log ("<width");
			thisRectTransform.anchoredPosition3D += new Vector3 (-thisRectTransform.anchoredPosition3D.x + thisRectTransform.rect.width / 2, 0, 0);
		}
		if (thisRectTransform.anchoredPosition3D.y + thisRectTransform.rect.height / 2 >= Screen.height) {
			Debug.Log (">height");
			thisRectTransform.anchoredPosition3D -= new Vector3 (0, Screen.height - thisRectTransform.anchoredPosition3D.y + thisRectTransform.rect.width / 2, 0);
		} else if (thisRectTransform.anchoredPosition3D.y - thisRectTransform.rect.height / 2 <= 0) {
			Debug.Log ("<height");
			thisRectTransform.anchoredPosition3D += new Vector3 (0, -thisRectTransform.anchoredPosition3D.y + thisRectTransform.rect.height / 2, 0);
		}
	}
	/// <summary>
	/// Sets the text of the tooltip
	/// </summary>
	/// <param name="textToDisplay">Text to display.</param>
	public void SetText (string textToDisplay)
	{
		tooltipText.text = textToDisplay;
		if (tooltipText.preferredWidth < maxWidth) {
			thisRectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, tooltipText.preferredWidth + innerHorizontalMargin * 2);
		} else {
			thisRectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, maxWidth + innerHorizontalMargin * 2);
		}
		thisRectTransform.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, tooltipText.preferredHeight + innerVerticalMargin * 2);
	}
}
