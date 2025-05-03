using GK;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Vertex = Graph<Intersection, Street>.Vertex;
using Edge = Graph<Intersection, Street>.Edge;

public enum StreetType
{
    Arterial,   // Main roads with highest capacity
    Collector,  // Medium capacity streets connecting arterials to locals
    Local       // Lowest capacity neighborhood streets
}

[System.Serializable]
public struct Intersection
{
    public Vector2 position;

    public Intersection(Vector2 _position)
    {
        position = _position;
    }
}

[System.Serializable]
public struct Street
{
    public bool bidirectional;
    public StreetType type;

    public Street(bool _bidirectional, StreetType _type = StreetType.Local)
    {
        bidirectional = _bidirectional;
        type = _type;
    }
}

/// <summary>
/// Represents a complete street network converted from a Voronoi diagram
/// </summary>
[System.Serializable]
public class StreetGraph
{
    public Graph<Intersection, Street> graph;

    [Header("Debug")]
    [SerializeField] public bool doDebug = true;
    [SerializeField] private Color intersectionColor = Color.blue;
    [SerializeField] private Color arterialColor = Color.red;
    [SerializeField] private Color collectorColor = Color.yellow;
    [SerializeField] private Color localColor = Color.white;
    [SerializeField] private float intersectionRadius = 0.5f;

    public StreetGraph(Graph<Intersection, Street> _graph)
    {
        graph = _graph;
    }

    public List<Polygon> GetPolygons()
    {
        List<Polygon> polygons = new List<Polygon>();

        // Create a copy of the graph to work with
        var workingGraph = new Graph<Intersection, Street>();
        Dictionary<Vertex, Graph<Intersection, Street>.Vertex> originalToWorking = new Dictionary<Vertex, Graph<Intersection, Street>.Vertex>();

        // Copy all vertices
        foreach (var vertex in graph.Vertices)
        {
            var newVertex = workingGraph.AddVertex(vertex.data);
            originalToWorking[vertex] = newVertex;
        }

        // Copy all edges
        foreach (var edge in graph.Edges)
        {
            workingGraph.AddEdge(
                originalToWorking[edge.Start],
                originalToWorking[edge.End],
                edge.data,
                false // Make undirected for cycle detection
            );
        }

        // List to keep track of edges we've processed
        HashSet<Edge> processedEdges = new HashSet<Edge>();

        // Start from an arbitrary edge and find cycles
        foreach (var startEdge in graph.Edges)
        {
            if (processedEdges.Contains(startEdge))
                continue;

            // Try to find a cycle starting from this edge
            var cycle = FindCycle(startEdge, processedEdges);

            if (cycle != null && cycle.Count > 2) // Ensure we have at least 3 vertices for a valid polygon
            {
                Polygon polygon = new Polygon();

                // Add vertices to the polygon
                Dictionary<Vertex, Graph<Vector3, float>.Vertex> vertexMap = new Dictionary<Vertex, Graph<Vector3, float>.Vertex>();

                foreach (var vertex in cycle)
                {
                    var pos = new Vector3(vertex.data.position.x, 0, vertex.data.position.y);
                    vertexMap[vertex] = polygon.AddVertex(pos);
                }

                // Add edges to form the closed polygon
                for (int i = 0; i < cycle.Count; i++)
                {
                    var currentVertex = cycle[i];
                    var nextVertex = cycle[(i + 1) % cycle.Count]; // Wrap around to first vertex

                    polygon.AddEdge(vertexMap[currentVertex], vertexMap[nextVertex], Vector3.Distance(vertexMap[currentVertex].data, vertexMap[nextVertex].data));
                }

                polygons.Add(polygon);
            }
        }

        return polygons;
    }

    private List<Vertex> FindCycle(Edge startEdge, HashSet<Edge> processedEdges)
    {
        var startVertex = startEdge.Start;
        var currentVertex = startEdge.End;

        List<Vertex> path = new List<Vertex> { startVertex };
        HashSet<Edge> visitedEdges = new HashSet<Edge> { startEdge };
        processedEdges.Add(startEdge);

        while (currentVertex != startVertex)
        {
            path.Add(currentVertex);

            // Find the next edge to follow
            Edge nextEdge = null;
            float bestAngle = float.MaxValue;

            foreach (var edge in currentVertex.Connections)
            {
                if (visitedEdges.Contains(edge))
                    continue;

                var otherVertex = edge.Start == currentVertex ? edge.End : edge.Start;

                // If this is the start vertex and we've visited at least 2 edges, we've found a cycle
                if (otherVertex == startVertex && path.Count > 2)
                {
                    processedEdges.Add(edge);
                    return path;
                }

                // Otherwise, find the edge with the smallest angle from the current direction
                if (path.Count > 1)
                {
                    var previousVertex = path[path.Count - 2];
                    var currentDir = (currentVertex.data.position - previousVertex.data.position).normalized;
                    var candidateDir = (otherVertex.data.position - currentVertex.data.position).normalized;

                    // Calculate the angle between directions (we want to turn as little as possible)
                    float angle = Vector2.SignedAngle(currentDir, candidateDir);

                    // Always prefer turning right (negative angle) for clockwise traversal
                    if (angle > 0) angle = 360 - angle;

                    if (angle < bestAngle)
                    {
                        bestAngle = angle;
                        nextEdge = edge;
                    }
                }
                else
                {
                    // For the first edge, any edge will do
                    nextEdge = edge;
                    break;
                }
            }

            if (nextEdge == null)
            {
                // No valid edge found, this path doesn't form a cycle
                return null;
            }

            visitedEdges.Add(nextEdge);
            processedEdges.Add(nextEdge);

            // Move to the next vertex
            currentVertex = nextEdge.Start == currentVertex ? nextEdge.End : nextEdge.Start;
        }

        return path;
    }

    public void DrawGizmos()
    {
        if (!doDebug || graph == null)
            return;

        // Draw intersections
        Gizmos.color = intersectionColor;
        foreach (var intersection in graph.Vertices)
        {
            Vector3 pos = new Vector3(intersection.data.position.x, 0, intersection.data.position.y);
            Gizmos.DrawSphere(pos, intersectionRadius);
        }

        // Draw street segments
        foreach (var segment in graph.Edges)
        {
            if (segment.data.type == StreetType.Arterial)
                Gizmos.color = arterialColor;
            else if (segment.data.type == StreetType.Collector)
                Gizmos.color = collectorColor;
            else
                Gizmos.color = localColor;

            Vector2 startPos = segment.Start.data.position;
            Vector2 endPos = segment.End.data.position;
            Gizmos.DrawLine(
                new Vector3(startPos.x, 0, startPos.y),
                new Vector3(endPos.x, 0, endPos.y)
            );
        }
    }
}