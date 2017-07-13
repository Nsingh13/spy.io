using UnityEngine;
using System.Collections;

public class Trigger : Photon.MonoBehaviour {

	public SVGImporter.SVGRenderer thisSvg;

	public Color transparent;
	public Color opaque;


	// Use this for initialization
	void Start () {
	}


	public void OnTriggerEnter2D(Collider2D other)
	{
		
		if (other.gameObject.tag == "Player" ) {

			GameLoop myGL = other.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

			if (myGL.pv.isMine == true) {

				// Make building transparent
				thisSvg = this.gameObject.GetComponent<SVGImporter.SVGRenderer> ();

				thisSvg.color = transparent;
			}

			// If not me
			if (myGL.pv.isMine == false) {

			}

		}
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player") {
			
			GameLoop myGL = other.gameObject.transform.parent.gameObject.transform.parent.gameObject.GetComponent<GameLoop> ();

			if (myGL.pv.isMine == true) {

				// Make building visible
				thisSvg.color = opaque;
			}

			// If not me 
			if (myGL.pv.isMine == false) {

			}

		}
	}

}
