using UnityEngine;
using System.Collections;

public class GoogleScrollEffects : MonoBehaviour {

	public GameObject scrollRect;
	public GameObject title;
	public GameObject triangleLeft;
	public GameObject triangleRight;
	public GameObject dots;

	public RectTransform content; // Content box
	public RectTransform center;
	public GameObject[] windows;
	public GameObject[] googleDots;

	private float[] distance; // All windows distance to center
	private bool dragging = false; // Only snap when dragging is false
	private int windowDistance; //hold distance between the windows
	private int minWindowNum; // To hold the number of the window with lowest distance to center

	// Dots
	private Vector2 minDotSize;
	private Vector2 maxDotSize;

	// Window Sizes
	private Vector2 minWindowSize;
	private Vector2 maxWindowSize;

	//Begin and End Touches
	private float beginTouchX; 
	private float endTouchX;

	public RectTransform tutorialWindow;

	public GameManager GM;

	void Start()
	{
		GM = GameObject.Find("GameController").GetComponent<GameManager>();

		int windowslength = windows.Length;
		distance = new float[windowslength];

		// Get distance between windows
		windowDistance = (int)Mathf.Abs (windows [1].GetComponent<RectTransform> ().anchoredPosition.x
		- windows [0].GetComponent<RectTransform> ().anchoredPosition.x);

		// Set min/max size of dots
		minDotSize = new Vector2(0.1f, 0.1f);
		maxDotSize = new Vector2(0.14f, 0.14f);

		// Set min/max size of windows
		minWindowSize = new Vector2(2.16f, 2.3f);
		maxWindowSize = new Vector2(3.43f, 3f);
	}

	void Update()
	{
		// ************** EFFECT 1: SNAPPING *******************

		// Go through all buttons to check which is closer to center
		for (int i = 0; i < windows.Length; i++) {
			distance [i] = Mathf.Abs (center.transform.position.x - windows [i].transform.position.x);
		}

		float minDistance = Mathf.Min (distance); // Get min distance

		// Get number of button with the min distance
		for (int a = 0; a < windows.Length; a++) {

			if (minDistance == distance [a]) {
				minWindowNum = a;
			}

		}
			
		// Change CenterToCompare Position to Improve Scrolling
		if (Input.touchCount >= 1) {
			
			Touch t = Input.GetTouch (0);

			if (t.phase == TouchPhase.Began) {
				beginTouchX = t.position.x;
			}

			if (t.phase == TouchPhase.Ended) {
				 endTouchX = t.position.x;

				if (endTouchX - beginTouchX > 0) {
					//swiped right
					//CTC = left side
					center.anchoredPosition = new Vector2(-110f, -24f);
				}

				if (endTouchX - beginTouchX < 0) {
					//swiped left
					//CTC = right side
					center.anchoredPosition = new Vector2(110f, -24f);
				}
			}

		}

		if (!dragging) {
			//lerp panel to center of that window
			LerpToWindow (minWindowNum * -windowDistance);
			//Update the google dots
			UpdateDots (minWindowNum);
			// Update size of windows
			UpdateWindowSize (minWindowNum);
		}

	}

	void LerpToWindow(int position)
	{
		float newX = Mathf.Lerp (content.anchoredPosition.x, position, Time.deltaTime * 10f);
		Vector2 newPosition = new Vector2 (newX, content.anchoredPosition.y);

		content.anchoredPosition = newPosition;

		// Load Game?
		if (minWindowNum == 3) 
		{

			if (GM.firstPlay == false) {
				GM.moon.SetActive (false);
				GM.menu.SetActive (true);
				content.anchoredPosition = new Vector2 (0, content.anchoredPosition.y);
				GM.tutorial.SetActive (false);
			} 

			else if (GM.firstPlay == true)
			{
				content.anchoredPosition = new Vector2 (0, content.anchoredPosition.y);
				GM.loadGame ();
			}
		}
	}

	// ************** EFFECT 2: DOTS *******************

	void UpdateDots(int dotToUpdate)
	{
		for (int d = 0; d < googleDots.Length; d++) {

			// Dot should not be big --> make smaller
			if (d != dotToUpdate)
			{
				RectTransform bigDot = googleDots[d].GetComponent<RectTransform> ();

				float newX = Mathf.Lerp (bigDot.localScale.x, minDotSize.x, Time.deltaTime * 5f);
				float newY = Mathf.Lerp (bigDot.localScale.y, minDotSize.y, Time.deltaTime * 5f);

				Vector2 newSize = new Vector2 (newX, newY);

				bigDot.localScale = newSize;

			}

			// Dot should be big --> make bigger
			if (d == dotToUpdate) {
				
				RectTransform smallDot = googleDots[d].GetComponent<RectTransform> ();

				float newX = Mathf.Lerp (smallDot.localScale.x, maxDotSize.x, Time.deltaTime * 5f);
				float newY = Mathf.Lerp (smallDot.localScale.y, maxDotSize.y, Time.deltaTime * 5f);

				Vector2 newSize = new Vector2 (newX, newY);

				smallDot.localScale = newSize;

			} 
				
		}
	}

	// ************** EFFECT 3: Window Size *******************

	void UpdateWindowSize(int biggerWindow)
	{
		for (int w = 0; w < windows.Length; w++) {

			// if is window closest to center
			if (w == biggerWindow) {
				
				RectTransform smallWindow = windows [w].GetComponent<RectTransform> ();

				float newX = Mathf.Lerp (smallWindow.localScale.x, maxWindowSize.x, Time.deltaTime * 7f);
				float newY = Mathf.Lerp (smallWindow.localScale.y, maxWindowSize.y, Time.deltaTime * 7f);

				Vector2 newSize = new Vector2 (newX, newY);

				smallWindow.localScale = newSize;

			}

			// if are windows farther away
			if (w != biggerWindow) {

				RectTransform bigWindow = windows [w].GetComponent<RectTransform> ();

				float newX = Mathf.Lerp (bigWindow.localScale.x, minWindowSize.x, Time.deltaTime * 7f);
				float newY = Mathf.Lerp (bigWindow.localScale.y, minWindowSize.y, Time.deltaTime * 7f);

				Vector2 newSize = new Vector2 (newX, newY);

				bigWindow.localScale = newSize;

			}

		}
			
	}



	public void StartDrag()
	{
		dragging = true;
	}

	public void EndDrag()
	{
		dragging = false;
	}

}
