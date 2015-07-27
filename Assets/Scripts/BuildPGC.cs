using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class BuildPGC : MonoBehaviour
{
	public Transform Forest0Block;
	public Transform Hill0Block;
	public Transform Mountain0Block;
	public Transform Plain0Block;
	public Transform River0Block;
	public Transform Road0Block;
	public Transform Sea0Block;
	public Transform ShoalCorner0Block;	
	public Transform ShoalStraight0Block;
	public Transform ShoalStraight1Block;

	public Transform Airport;
	public Transform HQ;
	public Transform City;
	public Transform Port;
	public Transform Factory;
	public Transform CommTower; 
	public Transform Bunker;

	public int width;
	public int height;
	public int num_players;


	//MUST BE A POWER OF 2

	// Use this for initaialization
	void Start ()
	{
		
	}
	public MapData BuildMap ()
	{
		//create fitness function
		float avg = -1000;
		float[][] terrainMap = null;
//		while (avg < -0.25 || avg > .5) {
//			avg = -1000;
//			terrainMap = createFractal (); 
//			for (int x = 0; x < width; x++) {
//				for (int y = 0; y < height; y++) {
//					avg = avg + terrainMap [x] [y];
//				}
//			}
//			avg = avg / (width * height);
//		}
		terrainMap = createFractal ();
		//terrainMap = createCoastline (terrainMap);
		MapData data = new MapData ("Random", terrainMap.Length, terrainMap [0].Length, "Randomly generated map");
		List<PlayerObject> playerObjects = new List<PlayerObject> ();
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (terrainMap [x] [y] <= -0.25) {
					data.mapData [x] [y] = new TerrainObject (Sea0Block.name, new Vector3 (x, 0, y), Quaternion.identity);
				} else if (terrainMap [x] [y] > -0.25 && terrainMap [x] [y] <= -0.15) {
					//Instantiate(Plain0Block, new Vector3(x, 0, y), Quaternion.identity);
					data.mapData [x] [y] = new TerrainObject (Plain0Block.name, new Vector3 (x, 0, y), Quaternion.identity);
					var building = placeBuilding (1, x, y, terrainMap);
					if (building != null)
						playerObjects.Add (building);
				} else if (terrainMap [x] [y] > -0.15 && terrainMap [x] [y] <= 0.4) {
					//Instantiate(Plain0Block, new Vector3(x, 0, y), Quaternion.identity);
					data.mapData [x] [y] = new TerrainObject (Plain0Block.name, new Vector3 (x, 0, y), Quaternion.identity);
					var building = placeBuilding (2, x, y, terrainMap);
					if (building != null)
						playerObjects.Add (building);
				} else if (terrainMap [x] [y] > 0.4 && terrainMap [x] [y] <= 0.75) {
					//Instantiate(Forest0Block, new Vector3(x, 0, y), Quaternion.identity);
					data.mapData [x] [y] = new TerrainObject (Forest0Block.name, new Vector3 (x, 0, y), Quaternion.identity);
					var building = placeBuilding (3, x, y, terrainMap);
					if (building != null)
						playerObjects.Add (building);
				} else if (terrainMap [x] [y] > 0.75 && terrainMap [x] [y] <= 0.90) {
					//Instantiate(Hill0Block, new Vector3(x, 0, y), Quaternion.identity);
					data.mapData [x] [y] = new TerrainObject (Hill0Block.name, new Vector3 (x, 0, y), Quaternion.identity);
					var building = placeBuilding (4, x, y, terrainMap);
					if (building != null)
						playerObjects.Add (building);
				} else if (terrainMap [x] [y] > 0.90) {
					//Instantiate(Mountain0Block, new Vector3(x, 0, y), Quaternion.identity);
					data.mapData [x] [y] = new TerrainObject (Mountain0Block.name, new Vector3 (x, 0, y), Quaternion.identity);
				}
			}
		}
		for (int i = 0; i < playerObjects.Count; i++) {
			if (playerObjects [i].name.Equals ("FactoryProp")) {
				playerObjects [i].side = Random.Range (1, num_players + 1);
			} else {
				playerObjects [i].side = Random.Range (0, num_players + 1);
			}
		}
		var hqs = placeHQ (terrainMap);
		playerObjects.AddRange (hqs);
		data.properties = playerObjects.ToArray ();
		data.maxPlayers = num_players;
		return data;
	}
	// Update is called once per frame
	void Update ()
	{
	}

	IEnumerable<PlayerObject> placeHQ (float[][] terrainMap)
	{
		int rounds = 0;
		int[] hqpos = new int[num_players * 2]; 
		int players = 0;
		List<PlayerObject> objects = new List<PlayerObject> ();
		while (players < num_players && rounds < 25) {
			for (int x = 0; x < width; x++) {
				for (int y = 0; y < height; y++) {
					if (terrainMap [x] [y] > -0.25 && terrainMap [x] [y] <= -0.15) {
						int rand = Random.Range (0, 125);
						if (rand < 1 && players < num_players) {
							if (players == 0) {
								objects.Add (new PlayerObject (HQ.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0));								
								hqpos [0] = x;
								hqpos [1] = y;
								players++;
							} else {
								bool can_do = true;
								for (int z = 0; z <= players; z = z + 2) {
									if (dist (x, y, hqpos [(2 * z)], hqpos [(z * 2 + 1)]) < width / num_players) {
										can_do = false;
									}
								}
								if (can_do) {
									objects.Add (new PlayerObject (HQ.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0));							
									hqpos [players * 2] = x;
									hqpos [players * 2 + 1] = y;
									players++;
								}
							}
						}
					}
				}
			}
			rounds++;
		}
		for (int i = 0; i < objects.Count; i++) {
			objects [i].side = i + 1;
		}
		return objects.Take (num_players);
	}

	float dist (int x1, int y1, int x2, int y2)
	{
		return Mathf.Sqrt (Mathf.Pow ((x2 - x1), 2) + Mathf.Pow ((y2 - y1), 2));
	}


	PlayerObject placeBuilding (int space, int x, int y, float[][] terrainMap)
	{
		int rand = Random.Range (0, 101);
		switch (space) {
		case 1://coast
			if (rand < 2) {
				try {
					if (terrainMap [x - 1] [y] <= -0.25 || terrainMap [x + 1] [y] <= -0.25 || terrainMap [x] [y + 1] <= -0.25 || terrainMap [x] [y - 1] <= -0.25) {
						return new PlayerObject (Port.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
					}
				} catch {
				}
			}
			break;
		case 2://plain
			if (rand < 6) {//city
				return new PlayerObject (City.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);

			} else if (rand < 7) {//airport
				return new PlayerObject (Airport.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);

			} else if (rand < 8) {//factory
				return new PlayerObject (Factory.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);

			} else if (rand < 9) {//commTower
				return new PlayerObject (CommTower.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
			}
			break;
		case 3://forest
			if (rand < 6) {//city
				return new PlayerObject (City.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
		
			} else if (rand < 7) {//airport
				return new PlayerObject (Airport.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
		
			} else if (rand < 8) {//factory
				return new PlayerObject (Factory.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
		
			} else if (rand < 9) {//commTower
				return new PlayerObject (CommTower.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
			}
			break;
		case 4://mountain
			if (rand < 6) {//city
				return new PlayerObject (City.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
		
			} else if (rand < 7) {//airport
				return new PlayerObject (Airport.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
		
			} else if (rand < 8) {//factory
				return new PlayerObject (Factory.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
		
			} else if (rand < 9) {//commTower
				return new PlayerObject (CommTower.name, new Vector3 (x, 0.5f, y), Quaternion.identity, 0);
			}
			break;
		}
		return null;
	}

	float[][] createCoastline (float[][] terrainMap)
	{
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (terrainMap [x] [y] <= -0.25) {
					if (x + 1 < width && terrainMap [x + 1] [y] > -0.25) {
						terrainMap [x + 1] [y] = 90;
					}
					/*if (x - 1 >= 0 && terrainMap[x-1][y] > -0.25){
						terrainMap[x-1][y] = 270;
					}
					if (y + 1 < height && terrainMap[x][y+1] > -0.25){
						terrainMap[x][y+1] = 180;
					}
					if (y - 1 >= 0 && terrainMap[x][y-1] > -0.25){
						terrainMap[x][y-1] = 360;
					}*/
				}
			}
		}
		return terrainMap;
	}

	float[][] createFractal ()
	{
		float[][] terrainMap = new float[width][];
		for (int x = 0; x < width; x++) {
			terrainMap [x] = new float[height];
			for (int y = 0; y < height; y++) {
				terrainMap [x] [y] = -10000;
			}
		}
		terrainMap [0] [0] = 0;
		terrainMap [0] [height - 1] = 0;
		terrainMap [width - 1] [0] = 0;
		terrainMap [width - 1] [height - 1] = 0;
		float h = 2f;
		int step = 1;
		while (step < Mathf.Log(width-1, 2)+1) {
			terrainMap = squareStep (terrainMap, h, step);
			terrainMap = diamondStep (terrainMap, h, step);
			h = h / 2;
			step = step + 1;
		}
		return terrainMap;
	}

	float[][] squareStep (float[][] terrainMap, float h, int step)
	{
		float[][] ans = new float[width][];
		for (int x = 0; x < width; x++) {
			ans [x] = new float[height];
			for (int y = 0; y < height; y++) {
				ans [x] [y] = -10000;
			}
		}
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (terrainMap [x] [y] != -10000) {
					ans [x] [y] = terrainMap [x] [y];
					int midwidth = (int)((width - 1) / Mathf.Pow (2, step));
					int midheight = (int)((height - 1) / Mathf.Pow (2, step));
					if (midwidth * 2 + x < width && midheight * 2 + y < height) {
						float avg = terrainMap [x] [y] + terrainMap [x] [y + midheight * 2] + terrainMap [midwidth * 2 + x] [y] + terrainMap [x + midwidth * 2] [midheight * 2 + y];
						avg = avg / 4 + Random.Range (-h, h);
						ans [(x + midwidth)] [(y + midheight)] = avg;
					}
				}
			}
		}
		return ans;
	}

	float[][] diamondStep (float[][] terrainMap, float h, int step)
	{
		float[][] ans = new float[width][];
		for (int x = 0; x < width; x++) {
			ans [x] = new float[height];
			for (int y = 0; y < height; y++) {
				ans [x] [y] = -10000;
			}
		}
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (terrainMap [x] [y] != -10000) {
					ans [x] [y] = terrainMap [x] [y];
					int midwidth = (int)((width - 1) / Mathf.Pow (2, step));
					int midheight = (int)((height - 1) / Mathf.Pow (2, step));
					if (x + midwidth < width && y + midheight < height) {
						float avg = terrainMap [x] [y] + terrainMap [x + midwidth] [y + midheight];
						if (y - midheight < 0) {//bot contib
							avg = avg + terrainMap [x + midwidth] [y + midheight];
						} else {
							avg = avg + terrainMap [x + midwidth] [y - midheight];
						}
						if (x + midwidth * 2 >= width) {//right contrib
							avg = avg + terrainMap [x] [y];
						} else {
							avg = avg + terrainMap [x + midwidth * 2] [y];
						}
						avg = avg / 4 + Random.Range (-h, h);
						ans [x + midwidth] [y] = avg;
						avg = terrainMap [x] [y] + terrainMap [x + midwidth] [y + midheight];
						if (y + midheight * 2 >= height) {
							avg = avg + terrainMap [x] [y];
						} else {
							avg = avg + terrainMap [x] [y + midheight * 2];
						}
						if (x - midwidth < 0) {
							avg = avg + terrainMap [x + midwidth] [y + midheight];
						} else {
							avg = avg + terrainMap [x - midwidth] [y + midheight];
						}
						avg = avg / 4 + Random.Range (-h, h);
						ans [x] [y + midheight] = avg;
					}
				}
			}
		}
		return ans;
	}
}