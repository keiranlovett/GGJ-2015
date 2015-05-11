using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace GatewayGames.ShooterAI
{
	
	public enum ShooterAIAudioOptions { Patrol, Investigate, Engage, Cover, Panic};
	
		
	public class GatewayGamesAudioControl : MonoBehaviour 
	{
		
		public bool soundEnabled = true; //whether sound is enabled or not
		
		[Range(0,1f)]
		public float basePitch = 1f; //the base pitch
		
		[Range(0, 0.9f)]
		public float randomPitch = 0.1f; //the random pitch +- relative value
		
		
		[ Range( 0, 1f) ]
		public float chanceForSound = 0.5f; //the chance for the audio to play
		
		public AudioClip[] patrolSounds; //the patrol sounds
		public AudioClip[] investigateSounds; //the investigate sounds
		public AudioClip[] engageSounds; //the engage sounds
		public AudioClip[] coverSounds; //the cover sounds
		public AudioClip[] panicSounds; //the panic sounds
		
		
		//cache
		private AudioSource c_Audio;
		private GatewayGamesBrain brain;
		private CurrentState previousState;
		private bool previousPanic;
		
		
		
		void Awake()
		{
				//iniate audio source and cache variables
			if(GetComponent<AudioSource>() == null)
			{
				c_Audio = gameObject.AddComponent<AudioSource>();
			}
			else
			{
				c_Audio = GetComponent<AudioSource>();
			}
			
			brain = GetComponent<GatewayGamesBrain>();
			previousState = brain.currentState;
			previousPanic = brain.inPanic;
			
			//set the pitch correctly
			c_Audio.pitch = basePitch;
			c_Audio.pitch = c_Audio.pitch + Random.Range( -randomPitch, randomPitch);
			
		}
		
		
		
			
		
		void Update()
		{
			//set the correct sounds
			CheckAndSetCorrectSounds();
		}
		
		
		
			
		/// <summary>
		/// Checks and sets the correct sounds.
		/// </summary>
		void CheckAndSetCorrectSounds()
		{
			
			//if we've changed the state in the last frame
			if(previousState != brain.currentState)
			{
				//set the correct sound
				switch(brain.currentState)
				{
				case CurrentState.Patrol: PlaySound(ShooterAIAudioOptions.Patrol); break;
				case CurrentState.Investigate: PlaySound( ShooterAIAudioOptions.Investigate); break;
				case CurrentState.Engage: PlaySound( ShooterAIAudioOptions.Engage); break;
					
				}	
			}
			
			//check if we've started panicking
			if( previousPanic != brain.inPanic && brain.inPanic == true)
			{
				PlaySound( ShooterAIAudioOptions.Panic);
			}
			
			
			//set the current state as the last state
			previousState = brain.currentState;
			}
		
		
		
		
		
			
		/// <summary>
		/// Plays a sound.
		/// </summary>
		/// <param name="typeOfSound">Type of sound.</param>
		public void PlaySound( ShooterAIAudioOptions typeOfSound)
		{
			//test if we're even allowed to play sounds
			if( soundEnabled == false)
			{
				return;
			}
			
			//check if the chances come together
			if( Random.Range(0f, 1f) > chanceForSound )
			{
				return;
			}
			
			//initiate the sound that we will play
			AudioClip soundToPlay = null;
			
			//find out which sound to play
			switch( typeOfSound)
			{
			case ShooterAIAudioOptions.Patrol: soundToPlay = patrolSounds[ (int)Random.Range(0, patrolSounds.Length) ]; break;
			case ShooterAIAudioOptions.Investigate: soundToPlay = investigateSounds[ (int)Random.Range(0, investigateSounds.Length) ]; break;
			case ShooterAIAudioOptions.Engage: soundToPlay = engageSounds[ (int)Random.Range(0, engageSounds.Length) ]; break;
			case ShooterAIAudioOptions.Cover: soundToPlay = coverSounds[ (int)Random.Range(0, coverSounds.Length) ]; break;
			case ShooterAIAudioOptions.Panic: soundToPlay = panicSounds[ (int)Random.Range(0, panicSounds.Length) ]; break;
				
			}
			
			
			//play the sound
			c_Audio.PlayOneShot( soundToPlay);
			
		}
		
		
		
	}
}