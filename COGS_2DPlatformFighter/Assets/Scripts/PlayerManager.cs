using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public bool isGrounded { get; set; } //Whether or not the player is grounded
    public bool facingRight { get; set; } //Whether or not the player is facing right
    public BoxCollider2D hitbox; //BoxCollider2D on the player character to be used as hitbox
    public bool isDodging { get; set; }//Whether or not the player is dodging
    public bool isAttacking { get; set; } //Keeping track of whether or not the player is attacking(To prevent multiple attacks at once)

    public enum PlayerState
    {
        IDLE,
        GROUNDED,
        DODGING,
        AERIAL,
        SLIDING, //SLIDING = Air Slide = Air Dodge
        ATTACKING,
        GRABBING,
        GRABBED,
        STAGGERED
    }

    public enum PlayerThrow
    {
        FORWARD_THROW,
        BACK_THROW,
        DOWN_THROW,
        UP_THROW
    }

    PlayerStateStack pss; //Stack of  PlayerStates to keep track of player state


    // Use this for initialization
    void Start () {
        facingRight = true;
        pss = GetComponent<PlayerStateStack>();
        //Physics2D.IgnoreLayerCollision(2, 2, true); //Ignore layer collision between Ignore Raycast Layers (What I currently have the players on)
    }

    // Update is called once per frame
    void Update () {
        
	}

    public void PlayerStagger(Collider2D col)
    {
        Debug.Log("HIT! by " + col.name + " = Totally sent flying");
    }

    public void PlayerThrown(PlayerThrow throwType)
    {
        Debug.Log(this.transform.name + " " + throwType + "N!");
    }
}
