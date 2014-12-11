using UnityEngine;
using System.Collections.Generic;

public class SkirmishEndMenu : MonoBehaviour {
	private List<PlayerInGameStatistics> playerStatistics;
	public float statisticsDisplayWidth, statisticsDisplayHeight, statisticsDisplayHeightOffset;
	void Start () {
	
	}
	
	void Update () {
		
	}
	void OnGUI()
	{
		if(playerStatistics != null)
		{
			GUILayout.BeginArea(new Rect(Screen.width * (1 - statisticsDisplayWidth)/2, statisticsDisplayHeightOffset * Screen.height, statisticsDisplayWidth * Screen.width, statisticsDisplayHeight * Screen.height));
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Name");
			GUILayout.Label("Funds Gathered");
			GUILayout.Label("Funds Spent");
			GUILayout.Label("Units Created");
			GUILayout.Label("Units Lost");
			GUILayout.EndHorizontal();
			for(int i = 0; i < playerStatistics.Count; i++)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(playerStatistics[i].name);
				GUILayout.Label(playerStatistics[i].totalFundsGathered.ToString());
				GUILayout.Label(playerStatistics[i].totalFundsSpent.ToString());
				GUILayout.Label(playerStatistics[i].unitsCreated.ToString());
				GUILayout.Label(playerStatistics[i].unitsLost.ToString());
				GUILayout.EndHorizontal();
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
		if(GUI.Button(new Rect(.375f*Screen.width, (9/10f) * Screen.height, .25f*Screen.width, (1/10f) * Screen.height - 1), "Return to Menu"))
		{
			Utilities.LoadTitleScreen();
		}
	}
	public void SetGameStatistics(List<PlayerInGameStatistics> pigs)
	{
		playerStatistics = pigs;
		foreach(PlayerInGameStatistics p in playerStatistics)
		{
			if(p.name.Equals("--Neutral--"))
			{
				playerStatistics.Remove(p);
				break;
			}
		}
		playerStatistics.Sort();
	}
}
