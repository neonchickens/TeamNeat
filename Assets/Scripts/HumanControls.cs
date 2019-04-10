using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanControls : Controls
{
    private Player player;

    public new void FixedUpdate()
    {

        Debug.Log(rb.rotation.w + "; " + rb.rotation.x + "; " + rb.rotation.y + "; " + rb.rotation.z);

        player = null;
        if (player != null)
        {
            player.step();
        } else
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                StrafeForward();
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                StrafeBackward();
            }

            if (Input.GetAxis("Horizontal") > 0)
            {
                StrafeRight();
            }
            else if (Input.GetAxis("Horizontal") < 0)
            {
                StrafeLeft();
            }

            if (Input.GetAxis("Turn") > 0)
            {
                TurnRight();
            }
            else if (Input.GetAxis("Turn") < 0)
            {
                TurnLeft();
            }

            if (Input.GetAxis("Jump") > 0)
            {
                ShootPuck();
            }
        }
    }

    public new void setPlayer(Player player)
    {
        if (player != null)
        {
            player.controls = null;
        }
    }

}
