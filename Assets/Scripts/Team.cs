using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Team
{

    public List<Player> players;
    private int points;
    
    private int seasonGameDiff = 0;
    private int seasonPointDiff = 0;
    private int seasonGamesWon = 0;
    private int seasonPoints = 0;
    private int seasonDistance = 0;

    private static int idIncrement = 1;
    public new readonly string name;
    public readonly int id;

    public Team(string name, List<Player> players)
    {
        id = idIncrement++;

        if (name == null)
        {
            name = id.ToString();
        }
        this.name = name;

        this.players = players;

        points = 0;
    }

    public Team()
    {
        id = idIncrement++;

        if (name == null)
        {
            name = id.ToString();
        }

        this.players = new List<Player>();

        points = 0;
    }

    public Player AddPlayer(Player p)
    {
        players.Add(p);
        return p;
    }

    public void AddPoints(int points)
    {
        this.points += points;
    }

    public int GetPoints()
    {
        return this.points;
    }

    public int GetTeamFitness()
    {
        return seasonGamesWon * 100 + seasonPoints * 10 + seasonDistance;
    }
    
    void SortPlayers()
    {
        List<Player> temp = new List<Player>();
        for (int i = 0; i < players.Count; i++)
        {
            float max = 0;
            int maxIndex = 0;
            for (int j = 0; j < players.Count; j++)
            {
                if (players[j].fitness > max)
                {
                    max = players[j].fitness;
                    maxIndex = j;
                }
            }
            temp.Add(players[maxIndex]);
            players.RemoveAt(maxIndex);
            i--;
        }
        players = temp;
    }

    public Player RemoveWorst()
    {
        SortPlayers();
        Player p;
        p = players[players.Count - 1];
        players.RemoveAt(players.Count - 1);
        return p;
    }

    public void StartGame()
    {
        points = 0;
    }

    public void EndGame(Team enemy)
    {
        foreach (Player p in players)
        {
            seasonDistance += (int)p.distance;
            p.EndGame();
        }

        seasonGameDiff += (points > enemy.points ? 1 : -1);
        seasonPointDiff += points - enemy.points;
        seasonGamesWon += (points > enemy.points ? 1 : 0);
        seasonPoints += points;
    }

    public void EndSeason()
    {
        foreach (Player p in players)
        {
            p.EndSeason();
        }

        seasonDistance = 0;
        seasonGameDiff = 0;
        seasonPointDiff = 0;
        seasonGamesWon = 0;
        seasonPoints = 0;
    }
}
