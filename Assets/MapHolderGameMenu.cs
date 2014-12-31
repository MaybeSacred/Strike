using UnityEngine;
using System.Collections.Generic;

public class MapHolderGameMenu : MonoBehaviour {
	List<TerrainBlock> blocks;
	void Awake(){
		blocks = new List<TerrainBlock>();
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnMapUpdate(MapData mapToRender){
		blocks.Clear();
		for(int i = 0; i < mapToRender.mapData.Length; i++){
			for(int j = 0; j < mapToRender.mapData[i].Length; j++){
				//TerrainBlock b = 
			}
		}
	}
}
