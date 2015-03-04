using UnityEngine;
using System.Collections;

public class OptionsMenu : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
	
	}
	/// <summary>
	/// Returns to the splash menu
	/// </summary>
	public void OnReturnToMenu ()
	{
		Application.LoadLevel ("Splash");
	}
}
