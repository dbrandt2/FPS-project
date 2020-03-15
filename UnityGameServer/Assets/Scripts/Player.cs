using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    //player id information
    public int id;
    public string username;
    public CharacterController controller;

    //player movement
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed;
    private float yVelocity = 0;

    //player health/stamina
    public float health;
    public float maxHealth = 100f;

    //player shoot/attacking
    public Transform shootOrigin;


    private bool[] inputs;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int __id, string __username)
    {
        id = __id;
        username = __username;
        health = maxHealth;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {

        if (health <= 0f)
        {
            return;
        }
        Vector2 __inputDirection = Vector2.zero;
        if (inputs[0])
        {
            __inputDirection.y += 1;
        }
        if (inputs[1])
        {
            __inputDirection.x -= 1;
        }
        if (inputs[2])
        {
            __inputDirection.y -= 1;
        }
        if (inputs[3])
        {
            __inputDirection.x += 1;
        }

        Move(__inputDirection);
    }

    public void Move(Vector2 __inputDirection)
    {

        Vector3 __moveDirection = transform.right * __inputDirection.x + transform.forward * __inputDirection.y;
        __moveDirection *= moveSpeed;

        //handle gravity
        if (controller.isGrounded)
        {
            yVelocity = 0;
            //if player jumped
            if (inputs[4])
            {
                yVelocity = jumpSpeed;
            }
        }
        yVelocity += gravity;

        __moveDirection.y = yVelocity;
        controller.Move(__moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInputs(bool[] __inputs, Quaternion __rotation)
    {
        inputs = __inputs;
        transform.rotation = __rotation;
    }

    public void Shoot(Vector3 __veiwDirection)
    {
        if (Physics.Raycast(shootOrigin.position, __veiwDirection, out RaycastHit __hit, 25f))
        {
            if (__hit.collider.CompareTag("Player"))
            {
                //modify damage amount
                __hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }

    public void TakeDamage(float __damage)
    {
        if (health <= 0)
        {
            return;
        }

        health -= __damage;
        if (health <= 0f)
        {
            //respawn player
            health = 0f;
            controller.enabled = false;
            //respawn position
            if (id == 1)
            {
                transform.position = new Vector3(4, .5f, -4);
            }
            else if (id == 2)
            {
                transform.position = new Vector3(-4, 0.5f, 4);
            }
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }
        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    { 
        //respawn delay
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }
}

