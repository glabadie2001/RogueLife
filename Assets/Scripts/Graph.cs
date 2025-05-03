using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Graph<V, E>
{
    [SerializeField] private List<Vertex> vertices = new();
    [SerializeField] private List<Edge> edges = new();

    // Counter for generating unique vertex IDs
    private int nextVertexId = 0;

    public List<Vertex> Vertices => vertices;
    public List<Edge> Edges => edges;

    public Vertex AddVertex(V vertexData)
    {
        Vertex res = new Vertex(nextVertexId++, vertexData);
        vertices.Add(res);
        return res;
    }

    public Edge AddEdge(Vertex start, Vertex end, E data, bool directed = false)
    {
        Edge res = new Edge(start, end, data, directed);

        edges.Add(res);
        start.AddConnection(res);
        end.AddConnection(res);

        return res;
    }

    [System.Serializable]
    public class Vertex : IComparable<Vertex>
    {
        [SerializeField] private List<Edge> connections;

        // Unique ID for this vertex in the graph
        public int Id { get; private set; }
        public V data;

        public List<Edge> Connections { get { return connections; } }

        public Vertex()
        {
            connections = new();
        }

        public Vertex(int id, V data) : this()
        {
            Id = id;
            this.data = data;
        }

        public void AddConnection(Edge connection)
        {
            connections.Add(connection);
        }

        public List<Vertex> GetNeighbors()
        {
            List<Vertex> neighbors = new();
            foreach (Edge edge in connections)
            {
                if (edge.Start == this)
                    neighbors.Add(edge.End);
                else
                    neighbors.Add(edge.Start);
            }
            return neighbors;
        }

        // Implement IComparable based on ID
        public int CompareTo(Vertex other)
        {
            return Id.CompareTo(other.Id);
        }

        // Override Equals and GetHashCode for proper comparison
        public override bool Equals(object obj)
        {
            if (obj is Vertex other)
            {
                return Id == other.Id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    [System.Serializable]
    public class Edge
    {
        [SerializeReference] private Vertex start;
        [SerializeReference] private Vertex end;
        [SerializeField] bool directed;

        public E data;

        public Vertex Start => start;
        public Vertex End => end;

        public Edge(Vertex _start, Vertex _end, E _data, bool _directed = false)
        {
            start = _start;
            end = _end;
            data = _data;
            directed = _directed;
        }
    }
}

[System.Serializable]
public class Polygon : Graph<Vector3, float>
{
    public Polygon[] Subdivide()
    {
        Polygon[] res = new Polygon[2];

        return res;
    }
}