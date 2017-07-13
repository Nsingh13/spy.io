using System;
using UnityEngine;
using UnityEngine.UI;
using SVGImporter;
using System.Collections;


public class PlayerController : Photon.MonoBehaviour
{

	public float movementSpeed = 0.09f;
	public Vector3 movementVector = Vector3.zero;
	public float jumpForce = 53f;
	public int maxJumps = 2;
	public int currentJump = 0;

	public Animator anim;

	public GameObject Joystick;
	public CNAbstractController MovementJoystick;
	[SerializeField] private Button jumpButton = null;
	[SerializeField] private Button fireButton = null;
	[SerializeField] private Button drinkButton = null;

	public GameObject drinkButtonGO;
	public SVGImage drinkButtonImage;

	public Rigidbody2D playerRb;
	public GameObject player;

	public bool facingLeft = false;

	private Transform _mainCameraTransform;
	private Transform _transformCache;
	private Transform _playerTransform;

	public PhotonView pv;
	public PlayerController thisPC;
	public GameLoop myGL;
	public PowerupManager PM;

	public GameObject powerupMessage;
	public Text powerupMessageText;

	public Color opaque = new Color (255f, 255f, 255f, 255f);
	public Color transparent = new Color (255f, 255f, 255f, 0f);
	public Color lerpedColor = new Color (255f, 255f, 255f, 0f);
	public float lerpSmoother = 5f;

	public float drinkButtonBounceHeight = 35f;
	public float drinkButtonBounceMax;
	public float drinkButtonBounceMin;

	public bool powerupInProgress = false;

	//Peek a Boo Powerup
	public Camera myCamera;
	public float normalCamSize = 0f;
	public float zoomedOutCamSize = 0f;

	//Super Speed Powerup
	public float superSpeedMovementSpeed = 0.15f;
	public float normalMovementSpeed = 0.09f;

	public AudioSource equipSFX;

	public AudioClip slurpSFX;
	public AudioClip gunSFX;

	void Awake()
	{
		Joystick = GameObject.FindGameObjectWithTag ("Joystick");
		MovementJoystick = ((CNJoystick)Joystick.GetComponent (typeof(CNJoystick)));

		fireButton = GameObject.Find ("Fire Button").GetComponent<Button> ();
		jumpButton = GameObject.Find ("Jump Button").GetComponent<Button> ();
		drinkButton = GameObject.Find ("Drink Button").GetComponent<Button> ();
		drinkButtonGO = GameObject.Find ("Drink Button");
		drinkButtonImage = drinkButtonGO.GetComponentInChildren<SVGImage> ();

		equipSFX = this.gameObject.GetComponent<AudioSource> ();
	}

	void Start()
	{
		//Set movement vector for delay calculation in "Network Player" script
		movementVector = new Vector3(movementSpeed, 0f, 0f);

		//Get Photon View to use for Buttons
		pv = this.gameObject.GetComponent<PhotonView> ();

		if (pv == null) {
			Debug.LogError ("You Have no Photon View Attached in the Player GO to call FlashMuzzle!");
		}
			

		//Button Functions
		fireButton.onClick.AddListener(() => { pv.RPC ("onFirePress", PhotonTargets.All); });
		fireButton.onClick.AddListener(() => { PlayGunSound(); });

		drinkButton.onClick.AddListener(() => {  pv.RPC ("onDrinkPress", PhotonTargets.All); });
		drinkButton.onClick.AddListener(() => { onDrinkPressPowerup(); });
		drinkButton.onClick.AddListener(() => { PlayDrinkSound(); });

		jumpButton.onClick.AddListener(() => { onJumpPress(); });

		thisPC = this.gameObject.GetComponent<PlayerController> ();
		myGL = this.gameObject.GetComponent<GameLoop> ();
		PM = GameObject.Find ("GameController").GetComponent<PowerupManager> ();

		// You can also move the character with an event system
		// You MUST CHOOSE one method and use ONLY ONE a frame
		// If you want the event based control, uncomment this line
		// MovementJoystick.JoystickMovedEvent += MoveWithEvent;

		_mainCameraTransform = Camera.main.GetComponent<Transform>();
		_transformCache = GetComponent<Transform>();
		//_playerTransform = _transformCache.FindChild("MS_Front");

		powerupMessage = GameObject.Find ("Powerup Message").gameObject;
		powerupMessageText = powerupMessage.GetComponent<Text> ();

		lerpedColor = transparent;

		myCamera = this.gameObject.transform.FindChild ("Main Camera").gameObject.GetComponent<Camera>();
		normalCamSize = myCamera.orthographicSize;
		zoomedOutCamSize = myCamera.orthographicSize * 2;

		drinkButtonBounceMax = drinkButtonGO.transform.localPosition.y + drinkButtonBounceHeight;
		drinkButtonBounceMin = drinkButtonGO.transform.localPosition.y;

		superSpeedMovementSpeed = 0.2f;
		normalMovementSpeed = 0.09f;
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
	}

	void Update()
	{
		powerupMessageText.color = Color.Lerp (powerupMessageText.color, lerpedColor, lerpSmoother * Time.deltaTime);

	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (Joystick.activeInHierarchy) {
			
			var movement = new Vector2 (
				              MovementJoystick.GetAxis ("Horizontal"),
				              MovementJoystick.GetAxis ("Vertical"));
		
			CommonMovementMethod (movement);

			// Joystick at Center
			if (movement.x == 0) {
				//Idle Animation
				if (thisPC.facingLeft == false) {
					anim.SetInteger ("MoveState", 10);
				}

				if (thisPC.facingLeft == true) {
					anim.SetInteger ("MoveState", -10);
				}
			}
			//movement left
			if (movement.x <= -0.1f) {

				//Change Direction Data
				if (thisPC.facingLeft == false)
					pv.RPC ("FacedLeft", PhotonTargets.AllBuffered);

				//Left Walk Animation
				anim.SetInteger ("MoveState", -1);

				//Movement
				player.transform.position = new Vector3 (player.transform.position.x - movementSpeed, player.transform.transform.position.y, -10);
			}
		

			//movement right
			if (movement.x >= 0.1f) {
				//Change Direction Data
				if (thisPC.facingLeft == true)
					pv.RPC ("FacedRight", PhotonTargets.AllBuffered);
			
				//Left Walk Animation
				anim.SetInteger ("MoveState", 1);

				//Movement
				player.transform.position = new Vector3 (player.transform.position.x + movementSpeed, player.transform.transform.position.y, -10);
			}
		
			//movement front
			if (movement.y <= -0.6f) {

			}
		
			//movement back
			if (movement.y >= 0.6f) {

			}
		
			if (Input.GetAxisRaw ("Horizontal") > 0) {
				transform.eulerAngles = new Vector2 (0, 0);
			}
		
			if (Input.GetAxisRaw ("Horizontal") < 0) {
				transform.eulerAngles = new Vector2 (0, 180);
			}

		
		}
	}
		
	
	private void MoveWithEvent(Vector2 inputMovement)
	{
		var movement = new Vector2(
			inputMovement.x,
			inputMovement.y);
		
		CommonMovementMethod(movement);
	}
	
	private void CommonMovementMethod(Vector2 movement)
	{
		
		
		movement = _mainCameraTransform.TransformDirection(movement);
		//movement.z = 0f;
		movement.Normalize();



	}

	[PunRPC]
	public void FacedRight()
	{
		PlayerController thisPLC = this.gameObject.GetComponent<PlayerController> ();

		thisPLC.facingLeft = false;
		Animator armAnimGlobal = this.gameObject.transform.FindChild("Arms").gameObject.GetComponent<Animator>();

		if (armAnimGlobal.GetInteger ("EquipAnim") == -1 || armAnimGlobal.GetInteger ("EquipAnim") == -3) 
		{
			// Shoot Right Idle
			armAnimGlobal.SetInteger ("EquipAnim", 3);
			armAnimGlobal.Play ("ShootIdleRightREAL", -1, 0f);
		}

		else if (armAnimGlobal.GetInteger ("EquipAnim") == -2 || armAnimGlobal.GetInteger ("EquipAnim") == -4) 
		{
			armAnimGlobal.SetInteger ("EquipAnim", 4);
			armAnimGlobal.Play ("DrinkRightIdleREAL", -1, 0f);
		}
	}

	[PunRPC]
	public void FacedLeft()
	{
		PlayerController thisPLC = this.gameObject.GetComponent<PlayerController> ();

		thisPLC.facingLeft = true;
		Animator armAnimGlobal = this.gameObject.transform.FindChild("Arms").gameObject.GetComponent<Animator>();

		if (armAnimGlobal.GetInteger ("EquipAnim") == 1 || armAnimGlobal.GetInteger ("EquipAnim") == 3) 
		{
			armAnimGlobal.SetInteger ("EquipAnim", -3);
			armAnimGlobal.Play ("ShootIdleLeftREAL", -1, 0f);
		}

		else if (armAnimGlobal.GetInteger ("EquipAnim") == 2 || armAnimGlobal.GetInteger ("EquipAnim") == 4) 
		{
			armAnimGlobal.SetInteger ("EquipAnim", -4);
			armAnimGlobal.Play ("DrinkLeftIdleREAL", -1, 0f);
		}
	}

	[PunRPC]
	public void onFirePress()
	{
		Fire thisFireLocal = this.gameObject.transform.FindChild ("Firepoint").gameObject.GetComponent<Fire> ();

	//	if (thisFireLocal.bulletShot == false && thisFireLocal.reloading == false) {

			AudioSource thisEquipSFX = this.gameObject.GetComponent<AudioSource> ();

			if (thisEquipSFX != null) 
			{
				if (GameManager.sfxOff == false) 
				{
					thisEquipSFX.clip = gunSFX;
					thisEquipSFX.Play ();
				}
			}

			//Get Gun and Glass
			GameObject Gun = this.gameObject.transform.FindChild("Arms/Gus_Arm/Holder/Gun").gameObject;
			GameObject Glass = this.gameObject.transform.FindChild ("Arms/Gus_Arm/Holder/Glass").gameObject;

			//Get this instance of the PlayerController
			PlayerController thisPLC = this.gameObject.GetComponent<PlayerController> ();

			//Disable Glass
			if (Glass.activeInHierarchy == true) 
				Glass.SetActive (false);

			//Enable Gun
			if (Gun.activeInHierarchy == false)
				Gun.SetActive (true);

			if (thisPLC.facingLeft == true) {



				//Get Arm Animatior
				Animator armAnim = this.gameObject.transform.FindChild("Arms").gameObject.GetComponent<Animator>();

				// Shoot Gun Left
				armAnim.SetInteger ("EquipAnim", -1);

				armAnim.Play ("ShootIdleLeft", -1, 0f);

				thisFireLocal.Shoot (true);
			}

			if (thisPLC.facingLeft == false) {



				//Get Arm Animatior
				Animator armAnim = this.gameObject.transform.FindChild("Arms").gameObject.GetComponent<Animator>();

				// Shoot Gun Right
				armAnim.SetInteger ("EquipAnim", 1);

				armAnim.Play ("ShootIdleRight", -1, 0f);

				thisFireLocal.Shoot (false);

			}
			
		//	}
			
	}

	public void PlayGunSound()
	{
//		Fire thisFireLocal = this.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Torso/Bullet").GetComponent<Fire> ();
//
//		if (thisFireLocal.bulletShot == false && thisFireLocal.reloading == false) {
//			equipSFX.clip = gunSFX;
//			equipSFX.Play ();
//		}
	}

	[PunRPC]
	public void onDrinkPress()
	{


		GameObject Glass = this.gameObject.transform.FindChild ("Arms/Gus_Arm/Holder/Glass").gameObject;
		GameObject Gun = this.gameObject.transform.FindChild("Arms/Gus_Arm/Holder/Gun").gameObject;

		//Get this instance of the PlayerController
		PlayerController thisPLC = this.gameObject.GetComponent<PlayerController>();

		//Disable Gun
		if (Gun.activeInHierarchy == true)
			Gun.SetActive (false);


		//Enable Glass
		if (Glass.activeInHierarchy == false) 
			Glass.SetActive (true);
		

		if (thisPLC.facingLeft == true) {

			//Get Arm Animatior
			Animator armAnim = this.gameObject.transform.FindChild("Arms").gameObject.GetComponent<Animator>();

			// Drink Left
			armAnim.SetInteger ("EquipAnim", -2);

			armAnim.Play("DrinkLeftIdle", -1, 0f);
		}

		if (thisPLC.facingLeft == false) {

			//Get Arm Animatior
			Animator armAnim = this.gameObject.transform.FindChild("Arms").gameObject.GetComponent<Animator>();

			// Drink Right
			armAnim.SetInteger ("EquipAnim", 2);

			armAnim.Play("DrinkRightIdle", -1, 0f);
		}
			

	}

	public void PlayDrinkSound()
	{
		if (GameManager.sfxOff == false) 
		{
			equipSFX.clip = slurpSFX;
			equipSFX.Play ();
		}
	}

	public void onDrinkPressPowerup()
	{
		// Handle Powerups -------------------------------------------------------------------------------------------------------------------------------------------
		if (powerupInProgress == false) {
			switch (myGL.currentPowerup) {
			case 0:
				break;
			case 1:
				myGL.currentPowerup = 0;
				drinkButtonImage.color = Color.white;
				StartCoroutine ("powerupMessageAnimation", 1);
				break;
			case 2:
				myGL.currentPowerup = 0;
				drinkButtonImage.color = Color.white;
				StartCoroutine ("powerupMessageAnimation", 2);
				break;
			case 3:
				myGL.currentPowerup = 0;
				drinkButtonImage.color = Color.white;
				StartCoroutine ("powerupMessageAnimation", 3);
				break;
			}
		}
	}

	public IEnumerator powerupButtonBounce()
	{
		if ((int)myGL.pv.owner.customProperties["t"] == 1)
			drinkButtonImage.color = Color.red;

		if((int)myGL.pv.owner.customProperties["t"] != 1)
			drinkButtonImage.color = Color.cyan;
		
		iTween.MoveTo (drinkButtonGO, iTween.Hash ("islocal", true, "x", drinkButtonGO.transform.localPosition.x, "y", drinkButtonBounceMax, "easetype", iTween.EaseType.linear, "time", 0.2f));

		yield return new WaitForSeconds (0.2f);

		iTween.MoveTo (drinkButtonGO, iTween.Hash ("islocal", true, "x", drinkButtonGO.transform.localPosition.x, "y", drinkButtonBounceMin, "easetype", iTween.EaseType.easeOutCirc, "time", 0.19f));
	}

	public IEnumerator powerupMessageAnimation(int selectedPowerup)
	{
		//Reset Scale and Color
		powerupInProgress = true;
		lerpSmoother = 0.005f;
		powerupMessage.transform.localScale = new Vector3(2f, 2f, 2f);
	    lerpedColor = transparent;

		switch (selectedPowerup) 
		{
		case 0:
			break;
		case 1:
			powerupMessageText.text = "Peek-a-Boo";
			StartCoroutine ("PeekaBooPowerup", 6f);
			break;
		case 2:
			powerupMessageText.text = "Super Speed";
			StartCoroutine ("SuperSpeedPowerup", 5f);
			break;
		case 3:
			powerupMessageText.text = "Immortality";
			StartCoroutine ("ImmortalityPowerup", 5f);
			break;
		}

		yield return new WaitForSeconds (0.1f);

		//Itween Show
		iTween.ScaleTo(powerupMessage, iTween.Hash("x", 1, "y", 1, "z", 1, "time", 0.3, "easetype", iTween.EaseType.linear));
		lerpedColor = opaque;

		yield return new WaitForSeconds (0.1f);

		lerpSmoother = 13f;

		yield return new WaitForSeconds (1.5f);

		//Itween Hide
		iTween.ScaleTo(powerupMessage, iTween.Hash("x", 2, "y", 2, "z", 2, "time", 0.3, "easetype", iTween.EaseType.linear));
		lerpedColor = transparent;
	}

	public void onJumpPress()
	{
		//Jump
		if (currentJump < maxJumps) 
		{
			playerRb.AddForce (new Vector2 (0f, jumpForce));
			currentJump++;
		}

	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.isTrigger == false && currentJump != 0) 
		{
			currentJump = 0;
		}
			
	}

	public IEnumerator PeekaBooPowerup(float time)
	{
		//TODO: Lerp Camera Zoom

		// Reset Cam
		myCamera.orthographicSize = normalCamSize;

		yield return new WaitForSeconds (0.1f);

		myCamera.orthographicSize = zoomedOutCamSize;

		yield return new WaitForSeconds (time);

		myCamera.orthographicSize = normalCamSize;
		powerupInProgress = false;
	}

	public IEnumerator SuperSpeedPowerup(float time)
	{
		movementSpeed = superSpeedMovementSpeed;

		yield return new WaitForSeconds (time);

		movementSpeed = normalMovementSpeed;

		powerupInProgress = false;
	}

	public IEnumerator ImmortalityPowerup(float time)
	{
		// Hand over to GameLoop
		myGL.pv.RPC ("Immortalize", PhotonTargets.AllBuffered);

		yield return new WaitForSeconds (time);

		myGL.pv.RPC ("UnImmortalize", PhotonTargets.AllBuffered);

		powerupInProgress = false;

	}
}
