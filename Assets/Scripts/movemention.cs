using System;
using System.Collections.Generic;
using UnityEngine;

public class movemention : MonoBehaviour
{
    public int speedRotation = 1;
    public float speed = 1f;
    public GameObject player;
    public Terrain terrain;
    public CharacterController characterController;
    Vector3 position;
    

    // Update is called once per frame
    //up - 273, down - 274, right - 275, left - 276
    void Update()
    {
        float X = Input.GetAxis("Mouse X") * speedRotation * Time.deltaTime;
        float Y = -Input.GetAxis("Mouse Y") * speedRotation * Time.deltaTime;
        Vector3 euler = transform.rotation.eulerAngles;
        position = new Vector3(0, 0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            position = transform.forward*speed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            position = transform.right * speed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            position = -transform.forward * speed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            position = -transform.right * speed;
        }

        if((transform.position.x + position.x) >= terrain.terrainData.size.x || (transform.position.x + position.x) <= 0)
        {
            position.x = 0;
        }

        if ((transform.position.z + position.z) >= terrain.terrainData.size.z || (transform.position.z + position.z) <= 0)
        {
            position.z = 0;
        }

        float y = (euler.y + X) % 360;
        float x = (euler.x + Y) % 360;
        characterController.Move(position);
        transform.rotation = Quaternion.Euler(x, y, 0);

    }
}
