using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class League
{
    private List<Team> teams;
    private List<Player> players;

    private int teamsNo;
    private int playersNo;

    private int season = 1;
    private int round = 0;
    private int roundsInSeason = 3;

    private bool playoffs = false;
    private List<Team> playoffBracket;

    private static League league;

    List<ConnectionHistory> lstInnovations;
    List<Species> species;

    private League(int teamsNo, int playersNo)
    {
        lstInnovations = new List<ConnectionHistory>();
        species = new List<Species>();

        this.teamsNo = teamsNo;
        this.playersNo = playersNo;

        teams = new List<Team>();
        players = new List<Player>();
        for (int t = 0; t < teamsNo; t++)
        {
            List<Player> teamates = new List<Player>();
            for (int p = 0; p < playersNo; p++)
            {
                teamates.Add(new Player());
                teamates[p].brain.GenerateNetwork();
                teamates[p].brain.Mutate(lstInnovations);
            }
            teams.Add(new Team(null, teamates));
            players.AddRange(teamates);
        }
    }

    public static League GetInstance()
    {
        if (league == null)
        {
            league = new League(8, 3);
        }

        return league;
    }

    public List<Team> GetRound()
    {
        SortTeams();
        if (round == roundsInSeason)
        {

            string log = "Season " + season + " ranks: \n";
            foreach (Team t in teams)
            {
                log += "Team " + t.name + " (" + t.GetTeamFitness() + ")\n";
            }
            log += "\n";

            season++;
            float avgFitness = 0;
            foreach (Player p in players)
            {
                avgFitness += p.fitness;
            }
            log += "Avg Fitness: " + (avgFitness / players.Count) + "\n";
            Debug.Log(log);

            Draft();
            
            foreach (Team t in teams)
            {
                t.EndSeason();
            }

            round = 0;
        }
        Debug.Log("Round: " + (round + 1));
        round++;
        return new List<Team>(teams);
    }

    private void Draft()
    {
        Debug.Log("Draft");
        SortTeams();

        for (int t = teams.Count - 1; t >= 0; t--)
        {
            string s = "";
            for (int p = 0; p < teams[t].players.Count; p++)
            {
                s += teams[t].players[p].fitness + ": ";
            }
            Debug.Log("Team " + teams[t].name + ": " + s);
        }

        //Bad teams lose more players
        for (int t = teams.Count - 1; t >= 0; t--)
        {
            if (t > teamsNo - (float)teamsNo / playersNo)
            {
                Player pRemove = teams[t].RemoveWorst();
                players.Remove(pRemove);
                Debug.Log("Team " + teams[t].name + " (" + teams[t].GetTeamFitness() + "): Removing " + pRemove.fitness);
                pRemove = teams[t].RemoveWorst();
                players.Remove(pRemove);
                Debug.Log("Team " + teams[t].name + " (" + teams[t].GetTeamFitness() + "): Removing " + pRemove.fitness);
            }
            else if (t > teamsNo - 2 * (float)teamsNo / playersNo)
            {
                Player pRemove = teams[t].RemoveWorst();
                players.Remove(pRemove);
                Debug.Log("Team " + teams[t].name + " (" + teams[t].GetTeamFitness() + "): Removing " + pRemove.fitness);
            }
        }

        Speciate();

        //Generate rookies
        List<Player> rookies = new List<Player>();
        for (int r = 0; r < teamsNo * playersNo; r++)
        {
            for (int s = 0; s < species.Count; r++, s++)
            {
                rookies.Add(species[s].CreatePlayer(lstInnovations));
            }
        }
        
        Debug.Log(rookies.Count);

        //Teams that lost players get new ones
        for (int t = teams.Count - 1; t >= 0; t--)
        {
            if (t > teamsNo - (float)teamsNo / playersNo)
            {
                Debug.Log("Team " + teams[t].name + " has changed 2 players.");
                Player r1 = rookies[Random.Range(0, rookies.Count)];
                players.Add(teams[t].AddPlayer(r1));
                rookies.Remove(r1);
                Player r2 = rookies[Random.Range(0, rookies.Count)];
                players.Add(teams[t].AddPlayer(r2));
                rookies.Remove(r2);
            }
            else if (t > teamsNo - 2 * (float)teamsNo / playersNo)
            {
                Debug.Log("Team " + teams[t].name + " has changed 2 players.");
                Player r1 = rookies[Random.Range(0, rookies.Count)];
                players.Add(teams[t].AddPlayer(r1));
                rookies.Remove(r1);
            }
        }

    }
    
    void Speciate()
    {
        foreach (Species s in species)
        {
            s.players.Clear();
        }

        for (int i = 0; i < players.Count; i++)
        {
            bool speciesFound = false;
            foreach (Species s in species)
            {
                if (s.SameSpecies(players[i].brain))
                {
                    s.AddToSpecies(players[i]);
                    speciesFound = true;
                    break;
                }
            }
            if (!speciesFound)
            {
                species.Add(new Species(players[i]));
            }
        }

        for (int s = 0; s < species.Count; s++)
        {
            if (species[s].players.Count > 0)
            {
                species[s].SetAverage();
            } else
            {
                species.RemoveAt(s);
                s--;
            }
        }
    }

    void SortSpecies()
    {
        foreach (Species s in species)
        {
            s.SortSpecies();
        }

        List<Species> temp = new List<Species>();
        for (int i = 0; i < species.Count; i++)
        {
            float max = 0;
            int maxIndex = 0;
            for (int j = 0; j < species.Count; j++)
            {
                if (species[j].bestfitness > max)
                {
                    max = species[j].bestfitness;
                    maxIndex = j;
                }
            }
            temp.Add(species[maxIndex]);
            species.RemoveAt(maxIndex);
            i--;
        }
        species = temp;
    }

    float GetAvgSpeciesSum()
    {
        float sum = 0;
        foreach (Species s in species)
        {
            sum += s.avgfitness;
        }
        return sum;
    }

    void SortTeams()
    {
        List<Team> temp = new List<Team>();
        for (int i = 0; i < teams.Count; i++)
        {
            float max = 0;
            int maxIndex = 0;
            for (int j = 0; j < teams.Count; j++)
            {
                if (teams[j].GetTeamFitness() > max)
                {
                    max = teams[j].GetTeamFitness();
                    maxIndex = j;
                }
            }
            temp.Add(teams[maxIndex]);
            teams.RemoveAt(maxIndex);
            i--;
        }
        teams = temp;
    }


}
