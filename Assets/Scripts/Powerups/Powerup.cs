using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour {

	public Transform myLocation;
	public PowerupManager PM;

	public static int spawnNum;

	public bool collectedPowerup = false;

	// Use this for initialization
	void Start () {
		PM = GameObject.Find ("GameController").GetComponent<PowerupManager> ();

		spawnNum++;

		this.gameObject.name = "Powerup " + spawnNum;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D (Collider2D HitOBJ)
	{
		if (HitOBJ.gameObject.tag == "Player" && HitOBJ.gameObject.activeInHierarchy && this.collectedPowerup == false) 
		{
			GameLoop HitGL = HitOBJ.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();
			PlayerController HitPC = HitOBJ.transform.parent.gameObject.transform.parent.gameObject.GetComponent<PlayerController> ();

			HitGL.currentPowerup = Random.Range(1,4);

			HitPC.StartCoroutine ("powerupButtonBounce");

			PM.pv.RPC ("CollectPowerup", PhotonTargets.AllBuffered, this.gameObject.name, Random.Range (0, PM.PowerupSpawnLocations.Count));
		}
	}
}
