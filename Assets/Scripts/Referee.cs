using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Referee : MonoBehaviour
{
    public List<Team> teams;
    public Queue<Team> gamesInRound;
    public List<Controls> controls;
    public Transform[] spawns;
    public Puck puck;
    public Text time;

    public GameObject panel;
    public GameObject prefabNode;

    public bool testBuild = false;

    public float matchTimeStart = 30;
    public float matchTime;
    public bool matchActive = false;

    private static Referee r;

    public int run;
    

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartNewRun());


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


    // remember to use StartCoroutine when calling this function!
    IEnumerator StartNewRun()
    {
        //Connect to questions database
        string questions_url = "http://localhost/new_run.php";

        // Create a form object for sending data to the server
        WWWForm form = new WWWForm();
        var download = UnityWebRequest.Post(questions_url, form);

        // Wait until the download is done
        yield return download.SendWebRequest();

        if (download.isNetworkError || download.isHttpError)
        {
            //If we can't connect to the server for some reason
            print("Error downloading: " + download.error);

        }
        else
        {
            //Connected to server
            Debug.Log(download.downloadHandler.text);

            //Will load the queue object with our json data
            run = int.Parse(download.downloadHandler.text.Trim());

        }
    }


    public void LogStats(Stats stats)
    {
        StartCoroutine(DBStats(stats));
    }

    public IEnumerator DBStats(Stats stats)
    {
        //Connect to questions database
        string attempts_url = "http://localhost/record_stats.php";

        // Create a form object for sending data to the server
        WWWForm form = new WWWForm();
        form.AddField("runid", run);
        form.AddField("seasonid", stats.season);
        form.AddField("playerid", stats.playerid);
        form.AddField("points", stats.points);
        form.AddField("throws", stats.throws);
        form.AddField("assists", stats.assists);
        form.AddField("steals", stats.steals);
        form.AddField("turnovers", stats.turnovers);
        form.AddField("saves", stats.saves);

        var download = UnityWebRequest.Post(attempts_url, form);

        // Wait until the download is done
        yield return download.SendWebRequest();

        if (download.isNetworkError || download.isHttpError)
        {
            Debug.Log("Error downloading: " + download.error);

        }
        else
        {
            Debug.Log(download.downloadHandler.text + "\nAttempt sent successfully");
        }
    }

    public void LogDraft(int teamid, int plrid)
    {
        StartCoroutine(DBDraft(teamid, plrid));
    }

    public IEnumerator DBDraft(int teamid, int plrid)
    {
        //Connect to questions database
        string attempts_url = "http://localhost/new_player.php";

        // Create a form object for sending data to the server
        WWWForm form = new WWWForm();
        form.AddField("runid", run);
        form.AddField("teamid", teamid);
        form.AddField("playerid", plrid);

        var download = UnityWebRequest.Post(attempts_url, form);

        // Wait until the download is done
        yield return download.SendWebRequest();

        if (download.isNetworkError || download.isHttpError)
        {
            Debug.Log("Error downloading: " + download.error);

        }
        else
        {
            Debug.Log(download.downloadHandler.text + "\nAttempt sent successfully");
        }
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

        Ray ray = FindObjectOfType<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit[] rayhit = Physics.RaycastAll(ray);
        if (Input.GetMouseButtonUp(0) && rayhit.Length > 0)
        {
            foreach (RaycastHit r in rayhit)
            {
                Controls c = r.collider.gameObject.GetComponent<Controls>();
                if (c != null)
                {
                    Player p = c.getPlayer();
                    panel.SetActive(true);
                    Debug.Log("Player selected");
                    p.brain.DrawGenome(panel, prefabNode);
                }
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            panel.SetActive(false);
            LineRenderer[] lr = (panel.GetComponents<LineRenderer>());
            for (int i = lr.Length - 1; i >= 0; i--)
            {
                Destroy(lr[i]);
            }
            int childs = panel.transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                GameObject.DestroyImmediate(panel.transform.GetChild(i).gameObject);
            }
        }
    }

    public void StartGame()
    {
        if (!testBuild)
        {
            if (gamesInRound.Count == 0)
            {
                gamesInRound = new Queue<Team>(League.GetInstance().GetNextRound());
            }

            teams = new List<Team>();
            teams.Add(gamesInRound.Dequeue());
            teams.Add(gamesInRound.Dequeue());
            Debug.Log("Team " + teams[0].name + "(" + teams[0].GetTeamFitness() + ") vs. Team " + teams[1].name + "(" + teams[1].GetTeamFitness() + ")");

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
            goals[g].Reset();
        }

        puck.Reset();
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
