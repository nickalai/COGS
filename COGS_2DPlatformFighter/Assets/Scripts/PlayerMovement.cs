using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [SerializeField] private float walkSpeed = 7.5f; //The speed at which the player walks
    [SerializeField] private float runSpeed = 15f; //The speed at which the player runs
    [SerializeField] private float jumpForce = 500f; //The force applied on player jump
    [SerializeField] private float airSpeed = 10f; //The speed at which the player moves through the air
    [SerializeField] private short maxJumps = 2; //The max number of jumps players have

    private PlayerManager pm;
    private PlayerStateStack pss;

    private float smoothTime = 0.2f; //Internal smoothing value used for SmoothDamp
    private float horizontalMove; //The current velocity of the player
    private short jumpsLeft; //How many jumps the player has left
    private Rigidbody2D rb; //The Rigidbody2D on the player character
    private Vector2 smoothingVelocity = Vector2.zero; //Internal Vector2 to be used as ref parameter for SmoothDamp

    private bool hasAirDodged = false;
    private float dodgeRollLength = 0.5f; //How long the player is in the dodge roll (How long their hitbox will remain off)
    private float rollForce = 500f;
    private float airDodgeLength = 0.5f; //How long the player is in a neutral air dodge (How long their hitbox will remain off)
    private float airDodgeForce = 500f;
    private float spotDodgeLength = 0.25f; //How long the player is in a spot Dodge
    private bool isFastFalling = false;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<PlayerManager>();
        pss = GetComponent<PlayerStateStack>();
        jumpsLeft = maxJumps;
	}
	
	// Update is called once per frame
	void Update () {

        RaycastHit2D beamToFloor = Physics2D.Raycast(transform.position, -Vector2.up, 1.3f); //1.294 should be distance to ground, but 1.3f allows leniency to avoid bugs (1.2978 occasionally popped up and prevented flipping)

        //Jump if player has jumps left
        if (Input.GetButtonDown("Jump") && jumpsLeft > 0 && (pss.Peek() == PlayerManager.PlayerState.GROUNDED || pss.Peek() == PlayerManager.PlayerState.IDLE || pss.Peek() == PlayerManager.PlayerState.AERIAL))
        {
            PlayerJump();
            pss.SetState(PlayerManager.PlayerState.AERIAL);
        }
        else if (beamToFloor.collider != null) //If you're close to the floor and haven't jumped, you must be grounded.
        {
            pss.SetState(PlayerManager.PlayerState.GROUNDED);
            isFastFalling = false;
            hasAirDodged = false;
            jumpsLeft = maxJumps; //When you're grounded, you regain your max jumps
        }

        //Change player movement based on whether or not player is sprinting/in the air
        if(pss.Peek() == PlayerManager.PlayerState.GROUNDED)
        {
            if (Input.GetButton("Sprint"))
            {
                horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            }
            else
            {
                horizontalMove = Input.GetAxisRaw("Horizontal") * walkSpeed;
            }
        }
        else if(pss.Peek() == PlayerManager.PlayerState.GRABBED || pss.Peek() == PlayerManager.PlayerState.GRABBING) //If you are being grabbed/grabbing, you can't move
        {
            horizontalMove = 0;
        }
        else //If you're not being grabbed or grabbing, you can move
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * airSpeed;
            //If you're in the air and press down after/at the peak of your jump, you fast fall
            if (pss.Peek() == PlayerManager.PlayerState.AERIAL)
            {
                if (Input.GetButtonDown("Vertical") && Input.GetAxisRaw("Vertical") < 0 && rb.velocity.y <= 0 && !isFastFalling)
                {
                    rb.AddForce(new Vector2(0f, -1 * jumpForce));
                    isFastFalling = true;
                }
            }
        }


        //Move player based on their current speed
        PlayerMove(horizontalMove);
        
        //If the player is on the ground (GROUNDED or IDLE), they can dodge roll/spot dodge
        if(Input.GetButtonDown("Dodge") && (pss.Peek() == PlayerManager.PlayerState.IDLE || pss.Peek() == PlayerManager.PlayerState.GROUNDED))
        {
            if(Input.GetAxisRaw("Vertical") < 0)
            {
                StartCoroutine(PlayerSpotDodge());
            }
            else
            {
                StartCoroutine(PlayerDodgeRoll());
            }
        }
        //If they're in the air and try to air dodge, they do unless they already have
        else if (Input.GetButtonDown("Dodge") && pss.Peek() == PlayerManager.PlayerState.AERIAL && !hasAirDodged) 
        {
            StartCoroutine(PlayerAirDodge());
        }



    }


    /*~~~~~~~~~~~~~~ ALL THINGS BELOW NEED TO BE WORKED ON ~~~~~~~~~~~~~~~~~~~~*/
    //Use Start() and Update() as needed


    //Function for moving to the left or right
    void PlayerMove(float horizontalVelocity)
    {
        //Flip the character model if it changes horizontal direction on the ground
        if(pm.facingRight && horizontalVelocity < 0 && (pss.Peek() == PlayerManager.PlayerState.GROUNDED))
        {
            PlayerFlip();
        }
        else if(!pm.facingRight && horizontalVelocity > 0 && (pss.Peek() == PlayerManager.PlayerState.GROUNDED))
        {
            PlayerFlip();
        }

        Vector2 targetVelocity;

        if (isFastFalling && rb.velocity.y < 0)  //Precondition: The player is 'fastfalling' and moving downwards
        {
            targetVelocity = new Vector2(horizontalVelocity, 2 * rb.velocity.y);
        }
        else
        {
            targetVelocity = new Vector2(horizontalVelocity, rb.velocity.y);
        }

        rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref smoothingVelocity, smoothTime);
    }

    //Function for player jumping and double jump
    void PlayerJump()
    {
        //Reset the vertical velocity to 0 before adding jump force(to keep jumps consistent)
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(new Vector2(0f, jumpForce));
        --jumpsLeft;
    }

    //Function for the player dodge rolling
    IEnumerator PlayerDodgeRoll() //IEnumerator for waitForSeconds()
    {
        pss.Push(PlayerManager.PlayerState.DODGING);
        pm.hitbox.enabled = false; //Disabling boxCollider hitbox
        if(pm.facingRight)
        {
            rb.AddForce(new Vector2(rollForce, 0f));
        }
        else
        {
            rb.AddForce(new Vector2(rollForce * -1, 0f));
        }
        yield return new WaitForSeconds(dodgeRollLength);
        pss.Pop(); //Since GROUNDED is set via SetState and we've disabled the hitbox, there's no way that this Pop could be anything except the DODGING we just pushed.
        pm.hitbox.enabled = true; //Re-enabling boxCollider hitbox
    }

    //Function for the player dodge rolling
    IEnumerator PlayerSpotDodge() //IEnumerator for waitForSeconds()
    {
        pss.Push(PlayerManager.PlayerState.DODGING);
        pm.hitbox.enabled = false; //Disabling boxCollider hitbox
        yield return new WaitForSeconds(spotDodgeLength);
        pss.Pop();
        pm.hitbox.enabled = true; //Re-enabling boxCollider hitbox
    }

    //Function for player air dodging
    IEnumerator PlayerAirDodge()
    {

        //Air Dodge
        pss.Push(PlayerManager.PlayerState.SLIDING); //SLIDING = Air Slide = Air Dodge
        hasAirDodged = true;
        pm.hitbox.enabled = false; //Disabling boxCollider hitbox

        //Apply force based on vertical/Horizontal input after zeroing out velocity
        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
        {
            //Neutral Dodge keeps initial velocity
        }
        else
        {
            rb.velocity = new Vector2(0f, 0f);
        }
        rb.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * airDodgeForce, Input.GetAxisRaw("Vertical") * airDodgeForce));

        yield return new WaitForSeconds(airDodgeLength);
        pss.Pop();
        pm.hitbox.enabled = true; //Re-enabling boxCollider hitbox

    }

    //Function to flip player character horizontally
    public void PlayerFlip()
    {
        pm.facingRight = !pm.facingRight; //Flip the variable storing the direction character's facing
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y); //Use negative scale to 'flip' player character
    }
}
