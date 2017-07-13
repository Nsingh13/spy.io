using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SVGImporter;

public class GameLoop : Photon.MonoBehaviour
{

	public PhotonView pv;

	//Game Specific Variables
	public bool killedMurderer = false;
	public bool autoRespawn = false;
	public bool diedAsSpy = false;
	public bool isImmortal = false;
	public bool startedBackupDeath = false;
	public static bool refreshAllLeaderboards = false;
	public int assignedTeam = 0;
	// For Bonus Points

	public float redEyeWaitTime = 1f;
	public int rank = 0;

	public GameLoop thisLoop;
	public GameManager GM;
	public NetworkManager NM;
	public Leaderboard leaderboard;
	public PlayerController PC;

	//Pickups
	public GameObject redPickupPrefab;
	public GameObject bluePickupPrefab;

	//Notification Bar
	public GameObject banner;
	public GameObject logo;
	public GameObject youAre;
	public GameObject team;

	public Vector2 bannerDownPos;
	public Vector2 bannerUpPos;

	public Vector2 spyLogoPos;
	public Vector2 spyYouArePos;
	public Vector2 spyTeamPos;
	public Vector2 spyTeamScale;

	public Vector2 murdererLogoPos;
	public Vector2 murdererYouArePos;
	public Vector2 murdererTeamPos;
	public Vector2 murdererTeamScale;

	public Image teamImage;
	public Sprite spyImage;
	public Sprite murdererImage;

	public Text teamText;

	public float bannerReachTime;
	public float bannerStayTime;

	public GameObject[] players;

	// Sends YOUR CLIENT'S death data via MASTER CLIENT in order for YOUR CLIENT
	// to take any LOCAL appropriate action necessary. See 'Deploy Pickup'.
	public int myDeathStage = 0;

	public string deathReasonGlobal = "";

	public GaussianBlur worldBlur;

	public GameObject murderer;
	public GameLoop murdererGL;
	public static int playersLeftToJoin;

	//Body Parts
	public SVGRenderer myTorso;
	public SVGRenderer myLeftArm;
	public SVGRenderer myRightArm;
	public SVGRenderer myLeftLeg;
	public SVGRenderer myRightLeg;
	public SVGRenderer myLeftEye;
	public SVGRenderer myRightEye;

	public Color white;
	public Color black;
	public Color halfAlpha;

	public int currentPowerup = 0;


	// Use this for initialization

	void Awake ()
	{
		

		// Get Text Objects
		//	scoreText = GameObject.Find("Current Score").gameObject.transform.Find("ScoreNum").gameObject.GetComponent<Text>();
		teamText = GameObject.Find ("Current Team").gameObject.transform.Find ("Team").gameObject.GetComponent<Text> ();

		worldBlur = this.gameObject.transform.FindChild ("Main Camera").gameObject.GetComponent<GaussianBlur> ();

		thisLoop = this.gameObject.GetComponent<GameLoop> ();
		leaderboard = GameObject.Find ("Realtime Leaderboard").GetComponent<Leaderboard> ();

		GM = GameObject.Find ("GameController").GetComponent<GameManager> ();
		NM = GM.gameObject.GetComponent<NetworkManager> ();

		thisLoop.assignedTeam = 0;

		pv = this.gameObject.GetComponent<PhotonView> ();

		if (pv == null) {
			Debug.LogError ("You Have no Photon View Attached in the Player GO to call FlashMuzzle!");
		}

		thisLoop.autoRespawn = false;
		thisLoop.diedAsSpy = false;
	}

	void Start ()
	{


		GM.worldBlur = this.worldBlur;

		// Assign all Body Parts
		myTorso = this.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Torso").gameObject.GetComponent<SVGRenderer> ();
		myLeftArm = this.gameObject.transform.FindChild ("Arms/Gus_Arm").gameObject.GetComponent<SVGRenderer> ();
		myRightArm = this.gameObject.transform.FindChild ("Arms/Gus_Arm (R)").gameObject.GetComponent<SVGRenderer> ();
		myLeftLeg = this.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Leg").gameObject.GetComponent<SVGRenderer> ();
		myRightLeg = this.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Leg (R)").gameObject.GetComponent<SVGRenderer> ();
		myLeftEye = this.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Eye").gameObject.GetComponent<SVGRenderer> ();
		myRightEye = this.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Eye (R)").gameObject.GetComponent<SVGRenderer> ();


		players = GameObject.FindGameObjectsWithTag ("P");

		// Set All Players' Username && Skin
		for (int p = 0; p < PhotonNetwork.playerList.Length; p++) {
			
			string nameToSet = PhotonNetwork.playerList [p].name;

			GameObject targetPlayer = players [p].gameObject;
			GameLoop targetPlayerGL = players [p].gameObject.GetComponent<GameLoop> ();

			TextMesh usernameWorldText = targetPlayer.gameObject.transform.FindChild ("Username").gameObject.GetComponent<TextMesh> ();

			// If Player Never Entered a Username
			//if (PhotonNetwork.offlineMode == false) 
		//	{
				if (targetPlayerGL.pv.owner.name == "" || targetPlayerGL.pv.owner.name == "Enter Username...") {
					targetPlayerGL.pv.owner.name = "No Name";
				}

				usernameWorldText.text = targetPlayerGL.pv.owner.name;
				targetPlayer.gameObject.name = targetPlayerGL.pv.owner.name;
			

				// Set all players' skin
				if ((int)targetPlayerGL.pv.owner.customProperties ["cs"] < GM.charColors.Length) {
					if (targetPlayerGL.myTorso != null)
						targetPlayerGL.myTorso.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
					if (targetPlayerGL.myRightLeg != null)
						targetPlayerGL.myRightLeg.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
					if (targetPlayerGL.myRightArm != null)
						targetPlayerGL.myRightArm.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
					if (targetPlayerGL.myLeftLeg != null)
						targetPlayerGL.myLeftLeg.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
					if (targetPlayerGL.myLeftArm != null)
						targetPlayerGL.myLeftArm.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
				}
		//	}

		}


		// Refresh Leaderboard
		leaderboard.Refresh ();



	}

	// Update is called once per frame
	void Update ()
	{

			//Check if dead --> Start BACKUP death coroutine
		if ((int)PhotonNetwork.player.customProperties ["hp"] <= 0 && thisLoop.startedBackupDeath == false)
		{
			thisLoop.StartCoroutine ("BackupDeath");
			thisLoop.startedBackupDeath = true;
		}

			// Refresh Leaderboard If Needed (USED WHEN MURDERER RESPAWNS AS SPY)
			if (GameLoop.refreshAllLeaderboards == true) {
				leaderboard.Refresh ();
				GameLoop.refreshAllLeaderboards = false;
			}

			// Update Username While In Game
			if (GameManager.setChangeNameTrue == true && GameManager.changeName == false) {
				pv.RPC ("SetChangeNameTrueForAll", PhotonTargets.All);
			}

			// Update Skin While In Game
			if (GameManager.setChangeSkinTrue == true && GameManager.changeSkin == false) {
				pv.RPC ("SetChangeSkinTrueForAll", PhotonTargets.All);
			}

			if (GameManager.setChangeNameTrue == true && GameManager.changeName == true) {
				// Set All Players' Username 
				for (int p = 0; p < PhotonNetwork.playerList.Length; p++) {

					string nameToSet = PhotonNetwork.playerList [p].name;

					GameObject targetPlayer = players [p].gameObject;
					GameLoop targetPlayerGL = players [p].gameObject.GetComponent<GameLoop> ();

					TextMesh usernameWorldText = targetPlayer.gameObject.transform.FindChild ("Username").gameObject.GetComponent<TextMesh> ();

					// If Player Never Entered a Username
					if (targetPlayerGL.pv.owner.name == "" || targetPlayerGL.pv.owner.name == "Enter Username...") {
						targetPlayerGL.pv.owner.name = "No Name";
					}

					usernameWorldText.text = targetPlayerGL.pv.owner.name;
					targetPlayer.gameObject.name = targetPlayerGL.pv.owner.name;


				}
		

				// Refresh Leaderboard
				leaderboard.Refresh ();

				GameManager.changeName = false;
				GameManager.setChangeNameTrue = false;
			}

			if (GameManager.setChangeSkinTrue == true && GameManager.changeSkin == true) {
				for (int p = 0; p < PhotonNetwork.playerList.Length; p++) {

					GameObject targetPlayer = players [p].gameObject;
					GameLoop targetPlayerGL = players [p].gameObject.GetComponent<GameLoop> ();

					if ((int)targetPlayerGL.pv.owner.customProperties ["cs"] < GM.charColors.Length) {
						targetPlayerGL.myTorso.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
						targetPlayerGL.myRightLeg.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
						targetPlayerGL.myRightArm.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
						targetPlayerGL.myLeftLeg.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
						targetPlayerGL.myLeftArm.color = GM.charColors [(int)targetPlayerGL.pv.owner.customProperties ["cs"]];
					}
				}

				GameManager.changeSkin = false;
				GameManager.setChangeSkinTrue = false;
			}

			// Update waiting for players blur popup IF ACTIVE
		if (PhotonNetwork.playerList.Length < NetworkManager.playersNeededToStartGame && PhotonNetwork.offlineMode == false) {
				playersLeftToJoin = NetworkManager.playersNeededToStartGame - PhotonNetwork.playerList.Length;

				if (worldBlur.blurActive == false)
					worldBlur.BlurMessage (2, 2, "");
			
				if (playersLeftToJoin > 0)
					worldBlur.UpdateMessage ("Not Enough Players. Need " + playersLeftToJoin + " More Player(s) to Play.");

			}



		// RANDOM TEAM ASSIGNMENT (Used by ChooseRandomMurderer Method)
		if (murdererGL != null) {
			
			if (thisLoop.assignedTeam == 1 && murdererGL.pv.isMine == true) {

				if (worldBlur.blurActive) {
					worldBlur.ClearMessage ();
				}

				if (thisLoop.pv.isMine)
					GM.ScaleBackInGameUI ();

				thisLoop.StartCoroutine ("murdererPopup", 2);

				thisLoop.assignedTeam = 0;

			}

			if (thisLoop.assignedTeam == 2 && murdererGL.pv.isMine == false) {

				if (worldBlur.blurActive) {
					worldBlur.ClearMessage ();
				}

				if (thisLoop.pv.isMine)
					GM.ScaleBackInGameUI ();

				thisLoop.StartCoroutine (spyPopup (2, false));

				thisLoop.assignedTeam = 0;

			}
		}

		if (thisLoop.myDeathStage == 1) {
			if (GM.UIvisible == false)
				GM.ChangeIdOKClicked ();	

			// IF IS SPY --> Died as Spy is TRUE
			if (thisLoop.pv.isMine) {
				if ((int)PhotonNetwork.player.customProperties ["t"] == 2) {
					thisLoop.diedAsSpy = true;
				}
			}
		}

		// BLUR MESSAGE ON LOCAL CLIENT
		if (thisLoop.myDeathStage == 2) {
			if (thisLoop.pv.isMine) {
				GM.HideInGameUI ();
				worldBlur.BlurMessage (2, 2, deathReasonGlobal);

				// Auto-Respawn ON and Become Spy If Died as Murderer
				if (thisLoop.diedAsSpy == false) {
					thisLoop.autoRespawn = true;
					thisLoop.gameObject.transform.position = NM.spawnPoints [NM.randomSpawnPoint].position;
					thisLoop.StartCoroutine (spyPopup (2, true));
					thisLoop.diedAsSpy = true;
				}
			}
				
				
			thisLoop.myDeathStage = 0;
		}

		// LEAVE GAME RETURN TO MENU ON LOCAL CLIENT (pass control over to GM)
		if (thisLoop.myDeathStage == 3) {
//			Debug.Log ("Death Stage is 3");
		
			// If NOT AutoRespawn
			if (thisLoop.autoRespawn == false) {
				if (thisLoop.pv.isMine) {
					PhotonNetwork.LeaveRoom ();
				}
			}

			// AutoRespawn?
			if (thisLoop.autoRespawn) {
				if (thisLoop.pv.isMine) {
					thisLoop.StopCoroutine ("BackupDeath");
					worldBlur.ClearMessage ();
					GM.ShowInGameUI ();

					thisLoop.diedAsSpy = false;
					thisLoop.autoRespawn = false;
				}
			}


			thisLoop.myDeathStage = 0;
		}
	}

	public IEnumerator spyPopup (int RPCstate, bool refreshLeaderboards)
	{

		if (pv.isMine) {

			if (RPCstate == 1)
				pv.RPC ("becomeSpy", PhotonTargets.AllBuffered, refreshLeaderboards);

			if (RPCstate == 2)
				becomeSpy (refreshLeaderboards);

			NM.backgroundMusic.clip = NM.foggyClub;
			NM.backgroundMusic.Play ();

			thisLoop.teamText.text = "Spy";
			thisLoop.teamText.color = GM.lightBlue;

			// Set Postion of all and Scale of 'Team'
			thisLoop.logo.transform.localPosition = thisLoop.spyLogoPos;
			thisLoop.youAre.transform.localPosition = thisLoop.spyYouArePos;
			thisLoop.team.transform.localPosition = thisLoop.spyTeamPos;
			thisLoop.team.transform.localScale = thisLoop.spyTeamScale;

			// Change Team Image
			thisLoop.teamImage.sprite = thisLoop.spyImage;

			// Move banner down
			iTween.MoveTo (banner, iTween.Hash ("x", bannerDownPos.x, "y", bannerDownPos.y, "islocal", true, "time", bannerReachTime, "easetype", iTween.EaseType.linear));

			yield return new WaitForSeconds (bannerStayTime);

			// Move banner back up
	
			iTween.MoveTo (banner, iTween.Hash ("x", bannerUpPos.x, "y", bannerUpPos.y, "islocal", true, "time", bannerReachTime, "easetype", iTween.EaseType.linear));

			thisLoop.assignedTeam = 0;

		}
		
	}


	public IEnumerator murdererPopup (int RPCstate)
	{

		
		if (pv.isMine) {

			if (RPCstate == 1)
				pv.RPC ("becomeMurderer", PhotonTargets.AllBuffered);

			if (RPCstate == 2)
				becomeMurderer ();

			NM.backgroundMusic.clip = NM.intenseSynth;
			NM.backgroundMusic.Play ();

			teamText.text = "Murderer";
			teamText.color = Color.red;

			// Set Postion of all and Scale of 'Team'
			logo.transform.localPosition = murdererLogoPos;
			youAre.transform.localPosition = murdererYouArePos;
			team.transform.localPosition = murdererTeamPos;
			team.transform.localScale = murdererTeamScale;

			// Change Team Image
			teamImage.sprite = murdererImage;

			// Move banner down
			iTween.MoveTo (banner, iTween.Hash ("x", bannerDownPos.x, "y", bannerDownPos.y, "islocal", true, "time", bannerReachTime, "easetype", iTween.EaseType.linear));

			yield return new WaitForSeconds (bannerStayTime);

			// Move banner back up

			iTween.MoveTo (banner, iTween.Hash ("x", bannerUpPos.x, "y", bannerUpPos.y, "islocal", true, "time", bannerReachTime, "easetype", iTween.EaseType.linear));

		}
			
	}

	//[PunRPC]
	public void becomeSpy (bool refreshLeaderboards)
	{


		NM.playerProperties ["t"] = 2;
		NM.playerProperties ["s"] = GM.previousScore;
		PhotonNetwork.player.SetCustomProperties (NM.playerProperties);

		if (PhotonNetwork.offlineMode == false) 
		{
			NM.roomProperties ["gs"] = true;
			NM.roomProperties ["mas"] = (bool)PhotonNetwork.room.customProperties ["mas"];
			PhotonNetwork.room.SetCustomProperties (NM.roomProperties);
		}

		if (refreshLeaderboards == true)
			pv.RPC ("SetRefreshLeaderBoardsTrue", PhotonTargets.All);


	}

	[PunRPC]
	public void SetRefreshLeaderBoardsTrue ()
	{
		GameLoop.refreshAllLeaderboards = true;
	}

	public void becomeMurderer ()
	{
		


		NM.playerProperties ["t"] = 1;
		NM.playerProperties ["s"] = GM.previousScore;
		PhotonNetwork.player.SetCustomProperties (NM.playerProperties);

		if (PhotonNetwork.offlineMode == false) 
		{
			NM.roomProperties ["gs"] = true;
			NM.roomProperties ["mas"] = true;
			PhotonNetwork.room.SetCustomProperties (NM.roomProperties);
		}

		pv.RPC ("SetRefreshLeaderBoardsTrue", PhotonTargets.All);

	}

	[PunRPC]
	public void SetChangeNameTrueForAll ()
	{
		GameManager.setChangeNameTrue = true;
		GameManager.changeName = true;
	}

	[PunRPC]
	public void SetChangeSkinTrueForAll ()
	{
		GameManager.setChangeSkinTrue = true;
		GameManager.changeSkin = true;
	}


	public void AddPoints (string targetPickupName)
	{
		if(pv.isMine)
		GM.previousScore = GM.previousScore + 100;
		
		NM.playerProperties ["s"] = GM.previousScore;
		PhotonNetwork.player.SetCustomProperties (NM.playerProperties);

		// Destroy Pickup
		DestroyPickup (targetPickupName);

	}

	public IEnumerator BackupDeath()
	{
		yield return new WaitForSeconds (4f);

		if (thisLoop.pv.isMine)
		{
			PhotonNetwork.LeaveRoom ();
		}
	}

	[PunRPC]
	public IEnumerator DeployPickup (int killedPlayerID, int killerID, bool isKilledPlayerMurderer, bool isKillerMurderer,  int killedPlayerPoints, string deathReason)
	{
		if (killedPlayerID == thisLoop.pv.viewID) 
		{
			if (thisLoop.pv.isMine) 
			{
				//Start Backup Death
				NM.playerProperties ["hp"] = 0;
				PhotonNetwork.player.SetCustomProperties (NM.playerProperties);
			}

			// How Many Pickups to Instantiate Data
			int pickupsToInstantiate = 0;
			int numOfPointsFor1Pickup = 100;
			int maxPickupsToInstantiate = 7;

			Vector3 killedPlayerLocation = new Vector3 ();

			GameObject killerGO = null;

			// Get all Players
			GameObject[] players;

			players = GameObject.FindGameObjectsWithTag ("P");

			//Who is killer?
			for (int p = 0; p < players.Length; p++) {
				if (players [p].GetPhotonView ().viewID == killerID) {
					killerGO = players [p].gameObject;
				}
			}

			// Get Killer's Killer and GameLoop Scripts
			Kill killer = killerGO.transform.FindChild ("Firepoint").gameObject.GetComponent<Kill> ();
			GameLoop killerGL = killerGO.GetComponent<GameLoop> ();

//		//which one killed?
			for (int p = 0; p < players.Length; p++) {
//			
				if (players [p].GetPhotonView ().viewID == killedPlayerID) {

//				// Get Killed Player Body Parts
					GameObject killedPlayer = players [p].gameObject;
					GameLoop killedPlayerGL = killedPlayer.GetComponent<GameLoop> ();

					GameObject killedPlayerSkeleton = killedPlayer.transform.FindChild ("Gus_Skeleton").gameObject;
					GameObject killedPlayerArms = killedPlayer.transform.FindChild ("Arms").gameObject;

					Vector3 deathLocation = killedPlayer.transform.position;

					// Get Killer's Eyes (Image)
					SVGRenderer killerEyeLeft = killerGL.myLeftEye;
					SVGRenderer killerEyeRight = killerGL.myRightEye;

					// TODO: Start Death Anim HERE
					ParticleSystem Blood = killedPlayer.GetComponent<ParticleSystem> ();
					Blood.Play ();
	
					// Turn On Red Eyes IF MURDERER
					if (isKillerMurderer == true) {
						// Turn Killer's eyes red
						killerEyeLeft.color = Color.red;
						killerEyeRight.color = Color.red;
					}

					if (isKilledPlayerMurderer == false && killedPlayerGL.pv.isMine)
						killedPlayerGL.diedAsSpy = true;
					else if (isKilledPlayerMurderer && killedPlayerGL.pv.isMine)
						killedPlayerGL.diedAsSpy = false;
				
					// Hide Killed Player Body Parts
					killedPlayerSkeleton.SetActive (false);
					killedPlayerArms.SetActive (false);

					killedPlayerGL.myDeathStage = 1;

					yield return new WaitForSeconds (0.1f);

					// Blur Killed Player's View w/ Death Reason Message
					killedPlayerGL.deathReasonGlobal = deathReason;
					killedPlayerGL.myDeathStage = 2;


					yield return new WaitForSeconds (0.1f);

					//	Debug.Log ("PICKUP INSTANTIATED BRUH by " + this.gameObject.name);
					//	Debug.Log ("MASTER CLIENT is " + PhotonNetwork.masterClient.name);

					//	 Calculate how many Pickups we want to instantiate (1 per every 100 points the Killed player has)
					pickupsToInstantiate = killedPlayerPoints / numOfPointsFor1Pickup;

					if (pickupsToInstantiate < 1 || pickupsToInstantiate == null) {
						pickupsToInstantiate = 1;
					}

					if (pickupsToInstantiate > maxPickupsToInstantiate) {
						pickupsToInstantiate = maxPickupsToInstantiate;
					}

					if (PhotonNetwork.isMasterClient == true) {
							
						//Killed Murderer or Spy?
						if (isKilledPlayerMurderer == false) {

							while (pickupsToInstantiate != 0) {

								float randomX = Random.Range (-0.4f, 0.4f);
								float randomY = Random.Range (-0.3f, 0.3f);

								// Get Modified Killed Player Location
								killedPlayerLocation = new Vector3 (deathLocation.x + randomX, deathLocation.y + randomY, deathLocation.z);

								PhotonNetwork.InstantiateSceneObject ("bluePickup", killedPlayerLocation, this.gameObject.transform.rotation, 0, null);
								pickupsToInstantiate--;

							}
						}

						if (isKilledPlayerMurderer == true) {

							while (pickupsToInstantiate != 0) {

								float randomX = Random.Range (-0.4f, 0.4f);
								float randomY = Random.Range (-0.4f, 0.4f);

								// Get Modified Killed Player Location
								killedPlayerLocation = new Vector3 (deathLocation.x + randomX, deathLocation.y + randomY, deathLocation.z);

								PhotonNetwork.InstantiateSceneObject ("redPickup", killedPlayerLocation, this.gameObject.transform.rotation, 0, null);
								pickupsToInstantiate--;

							}

						}

					}

					yield return new WaitForSeconds (2.5f);

					// Turn Off Red Eyes HERE
					killerEyeLeft.color = Color.black;
					killerEyeRight.color = Color.black;

					if (isKilledPlayerMurderer == true) {
						killedPlayerSkeleton.SetActive (true);
						killedPlayerArms.SetActive (true);
					}

//				Debug.Log ("Pickup Deploy ENDED");

					yield return new WaitForSeconds (0.1f);

					killedPlayerGL.myDeathStage = 3;



				}
//
//
			}
		}
	
	}

	public void DestroyPickup (string targetPickupName)
	{
		// Find and Destroy
		GameObject targetPickup = GameObject.Find (targetPickupName);
		int newOwnerID = this.pv.ownerId;
		targetPickup.GetPhotonView ().TransferOwnership (newOwnerID);
		PhotonNetwork.Destroy (targetPickup);
		pv.RPC ("SetRefreshLeaderBoardsTrue", PhotonTargets.All);
	}

	[PunRPC]
	public void Immortalize ()
	{
		thisLoop.isImmortal = true;
		myTorso.color = halfAlpha;
		myRightArm.color = halfAlpha;
		myLeftArm.color = halfAlpha;
		myRightLeg.color = halfAlpha;
		myLeftLeg.color = halfAlpha;
		myRightEye.color = halfAlpha;
		myLeftEye.color = halfAlpha;
	}

	[PunRPC]
	public void UnImmortalize ()
	{
		thisLoop.isImmortal = false;
		myTorso.color = GM.charColors [(int)thisLoop.pv.owner.customProperties ["cs"]];
		myRightArm.color = GM.charColors [(int)thisLoop.pv.owner.customProperties ["cs"]];
		myLeftArm.color = GM.charColors [(int)thisLoop.pv.owner.customProperties ["cs"]];
		myRightLeg.color = GM.charColors [(int)thisLoop.pv.owner.customProperties ["cs"]];
		myLeftLeg.color = GM.charColors [(int)thisLoop.pv.owner.customProperties ["cs"]];
		myRightEye.color = black;
		myLeftEye.color = black;
	}
}
