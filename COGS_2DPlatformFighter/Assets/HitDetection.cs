using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetection : MonoBehaviour {

    PlayerManager pm;
    PlayerStateStack pss;

	// Use this for initialization
	void Start () {
        pm = transform.parent.GetComponent<PlayerManager>();
        pss = transform.parent.GetComponent<PlayerStateStack>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if(pss.Peek() != PlayerManager.PlayerState.GRABBED && IsInvoking("EscapeGrab"))
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

        if(col.tag == "Grab")
        {
            Debug.Log("Player Grabbed!");
            this.transform.parent.parent = col.transform.parent;
            col.transform.parent.GetComponent<PlayerStateStack>().Push(PlayerManager.PlayerState.GRABBING); //Add Grabbing State to grabber
            pss.Push(PlayerManager.PlayerState.GRABBED);
            Invoke("EscapeGrab", 5); //HOW DO CANCEL INVOKE?
            return;
        }

        pm.PlayerStagger(col);
    }

    void EscapeGrab()
    {
        //Players only escape the grab if they're still grabbed
        if(pss.Peek() == PlayerManager.PlayerState.GRABBED)
        {
            this.transform.parent.parent.GetComponent<PlayerStateStack>().Pop(); //Remove Grabbing state from grabber
            this.transform.parent.parent = null;
            Debug.Log("Player Escaped!");
            pss.Pop();
        }
    }
}
