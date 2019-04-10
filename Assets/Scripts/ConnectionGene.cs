using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionGene
{

    public Node fromNode;
    public Node toNode;

    public float weight;
    public bool enabled = true;
    public int innovationNo;
    
    public ConnectionGene(Node from, Node to, float w, int inno)
    {
        fromNode = from;
        toNode = to;
        weight = w;
        innovationNo = inno;
    }

    public void mutateWeight()
    {
        float rand2 = Random.value;
        if (rand2 < 0.1)
        {
            weight = Random.Range(0, 1);

        } else
        {
            //TODO check
            weight += (float)NextGaussianDouble() / 50;

            if (weight > 1)
            {
                weight = 1;
            } else if (weight < -1)
            {
                weight = -1;
            }
        }
    }

    public static double NextGaussianDouble()
    {
        double u, v, s;
        
        do
        {
            u = 2.0 * Random.value - 1.0;
            v = 2.0 * Random.value - 1.0;
            s = u * u + v * v;
        }
        while (s >= 1.0);

        double fac = Mathf.Sqrt((float)(-2.0 * Mathf.Log((float)s) / s));
        return u * fac;
    }

    public ConnectionGene clone(Node from, Node to)
    {
        ConnectionGene clone = new ConnectionGene(from, to, weight, innovationNo);
        clone.enabled = enabled;

        return clone;
    }
}
