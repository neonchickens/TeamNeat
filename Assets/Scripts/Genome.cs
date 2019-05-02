using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Genome
{
    public List<ConnectionGene> genes;
    public List<Node> nodes;
    public List<Node> network;

    public int inputs, outputs, layers = 2, nextNode = 0, biasNode;

    public Genome(int input, int output)
    {
        genes = new List<ConnectionGene>();
        nodes = new List<Node>();
        network = new List<Node>();

        this.inputs = input;
        this.outputs = output;

        for (int i = 0; i < inputs; i++)
        {
            nodes.Add(new Node(i));
            nextNode++;
            nodes[i].layer = 0;
        }

        for (int i = 0; i < outputs; i++)
        {
            nodes.Add(new Node(i + inputs));
            nodes[i + inputs].layer = 1;
            nextNode++;
        }

        nodes.Add(new Node(nextNode));
        biasNode = nextNode;
        nextNode++;
        nodes[biasNode].layer = 0;
    }

    public Genome(int input, int output, bool crossover)
    {
        inputs = input;
        outputs = output;

        genes = new List<ConnectionGene>();
        nodes = new List<Node>();
    }
    
    Node GetNode(int nodeNumber)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].number == nodeNumber)
            {
                return nodes[i];
            }
        }
        return null;
    }

    void ConnectNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].outputConnections.Clear();
        }

        for (int i = 0; i < genes.Count; i++)
        {
            genes[i].fromNode.outputConnections.Add(genes[i]);
        }
    }

    public float[] FeedForward(float[] inputValues)
    {
        for (int i = 0; i < inputs; i++)
        {
            nodes[i].outputVal = inputValues[i];
        }
        nodes[biasNode].outputVal = 1;

        for (int i = 0; i < network.Count; i++)
        {
            network[i].engage();
        }

        float[] outs = new float[outputs];
        for (int i = 0; i < outputs; i++)
        {
            outs[i] = nodes[inputs + i].outputVal;
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].inputSum = 0;
        }

        return  outs;
    }

    public void GenerateNetwork()
    {
        ConnectNodes();
        network = new List<Node>();

        for (int l = 0; l < layers; l++)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].layer == l)
                {
                    network.Add(nodes[i]);
                }
            }
        }
    }

    void AddNode(List<ConnectionHistory> innovationHistory)
    {


        if (genes.Count == 0)
        {
            AddConnection(innovationHistory);
            return;
        }
        int randomConnection = Random.Range(0, genes.Count);

        while (genes[randomConnection].fromNode == nodes[biasNode] && genes.Count != 1)
        {
            randomConnection = Random.Range(0, genes.Count);
        }

        genes[randomConnection].enabled = false;

        int newNodeNo = nextNode;
        nodes.Add(new Node(newNodeNo));
        nextNode++;

        int connectionInnovationNumber = GetInnovationNumber(innovationHistory, genes[randomConnection].fromNode, GetNode(newNodeNo));
        genes.Add(new ConnectionGene(genes[randomConnection].fromNode, GetNode(newNodeNo), 1, connectionInnovationNumber));

        connectionInnovationNumber = GetInnovationNumber(innovationHistory, GetNode(newNodeNo), genes[randomConnection].toNode);
        genes.Add(new ConnectionGene(GetNode(newNodeNo), genes[randomConnection].toNode, genes[randomConnection].weight, connectionInnovationNumber));
        GetNode(newNodeNo).layer = genes[randomConnection].fromNode.layer + 1;

        connectionInnovationNumber = GetInnovationNumber(innovationHistory, nodes[biasNode], GetNode(newNodeNo));
        genes.Add(new ConnectionGene(nodes[biasNode], GetNode(newNodeNo), 0, connectionInnovationNumber));

        if (GetNode(newNodeNo).layer == genes[randomConnection].toNode.layer)
        {
            for (int i = 0; i < nodes.Count - 1; i++)
            {
                if (nodes[i].layer >= GetNode(newNodeNo).layer)
                {
                    nodes[i].layer++;
                }
            }
            layers++;
        }
        ConnectNodes();
        //Debug.Log("added node");
    }

    void AddConnection(List<ConnectionHistory> innovationHistory)
    {
        if (FullyConnected())
        {
            return;
        }

        int rNode1 = Random.Range(0, nodes.Count);
        int rNode2 = Random.Range(0, nodes.Count);

        while (nodes[rNode1].layer == nodes[rNode2].layer || nodes[rNode1].isConnectedTo(nodes[rNode2]))
        {
            //reroll
            rNode1 = Random.Range(0, nodes.Count);
            rNode2 = Random.Range(0, nodes.Count);
        }
        
        if (nodes[rNode1].layer > nodes[rNode2].layer)
        {
            int temp = rNode1;
            rNode1 = rNode2;
            rNode2 = temp;
        }

        int connectionInnovationNumber = GetInnovationNumber(innovationHistory, nodes[rNode1], nodes[rNode2]);

        genes.Add(new ConnectionGene(nodes[rNode1], nodes[rNode2], Random.Range(-1f, 1f), connectionInnovationNumber));
        ConnectNodes();
        //Debug.Log("added connection");
    }

    void RemoveConnection(List<ConnectionHistory> innovationHistory)
    {
        if (genes.Count == 0)
        {
            AddConnection(innovationHistory);
            return;
        }
        
        int r = Random.Range(0, genes.Count);
        genes.RemoveAt(r);
        ConnectNodes();
        //Debug.Log("removed connection");
    }

    int GetInnovationNumber(List<ConnectionHistory> innovationHistory, Node from, Node to)
    {
        bool isNew = true;
        int connectionInnovationNumber = ConnectionHistory.nextConnectionNo;
        for (int i = 0; i < innovationHistory.Count; i++)
        {
            if (innovationHistory[i].matches(this, from, to))
            {
                isNew = false;
                connectionInnovationNumber = innovationHistory[i].innovationNumber;
                break;
            }
        }

        if (isNew)
        {
            List<int> innoNumbers = new List<int>();
            for (int i = 0; i < genes.Count; i++)
            {
                innoNumbers.Add(genes[i].innovationNo);
            }

            innovationHistory.Add(new ConnectionHistory(from.number, to.number, connectionInnovationNumber, innoNumbers));
            ConnectionHistory.nextConnectionNo++;
        }
        return connectionInnovationNumber;
    }

    bool FullyConnected()
    {
        int maxConnections = 0;
        int[] nodesInLayers = new int[layers];

        for (int i = 0; i < nodes.Count; i++)
        {
            nodesInLayers[nodes[i].layer] += 1;
        }

        for (int i = 0; i < layers - 1; i++)
        {
            int nodesInFront = 0;
            for (int j = i + 1; j < layers; j++)
            {
                nodesInFront += nodesInLayers[j];
            }
            maxConnections += nodesInLayers[i] * nodesInFront;
        }

        if (maxConnections == genes.Count)
        {
            return true;
        }
        return false;
    }

    public void Mutate(List<ConnectionHistory> innovationHistory)
    {
        if (genes.Count == 0)
        {
            AddConnection(innovationHistory);
        }

        float r1 = Random.value;
        if (r1 < .8)
        {
            for (int i = 0; i < genes.Count; i++)
            {
                genes[i].mutateWeight();
                //Debug.Log("mutated weight");
            }
        }

        float r2 = Random.value;
        if (r2 < 0.2)
        {
            AddConnection(innovationHistory);
        }


        float r3 = Random.value;
        if (r3 < 0.1)
        {
            AddNode(innovationHistory);
        }

        float r4 = Random.value;
        if (r2 < 0.2)
        {
            RemoveConnection(innovationHistory);
        }
    }

    public Genome Crossover(Genome parent2)
    {
        Genome child = new Genome(inputs, outputs, true);
        child.genes.Clear();
        child.nodes.Clear();
        child.layers = layers;
        child.nextNode = nextNode;
        child.biasNode = biasNode;
        List<ConnectionGene> childGenes = new List<ConnectionGene>();
        List<bool> isEnabled = new List<bool>();

        for (int i = 0; i < genes.Count; i++)
        {
            bool setEnabled = true;

            int parent2gene = MatchingGene(parent2, genes[i].innovationNo);
            if (parent2gene != -1)
            {
                if (!genes[i].enabled || !parent2.genes[parent2gene].enabled)
                {
                    if (Random.value < 0.75)
                    {
                        setEnabled = false;
                    }
                }

                float r1 = Random.value;
                if (r1 < 0.5)
                {
                    childGenes.Add(genes[i]);
                } else
                {
                    childGenes.Add(parent2.genes[parent2gene]);
                }
            }
            else
            {
                childGenes.Add(genes[i]);
                setEnabled = genes[i].enabled;
            }
            isEnabled.Add(setEnabled);
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            child.nodes.Add(nodes[i].clone());
        }

        for (int i = 0; i < childGenes.Count; i++)
        {
            child.genes.Add(childGenes[i].clone(child.GetNode(childGenes[i].fromNode.number), child.GetNode(childGenes[i].toNode.number)));
            child.genes[i].enabled = isEnabled[i];
        }

        child.ConnectNodes();
        return child;
    }

    int MatchingGene(Genome parent2, int innovationNumber)
    {
        for (int i = 0; i < parent2.genes.Count; i++)
        {
            if (parent2.genes[i].innovationNo == innovationNumber)
            {
                return i;
            }
        }
        return -1;
    }

    public Genome clone()
    {
        Genome clone = new Genome(inputs, outputs, true);

        for (int i = 0; i < nodes.Count; i++)
        {
            clone.nodes.Add(nodes[i].clone());
        }

        for (int i = 0; i < genes.Count; i++)
        {
            clone.genes.Add(genes[i].clone(clone.GetNode(genes[i].fromNode.number), clone.GetNode(genes[i].toNode.number)));
        }

        clone.layers = layers;
        clone.nextNode = nextNode;
        clone.biasNode = biasNode;
        clone.ConnectNodes();

        return clone;
    }

    void PrintGenome()
    {

    }

    public void DrawGenome(GameObject panel, GameObject node)
    {
        Dictionary<Node, Vector3> dicNodes = new Dictionary<Node, Vector3>();

        int xstep = (int)(panel.GetComponent<RectTransform>().rect.size.x / (layers + 2));
        int x = xstep;
        for (int l = 0; l < layers; l++)
        {
            List<Node> lstNodesInLayer = nodes.FindAll(
                delegate (Node n)
                {
                    return n.layer == l;
                });

            int ystep = (int)(panel.GetComponent<RectTransform>().rect.size.y / (lstNodesInLayer.Count + 2));
            for (int y = ystep, index = 0; index < lstNodesInLayer.Count; y += ystep, index++)
            {
                GameObject goNode = GameObject.Instantiate(node, panel.transform);
                goNode.transform.localPosition = new Vector3(x, -y, 0);
                dicNodes.Add(lstNodesInLayer[index], new Vector3(x, -y, 0));
            }

            x += xstep;
        }

        genes.ForEach(delegate (ConnectionGene gene)
        {
            GameObject go = new GameObject("Connection");
            go.transform.parent = panel.transform;
            go.transform.localPosition = new Vector3();
            go.transform.localScale = new Vector3(1, 1, 1);
            go.transform.localRotation = Quaternion.identity;
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = .1f;
            lr.endWidth = .1f;
            lr.useWorldSpace = false;
            Vector3 v3From = dicNodes[gene.fromNode];
            v3From.z = lr.transform.position.z - 1;
            Vector3 v3To = dicNodes[gene.toNode];
            v3To.z = lr.transform.position.z + 1;
            lr.positionCount = 2;
            lr.SetPosition(0, v3From);
            lr.SetPosition(1, v3To);

        });
    }
}
