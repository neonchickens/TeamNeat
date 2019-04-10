using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionHistory
{
    public int fromNode;
    public int toNode;
    public int innovationNumber;

    public static int nextConnectionNo = 1;

    public List<int> innovationNumbers = new List<int>();

    public ConnectionHistory(int from, int to, int inno, List<int> innovationNos)
    {
        fromNode = from;
        toNode = to;
        innovationNumber = inno;
        int[] intCopy = new int[innovationNos.Count];
        innovationNumbers = new List<int>(innovationNos);
    }

    public bool matches(Genome genome, Node from, Node to)
    {
        if (genome.genes.Count == innovationNumbers.Count)
        {
            if (from.number == fromNode && to.number == toNode)
            {
                for (int i = 0; i < genome.genes.Count; i++)
                {
                    if (!innovationNumbers.Contains(genome.genes[i].innovationNo))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }
}
