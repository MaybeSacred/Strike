using UnityEngine;
using System.Collections;

public class InGameCamera : MonoBehaviour {
	public float cameraSpeed;
	public float mouseSideScrollOffset;
	private float attemptedZoomPosition;//, currentDistanceFromLookAtPoint;
	public float closeYPoint, farYPoint;
	public float scrollSpeed;
	private float scrollRampup;											
	private bool scrolling;
	private Vector2 lastMousePosition;
	public Transform lookAtTransform;
	public Vector3 lookAtPoint {get; private set;}
	private Vector3 lookAtPointFollower;
	private Vector3 eulerAngles;
	public float rightClickMovementSpeed;
	public float lookAtTrackingSpeed;
	public bool otherMenuActive;
	// How much we should take the y-axis height into account for bounds checking
	public float cameraBoundsFactor;
	// The root canvas
	public RectTransform parentCanvas;
	void Awake(){
		parentCanvas = Instantiate(parentCanvas) as RectTransform;
	}
	void Start () {
		lookAtPoint = Vector3.zero;
		lookAtPointFollower = Vector3.zero;
		eulerAngles = new Vector3(45, 0, 0);
	}
	
	void Update () {
		if(InGameController.isPaused)
		{
			lookAtPointFollower = transform.position - (attemptedZoomPosition)*(transform.rotation*-Vector3.forward);
			lookAtPoint = lookAtPointFollower;
		}
		else
		{
			if(!otherMenuActive)
			{
				if(Input.GetMouseButton(0) && !InGameController.UnitSelected())
				{
					scrolling = true;
					float moveSpeed = (cameraSpeed + 1.5f*transform.position.y) * Time.deltaTime * rightClickMovementSpeed;
					lookAtPoint -= new Vector3(Mathf.Sin(eulerAngles.y*Mathf.Deg2Rad), 0, Mathf.Cos(-eulerAngles.y*Mathf.Deg2Rad)) * (Input.mousePosition.y - lastMousePosition.y) * moveSpeed;
					lookAtPoint -= new Vector3(Mathf.Cos(-eulerAngles.y*Mathf.Deg2Rad), 0, Mathf.Sin(-eulerAngles.y*Mathf.Deg2Rad)) * (Input.mousePosition.x - lastMousePosition.x) * moveSpeed;
				}
				else
				{
					if(Input.GetKey(KeyCode.LeftArrow) || (Input.mousePosition.x < mouseSideScrollOffset && Input.mousePosition.x >=0))
					{
						scrollRampup += Time.deltaTime;
						scrollRampup = Mathf.Clamp01(scrollRampup);
						scrolling = true;
						float moveSpeed = (cameraSpeed + transform.position.y) * scrollRampup * Time.deltaTime;
						if(Input.mousePosition.x < mouseSideScrollOffset){
							moveSpeed *= (mouseSideScrollOffset - Input.mousePosition.x)/((float)mouseSideScrollOffset);
						}
						lookAtPoint -= new Vector3(Mathf.Cos(-eulerAngles.y*Mathf.Deg2Rad), 0, Mathf.Sin(-eulerAngles.y*Mathf.Deg2Rad)) * moveSpeed;
					}
					if(Input.GetKey(KeyCode.RightArrow) || (Input.mousePosition.x > Screen.width - mouseSideScrollOffset  && Input.mousePosition.x <= Screen.width))
					{
						scrollRampup+= Time.deltaTime;
						scrollRampup = Mathf.Clamp01(scrollRampup);
						scrolling = true;
						float moveSpeed = (cameraSpeed + transform.position.y) * scrollRampup * Time.deltaTime;
						if(Input.mousePosition.x > Screen.width - mouseSideScrollOffset){
							moveSpeed *= (mouseSideScrollOffset - (Screen.width - Input.mousePosition.x))/((float)mouseSideScrollOffset);
						}
						lookAtPoint += new Vector3(Mathf.Cos(-eulerAngles.y*Mathf.Deg2Rad), 0, Mathf.Sin(-eulerAngles.y*Mathf.Deg2Rad)) * moveSpeed;
					}
					if(Input.GetKey(KeyCode.UpArrow) || (Input.mousePosition.y > Screen.height - mouseSideScrollOffset  && Input.mousePosition.y <= Screen.height))
					{
						scrollRampup+= Time.deltaTime;
						scrollRampup = Mathf.Clamp01(scrollRampup);
						scrolling = true;
						float moveSpeed = (cameraSpeed + transform.position.y) * scrollRampup * Time.deltaTime;
						if(Input.mousePosition.y > Screen.height - mouseSideScrollOffset){
							moveSpeed *= (mouseSideScrollOffset - (Screen.height - Input.mousePosition.y))/((float)mouseSideScrollOffset);
						}
						lookAtPoint += new Vector3(Mathf.Sin(eulerAngles.y*Mathf.Deg2Rad), 0, Mathf.Cos(-eulerAngles.y*Mathf.Deg2Rad)) * moveSpeed;
					}
					if(Input.GetKey(KeyCode.DownArrow) || (Input.mousePosition.y < mouseSideScrollOffset  && Input.mousePosition.y >=0))
					{
						scrollRampup += Time.deltaTime;
						scrollRampup = Mathf.Clamp01(scrollRampup);
						scrolling = true;
						float moveSpeed = (cameraSpeed + transform.position.y) * scrollRampup * Time.deltaTime;
						if(Input.mousePosition.y < mouseSideScrollOffset){
							moveSpeed *= (mouseSideScrollOffset - Input.mousePosition.y)/((float)mouseSideScrollOffset);
						}
						lookAtPoint -= new Vector3(Mathf.Sin(eulerAngles.y*Mathf.Deg2Rad), 0, Mathf.Cos(-eulerAngles.y*Mathf.Deg2Rad)) * moveSpeed;
					}
				}
			}
			LookingBoundsCheck();
			lastMousePosition = Input.mousePosition;
			attemptedZoomPosition = Mathf.Clamp(attemptedZoomPosition - Input.GetAxis("Mouse ScrollWheel")*6, closeYPoint, farYPoint);
			lookAtPointFollower = Vector3.Lerp(lookAtPointFollower, lookAtPoint, Time.deltaTime * lookAtTrackingSpeed);
			transform.position = Vector3.Lerp(transform.position, lookAtPointFollower + (attemptedZoomPosition)*(transform.rotation*-Vector3.forward), Time.deltaTime * scrollSpeed);
			//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eulerAngles), Time.deltaTime * cameraSpeed);
			//transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0);
		}
	}
	public void CenterCameraOnPoint(Vector3 point)
	{
		lookAtPoint = new Vector3(point.x, 0, point.z);
		LookingBoundsCheck();
	}
	
	private void LookingBoundsCheck()
	{
		if(lookAtPoint.x < InGameController.currentTerrain.lowerXMapBound + cameraBoundsFactor * transform.position.y)
		{
			lookAtPoint = new Vector3(InGameController.currentTerrain.lowerXMapBound + cameraBoundsFactor * transform.position.y, lookAtPoint.y, lookAtPoint.z);
		}
		else if(lookAtPoint.x > InGameController.currentTerrain.upperXMapBound - cameraBoundsFactor * transform.position.y)
		{
			lookAtPoint = new Vector3(InGameController.currentTerrain.upperXMapBound - cameraBoundsFactor * transform.position.y, lookAtPoint.y, lookAtPoint.z);
		}
		if(lookAtPoint.z < InGameController.currentTerrain.lowerZMapBound + cameraBoundsFactor * transform.position.y)
		{
			lookAtPoint = new Vector3(lookAtPoint.x, lookAtPoint.y, InGameController.currentTerrain.lowerZMapBound + cameraBoundsFactor * transform.position.y);
		}
		else if(lookAtPoint.z > InGameController.currentTerrain.upperZMapBound - cameraBoundsFactor * transform.position.y)
		{
			lookAtPoint = new Vector3(lookAtPoint.x, lookAtPoint.y, InGameController.currentTerrain.upperZMapBound - cameraBoundsFactor * transform.position.y);
		}
	}
}
