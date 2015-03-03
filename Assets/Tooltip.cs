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
	Canvas parentCanvas;
	
	// Use this for initialization
	void Awake ()
	{
		tooltipText = GetComponentInChildren<Text> ();
		thisRectTransform = GetComponent<RectTransform> ();
		parentCanvas = GetComponentInParent<Canvas> ();
	}
	void Start ()
	{
		
	}
	/// <summary>
	/// Sets the position of the tooltip
	/// </summary>
	/// <param name="desiredPosition">Desired position.</param>
	public void SetPosition (Vector3 desiredPosition)
	{
		thisRectTransform.anchoredPosition3D = desiredPosition / parentCanvas.scaleFactor;
		if (thisRectTransform.anchoredPosition3D.x + thisRectTransform.rect.width / 2 >= Screen.width / parentCanvas.scaleFactor) {
			thisRectTransform.anchoredPosition3D = new Vector3 ((Screen.width / parentCanvas.scaleFactor - thisRectTransform.rect.width / 2), thisRectTransform.anchoredPosition3D.y, 0);
		} else if (thisRectTransform.anchoredPosition3D.x - thisRectTransform.rect.width / 2 <= 0) {
			thisRectTransform.anchoredPosition3D = new Vector3 (thisRectTransform.rect.width / 2, thisRectTransform.anchoredPosition3D.y, 0);//+= new Vector3 ((-thisRectTransform.anchoredPosition3D.x + thisRectTransform.rect.width / 2) / parentCanvas.scaleFactor, 0, 0);
		}
		if (thisRectTransform.anchoredPosition3D.y + thisRectTransform.rect.height / 2 >= Screen.height / parentCanvas.scaleFactor) {
			thisRectTransform.anchoredPosition3D = new Vector3 (thisRectTransform.anchoredPosition3D.x, (Screen.height / parentCanvas.scaleFactor - thisRectTransform.rect.height / 2), 0);
		} else if (thisRectTransform.anchoredPosition3D.y - thisRectTransform.rect.height / 2 <= 0) {
			thisRectTransform.anchoredPosition3D = new Vector3 (thisRectTransform.anchoredPosition3D.x, thisRectTransform.rect.height / 2, 0);//+= new Vector3 (0, (-thisRectTransform.anchoredPosition3D.y + thisRectTransform.rect.height / 2), 0);
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
	/// <summary>
	/// Factory method for new tooltip
	/// </summary>
	/// <returns>The new tooltip.</returns>
	/// <param name="tooltipText">Tooltip text.</param>
	public static GameObject AddNewTooltip (GameObject objectToExtend, string tooltipText)
	{
		objectToExtend.AddComponent<TooltipData> ().mouseOverText = tooltipText;
		return objectToExtend;
	}
}
