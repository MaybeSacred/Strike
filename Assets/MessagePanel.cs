using UnityEngine;
using System;
using UnityEngine.UI;
/// <summary>
/// Represents a re-useable panel for displaying messages
/// </summary>
public class MessagePanel : MonoBehaviour
{
	static MessagePanel instance;
	Action onAccept, onDecline;
	Text messageType, message;
	Button accept, decline;
	void Start ()
	{
		if (instance == null) {
			instance = this;
			foreach (var button in GetComponentsInChildren<Button>(true)) {
				if (button.name.Contains ("ccept")) {
					accept = button;
				} else if (button.name.Contains ("ecline")) {
					decline = button;
				}
			}
			foreach (var text in GetComponentsInChildren<Text>(true)) {
				if (text.name.Contains ("Type")) {
					messageType = text;
				} else if (text.name.Equals ("Message")) {
					message = text;
				}
			}
			GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
			HidePanel ();
		} else {
			Destroy (gameObject);
		}
	}
	/// <summary>
	/// Shows the panel, with option for declination functionality
	/// </summary>
	/// <param name="messageType">Message type.</param>
	/// <param name="message">Message.</param>
	/// <param name="onAccept">On accept.</param>
	/// <param name="onDecline">On decline.</param>
	public static void ShowPanel (string messageType, string message, Action onAccept = null, Action onDecline = null, string acceptButtonText = "OK", string declineButtonText = "Cancel")
	{
		if (onAccept == null) {
			onAccept = () => {};
		}
		instance.onAccept = onAccept;
		instance.onDecline = onDecline;
		instance.gameObject.SetActive (true);
		// If we dont have a decline function provided, dont show decline button
		if (onDecline == null) {
			instance.decline.gameObject.SetActive (false);
		} else {
			instance.decline.gameObject.SetActive (true);
		}
		instance.message.text = message;
		instance.messageType.text = messageType;
		instance.accept.GetComponentsInChildren<Text> (true) [0].text = acceptButtonText;
		instance.decline.GetComponentsInChildren<Text> (true) [0].text = declineButtonText;
	}
	public void OnAccept ()
	{
		onAccept ();
		HidePanel ();
	}
	public void OnDecline ()
	{
		onDecline ();
		HidePanel ();
	}
	void HidePanel ()
	{
		gameObject.SetActive (false);
	}
}
