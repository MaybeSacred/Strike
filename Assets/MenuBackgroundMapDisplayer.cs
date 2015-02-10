using UnityEngine;
using System.Collections;

public class MenuBackgroundMapDisplayer : MonoBehaviour
{
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	public void DisplayMap (MapData data)
	{
		Transform[] t = GetComponentsInChildren<Transform> ();
		for (int i = 1; i < t.Length; i++) {
			Destroy (t [i].gameObject);
		}
		for (int i = 0; i < data.mapData.Length; i++) {
			for (int j = 0; j < data.mapData[i].Length; j++) {
				GameObject temp = Instantiate (Resources.Load<GameObject> (data.mapData [i] [j].name)) as GameObject;
				temp.GetComponent<TerrainBlock> ().enabled = false;
				temp.transform.parent = transform;
				temp.transform.localPosition = new Vector3 (i - data.mapData.Length / 2, 0, j - data.mapData [i].Length / 2);
				temp.transform.localRotation = data.mapData [i] [j].rotation.ToQuaternion ();
			}
		}
		for (int i = 0; i < data.properties.Length; i++) {
			GameObject temp = Instantiate (Resources.Load<GameObject> (data.properties [i].name)) as GameObject;
			temp.GetComponent<Property> ().enabled = false;
			temp.transform.parent = transform;
			Vector3 vec = data.properties [i].position.ToVector3 ();
			temp.transform.localPosition = new Vector3 (vec.x - data.mapData.Length / 2, vec.y, vec.z - data.mapData [0].Length / 2);
			temp.transform.localRotation = data.properties [i].rotation.ToQuaternion ();
		}
		transform.localPosition = new Vector3 (0, transform.localPosition.y, Mathf.Max (data.mapData.Length, data.mapData [0].Length) + 1);
	}
}
