using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour {
	// static instance available to other classes
	public static InGameGUI instance;
	// current block input device is over, if any
	private TerrainBlock blockMousedOver;
	// current property input device is over, if any
	private Property propertyMousedOver;
	// current unit input device is over, if any
	private UnitController unitMousedOver;
							//Current player's stats
	public InGamePlayerStatsView currentPlayerView,
		// Stats of unit/property that input device is over, if owner is different from current player
		hoveredPlayerView;
	// Displays high-level detail about the current terrain block
	public TerrainGameViewer terrainView;
	// A displayer for high-level property details
	public PropertyGameView propertyView;
	// A displayer for high-level unit details
	public UnitGameViewer unitView;
	// Advance turn button
	public UnityEngine.UI.Button advanceTurnButton;
	void Awake(){
		instance = this;
		SetupDisplayPanels();
	}
	/// <summary>
	/// Setup the display panels.
	/// </summary>
	void SetupDisplayPanels(){
		advanceTurnButton.onClick.AddListener(() => FindObjectOfType<InGameController>().AdvanceTurn());
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
		if(Physics.Raycast(Utilities.gameCamera.camera.ScreenPointToRay(Input.mousePosition), out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer("Default")))
		{
			TerrainBlock newMouseOver = hit.collider.GetComponent<TerrainBlock>();
			if(newMouseOver == null){
				Debug.Log("Null terrainBlock" + Input.mousePosition.ToString());
				Debug.Break();
			}
			if(blockMousedOver != newMouseOver){
				blockMousedOver = newMouseOver;
				terrainView.gameObject.SetActive(true);
				blockMousedOver.SetTerrainView(terrainView);
			}
			if(blockMousedOver.IsOccupied() && blockMousedOver.occupyingUnit.gameObject.activeSelf)
			{
				if(unitMousedOver != blockMousedOver.occupyingUnit){
					unitMousedOver = blockMousedOver.occupyingUnit;
					SetHoveredPlayerDisplay(unitMousedOver.GetOwner());
					SetCurrentUnitDisplay(unitMousedOver);
				}
			}
			else
			{
				unitMousedOver = null;
				unitView.gameObject.SetActive(false);
			}
			if(blockMousedOver.HasProperty())
			{
				if(propertyMousedOver != blockMousedOver.occupyingProperty){
					propertyMousedOver = blockMousedOver.occupyingProperty;
					SetHoveredPlayerDisplay(propertyMousedOver.GetOwner());
					SetCurrentPropertyDisplay(propertyMousedOver);
				}
			}
			else
			{
				propertyMousedOver = null;
				propertyView.gameObject.SetActive(false);
			}
			// Hides hovered view if nothing to display
			if(propertyMousedOver == null && unitMousedOver == null){
				hoveredPlayerView.gameObject.SetActive(false);
			}
		}
		else
		{
			// Set everything to null and displays off
			blockMousedOver = null;
			unitMousedOver = null;
			propertyMousedOver = null;
			unitView.gameObject.SetActive(false);
			propertyView.gameObject.SetActive(false);
			terrainView.gameObject.SetActive(false);
			hoveredPlayerView.gameObject.SetActive(false);
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
			// If "info" key, display detailed information about what is hovered over
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
			if(unitMousedOver != null)
			{
				ShowHealthDisplay(unitMousedOver.health.PrettyHealth(), unitMousedOver.transform.position);
			}
			if(propertyMousedOver != null)
			{
				if(unitMousedOver == null)
				{
					ShowHealthDisplay(propertyMousedOver.health.PrettyHealth(), propertyMousedOver.transform.position);
				}
			}
		}
	}
	/// <summary>
	/// Sets the current player display.
	/// </summary>
	/// <param name="currentPlayer">Current player.</param>
	public void SetCurrentPlayerDisplay(Player currentPlayer){
		currentPlayerView.gameObject.SetActive(true);
		currentPlayer.SetPlayerGUIView(currentPlayerView);
	}
	/// <summary>
	/// Sets the hovered-over player display.
	/// </summary>
	/// <param name="hoveredPlayer">Hovered player.</param>
	public void SetHoveredPlayerDisplay(Player hoveredPlayer){
		hoveredPlayerView.gameObject.SetActive(true);
		hoveredPlayer.SetPlayerGUIView(hoveredPlayerView);
	}
	/// <summary>
	/// Sets one of the player displays, if applicable. For when relevant properties have changed
	/// </summary>
	/// <param name="player">Player.</param>
	public void SetPlayerDisplay (Player player)
	{
		// Set current player display
		if(player == InGameController.GetCurrentPlayer()){
			SetCurrentPlayerDisplay(player);
		}// Else check if player is equal to unitMousedOver player
		else if(unitMousedOver != null){
			if(player == unitMousedOver.GetOwner()){
				SetHoveredPlayerDisplay(player);
			}
		}// Else check for propertyMousedOver player
		else if(propertyMousedOver != null){
			if(player == propertyMousedOver.GetOwner()){
				SetHoveredPlayerDisplay(player);
			}
		}
	}
	/// <summary>
	/// Sets the current property display.
	/// </summary>
	/// <param name="property">Property.</param>
	public void SetCurrentPropertyDisplay(Property property){
		propertyView.gameObject.SetActive(true);
		property.SetPropertyGUIView(propertyView);
	}
	/// <summary>
	/// Sets the current unit display.
	/// </summary>
	/// <param name="unit">Unit.</param>
	public void SetCurrentUnitDisplay(UnitController unit){
		unitView.gameObject.SetActive(true);
		unit.SetUnitGUIView(unitView);
	}
	/// <summary>
	/// Shows the health display for a unit or property
	/// </summary>
	/// <param name="health">Health.</param>
	/// <param name="placeToDraw">Place to draw.</param>
	public void ShowHealthDisplay(int health, Vector3 placeToDraw)
	{
		int numberOfHP = health;
		Vector3 unitPointOnScreen = Camera.main.WorldToScreenPoint(placeToDraw);
		float imageWidth = Mathf.Round(48/Mathf.Pow(unitPointOnScreen.z, .75f));
		GUI.BeginGroup(new Rect(unitPointOnScreen.x - imageWidth*5, Screen.height - unitPointOnScreen.y + imageWidth + 8, 10*imageWidth, imageWidth*2));
		for(int i = 0; i < numberOfHP; i++)
		{
			GUI.Label(new Rect(i*imageWidth, 0, imageWidth, imageWidth*2), Utilities.healthPoint[i]);
		}
		GUI.EndGroup();
	}
	/// <summary>
	/// Displays the pause menu
	/// </summary>
	void PauseMenu()
	{
		
	}
}
