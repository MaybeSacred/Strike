using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
[CustomEditor(typeof(MapDataDummy))]
[CanEditMultipleObjects]
public class MapDataExporter : Editor
{
	// Add menu item named "My Window" to the Window menu
	bool clearBlock;
	public static int maxMapX, maxMapZ;
	GameObject terrain, gameController;
	private List<GameObject> terrainPrefabs, buildingPrefabs;
	private List<Texture2D> terrainImages, buildingImages;
	private GameObject selectedBlockToPaint;
	int selectedBlockIndex = -1, selectedBuildingIndex = -1;
	private Plane groundPlane;
	bool deleteProperty;
	void OnEnable ()
	{
		terrain = GameObject.Find ("Terrain");
		maxMapX = 0;
		maxMapZ = 0;
		foreach (TerrainBlock tb in terrain.GetComponentsInChildren<TerrainBlock>()) {
			if (tb.transform.position.x > maxMapX) {
				maxMapX = Mathf.RoundToInt (tb.transform.position.x);
			}
			if (tb.transform.position.z > maxMapZ) {
				maxMapZ = Mathf.RoundToInt (tb.transform.position.z);
			}
		}
		maxMapX++;
		maxMapZ++;
		gameController = GameObject.Find ("InGameController");
		string[] rawPrefabFiles = Directory.GetFiles (Application.dataPath + @"\Editor Default Resources\", "*.prefab");
		List<string> blockNames = new List<string> ();
		List<string> buildingNames = new List<string> ();
		for (int i = 0; i < rawPrefabFiles.Length; i++) {
			if (rawPrefabFiles [i].Contains ("Block")) {
				blockNames.Add (rawPrefabFiles [i]);
			}
			if (rawPrefabFiles [i].Contains ("Prop")) {
				buildingNames.Add (rawPrefabFiles [i]);
			}
		}
		groundPlane = new Plane (Vector3.up, 0);
		terrainPrefabs = new List<GameObject> ();
		buildingPrefabs = new List<GameObject> ();
		blockNames = ExtractPrettyMapNames (blockNames);
		buildingNames = ExtractPrettyMapNames (buildingNames);
		LoadGameObjectsFromFiles (blockNames, terrainPrefabs);
		LoadGameObjectsFromFiles (buildingNames, buildingPrefabs);
		terrainImages = new List<Texture2D> ();
		
		foreach (GameObject t in terrainPrefabs) {
			t.SetActive (false);
			t.SetActive (true);
			Color32[] col = AssetPreview.GetAssetPreview (t).GetPixels32 ();
			Texture2D temp = new Texture2D (AssetPreview.GetAssetPreview (t).height, AssetPreview.GetAssetPreview (t).height);
			temp.SetPixels32 (col);
			temp.Apply ();
			TextureScale.Bilinear (temp, 64, 64);
			terrainImages.Add (temp);
		}
		buildingImages = new List<Texture2D> ();
		foreach (GameObject t in buildingPrefabs) {
			t.SetActive (false);
			t.SetActive (true);
			Color32[] col = AssetPreview.GetAssetPreview (t).GetPixels32 ();
			Texture2D temp = new Texture2D (AssetPreview.GetAssetPreview (t).height, AssetPreview.GetAssetPreview (t).height);
			temp.SetPixels32 (col);
			temp.Apply ();
			TextureScale.Bilinear (temp, 64, 64);
			buildingImages.Add (temp);
		}
	}
	void OnDisable ()
	{
		for (int i = 0; i < buildingImages.Count; i++) {
			DestroyImmediate (buildingImages [i]);
		}
		for (int i = 0; i < terrainImages.Count; i++) {
			DestroyImmediate (terrainImages [i]);
		}
	}
	void LoadGameObjectsFromFiles (List<string> names, List<GameObject> inList)
	{
		for (int i = 0; i < names.Count; i++) {
			if (File.Exists (Application.dataPath + @"\Editor Default Resources\" + names [i] + ".prefab")) {
				GameObject block = EditorGUIUtility.Load (names [i] + ".prefab") as GameObject;
				inList.Add (block);
			} else {
				Debug.Log ("No data found for block: " + names [i]);
			}
		}
	}
	List<string> ExtractPrettyMapNames (List<string> inNames)
	{
		for (int i = 0; i < inNames.Count; i++) {
			string[] temp = inNames [i].Split (new string[]{"\\", "/", "."}, System.StringSplitOptions.RemoveEmptyEntries);
			if (temp.Length > 1) {
				inNames [i] = temp [temp.Length - 2];
			}
		}
		return inNames;
	}
	Vector3 Round (Vector3 inVec)
	{
		inVec.x = Mathf.Round (inVec.x);
		inVec.y = 0;//Mathf.Round(inVec.y);
		inVec.z = Mathf.Round (inVec.z);
		return inVec;
	}
	void OnSceneGUI ()
	{
		if (selectedBlockToPaint != null) {
			Ray sceneRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			float distance;
			if (groundPlane.Raycast (sceneRay, out distance)) {
				Vector3 point = Round (sceneRay.GetPoint (distance));
				GUI.BeginGroup (new Rect (0, 0, 50, 50));
				GUI.Label (new Rect (0, 0, 50, 50), point.x + "," + point.z);
				GUI.EndGroup ();
				point = ClampVector (point);
				if (selectedBlockIndex > -1) {
					selectedBlockToPaint.transform.position = point;
				} else {
					selectedBlockToPaint.transform.position = point + .5f * Vector3.up;
				}
				if (Event.current.button == 0) {
					HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
				}
				if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0) {
					RaycastHit hit;
					if (selectedBlockIndex > -1) {
						if (Physics.Raycast (point + 3 * Vector3.up, Vector3.down, out hit, 10f, 1)) {
							DestroyImmediate (hit.collider.gameObject);
						}
					} else {
						if (Physics.Raycast (point + 5 * Vector3.up, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
							DestroyImmediate (hit.collider.gameObject);
						}
					}
					if (!clearBlock) {
						if (selectedBlockIndex > -1) {
							GameObject temp = PrefabUtility.InstantiatePrefab (terrainPrefabs [selectedBlockIndex]) as GameObject;
							temp.transform.position = point;
							temp.transform.parent = terrain.transform;
						} else {
							GameObject temp = PrefabUtility.InstantiatePrefab (buildingPrefabs [selectedBuildingIndex]) as GameObject;
							temp.transform.position = new Vector3 (point.x, .5f, point.z);
							temp.transform.parent = gameController.transform;
						}
					}
				} else if (Event.current.button == 1) {
					HandleUtility.Repaint ();
					Reset ();
				}
			}
		} else if (deleteProperty) {
			Ray sceneRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);
			float distance;
			if (groundPlane.Raycast (sceneRay, out distance)) {
				Vector3 point = Round (sceneRay.GetPoint (distance));
				point = ClampVector (point);
				if (Event.current.button == 0) {
					HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
				}
				if ((Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown) && Event.current.button == 0) {
					RaycastHit hit;
					if (Physics.Raycast (point + 5 * Vector3.up, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
						DestroyImmediate (hit.collider.gameObject);
					}
				} else if (Event.current.button == 1) {
					HandleUtility.Repaint ();
					Reset ();
				}
			}
		}
	}
	void Reset ()
	{
		clearBlock = false;
		deleteProperty = false;
		if (selectedBlockToPaint != null) {
			DestroyImmediate (selectedBlockToPaint);
		}
		selectedBlockIndex = -1;
		selectedBuildingIndex = -1;
		selectedBlockToPaint = null;
	}
	Vector3 ClampVector (Vector3 inVec)
	{
		if (inVec.x < 0) {
			inVec.x = 0;
		} else if (inVec.x > maxMapX - 1) {
			inVec.x = maxMapX - 1;
		}
		if (inVec.z < 0) {
			inVec.z = 0;
		} else if (inVec.z > maxMapZ - 1) {
			inVec.z = maxMapZ - 1;
		}
		return inVec;
	}
	public override void OnInspectorGUI ()
	{
		EditorGUILayout.BeginVertical ();
		GUILayout.Label (maxMapX.ToString ());
		maxMapX = Mathf.RoundToInt (GUILayout.HorizontalSlider (maxMapX, 0, 50));
		GUILayout.Label (maxMapZ.ToString ());
		maxMapZ = Mathf.RoundToInt (GUILayout.HorizontalSlider (maxMapZ, 0, 50));
		for (int i = 0; i < terrainPrefabs.Count; i++) {
			if ((i % 2) == 0) {
				EditorGUILayout.BeginHorizontal ();
			}
			string temp = terrainPrefabs [i].ToString ();
			temp = temp.Remove (temp.IndexOf ("(UnityEngine.GameObject)"));
			if (GUILayout.Button (terrainImages [i])) {
				Reset ();
				selectedBlockToPaint = Instantiate (terrainPrefabs [i]) as GameObject;
				selectedBlockToPaint.collider.enabled = false;
				selectedBlockIndex = terrainPrefabs.IndexOf (terrainPrefabs [i]);
			}
			if ((i % 2) == 1 || i == terrainPrefabs.Count - 1) {
				EditorGUILayout.EndHorizontal ();
			}
		}
		for (int i = 0; i < buildingPrefabs.Count; i++) {
			if ((i % 2) == 0) {
				EditorGUILayout.BeginHorizontal ();
			}
			string temp = buildingPrefabs [i].ToString ();
			temp = temp.Remove (temp.IndexOf ("(UnityEngine.GameObject)"));
			if (GUILayout.Button (buildingImages [i])) {
				Reset ();
				selectedBlockToPaint = Instantiate (buildingPrefabs [i]) as GameObject;
				selectedBlockToPaint.collider.enabled = false;
				selectedBuildingIndex = buildingPrefabs.IndexOf (buildingPrefabs [i]);
			}
			if ((i % 2) == 1 || i == buildingPrefabs.Count - 1) {
				EditorGUILayout.EndHorizontal ();
			}
		}
		if (GUILayout.Button ("Clear Properties")) {
			Property[] props = gameController.GetComponentsInChildren<Property> ();
			for (int i = 0; i < props.Length; i++) {
				DestroyImmediate (props [i].gameObject);
			}
		}
		if (GUILayout.Button ("Clear Block")) {
			Reset ();
			deleteProperty = true;
		}
		if (GUILayout.Button ("Fill Sea")) {
			FillMap ("Sea");
		}
		if (GUILayout.Button ("Fill Land")) {
			FillMap ("Land");
		}
		if (GUILayout.Button ("Clear Terrain")) {
			ClearTerrain ();
		}
		if (GUILayout.Button ("Beautify map")) {
			BeautifyMap ();
		}
		if (GUILayout.Button ("Export Map Data")) {
			ExportMapDataToFile ();
		}
		EditorGUILayout.EndVertical ();
	}
	void FillMap (string type)
	{
		switch (type) {
		case "Plain":
			{
				foreach (GameObject go in terrainPrefabs) {
					if (go.name.Contains ("Plain0")) {
						FillMap (go);
						break;
					}
				}
				break;
			}
		case "Sea":
			{
				foreach (GameObject go in terrainPrefabs) {
					if (go.name.Contains ("Sea0")) {
						FillMap (go);
						break;
					}
				}
				break;
			}
		}
	}
	void FillMap (GameObject prototype)
	{
		ClearTerrain ();
		for (int i = 0; i < maxMapX; i++) {
			for (int j = 0; j < maxMapZ; j++) {
				GameObject temp = PrefabUtility.InstantiatePrefab (prototype) as GameObject;
				temp.transform.position = new Vector3 (i, 0, j);
				temp.transform.parent = terrain.transform;
			}
		}
	}
	void ClearTerrain ()
	{
		var children = new List<GameObject> ();
		foreach (Transform child in terrain.transform)
			children.Add (child.gameObject);
		children.ForEach (child => DestroyImmediate (child));
	}
	void BeautifyMap ()
	{
		GameObject shoalCorner = EditorGUIUtility.Load ("ShoalCorner0Block.prefab") as GameObject;
		//Transform landCorner = EditorGUIUtility.Load("ShoalLandCorner0.prefab") as Transform;
		GameObject shoalStraight = EditorGUIUtility.Load ("ShoalStraight1Block.prefab") as GameObject;
		terrain = GameObject.Find ("Terrain");
		TerrainBlock[] temp = terrain.GetComponentsInChildren<TerrainBlock> ();
		GameObject[] blocks = new GameObject[temp.Length];
		for (int i = 0; i < temp.Length; i++) {
			blocks [i] = temp [i].gameObject;
		}
		float largestX = 0;
		float largestZ = 0;
		for (int i = 0; i < blocks.Length; i++) {
			if (blocks [i].transform.position.x > largestX) {
				largestX = blocks [i].transform.position.x;
			}
			if (blocks [i].transform.position.z > largestZ) {
				largestZ = blocks [i].transform.position.z;
			}
		}
		RaycastHit hit;
		for (int i = 0; i < blocks.Length; i++) {
			blocks [i].transform.position = new Vector3 (Mathf.Round (blocks [i].transform.position.x), 0, Mathf.Round (blocks [i].transform.position.z));
			if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x, 100f, blocks [i].transform.position.z), Vector3.down, out hit, 1000f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
				Property prop = hit.collider.GetComponent<Property> ();
				TerrainBlock east = null, west = null, south = null, north = null;
				if (prop.propertyType == UnitName.Bridge) {
					if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x + 1, 100f, blocks [i].transform.position.z), Vector3.down, out hit, 1000f, 1)) {
						east = hit.collider.GetComponent<TerrainBlock> ();
					}
					if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x - 1, 100f, blocks [i].transform.position.z), Vector3.down, out hit, 1000f, 1)) {
						west = hit.collider.GetComponent<TerrainBlock> ();
					}
					if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x, 100f, blocks [i].transform.position.z - 1), Vector3.down, out hit, 1000f, 1)) {
						south = hit.collider.GetComponent<TerrainBlock> ();
					}
					if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x, 100f, blocks [i].transform.position.z + 1), Vector3.down, out hit, 1000f, 1)) {
						north = hit.collider.GetComponent<TerrainBlock> ();
					}
					CorrectBridge (prop, east, west, north, south);
				}
			}
			if (blocks [i].name.Contains ("Shoal")) {
				TerrainBlock east = null, west = null, south = null, north = null;
				if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x + 1, 100f, blocks [i].transform.position.z), Vector3.down, out hit, 1000f, 1)) {
					east = hit.collider.GetComponent<TerrainBlock> ();
				}
				if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x - 1, 100f, blocks [i].transform.position.z), Vector3.down, out hit, 1000f, 1)) {
					west = hit.collider.GetComponent<TerrainBlock> ();
				}
				if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x, 100f, blocks [i].transform.position.z - 1), Vector3.down, out hit, 1000f, 1)) {
					south = hit.collider.GetComponent<TerrainBlock> ();
				}
				if (Physics.Raycast (new Vector3 (blocks [i].transform.position.x, 100f, blocks [i].transform.position.z + 1), Vector3.down, out hit, 1000f, 1)) {
					north = hit.collider.GetComponent<TerrainBlock> ();
				}
				blocks [i] = BeautifyShoalBlocksStraight (shoalStraight, blocks [i], east, west, north, south);
				blocks [i] = BeautifyShoalBlocksCorner (shoalCorner, blocks [i], east, west, north, south);
			}
		}
	}
	private void CorrectBridge (Property prop, TerrainBlock east, TerrainBlock west, TerrainBlock north, TerrainBlock south)
	{
		if (east != null && west != null) {
			RaycastHit hit;
			Property eastProp = null;
			if (Physics.Raycast (new Vector3 (east.transform.position.x, 100f, east.transform.position.z), Vector3.down, out hit, 1000f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
				eastProp = hit.collider.GetComponent<Property> ();
			}
			Property westProp = null;
			if (Physics.Raycast (new Vector3 (west.transform.position.x, 100f, west.transform.position.z), Vector3.down, out hit, 1000f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
				westProp = hit.collider.GetComponent<Property> ();
			}
			if ((east.typeOfTerrain != TERRAINTYPE.Sea && east.typeOfTerrain != TERRAINTYPE.River && west.typeOfTerrain != TERRAINTYPE.Sea && west.typeOfTerrain != TERRAINTYPE.River) || (eastProp != null && eastProp.propertyType == UnitName.Bridge) || (westProp != null && westProp.propertyType == UnitName.Bridge)) {
				prop.transform.eulerAngles = new Vector3 (0, 90, 0);
			}
		}
		if (north != null && south != null) {
			RaycastHit hit;
			Property northProp = null;
			if (Physics.Raycast (new Vector3 (north.transform.position.x, 100f, north.transform.position.z), Vector3.down, out hit, 1000f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
				northProp = hit.collider.GetComponent<Property> ();
			}
			Property southProp = null;
			if (Physics.Raycast (new Vector3 (south.transform.position.x, 100f, south.transform.position.z), Vector3.down, out hit, 1000f, 1 << LayerMask.NameToLayer ("PropertyLayer"))) {
				southProp = hit.collider.GetComponent<Property> ();
			}
			if ((north.typeOfTerrain != TERRAINTYPE.Sea && north.typeOfTerrain != TERRAINTYPE.River && south.typeOfTerrain != TERRAINTYPE.Sea && south.typeOfTerrain != TERRAINTYPE.River) || (northProp != null && northProp.propertyType == UnitName.Bridge) || (southProp != null && southProp.propertyType == UnitName.Bridge)) {
				prop.transform.rotation = Quaternion.identity;
			}
		}
	}
	private GameObject BeautifyShoalBlocksStraight (GameObject clone, GameObject current, TerrainBlock east, TerrainBlock west, TerrainBlock north, TerrainBlock south)
	{
		if (east != null && east.name.Contains ("Sea")) {
			if (west != null && !west.name.Contains ("Sea")) {
				current = BlockInstantiate (clone, current, 270);
			}
		}
		if (west != null && west.name.Contains ("Sea")) {
			if (east != null && !east.name.Contains ("Sea")) {
				current = BlockInstantiate (clone, current, 90);
			}
		}
		if (south != null && (south.name.Contains ("Sea"))) {
			if (north != null && !(north.name.Contains ("Sea"))) {
				current = BlockInstantiate (clone, current, 0);
			}
		}
		if (north != null && (north.name.Contains ("Sea"))) {
			if (south != null && !(south.name.Contains ("Sea"))) {
				current = BlockInstantiate (clone, current, 180);
			}
		}
		if (east != null && east.name.Contains ("Shoal")) {
			if (west != null && west.name.Contains ("Shoal")) {
				if (north != null && north.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 180);
				} else if (south != null && south.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 0);
				}
			}
		}
		if (north != null && (north.name.Contains ("Shoal"))) {
			if (south != null && (south.name.Contains ("Shoal"))) {
				if (east != null && east.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 270);
				} else if (west != null && west.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 90);
				}
			}
		}
		return current;
	}
	private GameObject BeautifyShoalBlocksCorner (GameObject clone, GameObject current, TerrainBlock east, TerrainBlock west, TerrainBlock north, TerrainBlock south)
	{
		if (east != null && east.name.Contains ("Sea")) {
			if (north != null && north.name.Contains ("Sea")) {
				if (south != null && !south.name.Contains ("Sea") && west != null && !west.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 0);
				}
			} else if (south != null && south.name.Contains ("Sea")) {
				if (north != null && !north.name.Contains ("Sea") && west != null && !west.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 90);
				}
			}
		} else if (west != null && west.name.Contains ("Sea")) {
			if (north != null && north.name.Contains ("Sea")) {
				if (south != null && !south.name.Contains ("Sea") && east != null && !east.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 270);
				}
			} else if (south != null && south.name.Contains ("Sea")) {
				if (north != null && !north.name.Contains ("Sea") && east != null && !east.name.Contains ("Sea")) {
					current = BlockInstantiate (clone, current, 180);
				}
			}
		}
		return current;
	}
	private GameObject BlockInstantiate (GameObject clone, GameObject current, int angle)
	{
		Vector3 position = current.transform.position;
		if (current != null) {
			DestroyImmediate (current);
		}
		current = PrefabUtility.InstantiatePrefab (clone) as GameObject;
		current.transform.position = position;
		Transform[] children = current.GetComponentsInChildren<Transform> ();
		current.transform.rotation = Quaternion.AngleAxis (angle, Vector3.up);
		for (int i = 0; i < children.Length; i++) {
			if (children [i].name.Contains ("Fog")) {
				children [i].transform.eulerAngles = new Vector3 (-90, 0, 0);
			}
		}
		current.transform.parent = terrain.transform;
		return current;
	}
	public void ExportMapDataToFile ()
	{
		GameObject terrain = GameObject.Find ("Terrain");
		TerrainBlock[] blocks = terrain.GetComponentsInChildren<TerrainBlock> ();
		string[] slicedMapName = EditorApplication.currentScene.Split (new char[] {
			'.',
			'/',
			'\\'
		});
		MapData outgoingData = TerrainBuilder.CreateMapData (blocks, slicedMapName [slicedMapName.Length - 2]);
		Debug.Log (outgoingData.IsPlayable ());
		Stream sw = File.Create (EditorApplication.currentScene.Split ('.') [0] + ".bin");
		BinaryFormatter serializer = new BinaryFormatter ();
		serializer.Serialize (sw, outgoingData);
		sw.Close ();
		ExportMapNames ();
	}
	public void ExportMapNames ()
	{
		string[] temp = Directory.GetFiles (Application.dataPath + @"\Maps\", "*.unity");
		Stream sw = File.Create (Application.dataPath + @"\Maps\MapNames.bin");
		BinaryFormatter serializer = new BinaryFormatter ();
		serializer.Serialize (sw, temp);
		sw.Close ();
	}
}
