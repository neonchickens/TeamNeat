using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int number;
    public float inputSum = 0;
    public float outputVal = 0;
    public List<ConnectionGene> outputConnections;
    public int layer = 0;
    
    public Node(int no)
    {
        outputConnections = new List<ConnectionGene>();
        number = no;
    }

    public void engage()
    {
        if (layer != 0)
        {
            outputVal = sigmoid(inputSum);
        }
        
        for (int i = 0; i < outputConnections.Count; i++)
        {
            if (outputConnections[i].enabled)
            {
                outputConnections[i].toNode.inputSum = outputConnections[i].weight * outputVal;
            }
        }
    }

    float sigmoid(float x)
    {
        float y = 1 / (1 + Mathf.Exp(-4.9f * x));
        return y;
    }

    public bool isConnectedTo(Node node)
    {
        if (node.layer == layer)
        {
            return false;
        }

        if (node.layer < layer)
        {
            for (int i = 0; i < node.outputConnections.Count; i++)
            {
                if (node.outputConnections[i].toNode == node)
                {
                    return true;
                }
            }
        } else
        {
            for (int i = 0; i < outputConnections.Count; i++)
            {
                if (outputConnections[i].toNode == node)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Node clone()
    {
        Node clone = new Node(number);
        clone.layer = layer;
        return clone;
    }
    
}
