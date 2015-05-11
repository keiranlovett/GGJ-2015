using UnityEngine;
using System.Collections;

public class ShooterAIFPSGUI : MonoBehaviour {
	
	public Texture2D crosshair; //the cross hair texture
	
	private float healthFactor = 1f; //the health factor to multiply so that it always looks like 100 at the start
	
	void Awake()
	{
		//set the vars
		healthFactor = 100f/GetComponent<ShooterAIFPSHealth>().health;
	}
	
	
	
	
	void OnGUI()
	{
		float sw = Screen.width/2f;
		float sh = Screen.height/2f;
		
		GUI.DrawTexture( new Rect( sw - 25f, sh - 25f, 50f, 50f), crosshair);
		GUI.Label( new Rect( 10, 10, 100, 100), "Health: " + (int)(GetComponent<ShooterAIFPSHealth>().health * healthFactor) );
	}
	
}
