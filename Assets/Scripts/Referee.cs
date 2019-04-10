using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Referee : MonoBehaviour
{
    public List<Team> teams;
    public Queue<Team> gamesInRound;
    public List<Controls> controls;
    public Transform[] spawns;
    public Puck puck;
    public Text time;

    public bool testBuild = false;

    public float matchTimeStart = 30;
    public float matchTime;
    public bool matchActive = false;

    private static Referee r;
    

    // Start is called before the first frame update
    void Start()
    {
        r = this;
        gamesInRound = new Queue<Team>();
        StartGame();
    }

    public static Referee getInstance()
    {
        if (r == null)
        {
            return null;
        }
        return r;
    }

    // Update is called once per frame
    void Update()
    {
        if (r.matchActive)
        {
            r.matchTime -= Time.deltaTime;
            time.text = ((int)r.matchTime).ToString();
        }

        if (r.matchActive && matchTime <= 0)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        if (!testBuild)
        {
            if (gamesInRound.Count == 0)
            {
                gamesInRound = new Queue<Team>(League.GetInstance().GetRound());
            }

            teams = new List<Team>();
            teams.Add(gamesInRound.Dequeue());
            teams.Add(gamesInRound.Dequeue());
            Debug.Log("Team " + teams[0].name + " vs. Team " + teams[1].name);

            for (int t = 0; t < teams.Count; t++)
            {
                for (int p = 0; p < teams[t].players.Count; p++)
                {
                    controls[t * 3 + p].setPlayer(teams[t].players[p]);
                }
                teams[t].StartGame();
            }
        } else
        {
            teams = new List<Team>();
            teams.Add(new Team());
            teams.Add(new Team());
        }

        Goal[] goals = FindObjectsOfType<Goal>();
        for (int g = 0; g < goals.Length; g++)
        {
            goals[g].team = teams[g];
        }

        matchTime = matchTimeStart;
        r.matchActive = true;
        Spawn();
    }

    public void Spawn()
    {
        puck.transform.parent = null;
        puck.transform.position = spawns[0].position;
        puck.transform.rotation = Quaternion.identity;
        puck.transform.localScale = new Vector3(0.25f, 0.1f, 0.25f);
        puck.GetComponent<Rigidbody>().velocity = new Vector3();

        for (int s = 1; s < spawns.Length; s++)
        {
            controls[s - 1].transform.position = spawns[s].transform.position;
            controls[s - 1].transform.LookAt(puck.transform);
            controls[s - 1].rb.velocity = new Vector3();
        }
    }

    public void EndGame()
    {
        foreach (Controls c in controls)
        {
            c.setPlayer(null);
        }
        for (int t = 0; t < teams.Count; t++)
        {
            teams[t].EndGame(teams[(t + 1) % teams.Count]);
        }

        r.matchActive = false;
        StartGame();
    }

    public float[] GetVision(Player myPlayer)
    {
        int puckOffset = 4;
        int genomeInputs = puckOffset + 7 * 6;
        float[] vision = new float[genomeInputs];

        for (int t = 0; t < teams.Count; t++)
        {
            for (int p = 0; p < teams[t].players.Count; p++)
            {
                if (teams[t].players[p].Equals(myPlayer))
                {
                    Vector3 v3Goal = new Vector3();
                    Vector3 v3EnemyGoal = new Vector3();
                    Goal[] goals = FindObjectsOfType<Goal>();
                    for (int g = 0; g < goals.Length; g++)
                    {
                        if (goals[g].team.Equals(teams[t]))
                        {
                            v3Goal = goals[g].gameObject.transform.position;
                            v3EnemyGoal = goals[(g + 1) % goals.Length].gameObject.transform.position;
                        }
                    }
                    
                    Vector3 v3Difference = new Vector3();
                    v3Difference.x = (v3Goal.x - puck.transform.position.x) / (v3Goal.x - v3EnemyGoal.x);
                    v3Difference.z = (puck.transform.position.z + 5) / 10.0f;
                    vision[0] = v3Difference.x;
                    vision[1] = v3Difference.z;
                    vision[2] = puck.rb.velocity.x;
                    vision[3] = puck.rb.velocity.z;

                    for (int teamates = 0; teamates < teams[t].players.Count; teamates++)
                    {
                        Controls c = teams[t].players[(p + teamates) % 3].controls;
                        Vector3 v3Player = c.transform.position;
                        v3Difference = new Vector3();
                        v3Difference.x = (v3Goal.x - v3Player.x) / (v3Goal.x - v3EnemyGoal.x);
                        v3Difference.z = (v3Player.z + 5) / 10.0f;

                        vision[puckOffset + (3 * teamates + 0)] = v3Difference.x;
                        vision[puckOffset + (3 * teamates + 1)] = v3Difference.z;
                        vision[puckOffset + (3 * teamates + 2)] = c.rb.velocity.x;
                        vision[puckOffset + (3 * teamates + 3)] = c.rb.velocity.y;
                        vision[puckOffset + (3 * teamates + 4)] = Mathf.Cos(c.GetAngle());
                        vision[puckOffset + (3 * teamates + 5)] = Mathf.Cos(c.GetAngle());
                        vision[puckOffset + (3 * teamates + 6)] = teams[t].players[(p + teamates) % 3].hasPuck ? 1.0f: 0.0f;
                    }

                    int ts = (t + 1) % teams.Count;
                    for (int enemies = 0; enemies < teams[ts].players.Count; enemies++)
                    {
                        Controls c = teams[ts].players[(p + enemies) % 3].controls;
                        Vector3 v3Player = c.transform.position;
                        v3Difference = new Vector3();
                        v3Difference.x = (v3Goal.x - v3Player.x) / (v3Goal.x - v3EnemyGoal.x);
                        v3Difference.z = (v3Player.z + 5) / 10.0f;

                        vision[puckOffset + 15 + (3 * enemies + 0)] = v3Difference.x;
                        vision[puckOffset + 15 + (3 * enemies + 1)] = v3Difference.z;
                        vision[puckOffset + 15 + (3 * enemies + 2)] = c.rb.velocity.x;
                        vision[puckOffset + 15 + (3 * enemies + 3)] = c.rb.velocity.y;
                        vision[puckOffset + 15 + (3 * enemies + 4)] = Mathf.Cos(c.GetAngle());
                        vision[puckOffset + 15 + (3 * enemies + 5)] = Mathf.Cos(c.GetAngle());
                        vision[puckOffset + 15 + (3 * enemies + 6)] = teams[t].players[(p + enemies) % 3].hasPuck ? 1.0f : 0.0f;
                    }
                }
            }
        }
        return vision;
    }
}
