    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageHazard : MonoBehaviour
{
    //public ThingToReference thing;
    public PlayerAttack.Attack hazard;

	void Start ()
    {

	}
	
	void Update ()
    {
		
	}

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            if (collider.GetComponent<Player>().playerDamage >= 125)
            {
                GameManager.Instance.PlayerKill(collider.GetComponent<Player>().playerNum);
            }
            else
            {
                collider.GetComponent<Player>().PlayerStagger(hazard);
            }
        }
    }
}
