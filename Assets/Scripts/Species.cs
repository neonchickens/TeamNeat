using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species
{
    public List<Player> players;
    public float bestfitness = 0;
    Player champ;
    public float avgfitness = 0;
    int staleness = 0;
    Genome rep;

    float excessCoeff = 1;
    float weightDiffCoeff = 0.5f;
    float compatibilityThreshold = 3;

    public Species()
    {
        players = new List<Player>();
    }

    public Species(Player p)
    {
        players = new List<Player>();
        players.Add(p);

        bestfitness = p.fitness;
        rep = p.brain.clone();
    }

    public bool SameSpecies(Genome g)
    {
        float compat;
        float excessAndDisjoint = GetExcessDisjoint(g, rep);
        float avgWeightDiff = FindAvgWeightDiff(g, rep);

        float largeGenomeNormaliser = g.genes.Count - 20;
        if (largeGenomeNormaliser < 1)
        {
            largeGenomeNormaliser = 1;
        }

        compat = (excessCoeff * excessAndDisjoint / largeGenomeNormaliser) + (weightDiffCoeff * avgWeightDiff);
        return (compatibilityThreshold > compat);
    }

    public void AddToSpecies(Player p)
    {
        players.Add(p);
    }

    float GetExcessDisjoint(Genome brain1, Genome brain2)
    {
        float matching = 0;
        for (int i = 0; i < brain1.genes.Count; i++)
        {
            for (int j = 0; j < brain2.genes.Count; j++)
            {
                if (brain1.genes[i].innovationNo == brain2.genes[j].innovationNo)
                {
                    matching++;
                    break;
                }
            }
        }
        return (brain1.genes.Count + brain1.genes.Count - 2 * matching);
    }

    float FindAvgWeightDiff(Genome brain1, Genome brain2)
    {
        float matching = 0;
        float totalDiff = 0;
        for (int i = 0; i < brain1.genes.Count; i++)
        {
            for (int j = 0; j < brain2.genes.Count; j++)
            {
                if (brain1.genes[i].innovationNo == brain2.genes[j].innovationNo)
                {
                    matching++;
                    totalDiff += Mathf.Abs(brain1.genes[i].weight - brain2.genes[i].weight);
                    break;
                }
            }
        }
        if (matching == 0)
        {
            return 100;
        }

        return totalDiff / matching;
    }

    public void SortSpecies()
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

        if (players[0].fitness > bestfitness)
        {
            staleness = 0;
            bestfitness = players[0].fitness;
            rep = players[0].brain.clone();

        } else
        {
            staleness++;
        }
    }

    public void SetAverage()
    {
        float sum = 0;
        for (int i = 0; i < players.Count; i++)
        {
            sum += players[i].fitness;
        }
        avgfitness = sum / players.Count;
    }

    public Player CreatePlayer(List<ConnectionHistory> innovationHistory)
    {
        Player child;
        if (Random.value < .25)
        {
            child = SelectPlayer().Clone();
        } else
        {
            Player p1 = SelectPlayer();
            Player p2 = SelectPlayer();

            if (p1.fitness > p2.fitness)
            {
                child = p1.Crossover(p2);
            } else
            {
                child = p2.Crossover(p1);
            }
        }

        child.brain.Mutate(innovationHistory);
        return child;
    }

    Player SelectPlayer()
    {
        float fitnessSum = 0;
        for (int i = 0; i < players.Count; i++)
        {
            fitnessSum += players[i].fitness;
        }

        float r = Random.Range(0, fitnessSum);
        float runningSum = 0;

        for (int i = 0; i < players.Count; i++)
        {
            runningSum += players[i].fitness;
            if (runningSum > r)
            {
                return players[i];
            }
        }

        return players[0];
    }

    void FitnessSharing()
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].fitness /= players.Count;
        }
    }
}
