using GK;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static void ForEach<T>(this IEnumerable<T> source, System.Action<T> action)
    {
        foreach (T element in source)
        {
            action(element);
        }
    }

    public static Vector2 PointOnRect(Rect rect, float y = 0f)
    {
        return new Vector2(Random.Range(rect.xMin, rect.xMax), Random.Range(rect.yMin, rect.yMax));
    }

    /// <summary>
    /// Checks if a point is inside the bounds
    /// </summary>
    public static bool IsPointInBounds(Vector2 point, Rect bounds)
    {
        return point.x >= bounds.xMin && point.x <= bounds.xMax &&
               point.y >= bounds.yMin && point.y <= bounds.yMax;
    }

    public static void ClipDiagram(VoronoiDiagram diagram, Rect bounds)
    {
        // Create a bounds object for ray casting
        Bounds bounds3D = new Bounds(
            new Vector3((bounds.xMin + bounds.xMax) * 0.5f, (bounds.yMin + bounds.yMax) * 0.5f, 0),
            new Vector3(bounds.width, bounds.height, 0));

        // Process all edges in the diagram
        for (int i = 0; i < diagram.Edges.Count; i++)
        {
            VoronoiDiagram.Edge edge = diagram.Edges[i];

            if (edge.Type != VoronoiDiagram.EdgeType.Segment) continue;

            // Get vertex positions
            Vector2 v1 = diagram.Vertices[edge.Vert0];
            Vector2 v2 = diagram.Vertices[edge.Vert1];

            // Check if either vertex is outside the bounds
            bool v1Outside = !IsPointInBounds(v1, bounds);
            bool v2Outside = !IsPointInBounds(v2, bounds);

            // If both vertices are inside, no clipping needed
            if (!v1Outside && !v2Outside)
                continue;

            // If both vertices are outside, we need to check if the edge passes through the bounds
            if (v1Outside && v2Outside)
            {
                // Check if the edge intersects with the bounds at all
                if (DoesLineIntersectRect(v1, v2, bounds))
                {
                    // Find both intersection points
                    Vector2 intersection1 = RayToBoundaryPoint2D(v1, (v2 - v1).normalized, bounds);
                    Vector2 intersection2 = RayToBoundaryPoint2D(v2, (v1 - v2).normalized, bounds);

                    // Update vertex positions
                    diagram.Vertices[edge.Vert0] = intersection1;
                    diagram.Vertices[edge.Vert1] = intersection2;
                }
                // If the edge doesn't intersect the bounds, mark it as invalid or remove it
                // This depends on your diagram implementation
            }
            // If only one vertex is outside, move it to the boundary
            else if (v1Outside)
            {
                // Cast ray from outside vertex to inside vertex
                Vector2 direction = (v2 - v1).normalized;
                Vector2 intersection = RayToBoundaryPoint2D(v1, direction, bounds);

                // Update vertex position
                diagram.Vertices[edge.Vert0] = intersection;
            }
            else // v2Outside
            {
                // Cast ray from outside vertex to inside vertex
                Vector2 direction = (v1 - v2).normalized;
                Vector2 intersection = RayToBoundaryPoint2D(v2, direction, bounds);

                // Update vertex position
                diagram.Vertices[edge.Vert1] = intersection;
            }
        }

        Debug.Log("Clipped diagram using ray casting method.");
    }

    // Helper method to check if a line segment intersects with a rectangle
    private static bool DoesLineIntersectRect(Vector2 p1, Vector2 p2, Rect rect)
    {
        // Return true if the line formed by p1 and p2 intersects with the rectangle
        // Quick check - if either point is inside, there's an intersection
        if (IsPointInBounds(p1, rect) || IsPointInBounds(p2, rect))
            return true;

        // Check if the line intersects any of the four sides of the rectangle
        Vector2[] rectCorners = new Vector2[4]
        {
        new Vector2(rect.xMin, rect.yMin), // Bottom-left
        new Vector2(rect.xMax, rect.yMin), // Bottom-right
        new Vector2(rect.xMax, rect.yMax), // Top-right
        new Vector2(rect.xMin, rect.yMax)  // Top-left
        };

        for (int i = 0; i < 4; i++)
        {
            Vector2 r1 = rectCorners[i];
            Vector2 r2 = rectCorners[(i + 1) % 4];

            if (DoLineSegmentsIntersect(p1, p2, r1, r2))
                return true;
        }

        return false;
    }

    // Check if two line segments intersect
    private static bool DoLineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        // Calculate the directions of the line segments
        Vector2 d1 = p2 - p1;
        Vector2 d2 = p4 - p3;

        // Calculate the denominator for the parametric equation
        float denominator = d1.x * d2.y - d1.y * d2.x;

        // If denominator is zero, lines are parallel or collinear
        if (Mathf.Abs(denominator) < 0.0001f)
            return false;

        // Calculate parameters for intersection
        Vector2 d3 = p1 - p3;
        float t1 = (d2.x * d3.y - d2.y * d3.x) / denominator;
        float t2 = (d1.x * d3.y - d1.y * d3.x) / denominator;

        // Check if intersection occurs within both line segments
        return t1 >= 0 && t1 <= 1 && t2 >= 0 && t2 <= 1;
    }

    // Modified version of the RayToBoundaryPoint2D function to accept bounds as parameter
    public static Vector2 RayToBoundaryPoint2D(Vector2 origin, Vector2 direction, Rect bounds)
    {
        float minX = bounds.xMin;
        float maxX = bounds.xMax;
        float minY = bounds.yMin;
        float maxY = bounds.yMax;

        const float epsilon = 0.0001f;
        float minDistance = float.MaxValue;
        Vector2 result = origin;

        // Check all four boundaries
        float[] boundaryValues = { minX, maxX, minY, maxY };
        int[] boundaryAxes = { 0, 0, 1, 1 }; // 0 for X, 1 for Y

        for (int i = 0; i < 4; i++)
        {
            int axis = boundaryAxes[i];
            float dirValue = axis == 0 ? direction.x : direction.y;

            // Skip if direction is too small
            if (Mathf.Abs(dirValue) <= epsilon) continue;

            float originValue = axis == 0 ? origin.x : origin.y;
            float t = (boundaryValues[i] - originValue) / dirValue;

            // Skip if intersection is behind the ray
            if (t <= 0) continue;

            // Calculate intersection point
            Vector2 intersection = origin + direction * t;

            // Check if intersection is within the boundary rectangle
            bool isWithinBounds = (axis == 0) ?
                (intersection.y >= minY && intersection.y <= maxY) :
                (intersection.x >= minX && intersection.x <= maxX);

            if (isWithinBounds && t < minDistance)
            {
                minDistance = t;
                result = intersection;
            }
        }

        return result;
    }

    //TODO: Separate Clipping from Graph Building
    public static Graph<Intersection, Street> FromVoronoiDiagram(VoronoiDiagram diagram, Rect bounds)
    {
        ClipDiagram(diagram, bounds);

        Dictionary<int, Graph<Intersection, Street>.Vertex> voronoiToVertex = new();
        Dictionary<int, Graph<Intersection, Street>.Edge> voronoiToEdge = new();

        Graph<Intersection, Street> res = new();

        // Create intersections from Voronoi vertices
        for (int i = 0; i < diagram.Vertices.Count; i++)
        {
            voronoiToVertex[i] = res.AddVertex(new(diagram.Vertices[i]));
        }

        // Create street segments from Voronoi edges
        for (int i = 0; i < diagram.Edges.Count; i++)
        {
            var edge = diagram.Edges[i];

            // Line Segments
            if (edge.Type == VoronoiDiagram.EdgeType.Segment)
            {
                var start = voronoiToVertex[edge.Vert0];
                var end = voronoiToVertex[edge.Vert1];
                Street data = new Street(true);

                var street = res.AddEdge(start, end, data);

                voronoiToEdge[i] = street;
            }
            // Project rays to boundaries
            else if (edge.Type == VoronoiDiagram.EdgeType.RayCW || edge.Type == VoronoiDiagram.EdgeType.RayCCW)
            {
                var start = voronoiToVertex[edge.Vert0];
                Vector2 rayDir = edge.Direction.normalized;

                // Calculate intersection with boundary in 2D
                Vector2 endPos = Helpers.RayToBoundaryPoint2D(start.data.position, rayDir, bounds);
                var end = res.AddVertex(new(endPos));

                // Create a street segment from the ray
                var street = res.AddEdge(start, end, new(true));

                voronoiToEdge[i] = street;
            }
            else
            {
                throw new System.NotImplementedException($"StreetGraph cannot process type {edge.Type}.");
            }
        }

        return res;
    }

    public static Vector3 Vec2ToXZ(Vector2 v, float y = 0)
    {
        return new Vector3(v.x, y, v.y);
    }
}
