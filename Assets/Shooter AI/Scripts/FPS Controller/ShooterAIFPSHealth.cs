using UnityEngine;
using System.Collections;

public class ShooterAIFPSHealth : MonoBehaviour {
	
	
	public float health = 100f; //the amount of health
	
	
	private bool showDeathMessage = false; //whether to show the death message
	
	
	
	public void Damage()
	{
		//deduct health
		health -= 0.5f;
		
		//check if we're dead
		if(health <= 0f)
		{
			Die ();
		}
	}
	
	public void Damage(float amount)
	{
		//deduct health
		health -= amount;
		
		//check if we're dead
		if(health <= 0f)
		{
			Die ();
		}
	}

	
	
	
	/// <summary>
	/// Starts the death sequence
	/// </summary>
	public void Die()
	{
		showDeathMessage = true;
		Time.timeScale = 0f;
	}
	
	
	void OnGUI()
	{
	
		if(showDeathMessage == true)
		{
			// Create the texture and set its colour.
			Texture2D blackTexture = new Texture2D(1,1);
			blackTexture.SetPixel(0,0,Color.black);
			blackTexture.Apply();
			
			// Use the texture.
			GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), blackTexture);
			
			//show death message
			GUI.Label( new Rect( Screen.width/2, Screen.height/2, 100f, 50f), "You died! Press R to restart");
			
			if( Input.GetKeyDown( KeyCode.R ))
			{
				Time.timeScale = 1f;
				Application.LoadLevel (Application.loadedLevel);
			}
			
		}
	
	}
	
	
}
