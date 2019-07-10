using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetection : MonoBehaviour {

    Player p;
    PlayerStateStack pss;

	// Use this for initialization
	void Start () {
        p = transform.parent.GetComponent<Player>();
        pss = transform.parent.GetComponent<PlayerStateStack>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if(pss.Peek() != Player.PlayerState.GRABBED && IsInvoking("EscapeGrab"))
        {
            CancelInvoke(); //Cancel EscapeGrab if the player isn't grabbed.
        }

	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Player")
        {
            return;
        }

        if(col.tag == "Platform")
        {
            return;
        }

        if (col.tag == "Hitbox")
        {
            return;
        }

        if (col.tag.Equals("BlastZone")) //Blast Zone can't stagger the player
        {
            return;
        }

        if (col.tag == "Grab")
        {
            Debug.Log("Player Grabbed!");
            this.transform.parent.parent = col.transform.parent;
            col.transform.parent.GetComponent<PlayerStateStack>().Push(Player.PlayerState.GRABBING); //Add Grabbing State to grabber
            pss.Push(Player.PlayerState.GRABBED);
            Invoke("EscapeGrab", 5); //HOW DO CANCEL INVOKE?
            return;
        }

        p.PlayerStagger(col);
    }

    void EscapeGrab()
    {
        //Players only escape the grab if they're still grabbed
        if(pss.Peek() == Player.PlayerState.GRABBED)
        {
            this.transform.parent.parent.GetComponent<PlayerStateStack>().Pop(); //Remove Grabbing state from grabber
            this.transform.parent.parent = null;
            Debug.Log("Player Escaped!");
            pss.Pop();
        }
    }
}
