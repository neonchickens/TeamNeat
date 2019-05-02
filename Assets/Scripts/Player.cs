using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    private float fitness;
    public int gamesPlayed = 0;
    public float bestfitness;
    public float distance = 0;

    public Genome brain;
    public bool replay = false;

    public float unadjestedFitness;
    public int gen = 0;
    int genomeInputs = 4 + 7 * 6;
    int genomeOutputs = 6 + 1;

    float[] vision;
    float[] decision;

    public bool hasPuck = false;

    private Vector3 posLast;
    public Controls controls;

    private static int id_inc = 1;
    public readonly int id;

    public List<Stats> stats;

    public Player(int season)
    {
        id = id_inc++;
        stats = new List<Stats>();
        stats.Add(new Stats(season, id));

        vision = new float[genomeInputs];
        decision = new float[genomeOutputs];

        brain = new Genome(genomeInputs, genomeOutputs);
        brain.GenerateNetwork();
    }

    public void step()
    {
        vision = Referee.getInstance().GetVision(this);
        //Debug.Log(vision[0] + ": " + vision[1] + ": " + vision[2] + ": " + vision[3] + ": " + vision[4] + ": " + vision[5] + ": " + vision[6] + ": " + vision[7] + ";");

        decision = brain.FeedForward(vision);
        //Debug.Log(decision[0] + ": " + decision[1] + ": " + decision[2] + ": " + decision[3] + ": " + decision[4] + ": " + decision[5] + ": " + decision[6] + ";");

        if (decision[0] > .7)
        {
            controls.StrafeForward();
        }

        if (decision[1] > .7)
        {
            controls.StrafeBackward();
        }

        if (decision[2] > .7)
        {
            controls.StrafeLeft();
        }

        if (decision[3] > .7)
        {
            controls.StrafeRight();
        }

        if (decision[4] > .7)
        {
            controls.TurnLeft();
        }

        if (decision[5] > .7)
        {
            controls.TurnRight();
        }

        if (decision[6] > .7)
        {
            controls.ShootPuck();
        }

        if (posLast == null)
        {
            posLast = controls.transform.position;
        } else
        {
            float dist = Vector3.Magnitude(controls.transform.position - posLast);
            distance += dist;
            fitness += dist * (hasPuck ? 3 : 1);
            posLast = controls.transform.position;
        }
    }
    
    public Player Clone()
    {
        Player clone = new Player(League.GetInstance().GetSeason());
        clone.brain = brain;
        clone.fitness = fitness;
        clone.brain.GenerateNetwork();
        clone.gen = gen;
        clone.bestfitness = fitness;
        return clone;
    }

    public void AddAssist()
    {
        fitness += 75;
        stats[stats.Count - 1].assists++;
    }

    public void AddGoal()
    {
        fitness += 100;
        stats[stats.Count - 1].points++;
    }

    public void AddSave()
    {
        fitness += 75;
        stats[stats.Count - 1].saves++;
    }

    public void AddPass()
    {
        fitness += 25;
        stats[stats.Count - 1].throws++;
    }

    public void AddSteal()
    {
        fitness += 50;
        stats[stats.Count - 1].steals++;
    }

    public void AddTurnover()
    {
        fitness -= 50;
        stats[stats.Count - 1].turnovers++;
    }
    
    public void AddFirstPossession()
    {
        fitness += 50;
    }

    public float GetFitness()
    {
        return fitness;
    }

    public Player Crossover(Player p2)
    {
        Player child = new Player(League.GetInstance().GetSeason());
        child.brain = brain.Crossover(p2.brain);
        child.brain.GenerateNetwork();
        return child;
    }

    public void EndGame()
    {
        if (fitness > bestfitness)
        {
            bestfitness = fitness;
        }
        gamesPlayed++;
        distance = 0;
    }

    public void EndSeason()
    {
        fitness = 0;
        Referee.getInstance().LogStats(stats[stats.Count - 1]);
        stats.Add(new Stats(League.GetInstance().GetSeason(), id));
    }

}
