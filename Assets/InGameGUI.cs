using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour {
	private TerrainBlock blockMousedOver;
	private Property propertyMousedOver;
	private UnitController unitMousedOver;
	public GUIStyle healthBarStyle;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Physics.Raycast(Utilities.gameCamera.camera.ScreenPointToRay(Input.mousePosition), out hit, float.PositiveInfinity, 1<<LayerMask.NameToLayer("Default")))
		{
			blockMousedOver = hit.collider.GetComponent<TerrainBlock>();
			if(blockMousedOver.IsOccupied() && blockMousedOver.occupyingUnit.gameObject.activeSelf)
			{
				unitMousedOver = blockMousedOver.occupyingUnit;
			}
			else
			{
				unitMousedOver = null;
			}
			if(blockMousedOver.HasProperty())
			{
				propertyMousedOver = blockMousedOver.occupyingProperty;
			}
			else
			{
				propertyMousedOver = null;
			}
		}
		else
		{
			blockMousedOver = null;
			unitMousedOver = null;
			propertyMousedOver = null;
		}
	}
	void OnGUI()
	{
		if(InGameController.isPaused)
		{
			PauseMenu();
		}
		else
		{
			GUILayout.BeginArea(new Rect(0, 0, 100, 120));
			GUILayout.BeginVertical();
			GUILayout.Box(InGameController.GetCurrentPlayer().playerName);
			GUILayout.Box("Funds: " + InGameController.GetCurrentPlayer().funds.ToString());
			GUILayout.Box("Properties: " + InGameController.GetCurrentPlayer().GetNumberOfProperties());
			GUILayout.Box("Units: " + InGameController.GetCurrentPlayer().units.Count);
			GUILayout.EndVertical();
			GUILayout.EndArea();
			if(unitMousedOver != null && !unitMousedOver.GetOwner().IsSameSide(InGameController.GetCurrentPlayer()))
			{
				GUILayout.BeginArea(new Rect(100, 0, 100, 120));
				GUILayout.BeginVertical();
				GUILayout.Box(unitMousedOver.GetOwner().playerName);
				if(!unitMousedOver.GetOwner().IsNeutralSide())
				{
					GUILayout.Box("Funds: " + unitMousedOver.GetOwner().funds.ToString());
					GUILayout.Box("Properties: " + unitMousedOver.GetOwner().GetNumberOfProperties());
					GUILayout.Box("Units: " + (Utilities.fogOfWarEnabled?"--":unitMousedOver.GetOwner().units.Count.ToString()));
				}
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else if(propertyMousedOver != null && !propertyMousedOver.GetOwner().IsSameSide(InGameController.GetCurrentPlayer()))
			{
				GUILayout.BeginArea(new Rect(100, 0, 100, 120));
				GUILayout.BeginVertical();
				GUILayout.Box(propertyMousedOver.GetOwner().playerName);
				if(!propertyMousedOver.GetOwner().IsNeutralSide())
				{
					GUILayout.Box("Funds: " + propertyMousedOver.GetOwner().funds.ToString());
					GUILayout.Box("Properties: " + propertyMousedOver.GetOwner().GetNumberOfProperties());
					GUILayout.Box("Units: " + (Utilities.fogOfWarEnabled?"--":propertyMousedOver.GetOwner().units.Count.ToString()));
				}
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			if(Input.GetKey("i"))
			{
				if(unitMousedOver != null)
				{
					unitMousedOver.ShowDetailedInfo();
				}
				else if(propertyMousedOver != null)
				{
					propertyMousedOver.ShowDetailedInfo();
				}
				else if(blockMousedOver != null)
				{
					blockMousedOver.ShowDetailedInfo();
				}
			}
			if(blockMousedOver != null)
			{
				blockMousedOver.ShowTerrainBlockInfo();
			}
			if(unitMousedOver != null)
			{
				unitMousedOver.ShowUnitControllerInfo();
				ShowHealthDisplay(unitMousedOver.health, unitMousedOver.transform.position);
			}
			if(propertyMousedOver != null)
			{
				propertyMousedOver.ShowPropertyInfo();
				if(unitMousedOver == null)
				{
					ShowHealthDisplay(propertyMousedOver.health, propertyMousedOver.transform.position);
				}
			}
		}
	}
	public void ShowHealthDisplay(int health, Vector3 placeToDraw)
	{
		int numberOfHP = Utilities.ConvertFixedPointHealth(health);
		Vector3 unitPointOnScreen = Camera.main.WorldToScreenPoint(placeToDraw);
		float imageWidth = Mathf.Round(32/Mathf.Pow(unitPointOnScreen.z, .75f));
		GUI.BeginGroup(new Rect(unitPointOnScreen.x - imageWidth*5, Screen.height - unitPointOnScreen.y + imageWidth + 8, 10*imageWidth, imageWidth*2));
		for(int i = 0; i < numberOfHP; i++)
		{
			GUI.Label(new Rect(i*imageWidth, 0, imageWidth, imageWidth*2), Utilities.healthPoint[i], healthBarStyle);
		}
		GUI.EndGroup();
	}
	void PauseMenu()
	{
		if(GUI.Button(new Rect(Screen.width/2 - 60,Screen.height/2 - 30,120,60), "End Turn")) {
			InGameController.AdvanceTurn();
		}
	}
}
