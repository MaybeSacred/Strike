using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Controls and displays the end menu for skirmishes
/// </summary>
public sealed class SkirmishEndMenu : MonoBehaviour
{
	List<PlayerInGameStatistics> playerStatistics;
	public SkirmishEndMenuPlayer[] endPlayerDisplays;
	public void Start ()
	{
	
	}
	/// <summary>
	/// Sets the game statistics on each of the player displays
	/// </summary>
	/// <param name="pigs">Pigs.</param>
	public void SetGameStatistics (List<PlayerInGameStatistics> pigs)
	{
		playerStatistics = pigs;
		foreach (PlayerInGameStatistics p in playerStatistics) {
			if (p.name.Equals ("--Neutral--")) {
				playerStatistics.Remove (p);
				break;
			}
		}
		playerStatistics.Sort ();
		for (int i = 0; i < 8; i++) {
			if (i < playerStatistics.Count) {
				endPlayerDisplays [i].SetText (playerStatistics [i]);
			} else {
				endPlayerDisplays [i].gameObject.SetActive (false);
			}
		}
	}
	/// <summary>
	/// Returns the application to the start screen
	/// </summary>
	public void OnReturnToStartScreen ()
	{
		Utilities.LoadTitleScreen ();
	}
}
