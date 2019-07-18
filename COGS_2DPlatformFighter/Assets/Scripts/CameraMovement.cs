using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraMovement : MonoBehaviour {

    // VARIABLES //
    private GameObject gameManager;
    private GameManager gm;

    private new Transform transform;
    private List<Transform> playerTransforms;
    private Vector3 desiredPos;

    private Camera cam;
    public float camSpeed;


    // METHODS //

    private void Awake()
    {
        transform = GetComponent<Transform>();
        cam = GetComponent<Camera>();
    }


    private void Start () {
        gameManager = GameObject.Find("GameManager");
        gm = gameManager.GetComponent<GameManager>();
        /*
        var temp = GameObject.FindGameObjectsWithTag("Player");
        playerTransforms = new List<Transform>();
        for (int i = 0; i < temp.Length; i++)
        {
            playerTransforms.Add(temp[i].GetComponent<Transform>());
        }*/
    }
	

	private void Update () {
        
        var temp = GameObject.FindGameObjectsWithTag("Player");
        playerTransforms = new List<Transform>();
        for (int i = 0; i < gm.players.Length; i++)
        {
            playerTransforms.Add(gm.players[i].GetComponent<Transform>());
        }

        CalculateTransform();
        CalculateSize();
    }


    private void LateUpdate()
    {

        transform.position = Vector3.MoveTowards(transform.position, desiredPos, camSpeed);

        if (cam.orthographicSize < 9)
        {
            cam.orthographicSize = 9;
        }
        else if (cam.orthographicSize > 15)
        {
            cam.orthographicSize = 15;
        }
    }


    //Method to find largest and lowest x and y values
    private void CalculateTransform()
    {
        if (playerTransforms.Count <= 0) //Does not run to completion if no players have been found
        {
            return;
        }

        desiredPos = Vector3.zero;
        float distance = 0f;

        var camPosY = GetMaxY();
        var camPosX = GetMaxX();

        var distanceY = -(camPosY + 5f) * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var distanceX = -(camPosX / cam.aspect + 5f) * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        distance = distanceY < distanceX ? distanceY : distanceX;

        for (int i = 0; i < playerTransforms.Count; i++)
        {
            desiredPos += playerTransforms[i].position;
        }

        if (distance > -10f)
        {
            distance = -10f;
        }

        desiredPos /= playerTransforms.Count;
        desiredPos.z = distance;
    }

    private void CalculateSize()
    {
        float maxDifX = GetMaxX();
        float maxDifY = GetMaxY();

        if (maxDifX >= maxDifY)
        {
            cam.orthographicSize = (maxDifX / 3);
        }
        else if (maxDifY > maxDifX)
        {
            cam.orthographicSize = (maxDifY / 2) + 3;
        }
        else
        {
            Debug.Log("Something went wrong while attempting to change the cameras size.");
        }
    }

    private float GetMaxX()
    {
        var xSort = playerTransforms.OrderByDescending(p => p.position.x);
        var maxX = xSort.First().position.x - xSort.Last().position.x;

        return maxX;
    }

    private float GetMaxY()
    {
        var ySort = playerTransforms.OrderByDescending(p => p.position.y);
        var maxY = ySort.First().position.y - ySort.Last().position.y;

        return maxY;
    }
}
