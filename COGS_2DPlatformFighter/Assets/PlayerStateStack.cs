using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateStack : MonoBehaviour {

    public Player.PlayerState[] stateStack { get; set; }
    private int size = -1;
    private int capacity = 5;

	// Use this for initialization
	void Start () {
        stateStack = new Player.PlayerState[capacity];  //Start each stack with a maximum of 5 states stacked on each other, and expand if necessary
        Push(Player.PlayerState.IDLE); //Default state is IDLE
    }
	
    public void Push(Player.PlayerState playerState) //Adds a playerState to the stack
    {
        if(size < stateStack.Length) //If the pushed state fits on the stack, then add it as normal
        {
            ++size;
            stateStack[size] = playerState;
        }
        else //If the pushed state doesn't fit on the stack, double its capacity and then add as normal
        {
            capacity *= 2;
            Player.PlayerState[] tempStack = new Player.PlayerState[capacity];

            //Deep copy of the array
            for(int i = 0; i < stateStack.Length; ++i)
            {
                tempStack[i] = stateStack[i];
            }

            //Swapping references
            stateStack = tempStack;
            
            //Attempt to push the state again
            Push(playerState);
        }
    }

    public void Pop() //Pops a playerState from the stack
    {
        if(size >= 1) //Does not allow popping of the base state
        {
            --size; //When popping, you don't have to worry about clearing the index; when pushing it does that for you.
        }
        else
        {
            Debug.Log("ERROR: Attempting to pop a state off the stateStack at base state");
        }
    }

    public void SetState(Player.PlayerState someState) //Used to set base state
    {
        stateStack[0] = someState;
    }

    public Player.PlayerState Peek()
    {
        return stateStack[size]; //Should never error since we push IDLE in Start() and can never pop it
    }
}
