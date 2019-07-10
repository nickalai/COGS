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
        if (col.tag == "Attack")
        {
            /*
             *     public Attack Bair;
                    public Attack Fair;
                    public Attack Uair;
                    public Attack Dair;
                    public Attack Nair;

                    public Attack Ftilt;
                    public Attack Utilt;
                    public Attack Dtilt;
                    public Attack Jab;
             * */
            PlayerAttack attacker = col.transform.parent.GetComponent<PlayerAttack>();
            PlayerAttack.Attack attack;
            if (col.name.Equals("Bair"))
            {
                attack = attacker.Bair;
            }
            else if (col.name.Equals("Fair"))
            {
                attack = attacker.Fair;
            }
            else if(col.name.Equals("Uair"))
            {
                attack = attacker.Uair;
            }
            else if (col.name.Equals("Dair"))
            {
                attack = attacker.Dair;
            }
            else if(col.name.Equals("Nair"))
            {
                attack = attacker.Nair;
            }
            else if (col.name.Equals("Ftilt"))
            {
                attack = attacker.Ftilt;
            }
            else if(col.name.Equals("Utilt"))
            {
                attack = attacker.Utilt;
            }
            else if (col.name.Equals("Dtilt"))
            {
                attack = attacker.Dtilt;
            }
            else if(col.name.Equals("Jab"))
            {
                attack = attacker.Jab;
            }
            else
            {
                Debug.Log("UNKNOWN ATTACK: " + col.name);
                Debug.Log("Defaulting to attacker's jab");
                attack = attacker.Jab;
            }
            //If attacking player is flipped, flip attack knockback direction as well
            if(!col.transform.parent.GetComponent<Player>().facingRight)
            {
                attack.knockbackDirection = new Vector2(attack.knockbackDirection.x * -1, attack.knockbackDirection.y);
            }

            p.PlayerStagger(attack);
        }

        if (col.tag == "Projectile")
        {
            p.PlayerStagger(col);
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
