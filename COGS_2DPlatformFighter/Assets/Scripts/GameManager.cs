using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public enum Gamemode {
        STOCK,
        STAMINA,
        TIME
    }

    public Transform spawnpoint;
    //Separate prefabs because different control bindings/player specific details
    public GameObject player1Prefab;
    public GameObject player2Prefab;
    public static GameManager Instance;
    [SerializeField] private Gamemode gamemode;
    [SerializeField] private int player1Score;
    [SerializeField] private int player2Score;

    // Use this for initialization
    void Start() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update() {

    }

    public void SetGamemode(string gamemode)
    {
        //Using string parameter instead of Gamemode parameter because Unity doesn't seem to support enum parameters on button clicks
        Debug.Log(gamemode);
        if(gamemode.Equals("STOCK"))
        {
            this.gamemode = Gamemode.STOCK;
            //In STOCK, playerScore is stock count
            player1Score = 3;
            player2Score = 3;
        }
        else if(gamemode.Equals("STAMINA"))
        {
            this.gamemode = Gamemode.STAMINA;
            //In STAMINA, playerScore is health
            player1Score = 100;
            player2Score = 100;
        }
        else if(gamemode.Equals("TIME"))
        {
            this.gamemode = Gamemode.TIME;
            //In TIME, playerScore is time
            player1Score = 0;
            player2Score = 0;
        }
        SceneManager.LoadScene(1);
    }

    public void PlayerKill(int playerNum)
    {
        switch (gamemode)
        {
            case Gamemode.STOCK:
                if (playerNum == 1)
                {
                    player1Score -= 1;
                    if (player1Score <= 0)
                    {
                        Debug.Log("PLAYER 2 WINS!");
                    }
                    else
                    {
                        PlayerRespawn(1);
                    }
                }
                else if (playerNum == 2)
                {
                    player2Score -= 1;
                    if (player2Score <= 0)
                    {
                        Debug.Log("PLAYER 1 WINS!");
                    }
                    else
                    {
                        PlayerRespawn(2);
                    }
                }
                break;
            case Gamemode.TIME:
                //Going to need some sort of logic to determine whether it was an SD or a kill; possibly attaching "lastHit" as a player number and -1 when not hit, refreshing when on the ground
                break;
            case Gamemode.STAMINA:
                //A death in stamina means you lose.
                if(playerNum == 1)
                {
                    Debug.Log("PLAYER 2 WINS!");
                }
                else if(playerNum == 2)
                {
                    Debug.Log("PLAYER 1 WINS!");
                }
                break;
            default:
                //Empty default case just in case
                Debug.Log("Default case reached in GameManager.PlayerKill()");
                break;
        }

        Debug.Log("Player " + playerNum + " killed");
    }

    public void PlayerRespawn(int playerNum)
    {
        GameObject newPlayer;
        if(playerNum == 1 )
        {
            newPlayer = Instantiate(player1Prefab);
        }
        else
        {
            newPlayer = Instantiate(player2Prefab);
        }
        newPlayer.transform.position = spawnpoint.position;
        newPlayer.GetComponent<PlayerManager>().playerNum = playerNum;
    }
}
