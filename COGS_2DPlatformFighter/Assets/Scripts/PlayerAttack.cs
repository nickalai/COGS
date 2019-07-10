using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {
    
    [System.Serializable] public struct Attack
    {
        public GameObject hitbox; //For melee: where attacks will hit.  For projectile: where projectiles will spawn
        public float attackTime;  //How long each attack takes(aka how long until you can attack again)
        public int attackDamage;
        public float knockbackAmount;
        public Vector2 knockbackDirection;
    }

    public Attack Bair;
    public Attack Fair;
    public Attack Uair;
    public Attack Dair;
    public Attack Nair;

    public Attack Ftilt;
    public Attack Utilt;
    public Attack Dtilt;
    public Attack Jab;

    [SerializeField] private Attack FSpecial;
    [SerializeField] private Attack USpecial;
    [SerializeField] private Attack DSpecial;
    [SerializeField] private Attack FASpecial;
    [SerializeField] private Attack UASpecial;
    [SerializeField] private Attack DASpecial;

    [SerializeField] private GameObject Grab;

    [SerializeField] private GameObject projectile;

    private Player pm;
    private PlayerStateStack pss;
    private PlayerMovement pmove;
    private BoxCollider2D hitbox;

    private int maxColliders = 4; //Can only hit 4 players (including yourself)

    [SerializeField] private float projectileSpeed = 10f;

    private float grabTime = 0.2f;

    private Vector2 FSpecialDirection = new Vector2(1, 0);
    private Vector2 USpecialDirection = new Vector2(0, 1);
    private Vector2 DSpecialDirection = new Vector2(0, -1);
    private Vector2 FASpecialDirection = new Vector2(1,-1);
    private Vector2 UASpecialDirection = new Vector2(0, 1);
    private Vector2 DASpecialDirection = new Vector2(0, -1);


    // Use this for initialization
    void Start() {
        pm = GetComponent<Player>();
        pss = GetComponent<PlayerStateStack>();
        pmove = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update() {
        if(pss.Peek() == Player.PlayerState.GRABBING)
        {
            Transform[] transformList = this.GetComponentsInChildren<Transform>();
            GameObject grabbedPlayer = null;
            foreach(Transform t in transformList)
            {
                if(t.gameObject.tag == "Player")
                {
                    grabbedPlayer = t.gameObject;
                }
            }

            if(grabbedPlayer != null)
            {
                PlayerStateStack grabbedPlayerPss = grabbedPlayer.GetComponent<PlayerStateStack>();

                if (Input.GetButtonDown("Vertical"))
                {
                    if (Input.GetAxisRaw("Vertical") > 0)
                    {
                        Debug.Log("Up Throw");
                        pss.Pop(); //Popping Grabbing
                        grabbedPlayerPss.Pop(); //Popping Grabbed
                        grabbedPlayer.GetComponent<Player>().PlayerThrown(Player.PlayerThrow.UP_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                    else if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        Debug.Log("Down Throw");
                        pss.Pop(); //Popping Grabbing
                        grabbedPlayerPss.Pop(); //Popping Grabbed
                        grabbedPlayer.GetComponent<Player>().PlayerThrown(Player.PlayerThrow.DOWN_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                }
                if (Input.GetButtonDown("Horizontal"))
                {
                    if (Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight || Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight)
                    {
                        Debug.Log("Forward Throw");
                        pss.Pop(); //Popping Grabbing
                        grabbedPlayerPss.Pop(); //Popping Grabbed
                        grabbedPlayer.GetComponent<Player>().PlayerThrown(Player.PlayerThrow.FORWARD_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                    else if (Input.GetAxisRaw("Horizontal") < 0 && pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !pm.facingRight)
                    {
                        Debug.Log("Back Throw");
                        pss.Pop(); //Popping Grabbing
                        grabbedPlayerPss.Pop(); //Popping Grabbed
                        grabbedPlayer.GetComponent<Player>().PlayerThrown(Player.PlayerThrow.BACK_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                }

            }
            
        }

        if (Input.GetButtonDown("Attack")) //As long as they aren't in an animation, pressing attack will launch an attack
        {
            if (pss.Peek() == Player.PlayerState.GROUNDED || pss.Peek() == Player.PlayerState.IDLE)
            {
                GroundedAttack();
            }

            else if (pss.Peek() == Player.PlayerState.AERIAL)
            {
                AerialAttack();
            }
        }
        //You can't make multiple inputs at once, so else-if suite is optimal
        else if (Input.GetButtonDown("Special") && (pss.Peek() == Player.PlayerState.GROUNDED || pss.Peek() == Player.PlayerState.IDLE || pss.Peek() == Player.PlayerState.AERIAL)) //As long as they aren't in an animation, pressing attack will launch an attack
        {
            SpecialAttack();
        }

        else if (Input.GetButtonDown("Grab") && (pss.Peek() == Player.PlayerState.GROUNDED || pss.Peek() == Player.PlayerState.IDLE))
        {
            StartCoroutine(GrabPlayer());            
        }

        
    }


    /*~~~~~~~~~~~~~~ ALL THINGS BELOW NEED TO BE WORKED ON ~~~~~~~~~~~~~~~~~~~~*/
    //Use Start() and Update() as needed
    //For attacks that are reliant on movement, make the necessary calls to PlayerMovement

    //Function for managing all attacks, should make calls to the attack functions below

    //Nic's Edit: All attack functions call this as a coroutine with hitboxes and delay as parameters
    IEnumerator AttackCalled(Attack attack)
    {
        pss.Push(Player.PlayerState.ATTACKING);
        CircleCollider2D hitbox = attack.hitbox.GetComponent<CircleCollider2D>();
        hitbox.enabled = true;
        yield return new WaitForSeconds(attack.attackTime);
        hitbox.enabled = false;
        pss.Pop(); //ERROR HANDLING: WHAT IF THEY'RE STAGGERED?  THIS WOULD NO LONGER POP THE ATTACK, BUT IT WOULD POP THE STAGGER, Unless Stagger is implemented to always be position 1 (and not 0).  That way, stagger would overrule attacks
    }

    //Function for ground attacks
    void GroundedAttack()
    {
        //Btilt isn't a thing, can only be executed by turning around and ftilting.
        if (Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight) //Ftilt
        {
            StartCoroutine(AttackCalled(Ftilt));
        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Utilt
        {
            StartCoroutine(AttackCalled(Utilt));
        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Dtilt
        {
            StartCoroutine(AttackCalled(Dtilt));
        }
        else //Jab if attack is called and no directional attack is selected
        {
            StartCoroutine(AttackCalled(Jab));
        }

    }

    //Function for aerial attack
    void AerialAttack()
    {
        if (Input.GetAxisRaw("Horizontal") < 0 && pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !pm.facingRight) //Backair
        {
            StartCoroutine(AttackCalled(Bair));
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight) //Fair
        {
            StartCoroutine(AttackCalled(Fair));
        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Up air
        {
            StartCoroutine(AttackCalled(Uair));
        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Dair
        {
            StartCoroutine(AttackCalled(Dair));
        }
        else //Nair if attack is called and no directional attack is selected
        {
            StartCoroutine(AttackCalled(Nair));
        }

    }

    //Function for special attack
    void SpecialAttack()
    {
        //Need backwards side special for when you're moving in the air and using a side special (It needs to flip you)
        if (Input.GetAxisRaw("Horizontal") < 0 && pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !pm.facingRight) //Side Special (Backwards)
        {
            pmove.PlayerFlip(); //Need to flip them if they're facing backwards
            //Aerial Special
            StartCoroutine(SpawnProjectile(FASpecial, FASpecialDirection)); //Flipped FASpecial direction
            //Can't possibly be grounded
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight) //Side Special (Forwards)
        {

            if (pss.Peek() == Player.PlayerState.AERIAL)
            {
                //Aerial Special
                StartCoroutine(SpawnProjectile(FASpecial, FASpecialDirection));
            }
            else
            {
                //Grounded Special
                StartCoroutine(SpawnProjectile(FSpecial, FSpecialDirection));
            }

        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Up Special
        {

            if (pss.Peek() == Player.PlayerState.AERIAL)
            {
                //Aerial Special
                StartCoroutine(SpawnProjectile(UASpecial, UASpecialDirection));
            }
            else
            {
                //Grounded Special
                StartCoroutine(SpawnProjectile(USpecial, USpecialDirection));
            }

        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Down Special
        {

            if (pss.Peek() == Player.PlayerState.AERIAL)
            {
                //Aerial Special
                StartCoroutine(SpawnProjectile(DASpecial, DASpecialDirection));
            }
            else
            {
                //Grounded Special
                StartCoroutine(SpawnProjectile(DSpecial, DSpecialDirection));
            }

        }
        else //Neutral Special if attack is called and no directional attack is selected
        {
            Debug.Log("Neutral Special");

            if (pss.Peek() == Player.PlayerState.AERIAL)
            {
                //Aerial Special
            }
            else
            {
                //Grounded Special
            }

        }

    }

    //Function for grapping opponent
    IEnumerator GrabPlayer()
    {
        //Push and Pop of Grabbing are handled in HitDetection
        CircleCollider2D grabBox = Grab.GetComponent<CircleCollider2D>();
        grabBox.enabled = true;
        yield return new WaitForSeconds(grabTime);
        grabBox.enabled = false;
    }

    //Function to check if player is grounded
    void IsGrounded()
    {

    }

    //Spawns a Projectile at the given Attacks location
    IEnumerator SpawnProjectile(Attack specialAttack, Vector2 direction)
    {
        pss.Push(Player.PlayerState.ATTACKING);

        GameObject projectileClone = Instantiate(projectile);
        projectileClone.GetComponent<ProjectileScript>().shooter = this.gameObject;
        projectileClone.transform.position = specialAttack.hitbox.transform.position;

        Rigidbody2D rb = projectileClone.GetComponent<Rigidbody2D>();

        if (pm.facingRight) //If they're not facing right, everything is flipped!
        {
            rb.velocity = direction * projectileSpeed;
        }
        else
        {
            rb.velocity = new Vector2(direction.x * -1, direction.y) * projectileSpeed;
        }

        yield return new WaitForSeconds(specialAttack.attackTime);

        pss.Pop(); //ERROR HANDLING: WHAT IF THEY'RE STAGGERED?  THIS WOULD NO LONGER POP THE ATTACK, BUT IT WOULD POP THE STAGGER, Unless Stagger is implemented to always be position 1 (and not 0).  That way, stagger would overrule attacks.  But then the coroutine would have to be interrupted to prevent a pre-emptive pop


    }
}
