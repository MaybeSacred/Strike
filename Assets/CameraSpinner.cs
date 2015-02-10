using UnityEngine;
using System.Collections;
/// <summary>
/// Spins the camera around its local axis
/// </summary>
public class CameraSpinner : MonoBehaviour
{
	public float spinRate;
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		transform.eulerAngles += new Vector3 (0, spinRate * Time.deltaTime, 0);
	}
}
