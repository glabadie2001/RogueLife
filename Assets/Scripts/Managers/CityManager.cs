using GK;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class CityManager : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] Rect cityBounds;
    [SerializeField] float seaLevel = 0;
    [SerializeField] int seedCount = 50;
    [SerializeField] float cullDst = 50f;

    [Header("Street Settings")]
    [SerializeField] float artertyWidth = 25f;
    [SerializeField] float collectorWidth = 15f;
    [SerializeField] float localWidth = 10f;

    [Header("Pieces")]
    [SerializeField] GameObject street;

    [Header("Results")]
    [SerializeField] List<Neighborhood> city;
    [SerializeField] StreetGraph streetGraph;
    [SerializeField] VoronoiDiagram cityDiagram;
    [SerializeField] List<Polygon> regions;

    private void Start()
    {
        GenerateCity();
    }

    [Button("Regen City")]
    void GenerateCity()
    {
        DestroyCity();

        List<Vector2> seedsList = new List<Vector2>();
        for (int i = 0; i < seedCount; i++)
            seedsList.Add(Helpers.PointOnRect(cityBounds, seaLevel));

        seedsList = CullCloseSeeds(seedsList, cullDst);

        VoronoiCalculator v = new VoronoiCalculator();
        v.CalculateDiagram(seedsList, ref cityDiagram);

        streetGraph = new StreetGraph(Helpers.FromVoronoiDiagram(cityDiagram, cityBounds));
        regions = streetGraph.GetPolygons();

        // Generate the mesh
        GameObject streetObj = Instantiate(street, transform);
        Mesh streetMesh = StreetMeshGenerator.GenerateMesh(streetGraph, arterialWidth: artertyWidth, collectorWidth: collectorWidth, localWidth: localWidth);
        streetObj.GetComponent<MeshFilter>().mesh = streetMesh;
        streetObj.GetComponent<MeshCollider>().sharedMesh = streetMesh;
    }

    void DestroyCity()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
    }

    List<Vector2> CullCloseSeeds(List<Vector2> seeds, float minDistance)
    {
        List<Vector2> culledSeeds = new List<Vector2>();
        float sqrMinDistance = minDistance * minDistance;

        foreach (Vector2 seed in seeds)
        {
            bool tooClose = false;

            foreach (Vector2 acceptedSeed in culledSeeds)
            {
                if ((seed - acceptedSeed).sqrMagnitude < sqrMinDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                culledSeeds.Add(seed);
            }
        }

        Debug.Log($"Culled {seeds.Count - culledSeeds.Count} seeds. {culledSeeds.Count} seeds remaining.");
        return culledSeeds;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(cityBounds.center.x, seaLevel, cityBounds.center.y), new Vector3(cityBounds.size.x, 0, cityBounds.size.y));
    }

    private void OnDrawGizmosSelected()
    {
        if (regions != null)
        {
            Gizmos.color = Color.magenta;
            foreach (Polygon region in regions)
            {
                foreach (var edge in region.Edges)
                {
                    Gizmos.DrawLine(edge.Start.data, edge.End.data);
                }
            }
        }

        if (cityDiagram != null)
        {
            foreach (var edge in cityDiagram.Edges)
            {
                switch (edge.Type)
                {
                    case VoronoiDiagram.EdgeType.Segment:
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(Helpers.Vec2ToXZ(cityDiagram.Vertices[edge.Vert0]), Helpers.Vec2ToXZ(cityDiagram.Vertices[edge.Vert1]));
                        break;

                    case VoronoiDiagram.EdgeType.RayCW:
                        Gizmos.color = Color.red;
                        Gizmos.DrawRay(Helpers.Vec2ToXZ(cityDiagram.Vertices[edge.Vert0]), Helpers.Vec2ToXZ(edge.Direction));
                        break;

                    case VoronoiDiagram.EdgeType.RayCCW:
                        Gizmos.color = Color.blue;
                        Gizmos.DrawRay(Helpers.Vec2ToXZ(cityDiagram.Vertices[edge.Vert0]), Helpers.Vec2ToXZ(edge.Direction));
                        break;

                    default:
                        Debug.Log(edge.Type);
                        continue;
                }
            }

            foreach (var site in cityDiagram.Sites)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(Helpers.Vec2ToXZ(site), 10f);
            }
        }

        if (streetGraph != null && streetGraph.doDebug)
            streetGraph.DrawGizmos();
    }
}