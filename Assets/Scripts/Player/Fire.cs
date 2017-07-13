using UnityEngine;
using System.Collections;

public class Fire : Photon.MonoBehaviour {

	public int fireSpeed = 100;
	public bool reloading = false;
	public bool bulletShot = false;
	public float shotDistance = 10f;
	public static float reloadingTime = 0.01f;


	public float flashSize = 0f;
	public bool flashMuzzle = false;

	public Fire thisFire;
	public Kill thisKill;
	public PlayerController thisPC;

	public GameLoop myGL;

	public PhotonView pv;
	// Use this for initialization
	void Start () {

		thisFire = this.gameObject.GetComponent<Fire> ();
		thisPC = this.gameObject.transform.parent.gameObject.GetComponent<PlayerController> ();

		pv = this.gameObject.GetComponent<PhotonView>();

		myGL = this.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

	}

	// Update is called once per frame
	void Update () {


	}
		

	[PunRPC]
	public void Shoot(bool facingLeft)
	{

		Fire thisFireLocal = this.gameObject.GetComponent<Fire> ();
		Kill thisKillLocal = this.gameObject.GetComponent<Kill> ();
		Vector2 facingDir = Vector2.zero;

		StopCoroutine("MuzzleFlash");
		StartCoroutine ("MuzzleFlash");

		if (facingLeft == false) 
		{
			facingDir = Vector2.right;
		}

		else if (facingLeft) 
		{
			facingDir = Vector2.left;
		}

		RaycastHit2D hit = Physics2D.Raycast (transform.position, facingDir, shotDistance);

		if (hit.collider != null) 
		{
			if (hit.collider.gameObject.name == "Gus_Torso") 
			{
				GameLoop hitGL = hit.collider.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

				if (hitGL != null) 
				{
					if (thisFireLocal.myGL != null) 
					{
						if (hitGL.pv.viewID != thisFireLocal.myGL.pv.viewID)
							thisKillLocal.KillPlayer (hit.collider);
					}
				}
			}

			else if (hit.collider.gameObject.name == "Gus_Arm" || hit.collider.gameObject.name == "Gus_Arm (R)") 
			{
				GameLoop hitGL = hit.collider.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

				if (hitGL != null) 
				{
					if (thisFireLocal.myGL != null)
					{
						if (hitGL.pv.viewID != thisFireLocal.myGL.pv.viewID)
							thisKillLocal.KillPlayer (hit.collider.gameObject.transform.parent.gameObject.transform.parent.gameObject.transform.FindChild ("Gus_Skeleton/Gus_Torso").gameObject.GetComponent<Collider2D> ());
					}
				}
			}
				
		}
			
//		thisFireLocal.reloading = false;
//		thisFireLocal.bulletShot = false;

		//StartCoroutine ("Reload");
	}


//	public IEnumerator Reload()
//	{
//		Kill thisKillLocal = this.gameObject.GetComponent<Kill> ();
//		Fire thisFireLocal = this.gameObject.GetComponent<Fire> ();
//
//		thisFireLocal.reloading = true;
//
//		bulletShot = false;
//
//		yield return new WaitForSeconds (0.01f);
//
//	//	yield return new WaitForSeconds (0.01f);
//
//	//	this.gameObject.SetActive (false);
//		thisFireLocal.reloading = false;
//	}

	[PunRPC]
	public IEnumerator MuzzleFlash()
	{
		Fire thisFireLocal = this.gameObject.GetComponent<Fire> ();

		GameObject muzzleFlash = this.gameObject.transform.parent.gameObject.transform.FindChild("Arms/Gus_Arm/Holder/Gun/Firepoint/MuzzleFlash").gameObject;

		flashSize = Random.Range (0.5f, 0.9f);
		muzzleFlash.transform.localScale = new Vector2 (flashSize, flashSize);
		muzzleFlash.SetActive (true);

		yield return new WaitForSeconds (0.02f);

		 muzzleFlash.SetActive (false);

		yield return new WaitForSeconds (0.001f);

//		thisFireLocal.flashMuzzle = false;
	}
}
