using UnityEngine;
using System.Collections;

public class CameraArrowKeyController : MonoBehaviour {
	public float cameraSpeed;
	void Start () {
	
	}
	
	void Update () {
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			transform.position -= new Vector3(cameraSpeed*Time.deltaTime, 0, 0);
		}
		if(Input.GetKey(KeyCode.RightArrow))
		{
			transform.position += new Vector3(cameraSpeed*Time.deltaTime, 0, 0);
		}
		if(Input.GetKey(KeyCode.UpArrow))
		{
			transform.position += new Vector3(0, 0, cameraSpeed*Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.DownArrow))
		{
			transform.position -= new Vector3(0, 0, cameraSpeed*Time.deltaTime);
		}
	}
}
