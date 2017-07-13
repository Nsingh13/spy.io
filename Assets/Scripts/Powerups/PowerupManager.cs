using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerupManager : Photon.MonoBehaviour {

	public List<Transform> PowerupSpawnLocations = new List<Transform> ();

	public int numOfPowerupTypes = 3;

	public int pickupsToInstantiate = 3;

	public PhotonView pv;

	// Use this for initialization
	void Start () {	

	}

	// Update is called once per frame
	void Update () {

	}

	// Called from Network Manager by MC
	public void InstantiatePowerups()
	{
		for (int i = 0; i < pickupsToInstantiate; i++) 
		{
			int randomLocation = Random.Range (0, PowerupSpawnLocations.Count);
			PhotonNetwork.InstantiateSceneObject ("Powerup", PowerupSpawnLocations [randomLocation].position, this.gameObject.transform.rotation, 0, null);
		}
	}

	[PunRPC]
	public IEnumerator CollectPowerup(string targetPowerupName, int randomNum)
	{
		

		
		GameObject targetPowerup = GameObject.Find (targetPowerupName);

		if (targetPowerup != null) 
		{
			targetPowerup.SetActive (false);

			targetPowerup.transform.position = PowerupSpawnLocations [randomNum].position;

			yield return new WaitForSeconds (2f);

			targetPowerup.SetActive (true);
		}
			

	}

    

}

