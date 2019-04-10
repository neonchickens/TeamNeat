using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullControls : HumanControls
{
    private Player player;

    public new void FixedUpdate()
    {
        player = null;

    }

    public new void setPlayer(Player player)
    {
        if (player != null)
        {
            player.controls = null;
        }
    }
}
