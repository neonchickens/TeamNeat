using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puck : MonoBehaviour
{
    public Queue<Player> lstPlayerLastTouch;
    public Rigidbody rb;

    Player plrCurrent = null;

    // Start is called before the first frame update
    void Awake()
    {
        lstPlayerLastTouch = new Queue<Player>();
        rb = this.GetComponent<Rigidbody>();
    }

    public void Reset()
    {
        lstPlayerLastTouch = new Queue<Player>();
    }

    void OnTriggerEnter(Collider collision)
    {
        Controls c = collision.gameObject.GetComponent<Controls>();
        if (c != null)
        {
            c.ObtainPuck(this);

            rb.velocity = new Vector3();
            rb.isKinematic = true;

            plrCurrent = c.getPlayer();

            lstPlayerLastTouch.Enqueue(c.getPlayer());
            if (lstPlayerLastTouch.Count > 2)
            {
                lstPlayerLastTouch.Dequeue();
            }

            if (lstPlayerLastTouch.Count > 0)
            {
                bool teamates = true;
                List<Team> lstTeams = Referee.getInstance().teams;
                foreach (Team t in lstTeams)
                {
                    if (t.players.Contains(lstPlayerLastTouch.Peek()) && t.players.Contains(plrCurrent))
                    {
                        teamates = true;
                    }
                }

                if (teamates)
                {
                    lstPlayerLastTouch.Peek().AddPass();
                } else
                {
                    lstPlayerLastTouch.Peek().AddTurnover();
                }
            } else
            {
                plrCurrent.AddFirstPossession();
            }

        }
    }
}
