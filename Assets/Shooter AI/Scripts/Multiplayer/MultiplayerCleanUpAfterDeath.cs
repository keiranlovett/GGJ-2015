using UnityEngine;
using System.Collections;



namespace GatewayGames.ShooterAI
{
	[AddComponentMenu("Shooter AI/Multiplayer Clean Up After Death") ]

public class MultiplayerCleanUpAfterDeath : MonoBehaviour
{
    private bool isDead = false;
    public float respawnTime = 10.0f;


    void Update() {

        if (isDead)
        {
            respawnTime -= Time.deltaTime;
        
            if (respawnTime <= 0.0f)
            {
                RemoveBody();
            }
        }
    }
    
	public void AIDead()
    {
        isDead = true;
    }

    void RemoveBody() {

        PhotonNetwork.Destroy(gameObject);
    }

}

}