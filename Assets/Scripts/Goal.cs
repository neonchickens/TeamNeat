using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goal : MonoBehaviour
{
    public Team team;
    public Text score;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collision)
    {
        Puck p = collision.gameObject.GetComponent<Puck>();
        if (p != null)
        {
            team.AddPoints(1);
            score.text = team.GetPoints().ToString();

            while (p.lstPlayerLastTouch.Count > 0)
            {
                Player player = p.lstPlayerLastTouch.Dequeue();
                foreach (Player teamates in team.players)
                {
                    if (teamates.Equals(player))
                    {
                        player.fitness += 3;
                    } else
                    {
                        player.fitness -= 1;
                    }
                }
            }

            Referee.getInstance().Spawn();
        }
    }
}
