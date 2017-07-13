using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Collections;
using System.Collections.Generic;
using SVGImporter;
using System;	//<---- Important Files To Import
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

public class GameManager : MonoBehaviour, ISkippableVideoAdListener {

	public static bool noAdsBought = false;

	public NetworkManager NM;
	public Color lightBlue;

	// In-Game UI
	public GameObject inGameUI;
	public GameObject jumpButton;
	public GameObject fireButton;
	public GameObject drinkButton;
	public GameObject changeIdButton;
	public GameObject joystick;
	public GameObject leaderboard;
	public GameObject currentRank;
	public GameObject currentScore;
	public GameObject currentTeam;
	public Text deathMessage;

	public SVGImage drinkButtonImage;

	public Color transparent;

	public GameObject backgroundSmoke;
	public GameObject moon;

	//Menu
	public bool firstPlay = true;
	public bool firstPlayUnsaved = true;

	public GameObject menu;
	public GameObject tutorial;

	//Player Username
	public InputField usernameIF;
	public InputField usernameIFgame;

	public static bool setChangeNameTrue = false;
	public static bool changeName = false;
	public static bool setChangeSkinTrue = false;
	public static bool changeSkin = false;

	//In-Game UI Original Scale
	public Vector3 jumpButtonScale;
	public Vector3 fireButtonScale;
	public Vector3 drinkButtonScale;
	public Vector3 changeIdButtonScale;
	public Vector3 joystickScale;
	public Vector3 leaderboardScale;
	public Vector3 currentRankScale;
	public Vector3 currentScoreScale;
	public Vector3 currentTeamScale;

	public bool UIvisible = false;

	// Main Menu Sections
	public GameObject menuSection;
	public GameObject settingsSection;
	public GameObject skinChangeSection;

	public float centerY;
	public float topY;
	public float bottomY;

	//Main Menu Buttons
	public GameObject adRemoveButton;
	public GameObject settingsButton;
	public GameObject shareButton;
	public GameObject skinChangeButton;
	public GameObject skinChangeOKButton;
	public GameObject settingsOKButton;

	// Change ID In-Game
	public GaussianBlur worldBlur;
	public GameObject changeIdOKButton;
	public GameObject inGameSkinChanger;
	public GameObject inGameUsernameBox;
	public Vector3 inGameSkinChangerUpScale;
	public Vector3 inGameUsernameBoxUpScale;

	//Game Backgrounds
	public SVGRenderer background;
	public int currentBackgroundColor;
	public Color[] gameBackgroundColors;

	//Scores
	public GameObject previousScoreGO;
	public GameObject bestScoreGO;

	//Scores
	public int previousScore = 0;
	public int bestScore = 0;

	//Hats
	public Color[] charColors;
	public SVGImage charModel;
	public SVGImage inGameCharModel;

	//Settings
	public Slider volSlider;
	public Toggle sfxToggle;
	public static bool sfxOff = false;

	public GameObject backdropGO;
	public static Vector2 backdropPos;

	void Awake()
	{
		// Set Background First to Purple (IN NETWORK MANAGER)
		currentBackgroundColor = -1;

		//loads on awake
		Load ();

		jumpButtonScale = jumpButton.transform.localScale;
		fireButtonScale = fireButton.transform.localScale;
		drinkButtonScale = drinkButton.transform.localScale;
		changeIdButtonScale = changeIdButton.transform.localScale;
		joystickScale = joystick.transform.localScale;
		leaderboardScale = leaderboard.transform.localScale;
		currentRankScale = currentRank.transform.localScale;
		currentScoreScale = currentScore.transform.localScale;
		currentTeamScale = currentTeam.transform.localScale;

		Appodeal.confirm (Appodeal.SKIPPABLE_VIDEO);
	}

	// Use this for initialization
	void Start () {

		backdropPos = backdropGO.transform.localPosition;

		String appKey = "7826fd983bfad3559bc427f68035de140d6407ac89b9416e";
		Appodeal.initialize(appKey, Appodeal.INTERSTITIAL | Appodeal.SKIPPABLE_VIDEO);
		Appodeal.setSkippableVideoCallbacks(this);
	}
	
	// Update is called once per frame
	void Update () {


	}

	public void ShowAd()
	{
		if (noAdsBought == false)
		{
			Appodeal.show(Appodeal.SKIPPABLE_VIDEO);
		}
	}

	public void onSkippableVideoFailedToLoad() 
	{ 
		Appodeal.show(Appodeal.INTERSTITIAL);
	}

	public void onSkippableVideoLoaded() {  }
	public void onSkippableVideoShown() {  }
	public void onSkippableVideoFinished() { }
	public void onSkippableVideoClosed() { }

	public void loadGame()
	{
		backgroundSmoke.SetActive (false);
		inGameCharModel.color = charColors [(int)NM.playerProperties["cs"]];

		if (tutorial.activeInHierarchy)
			tutorial.SetActive (false);

		if (firstPlay == true) 
		{
			firstPlay = false;
			Save ();
		}

		if (firstPlayUnsaved == true)
			firstPlayUnsaved = false;
		
		NM.JoinRandomRoom ();
	}

	public void HideInGameUI()
	{
		inGameUI.SetActive (false);
		leaderboard.SetActive (false);
		currentRank.SetActive (false);
		currentScore.SetActive (false);
		currentTeam.SetActive (false);
		jumpButton.SetActive (false);
		fireButton.SetActive (false);
		drinkButton.SetActive (false);
		changeIdButton.SetActive (false);
		joystick.SetActive (false);

		UIvisible = false;

	}

	public void ShowInGameUI()
	{
		inGameUI.SetActive (true);
		leaderboard.SetActive (true);
		currentRank.SetActive (true);
		currentScore.SetActive (true);
		currentTeam.SetActive (true);
		jumpButton.SetActive (true);
		fireButton.SetActive (true);
		drinkButton.SetActive (true);
		changeIdButton.SetActive (true);
		joystick.SetActive (true);

		UIvisible = true;
	}

	public void ScaleDownInGameUI()
	{
		leaderboard.transform.localScale = Vector3.zero;
		currentRank.transform.localScale = Vector3.zero;
		currentScore.transform.localScale = Vector3.zero;
		currentTeam.transform.localScale = Vector3.zero;
		jumpButton.transform.localScale = Vector3.zero;
		fireButton.transform.localScale = Vector3.zero;
		drinkButton.transform.localScale = Vector3.zero;
		changeIdButton.transform.localScale = Vector3.zero;
		joystick.transform.localScale = Vector3.zero;
		CNJoystick joystickScript = joystick.transform.FindChild("CNJoystick").gameObject.GetComponent<CNJoystick> ();
		joystickScript.enabled = false;

		UIvisible = false;

	}

	public void ScaleBackInGameUI()
	{
		leaderboard.transform.localScale = leaderboardScale;
		currentRank.transform.localScale = currentRankScale;
		currentScore.transform.localScale = currentScoreScale;
		currentTeam.transform.localScale = currentTeamScale;
		jumpButton.transform.localScale = jumpButtonScale;
		fireButton.transform.localScale = fireButtonScale;
		drinkButton.transform.localScale = drinkButtonScale;
		changeIdButton.transform.localScale = changeIdButtonScale;
		joystick.transform.localScale = joystickScale;
		CNJoystick joystickScript = joystick.transform.FindChild("CNJoystick").gameObject.GetComponent<CNJoystick> ();
		joystickScript.enabled = true;

		UIvisible = true;

	}

	public void ShowMenu()
	{
		// Set Other Stuff
		backdropGO.transform.localPosition = backdropPos;
		backgroundSmoke.SetActive (true);
		moon.SetActive (false);
	
		deathMessage.color = transparent;

		// Bring Back Menu
		menu.SetActive (true);

	}

	public void disablePowerup()
	{
		if(drinkButtonImage.color != Color.white)
			drinkButtonImage.color = Color.white;
	}

	// Main Menu
	public void PlayClicked()
	{
		menu.SetActive (false);
		moon.SetActive (true);

		if (firstPlay == false) {
			loadGame ();
		}

		if (firstPlay == true) {
			tutorial.SetActive (true);
		}
			
	}

	public void HtpClicked()
	{
		menu.SetActive (false);
		moon.SetActive (true);

		tutorial.SetActive (true);

		if (firstPlay == true) {
			firstPlay = false;
			Save ();
		}
	}

	public void SettingsClicked()
	{
		// Only OK button active
		skinChangeButton.SetActive(false);
		shareButton.SetActive (false);
		adRemoveButton.SetActive (false);
		settingsButton.SetActive (false);
		skinChangeOKButton.SetActive (false);
		previousScoreGO.SetActive (false);
		bestScoreGO.SetActive (false);
		settingsOKButton.SetActive (true);

		iTween.MoveTo (settingsSection, iTween.Hash ("islocal", true, "x", settingsSection.transform.localPosition.x, "y", centerY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));
		iTween.MoveTo (menuSection, iTween.Hash ("islocal", true, "x", menuSection.transform.localPosition.x, "y", bottomY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));
	}

	public void SettingsOKClicked()
	{
		
		skinChangeButton.SetActive(true);
		shareButton.SetActive (true);
		adRemoveButton.SetActive (true);
		settingsButton.SetActive (true);
		skinChangeOKButton.SetActive (false);

		settingsOKButton.SetActive (false);

		iTween.MoveTo (settingsSection, iTween.Hash ("islocal", true, "x", settingsSection.transform.localPosition.x, "y", topY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));
		iTween.MoveTo (menuSection, iTween.Hash ("islocal", true, "x", menuSection.transform.localPosition.x, "y", centerY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));

		previousScoreGO.SetActive (true);
		bestScoreGO.SetActive (true);
	}

	public void SkinChangeClicked()
	{
		// Only OK button active
		skinChangeButton.SetActive(false);
		shareButton.SetActive (false);
		adRemoveButton.SetActive (false);
		settingsButton.SetActive (false);
		settingsOKButton.SetActive (false);
		previousScoreGO.SetActive (false);
		bestScoreGO.SetActive (false);
		skinChangeOKButton.SetActive (true);

		iTween.MoveTo (skinChangeSection, iTween.Hash ("islocal", true, "x", skinChangeSection.transform.localPosition.x, "y", centerY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));
		iTween.MoveTo (menuSection, iTween.Hash ("islocal", true, "x", menuSection.transform.localPosition.x, "y", topY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));
	}

	public void SkinChangeOKClicked()
	{
		PhotonNetwork.player.SetCustomProperties (NM.playerProperties);

		skinChangeButton.SetActive(true);
		shareButton.SetActive (true);
		adRemoveButton.SetActive (true);
		settingsButton.SetActive (true);
		settingsOKButton.SetActive (false);

		skinChangeOKButton.SetActive (false);

		iTween.MoveTo (skinChangeSection, iTween.Hash ("islocal", true, "x", skinChangeSection.transform.localPosition.x, "y", bottomY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));
		iTween.MoveTo (menuSection, iTween.Hash ("islocal", true, "x", menuSection.transform.localPosition.x, "y", centerY, "time", 0.25f, "easetype", iTween.EaseType.easeOutCirc));

		previousScoreGO.SetActive (true);
		bestScoreGO.SetActive (true);
	}

	public void ChangeIdClicked()
	{
		ScaleDownInGameUI ();
		changeIdButton.SetActive (false);
		changeIdOKButton.SetActive (true);

		inGameSkinChanger.SetActive (true);
		inGameUsernameBox.SetActive (true);

		iTween.ScaleTo (inGameSkinChanger, iTween.Hash ("islocal", true, "scale", inGameSkinChangerUpScale, "time", 0.15f, "easetype", iTween.EaseType.linear));
		iTween.ScaleTo (inGameUsernameBox, iTween.Hash ("islocal", true, "scale", inGameUsernameBoxUpScale, "time", 0.15f, "easetype", iTween.EaseType.linear));
	}

	public void ChangeIdOKClicked()
	{
		
		changeIdOKButton.SetActive (false);
		ScaleBackInGameUI ();
		changeIdButton.SetActive (true);

		inGameSkinChanger.transform.localScale = Vector3.zero;
		inGameUsernameBox.transform.localScale = Vector3.zero;

		inGameSkinChanger.SetActive (false);
		inGameUsernameBox.SetActive (false);

		if ((int)PhotonNetwork.player.customProperties ["cs"] != (int)NM.playerProperties ["cs"]) 
		{
			NM.playerProperties ["s"] = previousScore;
			PhotonNetwork.player.SetCustomProperties (NM.playerProperties);
			setChangeSkinTrue = true;
			changeSkin = false;
		}
	}

	public void UpdateUsername()
	{
		PhotonNetwork.playerName = usernameIF.text;
	}

	public void UpdateUsernameInGame()
	{
		PhotonNetwork.playerName = usernameIFgame.text;

		setChangeNameTrue = true;
		changeName = false;
	}

	public void ChangeBackgroundColor()
	{
		currentBackgroundColor++;

		if (currentBackgroundColor > (gameBackgroundColors.Length - 1))
			currentBackgroundColor = 0;

		background.color = gameBackgroundColors [currentBackgroundColor];
	}

	public void NextHatPress(bool ingame)
	{
		NM.playerProperties["cs"] = (int)NM.playerProperties["cs"] + 1;

		//TODO: If greater than color, move on to striped torso options
		if ((int)NM.playerProperties["cs"] >= charColors.Length) 
		{
			NM.playerProperties ["cs"] = 0;
		}

		if(ingame == false)
			charModel.color = charColors [(int)NM.playerProperties["cs"]];
		
		else
			inGameCharModel.color = charColors [(int)NM.playerProperties["cs"]];
	}

	public void PreviousHatPress(bool ingame)
	{
		NM.playerProperties["cs"] = (int)NM.playerProperties["cs"] - 1;

		//TODO: If less than 0, go to last one in striped torso options
		if ((int)NM.playerProperties["cs"] < 0) 
		{
			NM.playerProperties ["cs"] = charColors.Length - 1;
		}

		//TODO: If less than last on in striped options, go to last in color options

		if(ingame == false)
			charModel.color = charColors [(int)NM.playerProperties["cs"]];

		else
			inGameCharModel.color = charColors [(int)NM.playerProperties["cs"]];

	}

	public void playButtonAudio()
	{
		NM.uiAudio.Play ();
	}

	public void VolumeSliderChange()
	{
		NM.backgroundMusic.volume = volSlider.value;
	}

	public void sfxToggleChange()
	{
		if (sfxToggle.isOn == false) 
		{
			NM.uiAudio.volume = 0;
			sfxOff = true;
		}

		else if (sfxToggle.isOn == true) 
		{
			NM.uiAudio.volume = 0.8f;
			sfxOff = false;
		}
	}
		


	//Save Method
	public void Save()
	{
		//Sets a binaryformatter and creates a binary file in the persistant data file path
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream SAVEfile = File.Create(Application.persistentDataPath + "/Saveinfo.dat");

		SAVEdata data = new SAVEdata ();
		data.HS = bestScore;
		data.NAB = noAdsBought;
		data.FP = firstPlay;
		//Serializes Data in data class (HSdata)
		bf.Serialize (SAVEfile, data);
		SAVEfile.Close ();
	}

	//Load Method
	public void Load()
	{
		//if the following file exists...
		if (File.Exists (Application.persistentDataPath + "/Saveinfo.dat")) 
		{
			BinaryFormatter bf = new BinaryFormatter ();
			//open file
			FileStream SAVEfile = File.Open (Application.persistentDataPath + "/Saveinfo.dat", FileMode.Open);
			//takes out info
			SAVEdata data = (SAVEdata)bf.Deserialize(SAVEfile);
			//closes file
			SAVEfile.Close();
			//and lastly...
			bestScore = data.HS;
			noAdsBought = data.NAB;
			firstPlay = data.FP;
		}
	}

	//the info that is saved
	[Serializable]
	class SAVEdata
	{
		public int HS;
		public bool NAB;
		public bool FP;
	}
}
