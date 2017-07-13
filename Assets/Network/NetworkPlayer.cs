using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class NetworkPlayer : Photon.MonoBehaviour {

	private float LastKnownDataRecievedTime = 0f;
	private float syncTime = 0f;
	private float syncDelay = 0f;


	public GameObject myCamera;
	public PlayerController myPlayercontroller;

	Animator playerAnim;

	Vector3 Velocity;

	bool isAlive = true;
	public float lerpSmoothing = 5f;

	Vector3 position;
	private Vector3 syncStartPos = Vector3.zero;
	private Vector3 syncEndPos = Vector3.zero;

	// Use this for initialization
	void Start () {


		//Get Animation Component
		playerAnim = GetComponent<Animator> ();

		myPlayercontroller = this.gameObject.GetComponent<PlayerController> ();

		if (playerAnim == null) {
			Debug.LogError ("Yo, you don't have an animation component on this player!");
		}


		if (photonView.isMine) {

			PlayerController thisPC = this.gameObject.GetComponent<PlayerController> ();
			GameLoop thisGL = this.gameObject.GetComponent<GameLoop> ();
			Fire thisFire = this.gameObject.transform.FindChild ("Firepoint").gameObject.GetComponent<Fire> ();
			Kill thisKill = thisFire.gameObject.GetComponent<Kill> ();
			Rigidbody2D thisRB = this.gameObject.GetComponent<Rigidbody2D> ();	
			CircleCollider2D triggerJump = this.gameObject.GetComponent<CircleCollider2D> ();
			AudioListener myEars = this.gameObject.GetComponent<AudioListener> ();

			//Set Notification Bar Objects
			thisGL.banner = GameObject.Find("Team Popup").transform.FindChild("Backdrop").gameObject;
			thisGL.logo = GameObject.Find("Team Popup").transform.FindChild("Backdrop/Logo").gameObject;
			thisGL.youAre = GameObject.Find("Team Popup").transform.FindChild("Backdrop/You Are").gameObject;
			thisGL.team = GameObject.Find("Team Popup").transform.FindChild("Backdrop/Team").gameObject;
			thisGL.teamImage = GameObject.Find ("Team Popup").transform.FindChild ("Backdrop/Team").GetComponent<Image> ();

			myCamera.SetActive (true);
			triggerJump.enabled = true;
			thisPC.enabled = true;
			thisFire.enabled = true;
			thisKill.enabled = true;
			myEars.enabled = true;
			thisRB.gravityScale = 1;



		} else {			

			StartCoroutine ("Alive");
		}



	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) {

			//Send Data about OUR player over network
			stream.SendNext(transform.position);

			if(myPlayercontroller != null && myPlayercontroller.movementVector != null)
			stream.SendNext (myPlayercontroller.movementVector);

			//Animations
			if(playerAnim != null)
			stream.SendNext(playerAnim.GetInteger("MoveState"));


		} else {
			
			// Someone else's player --> Recieve data
			position = (Vector3)stream.ReceiveNext();
			Velocity = (Vector3)stream.ReceiveNext ();

			//Prediction --> Reduce Delay
			syncTime = 0f;
			syncDelay = Time.time - LastKnownDataRecievedTime;
			LastKnownDataRecievedTime = Time.time;

			syncEndPos = position + Velocity * syncDelay;
			syncStartPos = transform.position;

			//Animations
			if(playerAnim != null)
			playerAnim.SetInteger("MoveState", (int)stream.ReceiveNext());

		}
	}
	
	//While alive do this State-Machine
	IEnumerator Alive()
	{
		while (isAlive) {
			syncTime += Time.deltaTime;
			transform.position = Vector3.Lerp (syncStartPos, syncEndPos, syncTime / syncDelay); //before: Time.deltaTime * lerpSmoothing

			yield return null;
		}
	}
}
