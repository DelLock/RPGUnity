using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{


    public int facingDirection = 1;
    public float speed = 5;
    public Rigidbody2D rb;
    public Animator anim;
    public Player_Combat player_Combat;



    private void Update()
    {
        if (Input.GetButtonDown("Slash"))
        {
            player_Combat.Attack();
        }
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

if(horizontal >0 && transform.localScale.x <0 || 
    horizontal < 0 && transform.localScale.x > 0)
        {
            Flip();
        }

        anim.SetFloat("horizontal",MathF.Abs(horizontal));
        anim.SetFloat("vertical",MathF.Abs(vertical));
        rb.velocity = new Vector2(horizontal,vertical)*speed;
    }

    void Flip()
    {
        facingDirection*=-1;
        transform.localScale = new Vector3 (transform.localScale.x*-1, transform.localScale.y, transform.localScale.z);
    }
}
