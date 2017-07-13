using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class Leaderboard : Photon.MonoBehaviour {

	public GameObject[] ranks;
	public int currentPlayerAmount = 0;
	public int myRank;

	public Text myRankText;
	public Text outOfRankText;
	public Text scoreText;

	// Use this for initialization
	void Start () {
		currentPlayerAmount = PhotonNetwork.playerList.Length;
	}

	void Update()
	{
		// Check every update if player amount has changed
		if (currentPlayerAmount != PhotonNetwork.playerList.Length) {
		    Refresh ();
			currentPlayerAmount = PhotonNetwork.playerList.Length;
		}
	}
	
	public void Refresh()
	{
//		Debug.Log ("REFRESHING LEADERBOARD HOMIE");

		if(PhotonNetwork.offlineMode == false)
		{
		// Get all Players
		GameObject[] players = GameObject.FindGameObjectsWithTag ("P");

		// Sort Players by Points
		if (players != null) 
		{
			if (players.All (player => player.GetPhotonView () != null)) 
			{
				if (players.All (player => player.GetPhotonView ().owner != null)) 
				{
					if (players.All (player => player.GetPhotonView ().owner.customProperties["s"] != null)) 
					players = players.OrderByDescending (player => (int)player.GetPhotonView ().owner.customProperties ["s"]).ToArray ();
				}
			}
		}

		// For Each Player in the Game
		for (int i = 0; i < players.Length; i++) {
			// Update rank variable
			GameLoop targetPlayerGL = players [i].GetComponent<GameLoop> ();
			targetPlayerGL.rank = i + 1;

			// Is this player mine? Make his/her rank mine then.
			if (targetPlayerGL.pv.isMine) {
				myRank = targetPlayerGL.rank;
			}

		}

		//Update Personal ON SCREEN rank
		outOfRankText.text = players.Length.ToString();
		myRankText.text = myRank.ToString ();

		// For Each Rank in the Leaderboard
			for (int p = 0; p < ranks.Length; p++) {

				GameObject rankToUpdate = ranks [p];

				Text usernameToUpdate = rankToUpdate.transform.Find ("Username").GetComponent<Text> ();
				Text pointsToUpdate = rankToUpdate.transform.Find ("Points").GetComponent<Text> ();
		
				// If player does not exist
				if (p + 1 > players.Length) {
					usernameToUpdate.text = "-";
					pointsToUpdate.text = "-";
					return;
				}

				if (p + 1 <= players.Length) {
					GameLoop targetPlayerGL = players [p].GetComponent<GameLoop> ();


					// Set Username on Leaderboard
					if ((int)targetPlayerGL.pv.owner.customProperties ["t"] != 1) {
						usernameToUpdate.text = targetPlayerGL.pv.owner.name;
						usernameToUpdate.color = Color.white;
					}

					// IF MURDERER --> DO NOT DISPLAY REAL USERNAME
					if ((int)targetPlayerGL.pv.owner.customProperties ["t"] == 1) {
						usernameToUpdate.text = "Current Murderer";
						usernameToUpdate.color = Color.red;
					}
							


								
					// Set Points on Leaderboard
					pointsToUpdate.text = targetPlayerGL.pv.owner.customProperties ["s"].ToString ();

					if (targetPlayerGL.pv.isMine) {
						scoreText.text = PhotonNetwork.player.customProperties ["s"].ToString ();
					}
								
						  
					
				}

			}
		}

//		Debug.Log ("DONE");
	}
}


