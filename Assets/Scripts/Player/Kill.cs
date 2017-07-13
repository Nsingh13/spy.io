using UnityEngine;
using System.Collections;

public class Kill : Photon.MonoBehaviour
{



	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}


	public void KillPlayer (Collider2D hitOBJ)
	{
			
		if (hitOBJ.gameObject.tag == "Player") {

			GameLoop hitGL = hitOBJ.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

			// If shot player is NOT Immortal
			if (hitGL.isImmortal == false) {

				GameLoop myGL = this.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

			//	Debug.Log ("MY TEAM: " + myGL.pv.owner.customProperties ["t"]);
			//	Debug.Log ("PERSON HIT TEAM: " + hitGL.pv.owner.customProperties ["t"]);

				if ((int)myGL.pv.owner.customProperties["t"] == 2) {
					if ((int)hitGL.pv.owner.customProperties["t"] == 2) {
					
						//Kill Self
						//		Debug.Log ("You Killed a Fellow Spy!");
						if (myGL.isImmortal == false) 
						{
							myGL.pv.RPC ("DeployPickup", PhotonTargets.All, myGL.pv.viewID, myGL.pv.viewID, false, false, myGL.pv.owner.customProperties ["s"], "You Cannot Kill Other SPIES!");
						}
				

					}
					else if ((int)hitGL.pv.owner.customProperties["t"] == 1) {
					
						//Kill and Become Murderer

						//DIED AS SPY IS TRUE EVEN THOUGH BECOMES MURDERER
						if(myGL.pv.isMine)
						myGL.StartCoroutine("murdererPopup", 2); //SPY NOW MURDERER //MURDERER NOT YET SPY

						hitGL.pv.RPC ("DeployPickup", PhotonTargets.All, hitGL.pv.viewID, myGL.pv.viewID, true, false, hitGL.pv.owner.customProperties["s"], "Died as a Murderer? Respawning as a Spy...");

					}
				}

				else if ((int)myGL.pv.owner.customProperties["t"] == 1) {

					// Kill Whoever Hit
					//	Debug.Log ("Good! You Killed a Spy!");
					if ((int)hitGL.pv.owner.customProperties ["t"] == 2) 
					{
						hitGL.pv.RPC ("DeployPickup", PhotonTargets.All, hitGL.pv.viewID, myGL.pv.viewID, false, true, hitGL.pv.owner.customProperties ["s"], "The MURDERER has Killed You!");
					}

				}

			}

		}
	}


}
