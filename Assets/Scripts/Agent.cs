/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Agent gameobject prefab
using System.Collections.Generic;
using UnityEngine;

/*
 * This class defines an agent's personal reduced visibility graph (with its start and end positions)
 * and handles the agent's motion. The agent repeatedly moves to random destination points.
 * The agent's motion is composed of edges on its visibility graph and is determined by performing
 * the A* algorithm to find a shortest path from its start vertex to its destination vertex.
 */
public class Agent : MonoBehaviour
{
    public GameObject destinationNodePrefab; // The game object prefab that represents the agent's destination, used to instantiate a destination node 

    private GameObject destinationNode; // The actual destination node that has been instantiated
    private Material lineRendererMaterial; // The material used for line renderers
    private static System.Random random = new System.Random(); // Create an instance of the Random class
    private List<int[]> availableGridCoordinates = new List<int[]>(); // A list of grid coordinates available for a destination node to be placed

    private Vector3 initialStartPosition; // The position where the agent was initially spawned
    private Color agentColor; // The color of the agent
    private int agentId; // The number that identifies the agent
    private Vector3 currentStartVertex; // The agent's current start position
    private Vector3 currentDestinationVertex; // The agent's current destination position
    private int currentStartVertexIndex; // The index of the currentStartVertex in the vertices list
    private int currentDestinationVertexIndex; // The index of the currentDestinationVertex in the vertices list
    private List<int> shortestPathToDestination; // The shortest path from the start to destination vertex computed using the A* algorithm, as a list of vertex indices
    private int currentIndexOfPath; // The index of the shortestPathToDestination list that represents the vertex that the agent is currently moving to

    // The vertices and edges of the agent's personal reduced visibiliy graph
    private List<Vector3> vertices;
    private List<Vector3[]> edges;
    private int initialVerticesCount; // The number of elements in verticies when it was first initialized
    private int initialEdgesCount; // The number of elements in edges when it was first initialized

    private enum AgentState
    {
        MOVING_TO_DESTINATION,
        CHOOSE_NEW_DESTINATION,
        REPLAN_PATH,
        WAITING_TO_CHOOSE_NEW_DESTINATION,
        WAITING_TO_REPLAN_PATH
    }
    private AgentState agentState;
    private float agentWaitTime = 0.50f; // The amount of seconds an agent should wait before choosing a new destination

    private const float DISTANCE_UNIT = 2f; // The unit of distance by which the agent will move every GameTime.INVTERVAL seconds. Increasing this will increase the speed of the agent.

    private int replanningCounter = 0; // The amount of consecutive times the agent has tried to replan its path to avoid a collision
    private const int MAX_NUMBER_OF_REPLANNINGS = 3; // The maximum number of replannings the agent will do before choosing a new destination

    // Start is called before the first frame update
    void Start()
    {
        agentState = AgentState.CHOOSE_NEW_DESTINATION;
        gameObject.GetComponent<Renderer>().material.color = agentColor; // Set the color of the agent
        currentDestinationVertex = initialStartPosition;
        ReducedVisibilityGraphInit(); // Initialize the list of vertices and edges

        // The current start and destination vertices will always be the last two vertices in the vertices list
        currentStartVertexIndex = vertices.Count;
        currentDestinationVertexIndex = vertices.Count + 1;

        lineRendererMaterial = new Material(Shader.Find("Sprites/Default")); // Create the line renderer material

        PickRandomAvailableGridCoordinates(); // Skip the first available grid coordinate that is picked because it will always be just next to the agent

        /*
         * The following variables are initialized by the the AgentGenerator class:
         * agentColor
         * initialStartPosition
         * agentId
         * availableGridCoordinates
         */
    }

    // Update is called once per frame
    private void Update()
    {
        if (agentState == AgentState.CHOOSE_NEW_DESTINATION)
        { // If the agent needs to pick a new destination

            currentStartVertex = currentDestinationVertex; // The previous destination vertex becomes the current start vertex
            currentDestinationVertex = PickRandomAvailableGridCoordinates(); // Picks a random destination veretx that is available for an agent to go to

            transform.position = currentStartVertex; // Make sure the agent is actually at its currentStartVertex

            DestroyEdgesOfPath(); // Destroy the lines of the path the agent was previously moving through
            ResetReducedVisibilityGraph(); // Reset the list of edges and vertices

            // Add the start and destination vertices to the graph
            vertices.Add(currentStartVertex);
            vertices.Add(currentDestinationVertex);

            // Add bitangent edges from the start and destination verticies to every other vertex
            for (int i = vertices.Count - 2; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices.Count; j++)
                {
                    if (j == i || j == vertices.Count - 2 && i == vertices.Count - 1)
                    {
                        continue;
                    }

                    Vector3[] edge = new Vector3[] { vertices[i], vertices[j] }; // The current edge

                    // If the edge is bitangent, it needs to be added to the graph
                    if (ReducedVisibilityGraphGenerator.IsEdgeBitangent(edge, LevelPlatform.OBSTACLE_BLOCK_LENGTH))
                    {
                        edges.Add(edge);
                    }
                }
            }

            // Find the shortest path from the currentStartVertex to the currentDestinationVertex using the A* algorithm
            shortestPathToDestination = AStarAlgorithm.AStar(currentStartVertexIndex, currentDestinationVertexIndex, vertices, edges);

            // If a path to the destination was not found
            if (shortestPathToDestination == null)
            {
                return; // pick another destination node the next time the Update method is called
            }

            // Destroy and re-instantiate the destination node
            Destroy(destinationNode);
            destinationNode = InstantiateDestinationNode(currentDestinationVertex);

            // Indicate that the agent is now moving towards the new destination
            agentState = AgentState.MOVING_TO_DESTINATION;
            currentIndexOfPath = 1; // The agent is now moving to the second vertex of the shortestPathToDestination list (because it's already at the first)

            // Draw all edges of the agent's path to its current destination
            DrawEdgesOfPath();
        }
        else if (agentState == AgentState.REPLAN_PATH)
        {
            // If the agent has exceeded its maximum number of replannings
            if (replanningCounter >= MAX_NUMBER_OF_REPLANNINGS)
            {
                replanningCounter = 0; // Reset the replanning counter
                currentDestinationVertex = transform.position; // Set the current destination vertex to the agent's current position
                agentState = AgentState.WAITING_TO_CHOOSE_NEW_DESTINATION; // Make it choose a new destination
                Invoke("ChangeAgentState", agentWaitTime); // Change the agent state to pick a new destination after some time
                return;
            }

            // Otherwise
            replanningCounter++; // Increment the replanning counter

            /*
             * The replanning strategy is to make the agent move back to the previous vertex in its path and return to the vertex
             * it was trying to go to
             */

            if (currentIndexOfPath > 0)
            {
                currentIndexOfPath--; // Make the agent move to the previous vertex
            }
            else
            {
                currentIndexOfPath++; // Make the agent move to the next vertex
            }

            agentState = AgentState.MOVING_TO_DESTINATION; // Make the agent start following its path again
        }
        else if (agentState == AgentState.MOVING_TO_DESTINATION)
        { // Keep moving to the current vertex of the path
            int currentVertexIndex = shortestPathToDestination[currentIndexOfPath]; // Get the vertex index from the shortestPathToDestination list
            // If the agent has not reached the current vertex of the path it is moving to
            if (!transform.position.Equals(vertices[currentVertexIndex]))
            {
                if (!MoveToVertex(currentVertexIndex)) // Try to move towards the vertex
                { // If the agent did not move towards the vertex because it will collide with another agent
                    agentState = AgentState.WAITING_TO_REPLAN_PATH; // The path needs to be replanned
                    float timeToWait = 0.1f + (float)random.NextDouble(); // Pick a random time between 0.1 and 1.1 seconds for the agent to wait before replanning its path
                    Invoke("ChangeAgentState", timeToWait);
                }
            }
            else
            { // Agent has reached the current vertex
                if (currentIndexOfPath == shortestPathToDestination.Count - 1)
                { // If the agent has reached the destination vertex
                    agentState = AgentState.WAITING_TO_CHOOSE_NEW_DESTINATION; // Indicate that the destination has been reached
                    Invoke("ChangeAgentState", agentWaitTime); // Change the agent state to pick a new destination after some time
                    replanningCounter = 0; // Reset the replanning counter
                }
                else
                { // The agent has not reached the destination vertex
                    currentIndexOfPath++; // Indicate that the agent is now moving to the vertex at the next index of the path
                }
            }
        }
        else
        { // agentState is WAITING_TO_CHOOSE_NEW_DESTINATION or WAITING_TO_REPLAN_PATH
            // Do nothing
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Hey");

        if (collision.collider.name.Contains("Obstacle Block"))
        {
            Debug.Log("Hey");
        }
    }

    // Initialize the list of verticies and edges of the agent's reduced visibility graph
    // as a copy of the lists in the ReducedVisibilityGraphGenerator class
    private void ReducedVisibilityGraphInit()
    {
        vertices = new List<Vector3>();
        edges = new List<Vector3[]>();

        // Initialize the list of vertices
        foreach (Vector3 vertex in ReducedVisibilityGraphGenerator.vertices)
        {
            vertices.Add(vertex);
        }

        // Initialize the list of edges
        foreach (Vector3[] edge in ReducedVisibilityGraphGenerator.edges)
        {
            Vector3[] newEdge = new Vector3[2];
            newEdge[0] = edge[0];
            newEdge[1] = edge[1];
            edges.Add(newEdge);
        }

        // Initialize the count variables
        initialVerticesCount = vertices.Count;
        initialEdgesCount = edges.Count;
    }

    // Resets the list of vertices and edges by removing the two verticies and all of the edges that 
    // were added to it from the previous start and destination positions
    private void ResetReducedVisibilityGraph()
    {
        // Reset the list of vertices
        while (vertices.Count > initialVerticesCount)
        {
            vertices.RemoveAt(vertices.Count - 1);
        }

        // Reset the list of edges
        while (edges.Count > initialEdgesCount)
        {
            edges.RemoveAt(edges.Count - 1);
        }
    }

    private GameObject InstantiateDestinationNode(Vector3 position)
    {
        GameObject obj = Instantiate(destinationNodePrefab, position, transform.rotation) as GameObject;
        obj.name = "Destination of Agent " + agentId;
        obj.GetComponent<Renderer>().material.color = agentColor; // Set the destination node's color to match the agent's color
        obj.transform.parent = transform.parent.transform; // Make the destination node a child of this agent's parent
        return obj;
    }

    // Make the agent move one DISTANCE_UNIT every GameTime.INTERVAl seconds towards the vertex at index vertexIndex, which is vertices[vertexIndex]
    // Returns true if the agent moved to the position, false otherwise. The agent will only move to the position if no collision will occur.
    private bool MoveToVertex(int vertexIndex)
    {
        float distanceToMove = DISTANCE_UNIT * GameTime.TimeFactor(); // The amount of distance that the agent will move by to the approach the vertex

        Vector3 agentToVertex = vertices[vertexIndex] - transform.position; // The vector from the agent to the vertex
        Vector3 newAgentPosition; // The agent's new position after moving

        // If the agent is within one distanceToMove of the vertex
        if (agentToVertex.magnitude <= distanceToMove)
        {
            newAgentPosition = vertices[vertexIndex]; // Set the position of the agent extacly at the position of the vertex
        }
        else
        { // Make the agent move one distance unit towards the vertex
            newAgentPosition = transform.position + distanceToMove * agentToVertex.normalized;
        }

        // If a collision will occur once the agent is at its new position
        if (WillCollide(newAgentPosition))
        {
            return false; // The agent does not move towards the vertex
        }
        else
        {
            transform.position = newAgentPosition; // The agent moves
            return true;
        }

        // Helper method to check if the agent is about to collide with another agent when it is at a particular position
        bool WillCollide(Vector3 position)
        {
            // For each agent generated, other than this agent
            foreach (GameObject otherAgent in AgentGenerator.agents)
            {
                if (otherAgent.GetComponent<Agent>().agentId == agentId)
                {
                    continue;
                }

                Vector3 displacement = otherAgent.transform.position - position; // A displacement vector from position to the other agent's position

                // If the otherAgent is half of an obstacle block length away from this agent
                if (displacement.magnitude < LevelPlatform.OBSTACLE_BLOCK_LENGTH / 2f)
                {
                    return true; // A collision is about to occur
                }
            }

            return false; // No collision is about to occur
        }
    }

    // Picks a random element from availableGridCoordinates, excluding the one corresponding to the agent's currentStartVertex, and converts it to a Unity position
    private Vector3 PickRandomAvailableGridCoordinates()
    {
        // Find the element and index of the availablGridCoordinates list to exlude
        int[] coordinatesToExclude = LevelPlatform.ConvertPositionToCoordinates(new int[] { (int)currentStartVertex.x, (int)currentStartVertex.z });

        // Remove that element from the list and put it back at the end of the list
        availableGridCoordinates.RemoveAll(e => e[0] == coordinatesToExclude[0] && e[1] == coordinatesToExclude[1]);
        availableGridCoordinates.Add(coordinatesToExclude);

        // Pick a random element of the list, excluding the last element
        int randomIndex = random.Next(0, availableGridCoordinates.Count - 1);
        int[] coordinates = availableGridCoordinates[randomIndex];
        int[] position = LevelPlatform.ConvertCoordinatesToPosition(coordinates);
        return new Vector3(position[0], ReducedVisibilityGraphGenerator.VERTEX_Y, position[1]);
    }

    // Changes the agent state to CHOOSE_NEW_DESTINATION or REPLAN_PATH
    private void ChangeAgentState()
    {
        if (agentState == AgentState.WAITING_TO_CHOOSE_NEW_DESTINATION)
        {
            agentState = AgentState.CHOOSE_NEW_DESTINATION;
        }
        else if (agentState == AgentState.WAITING_TO_REPLAN_PATH)
        {
            agentState = AgentState.REPLAN_PATH;
        }
    }

    // Draws the edges in the agent's current path to its destination
    private void DrawEdgesOfPath()
    {
        int numVertices = shortestPathToDestination.Count; // The number of vertices in the path to draw
        List<Vector3> verticesOfPath = new List<Vector3>(); // A list of all vertices in the path to draw

        // Instantiate verticesOfPath
        foreach (int vertexIndex in shortestPathToDestination)
        {
            verticesOfPath.Add(vertices[vertexIndex]);
        }

        // Instantiate the line renderer
        LineRenderer lineRenderer = new GameObject("Path: " + currentStartVertex.x + "," + currentStartVertex.z + " to " + currentDestinationVertex.x + "," + currentDestinationVertex.z).AddComponent<LineRenderer>();
        lineRenderer.startWidth = 2f;
        lineRenderer.endWidth = 2f;
        lineRenderer.positionCount = numVertices;
        lineRenderer.useWorldSpace = true;
        lineRenderer.transform.parent = transform.parent;

        // Make the line renderer the same color as the agent
        lineRenderer.material = new Material(lineRendererMaterial);
        lineRenderer.startColor = agentColor;
        lineRenderer.endColor = agentColor;
        lineRenderer.material.color = agentColor;

        // Create the line
        lineRenderer.SetPositions(verticesOfPath.ToArray());
    }

    // Destroys the edge the agent was previously moving through
    private void DestroyEdgesOfPath()
    {
        if (transform.parent.childCount < 3)
        {
            return;
        }

        GameObject path = transform.parent.GetChild(2).gameObject;
        Destroy(path);
    }

    // Setter method for availableGridCoordinates
    public void SetAvailableGridCoordinates(List<int[]> coordinates)
    {
        availableGridCoordinates = coordinates;
    }

    // Setter method for initialStartPosition
    public void SetInitialStartPosition(Vector3 position)
    {
        initialStartPosition = position;
    }

    // Setter method for agentColor
    public void SetAgentColor(Color color)
    {
        agentColor = color;
    }

    // Setter method for agentId
    public void SetAgentId(int id)
    {
        agentId = id;
    }
}
