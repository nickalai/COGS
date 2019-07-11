using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraMovement : MonoBehaviour {

    // VARIABLES //

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
        var temp = GameObject.FindGameObjectsWithTag("Player");
        playerTransforms = new List<Transform>();
        for (int i = 0; i < temp.Length; i++)
        {
            playerTransforms.Add(temp[i].GetComponent<Transform>());
        }
    }
	

	private void Update () {
        CalculateTransform();
    }


    private void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, desiredPos, camSpeed);
    }


    //Method to find largest and lowest x and y values
    private void CalculateTransform()
    {
        if (playerTransforms.Count <= 0) //early out if no players have been found
        {
            return;
        }

        desiredPos = Vector3.zero;
        float distance = 0f;

        var ySort = playerTransforms.OrderByDescending(p => p.position.y);
        var xSort = playerTransforms.OrderByDescending(p => p.position.x);

        var camPosY = ySort.First().position.y - ySort.Last().position.y;
        var camPosX = xSort.First().position.x - xSort.Last().position.x;

        var distanceY = -(camPosY + 5f) * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        var distanceX = -(camPosX / cam.aspect + 5f) * 0.5f / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        distance = distanceY < distanceX ? distanceY : distanceX;

        for (int i = 0; i < playerTransforms.Count; i++)
        {
            desiredPos += playerTransforms[i].position;
        }

        if (distance > -10f) distance = -10f;

        desiredPos /= playerTransforms.Count;
        desiredPos.z = distance;
    }
}
