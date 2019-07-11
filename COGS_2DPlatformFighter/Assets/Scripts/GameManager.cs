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
    public Gamemode gamemode;
    [SerializeField] private int player1Score;
    [SerializeField] private int player2Score;
    [SerializeField] private int player1Damage;
    [SerializeField] private int player2Damage;

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
            //In STOCK, playerScore is stock count and playerDamage is percent
            player1Score = 3;
            player2Score = 3;
            player1Damage = 0;
            player1Damage = 0;
        }
        else if(gamemode.Equals("STAMINA"))
        {
            this.gamemode = Gamemode.STAMINA;
            //In STAMINA, playerScore is stock and playerDamage is health
            player1Score = 3;
            player2Score = 3;
            player1Damage = 100;
            player2Damage = 100;
        }
        else if(gamemode.Equals("TIME"))
        {
            this.gamemode = Gamemode.TIME;
            //In TIME, playerScore is score while playerDamage is percent
            player1Score = 0;
            player2Score = 0;
            player1Damage = 0;
            player2Damage = 0;
        }
        SceneManager.LoadScene(1);
    }

    public void PlayerKill(int playerNum)
    {
        //TODO: Destroy Player Object with that playerNum
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if(player.GetComponent<Player>().playerNum == playerNum)
            {
                Destroy(player);
                break; //There should only be one of each playerNum at any given time, so breaking here is efficient
            }
        }
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
                //TODO: Add Score logic for time
                int lastHit = -1;
                foreach(GameObject player in players)
                {
                    if(player.GetComponent<Player>().playerNum == playerNum)
                    {
                        lastHit = player.GetComponent<Player>().lastHit;
                    }
                }
                Debug.Log("Last hit: Player " + lastHit); //NOTE: This will always be 0, since last hit is reset when the player is on the ground when PlayerMove is attached until knockback is added.
                switch (lastHit)
                {
                    case 0: //If lastHit is 0, that means no player hit them and it's an SD
                        if(playerNum == 1)
                        {
                            player1Score -= 1;
                        }
                        else
                        {
                            player2Score -= 1;
                        }
                        break;
                    case 1: //If lastHit is 1, that means player1 was the last player to hit the dying player
                        player1Score += 1;
                        break;
                    case 2: //If lastHit is 2, that means player1 was the last player to hit the dying player
                        player2Score += 1;
                        break;
                }
                if(playerNum == 1)
                {
                    player1Score -= 1;
                }
                else if(playerNum == 2)
                {
                    player2Score -= 1;
                }
                PlayerRespawn(playerNum);
                break;
            case Gamemode.STAMINA:
                //Stamina score represents stock, so there can be more than one death if the rules are set to have more than one stock
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
            if(gamemode == Gamemode.STAMINA)
            {
                player1Damage = 100;
            }
            else
            {
                player1Damage = 0;
            }
        }
        else
        {
            newPlayer = Instantiate(player2Prefab);
            if (gamemode == Gamemode.STAMINA)
            {
                player2Damage = 100;
            }
            else
            {
                player2Damage = 0;
            }
        }
        newPlayer.transform.position = spawnpoint.position;
        newPlayer.GetComponent<Player>().playerNum = playerNum;
        newPlayer.GetComponent<Player>().lastHit = 0;

    }

    public void PlayerDamage(int playerNum, int damageValue)
    {
        if(gamemode == Gamemode.STAMINA)
        {
            if (playerNum == 1)
            {
                player1Damage -= damageValue;
                if(player1Damage <= 0)
                {
                    PlayerKill(1);
                }
            }
            else if (playerNum == 2)
            {
                player2Damage -= damageValue;
                if(player2Damage <= 0)
                {
                    PlayerKill(2);
                }
            }
        }
        else
        {
            if (playerNum == 1)
            {
                player1Damage += damageValue;
            }
            else if (playerNum == 2)
            {
                player2Damage += damageValue;
            }
        }
    }
}
