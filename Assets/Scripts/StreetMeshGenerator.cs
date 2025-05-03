using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static utility class for generating meshes from street graphs
/// </summary>
public static class StreetMeshGenerator
{
    public const float ROAD_THICKNESS = 0.5f;

    public const float TEXTURE_REPEAT_DISTANCE = 10.0f;

    /// <summary>
    /// Generate a mesh from a street graph with customizable settings
    /// </summary>
    public static Mesh GenerateMesh(
        StreetGraph streetGraph,
        float arterialWidth = 20.0f,
        float collectorWidth = 15.0f,
        float localWidth = 10.0f,
        bool generateUVs = true)
    {
        if (streetGraph == null)
        {
            Debug.LogError("No street graph provided!");
            return null;
        }

        Mesh streetMesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Get all street segments from the graph
        var segments = streetGraph.graph.Edges;
        var intersections = streetGraph.graph.Vertices;

        foreach (var segment in segments)
        {
            // Determine width based on street type
            float width;
            switch (segment.data.type)
            {
                case StreetType.Arterial:
                    width = arterialWidth;
                    break;
                case StreetType.Collector:
                    width = collectorWidth;
                    break;
                case StreetType.Local:
                default:
                    width = localWidth;
                    break;
            }

            // Add road segment to mesh (with thickness)
            AddRoadSegmentWithThickness(
                Helpers.Vec2ToXZ(segment.Start.data.position),
                Helpers.Vec2ToXZ(segment.End.data.position),
                width,
                ROAD_THICKNESS,
                vertices,
                triangles,
                uvs,
                generateUVs
            );
        }

        if (vertices.Count == 0)
        {
            Debug.LogWarning("No vertices generated for street mesh!");
            return null;
        }

        streetMesh.vertices = vertices.ToArray();
        streetMesh.triangles = triangles.ToArray();

        if (generateUVs)
        {
            streetMesh.uv = uvs.ToArray();
        }

        streetMesh.RecalculateNormals();
        streetMesh.RecalculateBounds();

        return streetMesh;
    }

    /// <summary>
    /// Adds a road segment with thickness to the mesh data
    /// </summary>
    private static void AddRoadSegmentWithThickness(
        Vector3 start,
        Vector3 end,
        float width,
        float thickness,
        List<Vector3> vertices,
        List<int> triangles,
        List<Vector2> uvs,
        bool generateUVs)
    {
        // Calculate road direction
        Vector3 direction = (end - start).normalized;

        // Calculate perpendicular direction for width
        Vector3 perpendicular = new Vector3(-direction.z, 0, direction.x);

        // Half width for each side of the road
        float halfWidth = width * 0.5f;

        // Calculate the vertices for the road segment
        // Bottom layer vertices (y = 0)
        Vector3 bottomLeft_start = start - perpendicular * halfWidth;
        Vector3 bottomRight_start = start + perpendicular * halfWidth;
        Vector3 bottomLeft_end = end - perpendicular * halfWidth;
        Vector3 bottomRight_end = end + perpendicular * halfWidth;

        // Top layer vertices (y = thickness)
        Vector3 topLeft_start = bottomLeft_start + new Vector3(0, thickness, 0);
        Vector3 topRight_start = bottomRight_start + new Vector3(0, thickness, 0);
        Vector3 topLeft_end = bottomLeft_end + new Vector3(0, thickness, 0);
        Vector3 topRight_end = bottomRight_end + new Vector3(0, thickness, 0);

        // Get current vertex count for triangle indices
        int vIndex = vertices.Count;

        // Add all vertices
        // Order matters for easier triangle creation and UV mapping
        // Bottom surface
        vertices.Add(bottomLeft_start);  // 0
        vertices.Add(bottomRight_start); // 1
        vertices.Add(bottomLeft_end);    // 2
        vertices.Add(bottomRight_end);   // 3

        // Top surface
        vertices.Add(topLeft_start);     // 4
        vertices.Add(topRight_start);    // 5
        vertices.Add(topLeft_end);       // 6
        vertices.Add(topRight_end);      // 7

        // Add triangles (2 per face, 6 faces total for the box)

        // Bottom face (ground, facing down)
        triangles.Add(vIndex + 0);
        triangles.Add(vIndex + 2);
        triangles.Add(vIndex + 1);

        triangles.Add(vIndex + 1);
        triangles.Add(vIndex + 2);
        triangles.Add(vIndex + 3);

        // Top face (facing up)
        triangles.Add(vIndex + 4);
        triangles.Add(vIndex + 5);
        triangles.Add(vIndex + 6);

        triangles.Add(vIndex + 5);
        triangles.Add(vIndex + 7);
        triangles.Add(vIndex + 6);

        // Front face (start of road segment)
        triangles.Add(vIndex + 0);
        triangles.Add(vIndex + 1);
        triangles.Add(vIndex + 4);

        triangles.Add(vIndex + 1);
        triangles.Add(vIndex + 5);
        triangles.Add(vIndex + 4);

        // Back face (end of road segment)
        triangles.Add(vIndex + 2);
        triangles.Add(vIndex + 6);
        triangles.Add(vIndex + 3);

        triangles.Add(vIndex + 3);
        triangles.Add(vIndex + 6);
        triangles.Add(vIndex + 7);

        // Left face
        triangles.Add(vIndex + 0);
        triangles.Add(vIndex + 4);
        triangles.Add(vIndex + 2);

        triangles.Add(vIndex + 2);
        triangles.Add(vIndex + 4);
        triangles.Add(vIndex + 6);

        // Right face
        triangles.Add(vIndex + 1);
        triangles.Add(vIndex + 3);
        triangles.Add(vIndex + 5);

        triangles.Add(vIndex + 3);
        triangles.Add(vIndex + 7);
        triangles.Add(vIndex + 5);

        // Add UVs if needed
        if (generateUVs)
        {
            float roadLength = Vector3.Distance(start, end);

            // Calculate UV tiling for length and width
            // For length, we want to tile every TEXTURE_REPEAT_DISTANCE units
            float lengthTiling = roadLength / TEXTURE_REPEAT_DISTANCE;

            // For width, we want to tile based on width as well
            // This ensures the texture repeats across the width rather than stretching
            float widthTiling = width / TEXTURE_REPEAT_DISTANCE;

            // Bottom face UVs - Main road surface (horizontal plane at y=0)
            uvs.Add(new Vector2(0, 0));                  // bottomLeft_start
            uvs.Add(new Vector2(widthTiling, 0));        // bottomRight_start
            uvs.Add(new Vector2(0, lengthTiling));       // bottomLeft_end
            uvs.Add(new Vector2(widthTiling, lengthTiling)); // bottomRight_end

            // Top face UVs - Roadtop facing up (horizontal plane at y=thickness)
            uvs.Add(new Vector2(0, 0));                  // topLeft_start
            uvs.Add(new Vector2(widthTiling, 0));        // topRight_start
            uvs.Add(new Vector2(0, lengthTiling));       // topLeft_end
            uvs.Add(new Vector2(widthTiling, lengthTiling)); // topRight_end
        }
    }
}