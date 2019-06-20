using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour {

    [SerializeField] private GameObject Bair;
    [SerializeField] private GameObject Fair;
    [SerializeField] private GameObject Uair;
    [SerializeField] private GameObject Dair;
    [SerializeField] private GameObject Nair;

    [SerializeField] private GameObject Ftilt;
    [SerializeField] private GameObject Utilt;
    [SerializeField] private GameObject Dtilt;
    [SerializeField] private GameObject Jab;

    [SerializeField] private GameObject FSpecial;
    [SerializeField] private GameObject USpecial;
    [SerializeField] private GameObject DSpecial;
    [SerializeField] private GameObject FASpecial;
    [SerializeField] private GameObject UASpecial;
    [SerializeField] private GameObject DASpecial;

    [SerializeField] private GameObject Grab;

    [SerializeField] private GameObject projectile;

    private PlayerManager pm;
    private PlayerStateStack pss;
    private PlayerMovement pmove;
    private BoxCollider2D hitbox;

    private int maxColliders = 4; //Can only hit 4 players (including yourself)

    //How long each attack takes(aka how long until you can attack again)
    private float backAirAttackTime = 0.2f;
    private float forwardAirAttackTime = 0.2f;
    private float upAirAttackTime = 0.2f;
    private float downAirAttacktime = 0.2f;
    private float neutralAirAttackTime = 0.2f;

    private float forwardTiltAttackTime = 0.2f;
    private float upTiltAttackTime = 0.2f;
    private float downTiltAttackTime = 0.2f;
    private float jabAttackTime = 0.2f;

    private float forwardSpecialAttackTime = 0.4f;
    private float upSpecialAttackTime = 0.4f;
    private float downSpecialAttackTime = 0.4f;
    private float neutralSpecialAttackTime = 0.4f;

    private float forwardAirSpecialAttackTime = 0.4f;
    private float upAirSpecialAttackTime = 0.4f;
    private float downAirSpecialAttackTime = 0.4f;
    private float neutralAirSpecialAttackTime = 0.4f;

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
        pm = GetComponent<PlayerManager>();
        pss = GetComponent<PlayerStateStack>();
        pmove = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update() {
        if(pss.Peek() == PlayerManager.PlayerState.GRABBING)
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
                        grabbedPlayer.GetComponent<PlayerManager>().PlayerThrown(PlayerManager.PlayerThrow.UP_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                    else if (Input.GetAxisRaw("Vertical") < 0)
                    {
                        Debug.Log("Down Throw");
                        pss.Pop(); //Popping Grabbing
                        grabbedPlayerPss.Pop(); //Popping Grabbed
                        grabbedPlayer.GetComponent<PlayerManager>().PlayerThrown(PlayerManager.PlayerThrow.DOWN_THROW); //Call thrown in playerManager when player is thrown
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
                        grabbedPlayer.GetComponent<PlayerManager>().PlayerThrown(PlayerManager.PlayerThrow.FORWARD_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                    else if (Input.GetAxisRaw("Horizontal") < 0 && pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !pm.facingRight)
                    {
                        Debug.Log("Back Throw");
                        pss.Pop(); //Popping Grabbing
                        grabbedPlayerPss.Pop(); //Popping Grabbed
                        grabbedPlayer.GetComponent<PlayerManager>().PlayerThrown(PlayerManager.PlayerThrow.BACK_THROW); //Call thrown in playerManager when player is thrown
                        grabbedPlayer.transform.parent = null;
                    }
                }

            }
            
        }

        if (Input.GetButtonDown("Attack")) //As long as they aren't in an animation, pressing attack will launch an attack
        {
            if (pss.Peek() == PlayerManager.PlayerState.GROUNDED || pss.Peek() == PlayerManager.PlayerState.IDLE)
            {
                GroundedAttack();
            }

            else if (pss.Peek() == PlayerManager.PlayerState.AERIAL)
            {
                AerialAttack();
            }
        }
        //You can't make multiple inputs at once, so else-if suite is optimal
        else if (Input.GetButtonDown("Special") && (pss.Peek() == PlayerManager.PlayerState.GROUNDED || pss.Peek() == PlayerManager.PlayerState.IDLE || pss.Peek() == PlayerManager.PlayerState.AERIAL)) //As long as they aren't in an animation, pressing attack will launch an attack
        {
            SpecialAttack();
        }

        else if (Input.GetButtonDown("Grab") && (pss.Peek() == PlayerManager.PlayerState.GROUNDED || pss.Peek() == PlayerManager.PlayerState.IDLE))
        {
            StartCoroutine(GrabPlayer());            
        }

        
    }


    /*~~~~~~~~~~~~~~ ALL THINGS BELOW NEED TO BE WORKED ON ~~~~~~~~~~~~~~~~~~~~*/
    //Use Start() and Update() as needed
    //For attacks that are reliant on movement, make the necessary calls to PlayerMovement

    //Function for managing all attacks, should make calls to the attack functions below

    //Nic's Edit: All attack functions call this as a coroutine with hitboxes and delay as parameters
    IEnumerator AttackCalled(GameObject attack, float attackTime)
    {
        pss.Push(PlayerManager.PlayerState.ATTACKING);
        CircleCollider2D hitbox = attack.GetComponent<CircleCollider2D>();
        hitbox.enabled = true;
        yield return new WaitForSeconds(attackTime);
        hitbox.enabled = false;
        pss.Pop(); //ERROR HANDLING: WHAT IF THEY'RE STAGGERED?  THIS WOULD NO LONGER POP THE ATTACK, BUT IT WOULD POP THE STAGGER, Unless Stagger is implemented to always be position 1 (and not 0).  That way, stagger would overrule attacks
    }

    //Function for ground attacks
    void GroundedAttack()
    {
        //Btilt isn't a thing, can only be executed by turning around and ftilting.
        if (Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight) //Ftilt
        {
            StartCoroutine(AttackCalled(Ftilt, forwardTiltAttackTime));
        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Utilt
        {
            StartCoroutine(AttackCalled(Utilt, upTiltAttackTime));
        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Dtilt
        {
            StartCoroutine(AttackCalled(Dtilt, downTiltAttackTime));
        }
        else //Jab if attack is called and no directional attack is selected
        {
            StartCoroutine(AttackCalled(Jab, jabAttackTime));
        }

    }

    //Function for aerial attack
    void AerialAttack()
    {
        if (Input.GetAxisRaw("Horizontal") < 0 && pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && !pm.facingRight) //Backair
        {
            StartCoroutine(AttackCalled(Bair, backAirAttackTime));
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight) //Fair
        {
            StartCoroutine(AttackCalled(Fair, forwardAirAttackTime));
        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Up air
        {
            StartCoroutine(AttackCalled(Uair, upAirAttackTime));
        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Dair
        {
            StartCoroutine(AttackCalled(Dair, downAirAttacktime));
        }
        else //Nair if attack is called and no directional attack is selected
        {
            StartCoroutine(AttackCalled(Nair, neutralAirAttackTime));
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
            StartCoroutine(SpawnProjectile(FASpecial, FASpecialDirection, forwardAirSpecialAttackTime)); //Flipped FASpecial direction
            //Can't possibly be grounded
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && !pm.facingRight || Input.GetAxisRaw("Horizontal") > 0 && pm.facingRight) //Side Special (Forwards)
        {

            if (pss.Peek() == PlayerManager.PlayerState.AERIAL)
            {
                //Aerial Special
                StartCoroutine(SpawnProjectile(FASpecial, FASpecialDirection, forwardAirSpecialAttackTime));
            }
            else
            {
                //Grounded Special
                StartCoroutine(SpawnProjectile(FSpecial, FSpecialDirection, forwardSpecialAttackTime));
            }

        }
        else if (Input.GetAxisRaw("Vertical") > 0) //Up Special
        {

            if (pss.Peek() == PlayerManager.PlayerState.AERIAL)
            {
                //Aerial Special
                StartCoroutine(SpawnProjectile(UASpecial, UASpecialDirection, upAirSpecialAttackTime));
            }
            else
            {
                //Grounded Special
                StartCoroutine(SpawnProjectile(USpecial, USpecialDirection, upSpecialAttackTime));
            }

        }
        else if (Input.GetAxisRaw("Vertical") < 0) //Down Special
        {

            if (pss.Peek() == PlayerManager.PlayerState.AERIAL)
            {
                //Aerial Special
                StartCoroutine(SpawnProjectile(DASpecial, DASpecialDirection, downAirSpecialAttackTime));
            }
            else
            {
                //Grounded Special
                StartCoroutine(SpawnProjectile(DSpecial, DSpecialDirection, downSpecialAttackTime));
            }

        }
        else //Neutral Special if attack is called and no directional attack is selected
        {
            Debug.Log("Neutral Special");

            if (pss.Peek() == PlayerManager.PlayerState.AERIAL)
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
    IEnumerator SpawnProjectile(GameObject specialAttack, Vector2 direction, float specialAttackTime)
    {
        pss.Push(PlayerManager.PlayerState.ATTACKING);

        GameObject projectileClone = Instantiate(projectile);
        projectileClone.GetComponent<ProjectileScript>().shooter = this.gameObject;
        projectileClone.transform.position = specialAttack.transform.position;

        Rigidbody2D rb = projectileClone.GetComponent<Rigidbody2D>();

        if (pm.facingRight) //If they're not facing right, everything is flipped!
        {
            rb.velocity = direction * projectileSpeed;
        }
        else
        {
            rb.velocity = new Vector2(direction.x * -1, direction.y) * projectileSpeed;
        }

        yield return new WaitForSeconds(specialAttackTime);

        pss.Pop(); //ERROR HANDLING: WHAT IF THEY'RE STAGGERED?  THIS WOULD NO LONGER POP THE ATTACK, BUT IT WOULD POP THE STAGGER, Unless Stagger is implemented to always be position 1 (and not 0).  That way, stagger would overrule attacks.  But then the coroutine would have to be interrupted to prevent a pre-emptive pop


    }
}
