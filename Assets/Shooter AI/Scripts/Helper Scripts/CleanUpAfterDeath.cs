using UnityEngine;
using System.Collections;



namespace GatewayGames.ShooterAI
{
	[AddComponentMenu("Shooter AI/Clean Up After Death") ]

public class CleanUpAfterDeath : MonoBehaviour
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

        Destroy(gameObject);
    }

}

}