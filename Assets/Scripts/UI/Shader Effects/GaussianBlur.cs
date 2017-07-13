using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GaussianBlur : MonoBehaviour
{
	public Material BlurMaterial;
	[Range(0, 10)]
	public int Iterations;
	[Range(0, 4)]
	public int DownRes;

	// Message Data
	public GameObject blurMessageGO;
	public Text blurMessageText;
	public Color opaque;
	public Color transparent;
	public Color lerpedColor;

	public bool blurActive = false;

	void Awake()
	{
		blurMessageGO = GameObject.FindGameObjectWithTag ("blurMessage");
		blurMessageText = blurMessageGO.GetComponent<Text> ();
	}

	void Update()
	{
		blurMessageText.color = Color.Lerp (blurMessageText.color, lerpedColor, 6f * Time.deltaTime);
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		int width = src.width >> DownRes;
		int height = src.height >> DownRes;

		RenderTexture rt = RenderTexture.GetTemporary(width, height);
		Graphics.Blit(src, rt);

		for (int i = 0; i < Iterations; i++)
		{
			RenderTexture rt2 = RenderTexture.GetTemporary(width, height);
			Graphics.Blit(rt, rt2, BlurMaterial);
			RenderTexture.ReleaseTemporary(rt);
			rt = rt2;
		}

		Graphics.Blit(rt, dst);
		RenderTexture.ReleaseTemporary(rt);
	}

	// Lerp the blur to full and choose / bring up message if not already up
	public void BlurMessage(int setIterations, int setDownRes, string messageText)
	{
		// BLUR
		blurActive = true;

		while (this.DownRes != setDownRes) 
		{
			this.DownRes++;
		}

		while (this.Iterations != setIterations) 
		{
			this.Iterations++;
		}

		// SHOW MESSAGE
		blurMessageText.text = messageText;

		lerpedColor = opaque;

		return;
	}

	// Just update the message bruh.. nothin else
	public void UpdateMessage(string messageText)
	{
		if(blurMessageText != null)
		blurMessageText.text = messageText;
	}

	// Lerp to clear the blur and remove message if not removed
	public void ClearMessage()
	{
		blurActive = false;

		// HIDE MESSAGE
		blurMessageText.text = "";

		lerpedColor = transparent;

		// UNBLUR
		while (this.DownRes != 0) 
		{
			this.DownRes--;
		}

		while (this.Iterations != 0) 
		{
			this.Iterations--;
		}

	}

	public void Blur(int setIterations, int setDownRes)
	{
		blurActive = true;
		// BLUR
		while (this.DownRes != setDownRes) 
		{
			this.DownRes++;
		}

		while (this.Iterations != setIterations) 
		{
			this.Iterations++;
		}


	}

	public void UnBlur()
	{
		blurActive = false;

		// UNBLUR
		while (this.DownRes != 0) 
		{
			this.DownRes--;
		}

		while (this.Iterations != 0) 
		{
			this.Iterations--;
		}

	}


}