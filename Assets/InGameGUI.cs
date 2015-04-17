using UnityEngine;
using System.Collections;
using System;

public class InGameGUI : MonoBehaviour
{
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
	public TerrainGameView terrainView;
	
	// A displayer for high-level property details
	public PropertyGameView propertyView;
	
	// A displayer for high-level unit details
	public UnitGameView unitView;
	
	// Component for displaying detailed unit info
	public DetailedInfoBoxViewer detailedTextBox;
	
	// Advance turn button
	public UnityEngine.UI.Button advanceTurnButton;
	// Move to next player unit
	public UnityEngine.UI.Button nextUnitButton;
	// Adds right-click functionality for mobile builds
	public UnityEngine.UI.Button undoButton;
	// Pause menu
	public GameObject pauseMenu;
	
	// Turn animation controller
	public PlayerTurnAnimation turnAnimation;
	
	// Displays units that can be created
	public UnitSelectionDisplayer unitSelectionDisplayer;
	
	// Displays the current player's general power
	public GeneralBarView generalBarView;
	
	// Displays a health bar
	public HealthBarView healthBarView;
	void Awake ()
	{
		instance = this;
		SetupDisplayPanels ();
	}
	/// <summary>
	/// Setup the display panels.
	/// </summary>
	void SetupDisplayPanels ()
	{
		advanceTurnButton.onClick.AddListener (() => InGameController.instance.AdvanceTurn ());
		nextUnitButton.onClick.AddListener (() => InGameController.instance.MoveCameraToNextPlayerUnit ());
		undoButton.onClick.AddListener (() => {
			InGameController.instance.SimulateRightClick ();
		});
		detailedTextBox.gameObject.SetActive (false);
		pauseMenu.SetActive (false);
		unitSelectionDisplayer.gameObject.SetActive (false);
		generalBarView.gameObject.SetActive (false);
	}
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		RaycastHit hit;
		if (Physics.Raycast (Utilities.gameCamera.GetComponent<Camera> ().ScreenPointToRay (Input.mousePosition), out hit, float.PositiveInfinity, 1 << LayerMask.NameToLayer ("Default"))) {
			TerrainBlock newMouseOver = hit.collider.GetComponent<TerrainBlock> ();
			if (newMouseOver == null) {
				Debug.Log ("Null terrainBlock" + Input.mousePosition.ToString ());
				Debug.Break ();
			}
			if (blockMousedOver != newMouseOver) {
				blockMousedOver = newMouseOver;
				terrainView.gameObject.SetActive (true);
				blockMousedOver.SetTerrainView (terrainView);
			}
			if (blockMousedOver.IsOccupied () && blockMousedOver.occupyingUnit.gameObject.activeSelf) {
				if (unitMousedOver != blockMousedOver.occupyingUnit) {
					unitMousedOver = blockMousedOver.occupyingUnit;
					SetHoveredPlayerDisplay (unitMousedOver.owner);
					SetCurrentUnitDisplay (unitMousedOver);
				}
			} else {
				unitMousedOver = null;
				unitView.gameObject.SetActive (false);
			}
			if (blockMousedOver.HasProperty ()) {
				if (propertyMousedOver != blockMousedOver.occupyingProperty) {
					propertyMousedOver = blockMousedOver.occupyingProperty;
					SetHoveredPlayerDisplay (propertyMousedOver.owner);
					SetCurrentPropertyDisplay (propertyMousedOver);
				}
			} else {
				propertyMousedOver = null;
				propertyView.gameObject.SetActive (false);
			}
			// Hides hovered view if nothing to display
			if (propertyMousedOver == null && unitMousedOver == null) {
				hoveredPlayerView.gameObject.SetActive (false);
			}
		} else {
			// Set everything to null and displays off
			blockMousedOver = null;
			unitMousedOver = null;
			propertyMousedOver = null;
			unitView.gameObject.SetActive (false);
			propertyView.gameObject.SetActive (false);
			terrainView.gameObject.SetActive (false);
			hoveredPlayerView.gameObject.SetActive (false);
		}
		// If "info" key, display detailed information about what is hovered over
		if (Input.GetKeyDown (Utilities.bindings.infoButton)) {
			detailedTextBox.gameObject.SetActive (!detailedTextBox.gameObject.activeSelf);
			if (unitMousedOver != null) {
				unitMousedOver.SetDetailedInfo (detailedTextBox);
			} else if (propertyMousedOver != null) {
				propertyMousedOver.SetDetailedInfo (detailedTextBox);
			}/* else if (blockMousedOver != null) {
				detailedInfoText.text = blockMousedOver.description;
			}*/
		}
	}
	void OnGUI ()
	{
		if (InGameController.instance.isPaused) {
			Pause ();
		} else {
			if (unitMousedOver != null) {
				healthBarView.gameObject.SetActive (true);
				healthBarView.SetPosition (unitMousedOver.transform.position);
				healthBarView.SetHealthDisplayed (unitMousedOver.health.PrettyHealth ());
			} else if (propertyMousedOver != null) {
				healthBarView.gameObject.SetActive (true);
				healthBarView.SetPosition (propertyMousedOver.transform.position);
				healthBarView.SetHealthDisplayed (propertyMousedOver.health.PrettyHealth ());
			} else {
				healthBarView.gameObject.SetActive (false);
			}
		}
	}
	/// <summary>
	/// Sets the current player display.
	/// </summary>
	/// <param name="currentPlayer">Current player.</param>
	public void SetCurrentPlayerDisplay (Player currentPlayer)
	{
		currentPlayerView.gameObject.SetActive (true);
		currentPlayer.SetPlayerGUIView (currentPlayerView);
	}
	/// <summary>
	/// Sets the hovered-over player display.
	/// </summary>
	/// <param name="hoveredPlayer">Hovered player.</param>
	public void SetHoveredPlayerDisplay (Player hoveredPlayer)
	{
		if (hoveredPlayer != InGameController.instance.GetCurrentPlayer ()) {
			hoveredPlayerView.gameObject.SetActive (true);
			hoveredPlayer.SetPlayerGUIView (hoveredPlayerView);
		}
	}
	/// <summary>
	/// Sets one of the player displays, if applicable. For when relevant properties have changed
	/// </summary>
	/// <param name="player">Player.</param>
	public void SetPlayerDisplay (Player player)
	{
		// Set current player display
		if (player == InGameController.instance.GetCurrentPlayer ()) {
			SetCurrentPlayerDisplay (player);
		}// Else check if player is equal to unitMousedOver player
		else if (unitMousedOver != null) {
			if (player == unitMousedOver.owner) {
				SetHoveredPlayerDisplay (player);
			}
		}// Else check for propertyMousedOver player
		else if (propertyMousedOver != null) {
			if (player == propertyMousedOver.owner) {
				SetHoveredPlayerDisplay (player);
			}
		}
	}
	
	/// <summary>
	/// Sets the current property display.
	/// </summary>
	/// <param name="property">Property.</param>
	public void SetCurrentPropertyDisplay (Property property)
	{
		if (property == propertyMousedOver) {
			propertyView.gameObject.SetActive (true);
			property.SetPropertyGUIView (propertyView);
		}
	}
	/// <summary>
	/// Sets the current unit display.
	/// </summary>
	/// <param name="unit">Unit.</param>
	public void SetCurrentUnitDisplay (UnitController unit)
	{
		if (unit == unitMousedOver) {
			unitView.gameObject.SetActive (true);
			unit.SetUnitGUIView (unitView);
		}
				
	}
	/// <summary>
	/// Forces skirmish ending.
	/// </summary>
	public void ForceQuitSkirmish ()
	{
		InGameController.instance.QuitSkirmish ();
	}
	/// <summary>
	/// Displays the pause menu
	/// </summary>
	public void Pause ()
	{
		pauseMenu.SetActive (InGameController.instance.isPaused);
	}
	/// <summary>
	/// Starts the turn change animation.
	/// </summary>
	/// <param name="nameToDisplay">Name to display.</param>
	public void StartTurnChange (string nameToDisplay)
	{
		turnAnimation.AddName (nameToDisplay);
	}
	/// <summary>
	/// Shows the unit selection display.
	/// </summary>
	/// <param name="producableUnits">Producable units.</param>
	/// <param name="maxFunds">Max funds.</param>
	/// <param name="productionCallback">Production callback.</param>
	/// <param name="unselectedCallback">Unselected callback.</param>
	public void ShowUnitSelectionDisplay (UnitName[] producableUnits, int maxFunds, Action<UnitName> productionCallback, Action unselectedCallback)
	{
		unitSelectionDisplayer.DisplayUnitList (producableUnits, maxFunds, productionCallback, unselectedCallback);
	}
	/// <summary>
	/// Hides the unit selection display.
	/// </summary>
	public void HideUnitSelectionDisplay ()
	{
		unitSelectionDisplayer.gameObject.SetActive (false);
	}
	/// <summary>
	/// Shows the general power.
	/// </summary>
	/// <param name="level">Level.</param>
	public void ShowGeneralPower (float level, Player callingPlayer)
	{
		if (callingPlayer == InGameController.instance.GetCurrentPlayer ()) {
			generalBarView.SetPowerLevel (level);
		}
	}
}
