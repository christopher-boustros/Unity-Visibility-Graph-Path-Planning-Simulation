/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the ReducedVisibilityGraph game object
using System.Collections.Generic;
using UnityEngine;

public class ReducedVisibilityGraphGenerator : MonoBehaviour
{
    public GameObject vertexObject; // The game object that represents a vertex

    public const float VERTEX_Y = LevelPlatform.Y + LevelPlatform.OBSTACLE_BLOCK_LENGTH / 2; // The Unity y-position of an agent
    public static List<Vector3> vertices = new List<Vector3>(); // The list of vertices in the graph
    public static List<Vector3[]> edges = new List<Vector3[]>(); // The list of edges in the graph

    // Awake is called before any other script's Start() method
    // This Awake method is set to execute after the ObstacleGenerator.cs Awake method in the project settings
    void Awake()
    {
        VerticesInit(); // Initialize the vertices list
        DrawVertices(); // Draw all the vertices

        EdgesInit(); // Initialize the edges list
        DrawEdges(); // Draw all the edges
    }

    /*
     * This method finds all reflex vertices from LevelPlatform.grid and adds them to the list of vertices.
     * The grid coordinates are converted to Unity positions.
     * 
     * The reflex vertices are always added one grid unit away from an obstacle block or alcove vertex so that when the edges
     * of the reduced visibility graph are created, they do not scrape against the obstacles or the exterior. This is the reason
     * why the ObstacleGenerator class generates invisible obstacle blocks at the same position of each obstacle block which are 
     * larger in size. These larger invisible obstacle blocks fill in the gaps between the obstacle blocks and the vertices so that
     * the raycasting that is done to initialize the edges list detects an obstacle between those gaps.
     */
    private void VerticesInit()
    {

        // A helper method that converts (x, z) grid coordinates to a Vector3 Unity position and adds it to the list of vertices
        void AddCoordinatesToList(int x, int z)
        {
            int[] coordinates = LevelPlatform.ConvertCoordinatesToPosition(new int[] { x, z }); // Convert the coordinates to a Unity (x, z) position
            Vector3 vertex = new Vector3(coordinates[0], VERTEX_Y, coordinates[1]); // Create the corresponding Vector3
            vertices.Add(vertex); // Add the vertex to the list of vertices
        }

        // For each cell of the grid
        for (int x = 0; x < LevelPlatform.grid.GetLength(0); x++)
        {
            for (int z = 0; z < LevelPlatform.grid.GetLength(1); z++)
            {
                LevelPlatform.CoordinateType coordinateType = LevelPlatform.grid[x, z];
                if (coordinateType == LevelPlatform.CoordinateType.ALCOVE_VERTEX_1 || coordinateType == LevelPlatform.CoordinateType.ALCOVE_VERTEX_2)
                { // If the cell is an alcove vertex
                    int[] pos = LevelPlatform.ConvertCoordinatesToPosition(new int[] { x, z }); // Get the Unity position of the coordinate
                    int vertexX, vertexZ; // The x and z grid coordinates of the vertex to add to the list

                    LevelPlatform.CoordinateType type1 = LevelPlatform.CoordinateType.ALCOVE_VERTEX_1;

                    // Move the vertex in one unit towards the interior of the MainFloor and one unit towards the interior
                    // of the alcove (so that it is not exactly on the edge of the MainFloor)
                    if (pos[0] == LevelPlatform.MAIN_FLOOR_MIN_X)
                    { // The vertex is for a left-side alcove
                        if (coordinateType == type1)
                        {
                            vertexX = x + 1;
                            vertexZ = z + 1;
                        }
                        else
                        {
                            vertexX = x + 1;
                            vertexZ = z - 1;
                        }
                    }
                    else if (pos[0] == LevelPlatform.MAIN_FLOOR_MAX_X)
                    { // The vertex is for a right-side alcove
                        if (coordinateType == type1)
                        {
                            vertexX = x - 1;
                            vertexZ = z + 1;
                        }
                        else
                        {
                            vertexX = x - 1;
                            vertexZ = z - 1;
                        }
                    }
                    else if (pos[1] == LevelPlatform.MAIN_FLOOR_MIN_Z)
                    { // The vertex is for a bottom-side alcove
                        if (coordinateType == type1)
                        {
                            vertexX = x + 1;
                            vertexZ = z + 1;
                        }
                        else
                        {
                            vertexX = x - 1;
                            vertexZ = z + 1;
                        }
                    }
                    else
                    { // The vertex is for a top-side alcove
                        if (coordinateType == type1)
                        {
                            vertexX = x + 1;
                            vertexZ = z - 1;
                        }
                        else
                        {
                            vertexX = x - 1;
                            vertexZ = z - 1;
                        }
                    }

                    AddCoordinatesToList(vertexX, vertexZ); // Add the vertex
                }
                else if (LevelPlatform.grid[x, z] == LevelPlatform.CoordinateType.BLOCK_CORNER_VERTEX)
                { // If the cell is a corner block on an L-shaped obstacle
                    bool blockToTheRight = LevelPlatform.grid[x + 1, z] == LevelPlatform.CoordinateType.BLOCK || LevelPlatform.grid[x + 1, z] == LevelPlatform.CoordinateType.BLOCK_VERTEX; // True if there is a block to the right of the corner block
                    bool blockAbove = LevelPlatform.grid[x, z + 1] == LevelPlatform.CoordinateType.BLOCK || LevelPlatform.grid[x, z + 1] == LevelPlatform.CoordinateType.BLOCK_VERTEX; // True if there is a block above the corner block

                    int vertexX, vertexZ; // The x and z grid coordinates of the vertex to add to the list

                    if (blockToTheRight)
                    { // There is a block to the right
                        vertexX = x - 1;
                    }
                    else
                    { // There is a block to the left
                        vertexX = x + 1;
                    }

                    if (blockAbove)
                    { // There is a block above
                        vertexZ = z - 1;
                    }
                    else
                    { // There is a block below
                        vertexZ = z + 1;
                    }

                    AddCoordinatesToList(vertexX, vertexZ); // Add the vertex just outside of the corner block
                }
                else if (LevelPlatform.grid[x, z] == LevelPlatform.CoordinateType.BLOCK_VERTEX)
                { // If the cell is one of the two vertex blocks of the L-shaped obstacle (not the corner block)
                    bool blockToTheRight = LevelPlatform.grid[x + 1, z] == LevelPlatform.CoordinateType.BLOCK || LevelPlatform.grid[x + 1, z] == LevelPlatform.CoordinateType.BLOCK_CORNER_VERTEX; // True if there is a block to the right of the vertex block
                    bool blockToTheLeft = LevelPlatform.grid[x - 1, z] == LevelPlatform.CoordinateType.BLOCK || LevelPlatform.grid[x - 1, z] == LevelPlatform.CoordinateType.BLOCK_CORNER_VERTEX; // True if there is a block to the left of the vertex block
                    bool blockAbove = LevelPlatform.grid[x, z + 1] == LevelPlatform.CoordinateType.BLOCK || LevelPlatform.grid[x, z + 1] == LevelPlatform.CoordinateType.BLOCK_CORNER_VERTEX; // True if there is a block above the vertex block
                    bool blockBelow = LevelPlatform.grid[x, z - 1] == LevelPlatform.CoordinateType.BLOCK || LevelPlatform.grid[x, z - 1] == LevelPlatform.CoordinateType.BLOCK_CORNER_VERTEX; // True if there is a block below the vertex block

                    // Two vertices need to be added to the list: one for each outer vertex of the block
                    int vertex1X, vertex1Z; // The x and z grid coordinates of the first vertex to add to the list
                    int vertex2X, vertex2Z; // The x and z grid coordinates of the second vertex to add to the list

                    if (blockToTheLeft || blockToTheRight)
                    { // There is a block either to the left or to the right
                        vertex1Z = z + 1;
                        vertex2Z = z - 1;

                        if (blockToTheRight)
                        { // The block is to the right
                            vertex1X = x - 1;
                            vertex2X = x - 1;
                        }
                        else
                        { // The block is to the left
                            vertex1X = x + 1;
                            vertex2X = x + 1;
                        }
                    }
                    else
                    { // There is a block either above or below
                        vertex1X = x + 1;
                        vertex2X = x - 1;

                        if (blockAbove)
                        { // The block is above
                            vertex1Z = z - 1;
                            vertex2Z = z - 1;
                        }
                        else
                        { // The block is below
                            vertex1Z = z + 1;
                            vertex2Z = z + 1;
                        }
                    }

                    // Add the two vertices on each outer vertex of the block to the list
                    AddCoordinatesToList(vertex1X, vertex1Z);
                    AddCoordinatesToList(vertex2X, vertex2Z);

                }
            }
        }
    }

    /*
     * This method initializes the edges list. It adds all edges that should be in the reduced visibility graph.
     * Edges that are added to the list are bitangent edges.
     * A bitangent edge is an edge between two reflex vertices (in the vertices list) that does not intersect an any object or the exterior, even when it is sligthly extended on either end
     * Raycasts are used to detect intersection between edges and objects.
     */
    private void EdgesInit()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = i + 1; j < vertices.Count; j++)
            {
                Vector3[] edge = new Vector3[] { vertices[i], vertices[j] }; // The current edge

                // If the edge is bitangent, it needs to be added to the graph
                if (IsEdgeBitangent(edge, 1.8f * LevelPlatform.OBSTACLE_BLOCK_LENGTH))
                {
                    edges.Add(edge);
                }
            }
        }
    }

    /*
     * Determine if the edge is bitangent, meaning it can be added to the reduced visibility graph
     * A bitangent edge is an edge that does not intestect with an obstacle block or the exterior,
     * even when extended a little from either side.
     * The parameter extension is the distance by which to extend the edge on either side when
     * checking for intersection with an obstacle or the exterior
     */
    public static bool IsEdgeBitangent(Vector3[] edge, float extension)
    {
        Vector3 direction = (edge[1] - edge[0]).normalized; // The direction from the first vertex to the second vertex of the edge
        float distance = (edge[1] - edge[0]).magnitude + extension; // The distance between the two vertices, with a slight extension

        // The reason we need to cast a ray in both directions is that we extend the ray slightly beyond the destination vertex
        // So, we cast from vertex1 to vertex2 to extend slightly beyond vertex2, and from vertex2 to vertex1 to extend slightly beyond vertex1
        RaycastHit[] hits1 = Physics.RaycastAll(edge[0], direction, distance); // Cast a ray from the first vertex (edge[0]) to the second vertex (edge[1]) and store the array of all objects hit
        RaycastHit[] hits2 = Physics.RaycastAll(edge[1], -direction, distance); // Cast a ray from the second vertex to the first vertex and store the array of all objects hit 

        // Check the array hits1 to see if an obstacle block (including an invisible obstacle block) or the exterior was hit
        foreach (RaycastHit hit in hits1)
        {
            if (hit.transform.name.Contains("Obstacle Block") || hit.transform.name.Contains("Exterior Cube"))
            { // An obstacle block or the exterior was hit
                return false; // Not bitangent
            }
        }

        // Check the array hits2 to see if an obstacle block was hit
        foreach (RaycastHit hit in hits2)
        {
            if (hit.transform.name.Contains("Obstacle Block") || hit.transform.name.Contains("Exterior Cube"))
            { // An obstacle block or the exterior was hit
                return false; // Not bitangent
            }
        }

        return true; // Bitangent because no obstacle block or the exterior was hit by a ray
    }

    // Draw the vertices in the list of vertices as game objects so that they are visible in the game
    private void DrawVertices()
    {
        // Create a child of the ReducedVisibilityGraph game object to store the vertex game objects
        GameObject child = new GameObject("Vertices");
        child.transform.parent = gameObject.transform;

        // For each vertex position in the list of vertices
        foreach (Vector3 vertexPosition in vertices)
        {
            // Create the game object for the vertex to instantiate
            GameObject obj = Instantiate(vertexObject, vertexPosition, transform.rotation) as GameObject; // Instatiate the vertex object
            obj.name = "Vertex " + vertexPosition.x + "," + vertexPosition.z;
            obj.transform.parent = child.transform; // Make the obstacle block a child of the child created above
        }
    }

    // Draw the edges in the list of edges using line renderers so that they are visible in the game
    private void DrawEdges()
    {
        // Create a child of the ReducedVisibilityGraph game object to store the lines renderers for the edges
        GameObject child = new GameObject("Edges");
        child.transform.parent = gameObject.transform;

        foreach (Vector3[] edge in edges)
        {
            // Instantiate the line renderer
            LineRenderer lineRenderer = new GameObject("Edge " + edge[0].x + "," + edge[0].z + " to " + edge[1].x + "," + edge[1].z).AddComponent<LineRenderer>();
            lineRenderer.startWidth = 1f;
            lineRenderer.endWidth = 1f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.transform.parent = child.transform; // Make the line renderer a child of the child created above

            // Create the line
            lineRenderer.SetPositions(edge);
        }
    }
}
