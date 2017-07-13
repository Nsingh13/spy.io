using UnityEngine;
using System.Collections;

public class Magnet : MonoBehaviour {

	public GameObject targetObject;
	public Magnet thisMag;
	public bool collectedPickup = false;
	public static int spawnNum = 0;

	// Use this for initialization
	void Start () {		
		
		spawnNum++;
		this.gameObject.name = "Pickup " + spawnNum;

		thisMag = this.gameObject.GetComponent<Magnet> ();

	}
	
	// Update is called once per frame
	void Update () {

	
	}

	void OnTriggerEnter2D (Collider2D HitOBJ)
	{

		if (HitOBJ.gameObject.tag == "Player" && thisMag.collectedPickup == false && HitOBJ.gameObject.activeInHierarchy) 
		{
			thisMag.targetObject = HitOBJ.gameObject;
//			Debug.Log ("Magnet is Being Attracted to " + HitOBJ.gameObject.name);
			Attract ();
			thisMag.collectedPickup = true;
		}
	}

	public void Attract ()
	{
		if (thisMag.targetObject != null) 
		{
			iTween.ScaleTo (this.gameObject, Vector3.zero, 0.09f);
			iTween.MoveTo (this.gameObject, iTween.Hash ("x", thisMag.targetObject.transform.position.x, "y", thisMag.targetObject.transform.position.y, "time", 0.1f, "oncomplete", "openContents", "easetype", iTween.EaseType.linear));
		}
	}

	public void openContents()
	{

		GameLoop targetGL = thisMag.targetObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

		// Add Points
	//	targetGL.pv.RPC("AddPoints", PhotonTargets.AllBuffered, this.gameObject.name, wasRedPickup);
		targetGL.AddPoints(this.gameObject.name);
	}

}
