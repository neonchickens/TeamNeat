using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controls : MonoBehaviour
{
    public Rigidbody rb;

    public float speed;
    public float turnspeed;
    public float shotpower;

    public GameObject goPuckPlaceHolder;
    
    public bool hasPuck = false;

    private Player player;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxDepenetrationVelocity = 10;
        rb.maxAngularVelocity = 5;
    }

    public void FixedUpdate()
    {
        if (player != null)
        {
            player.step();
        }
    }

    public void setPlayer(Player player)
    {
        this.player = player;
        if (player != null)
        {
            player.controls = this;
        }
    }

    public Player getPlayer()
    {
        return this.player;

    }

    public void ObtainPuck(Puck p)
    {
        p.gameObject.transform.parent = goPuckPlaceHolder.transform;
        p.gameObject.transform.localPosition = new Vector3();
        hasPuck = true;
    }

    public void LosePuck()
    {
        Rigidbody rbPlayer = GetComponent<Rigidbody>();
        Puck puck = goPuckPlaceHolder.GetComponentInChildren<Puck>();
        if (puck != null)
        {
            puck.transform.parent = null;
            Rigidbody rbPuck = puck.GetComponent<Rigidbody>();
            rb.velocity = rbPlayer.velocity;
        }
        hasPuck = false;

    }

    public void ShootPuck()
    {
        Puck puck = goPuckPlaceHolder.GetComponentInChildren<Puck>();
        if (puck != null)
        {
            Rigidbody rbPlayer = GetComponent<Rigidbody>();
            Rigidbody rbPuck = puck.GetComponent<Rigidbody>();
            if (rbPuck != null)
            {
                puck.transform.parent = null;
                rbPuck.isKinematic = false;
                rbPuck.velocity = rbPlayer.velocity + shotpower * (puck.gameObject.transform.position - this.gameObject.transform.position);
            }
        }
        hasPuck = false;
    }

    public void StrafeForward()
    {
        rb.AddForce(speed * transform.forward);
    }

    public void StrafeBackward()
    {
        rb.AddForce(speed * -transform.forward);
    }

    public void StrafeLeft()
    {
        rb.AddForce(speed * -transform.right);
    }

    public void StrafeRight()
    {
        rb.AddForce(speed * transform.right);
    }

    public void TurnLeft()
    {
        rb.rotation = Quaternion.Euler(rb.rotation.eulerAngles + turnspeed * new Vector3(0, -1, 0));
    }

    public void TurnRight()
    {
        rb.rotation = Quaternion.Euler(rb.rotation.eulerAngles + turnspeed * new Vector3(0, 1, 0));
    }

    public float GetAngle()
    {
        Vector3 v3Pos = transform.position;
        Vector3 v3Next = v3Pos + transform.forward;
        float angle = Mathf.Atan2(v3Next.y - v3Pos.y, v3Next.x - v3Pos.x);
        return angle;
    }
}
