using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using SVGImporter;

public class NetworkManager : Photon.MonoBehaviour
{

	const string VERSION = "v1.2.6";
	public string roomName = "RN1";

	public string playerPrefabName = "Player";

	public Transform[] spawnPoints;
	public int randomSpawnPoint;

	public GameObject loadingIndicator;
	public GameObject tutorialWindow;

	// In-Game UI
	public GameObject jumpButton;
	public GameObject fireButton;
	public GameObject drinkButton;
	public GameObject changeIdentityButton;
	public GameObject joystick;
	public GameObject leaderboardGO;
	public GameObject myRankGO;
	public GameObject currentScoreGO;
	public GameObject currentTeamGO;

	public GameManager GM;
	public GameLoop GL;
	public PowerupManager PM;
	public Leaderboard leaderboard;

	public PhotonView pv;

	public static int playersNeededToStartGame = 2;

	public static int randomPlayer;
	public static int playersLeftToJoin;

	// Custom Player Properties (for Team and Score)
	public ExitGames.Client.Photon.Hashtable playerProperties;

	// Custom Room Properties
	public ExitGames.Client.Photon.Hashtable roomProperties;

	//Scores
	public Text bestScoreText;
	public Text previousScoreText;

	public AudioSource backgroundMusic;
	public AudioSource uiAudio;

	public AudioClip jazzy1;
	public AudioClip foggyClub;
	public AudioClip intenseSynth;

	public GameObject[] playersGO;

	public GameObject botPrefab;

	void Awake ()
	{
		pv = this.gameObject.GetComponent<PhotonView> ();
	
	}

	void Start ()
	{
		loadingIndicator.SetActive (true);


		// t = team, s = score, cs = current skin, hp = health
		playerProperties = new ExitGames.Client.Photon.Hashtable ();
		playerProperties.Add ("t", (int)0);
		playerProperties.Add ("s", (int)0);
		playerProperties.Add ("cs", (int)0); 
		playerProperties.Add ("hp", (int)100);

		roomProperties = new ExitGames.Client.Photon.Hashtable ();
		roomProperties.Add ("gs", (bool)false);
		roomProperties.Add ("mas", (bool)false);

		PhotonNetwork.ConnectUsingSettings (VERSION);
	}

	void Update ()
	{
		
	}

//	void OnFailedToConnectToPhoton ()
//	{
//		PhotonNetwork.offlineMode = true;
//
//		// Change Background Music
//		backgroundMusic.clip = jazzy1;
//		backgroundMusic.Play ();
//
//		loadingIndicator.SetActive (false);
//		GM.disablePowerup ();
//		GM.ShowMenu ();
//		GM.ChangeBackgroundColor ();
//	}

	void OnJoinedLobby ()
	{
		// Change Background Music
		backgroundMusic.clip = jazzy1;
		backgroundMusic.Play ();
		
		loadingIndicator.SetActive (false);
		GM.disablePowerup ();
		GM.ShowMenu ();
		GM.ChangeBackgroundColor ();

		// Update Scores

		if (GM.previousScore > GM.bestScore) {
			GM.bestScore = GM.previousScore;
			GM.Save ();
		}
			
		previousScoreText.text = "Previous Score: " + GM.previousScore.ToString ();
		bestScoreText.text = "Best Score: " + GM.bestScore.ToString ();

		if (GM.firstPlayUnsaved == false)
			GM.ShowAd ();
	}

	public void JoinRandomRoom ()
	{

		GM.moon.SetActive (false);
		loadingIndicator.SetActive (true);

		PhotonNetwork.JoinRandomRoom (null, 0);

	}


	void OnPhotonRandomJoinFailed ()
	{
		RoomOptions roomOptions = new RoomOptions () {
			IsVisible = true,
			MaxPlayers = 9,
			CustomRoomProperties = roomProperties
		};
		PhotonNetwork.CreateRoom (null, roomOptions, null);
	}

	IEnumerator OnJoinedRoom ()
	{
		
		loadingIndicator.SetActive (false);
		GM.moon.SetActive (true);


		GM.previousScore = 0;

		//reset team to 0
		playerProperties ["t"] = 0;
		// reset score to 0
		playerProperties ["s"] = 0;
		playerProperties ["hp"] = 100;

		PhotonNetwork.player.SetCustomProperties (playerProperties);

		if (loadingIndicator.activeInHierarchy) {
			loadingIndicator.SetActive (false);
		}

		GM.ShowInGameUI ();

		SpawnPlayer ();

		if (PhotonNetwork.offlineMode == false) {
			//Instantiate Powerups if first one in game
			if (PhotonNetwork.isMasterClient && (bool)PhotonNetwork.room.customProperties ["gs"] == false) {
				PM.InstantiatePowerups ();
			}

			// Assign as Spy if game is already in session
			if (PhotonNetwork.playerList.Length >= NetworkManager.playersNeededToStartGame && (bool)PhotonNetwork.room.customProperties ["gs"] == true) {
				//	Debug.Log ("Game in session, now a spy");

				yield return new WaitForSeconds (1f);

				GL.StartCoroutine (GL.spyPopup (2, false));

			} else if (PhotonNetwork.playerList.Length >= NetworkManager.playersNeededToStartGame && (bool)PhotonNetwork.room.customProperties ["gs"] == false) {

				//Debug.Log("CHOOSE MURDERER CALLED FROM GAMELOOP");

				yield return new WaitForSeconds (1f);

				roomProperties ["gs"] = true;
				roomProperties ["mas"] = true;
				PhotonNetwork.room.SetCustomProperties (roomProperties);
		
				int randomNum = Random.Range (0, PhotonNetwork.playerList.Length);
				pv.RPC ("ChooseRandomMurderer", PhotonTargets.All, randomNum);

			}

		// If game has NEVER EVER began --> Show waiting for players blur popup
		else if (PhotonNetwork.playerList.Length < NetworkManager.playersNeededToStartGame && (bool)PhotonNetwork.room.customProperties ["gs"] == false) {
				//	Debug.Log ("Waiting for players to start");

				if (GL.pv.isMine)
					GM.ScaleDownInGameUI ();

			}
		}
			

		//Single Player ------------------------------------
		else if (PhotonNetwork.offlineMode) 
		{
			SpawnBot ("Donald Trump", Random.Range(0, 13));
//			SpawnBot ("Harambe", Random.Range(0, 13));
//			SpawnBot ("Navjot", Random.Range(0, 13));
//			SpawnBot ("Baljot", Random.Range(0, 13));
//			SpawnBot ("Harsharn", Random.Range(0, 13));
//			SpawnBot ("Manraj", Random.Range(0, 13));
//			SpawnBot ("Inderjeet", Random.Range(0, 13));
//			SpawnBot ("Avleen", Random.Range(0, 13));

			yield return new WaitForSeconds (0.1f);

			int randomNum = Random.Range (0, PhotonNetwork.playerList.Length);
			pv.RPC ("ChooseRandomMurderer", PhotonTargets.All, randomNum);

		}

	}

	IEnumerator OnPhotonPlayerDisconnected (PhotonPlayer other)
	{

		if ((bool)PhotonNetwork.room.customProperties ["gs"] == true) {
			// Check if there still is a murderer
			CheckForMurderer ();

			yield return new WaitForSeconds (0.5f);

			//	 If is MC and Murder is not Assigned while game is started --> assign murderer
			if (PhotonNetwork.isMasterClient && (bool)PhotonNetwork.room.customProperties ["mas"] == false) {
				int randomNum = Random.Range (0, PhotonNetwork.playerList.Length);
				pv.RPC ("ChooseRandomMurderer", PhotonTargets.All, randomNum);
			}

			yield return new WaitForSeconds (0.5f);

			roomProperties ["mas"] = true;
			roomProperties ["gs"] = true;
			PhotonNetwork.room.SetCustomProperties (roomProperties);


			//Debug.Log ("MAS STATUS: " + PhotonNetwork.room.customProperties ["mas"]);

		}

		//Debug.Log ("GAMESTARTED STATUS: " + PhotonNetwork.room.customProperties ["gs"]);

	}

	IEnumerator OnPhotonPlayerConnected (PhotonPlayer other)
	{


		if (GL.worldBlur.blurActive == true && PhotonNetwork.playerList.Length >= playersNeededToStartGame) {
			GL.worldBlur.ClearMessage ();
		}

		if ((bool)PhotonNetwork.room.customProperties ["gs"] == true) {
			// Check if there still is a murderer
			CheckForMurderer ();

			yield return new WaitForSeconds (0.5f);

			//	 If is MC and Murder is not Assigned while game is started --> assign murderer
			if (PhotonNetwork.isMasterClient && (bool)PhotonNetwork.room.customProperties ["mas"] == false) {
				int randomNum = Random.Range (0, PhotonNetwork.playerList.Length);
				pv.RPC ("ChooseRandomMurderer", PhotonTargets.All, randomNum);
			}

			yield return new WaitForSeconds (0.5f);

			roomProperties ["mas"] = true;
			roomProperties ["gs"] = true;
			PhotonNetwork.room.SetCustomProperties (roomProperties);


			//Debug.Log ("MAS STATUS: " + PhotonNetwork.room.customProperties ["mas"]);

		}
	}



	public void SpawnPlayer ()
	{
		randomSpawnPoint = Random.Range (0, spawnPoints.Length);

		GameObject myPlayer = (GameObject)PhotonNetwork.Instantiate (playerPrefabName, spawnPoints [randomSpawnPoint].position, spawnPoints [randomSpawnPoint].rotation, 0);
		GL = myPlayer.GetComponent<GameLoop> ();
	
	}

	public void SpawnBot(string name, int color)
	{
		int randomBotSpawnPoint = Random.Range (0, spawnPoints.Length);
		GameObject myBot = (GameObject)GameObject.Instantiate (botPrefab, spawnPoints [randomBotSpawnPoint].position, spawnPoints [randomBotSpawnPoint].rotation);

		TextMesh myBotName = myBot.transform.FindChild ("Username").gameObject.GetComponent<TextMesh> ();
		SVGRenderer myBotTorso = myBot.transform.FindChild ("Gus_Skeleton/Gus_Torso").gameObject.GetComponent<SVGRenderer> ();
		SVGRenderer myBotLeftLeg = myBot.transform.FindChild ("Gus_Skeleton/Gus_Leg").gameObject.GetComponent<SVGRenderer> ();
		SVGRenderer myBotLeftArm = myBot.transform.FindChild ("Arms/Gus_Arm").gameObject.GetComponent<SVGRenderer> ();
		SVGRenderer myBotRightLeg = myBot.transform.FindChild ("Gus_Skeleton/Gus_Leg (R)").gameObject.GetComponent<SVGRenderer> ();
		SVGRenderer myBotRightArm = myBot.transform.FindChild ("Arms/Gus_Arm (R)").gameObject.GetComponent<SVGRenderer> ();

		myBotName.text = name;
		myBotTorso.color = GM.charColors [color];
		myBotLeftLeg.color = GM.charColors [color];
		myBotLeftArm.color = GM.charColors [color];
		myBotRightLeg.color = GM.charColors [color];
		myBotRightArm.color = GM.charColors [color];
	}

	public void CheckForMurderer ()
	{
		if (PhotonNetwork.isMasterClient) {

			GameObject[] players = GameObject.FindGameObjectsWithTag ("P");

			for (int p = 0; p < PhotonNetwork.playerList.Length; p++) {
				GameLoop targetGL = players [p].gameObject.GetComponent<GameLoop> ();

				if ((int)targetGL.pv.owner.customProperties ["t"] == 1)
					return;

			}

			roomProperties ["mas"] = false;
			roomProperties ["gs"] = true;
			PhotonNetwork.room.SetCustomProperties (roomProperties);

		}
			

		return;
	}

	[PunRPC]
	public void ChooseRandomMurderer (int randomNum)
	{
		if (PhotonNetwork.offlineMode == false) {
			randomPlayer = randomNum; 
	
			playersGO = GameObject.FindGameObjectsWithTag ("P");

			playersGO = playersGO.OrderByDescending (player => player.GetPhotonView ().viewID).ToArray ();

			for (int p = 0; p < PhotonNetwork.playerList.Length; p++) {

				playersGO [p].GetComponent<GameLoop> ().murderer = playersGO [randomPlayer].gameObject;

				if (GL.murderer != null)
					playersGO [p].GetComponent<GameLoop> ().murdererGL = GL.murderer.GetComponent<GameLoop> ();
				else if (GL.murderer == null) {
					Debug.Log ("NO MURDERER ASSIGNED");
				}

			}

			//		Debug.Log (randomPlayer);

			if (GL.murdererGL != null)
				GL.murdererGL.assignedTeam = 1;

			//		 ALL OTHER PLAYERS ARE SPIES
			for (int p = 0; p < PhotonNetwork.playerList.Length; p++) {

				if (p != randomPlayer) {
					GameLoop spyGL = playersGO [p].GetComponent<GameLoop> ();

					if (spyGL != null)
						spyGL.assignedTeam = 2;

				}

			}


			//	if (PhotonNetwork.isMasterClient) 
			//	{
			roomProperties ["gs"] = true;
			roomProperties ["mas"] = true;
			PhotonNetwork.room.SetCustomProperties (roomProperties);
			//	}
		} 
			
	}

}
