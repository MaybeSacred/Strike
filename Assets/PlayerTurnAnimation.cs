using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class PlayerTurnAnimation : MonoBehaviour
{
	Queue<string> queuedDisplayNames;
	Animation anim;
	public Text playerName, playerNameShadow;
	void Awake ()
	{
		anim = GetComponent<Animation> ();
		queuedDisplayNames = new Queue<string> ();
	}
	// Use this for initialization
	void Start ()
	{
	
	}
	public void AddName (string inName)
	{
		queuedDisplayNames.Enqueue (inName);
		gameObject.SetActive (true);
		StartCoroutine (StartAnimation ());
	}
	IEnumerator StartAnimation ()
	{
		if (anim.isPlaying) {
			yield return new WaitForEndOfFrame ();
		}
		if (queuedDisplayNames.Peek () != null) {
			string temp = queuedDisplayNames.Dequeue ();
			if (temp.EndsWith ("s")) {
				temp += "' Turn";
			} else {
				temp += "'s Turn";
			}
			playerName.text = temp;
			playerNameShadow.text = temp;
			anim.Play ();
			yield return new WaitForEndOfFrame ();
		}
		if (!anim.isPlaying) {
			gameObject.SetActive (false);
		}
	}
	
}
