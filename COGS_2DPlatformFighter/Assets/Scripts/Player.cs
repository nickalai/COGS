using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public int playerNum; //playerNum starts at 1
    public bool isGrounded { get; set; } //Whether or not the player is grounded
    public bool facingRight { get; set; } //Whether or not the player is facing right
    public BoxCollider2D hitbox; //BoxCollider2D on the player character to be used as hitbox
    public bool isDodging { get; set; }//Whether or not the player is dodging
    public bool isAttacking { get; set; } //Keeping track of whether or not the player is attacking(To prevent multiple attacks at once)
    public int lastHit { get; set; } //int playerNum of last player to hit this one

    private Rigidbody2D rb;

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
        rb = GetComponent<Rigidbody2D>();
        //Physics2D.IgnoreLayerCollision(2, 2, true); //Ignore layer collision between Ignore Raycast Layers (What I currently have the players on)
    }

    public void PlayerStagger(Collider2D col) //PlayerStagger for all cases beyond player attacks
    {
        //TODO: Replace Set Damage with Attack Specific Damages
        Debug.Log("5 Damage Dealt by " + col.name);
        GameManager.Instance.PlayerDamage(playerNum, 5);
        //TODO: Add Knockback
        //TODO: Add Knockback Specific to each attack
        Debug.Log("HIT! by " + col.name + " = Totally sent flying");
        if (col.tag.Equals("Projectile")) {
            lastHit = col.GetComponent<ProjectileScript>().shooter.GetComponent<Player>().playerNum;
        }
        else if (col.transform.parent.gameObject.tag.Equals("Player")) //If a player isn't doing the damage, lastHit should not be updated
        {
            lastHit = col.transform.parent.gameObject.GetComponent<Player>().playerNum;
        }
    }

    public void PlayerStagger(PlayerAttack.Attack attack) //PlayerStagger for all player attacks
    {
        Debug.Log("5 Damage Dealt by " + attack.hitbox.name);
        GameManager.Instance.PlayerDamage(playerNum, attack.attackDamage);
        //TODO: Add Knockback
        rb.AddForceAtPosition(attack.knockbackDirection * attack.knockbackAmount, attack.hitbox.transform.position);
        //TODO: Add Knockback Specific to each attack
        Debug.Log("HIT! by " + attack.hitbox.name + " = Totally sent flying");
        lastHit = attack.hitbox.transform.parent.gameObject.GetComponent<Player>().playerNum;
    }

    public void PlayerThrown(PlayerThrow throwType)
    {
        Debug.Log(this.transform.name + " " + throwType + "N!");
    }
}
