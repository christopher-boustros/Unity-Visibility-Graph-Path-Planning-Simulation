/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Agents game object
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is used to generate a number of agents
 */
public class AgentGenerator : MonoBehaviour
{
    public GameObject agent; // The agent game object
    public static List<GameObject> agents = new List<GameObject>(); // A list of all agents generated

    public int numberOfAgents = 1; // The number of agents to generate. It is possible that fewer agents are generated if there are no more grid positions available.
    private static List<int[]> initialAvailableGridCoordinates = new List<int[]>(); // A list of all grid positions initially available for an agent to start on
    private static List<int[]> currentlyAvailableGridCoordinates = new List<int[]>(); // A list of all available grid positions that have not yet been used to spawn an agent
    private static List<Color> distinctColors = new List<Color>() { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta,
                                                            Color.cyan, Color.black, Color.gray, Color.white }; // A list of distinct colors, one for each agent to generate
    private static System.Random random = new System.Random(); // An instance of the Random class

    // Awake is called before any other script's Start() method
    // This Awake method is set to execute after the ReducedVisibilityGraphGenerator.cs Awake method in the project settings
    void Awake()
    {
        // Make sure the number of agents is positive
        if (numberOfAgents < 0)
        {
            numberOfAgents = 0;
        }

        AvailableGridCoordinatesInit(); // Initialize availableGridCoordinates
        DistinctColorsInit(); // Initialize distinctColors

        // Generate the agents at random avaiable grid positions
        for (int i = 0; i < numberOfAgents; i++)
        {
            if (currentlyAvailableGridCoordinates.Count == 0)
            { // No more available grid coordinates to place an agent
                break; // Stop generating agents
            }

            int randomIndex = random.Next(0, currentlyAvailableGridCoordinates.Count); // Pick a random index of availableGridCoordinates
            int[] coordinate = currentlyAvailableGridCoordinates[randomIndex]; // The random coordinate chosen
            currentlyAvailableGridCoordinates.RemoveAt(randomIndex); // Remove the chosen coordinate from the list
            Color color = distinctColors[i % distinctColors.Count]; // Pick a distinct color
            InstantiateAgent(coordinate[0], coordinate[1], color, i); // Instantiate the agent at the random coordinate with a distinct color
        }
    }

    // Initialize the list of initial and currently available grid positions
    private void AvailableGridCoordinatesInit()
    {
        // For each (x, z) coordinates in the grid
        for (int x = 0; x < LevelPlatform.grid.GetLength(0); x++)
        {
            for (int z = 0; z < LevelPlatform.grid.GetLength(1); z++)
            {
                // If the coordinate is available
                if (CoordinateIsAvailable(x, z))
                {
                    initialAvailableGridCoordinates.Add(new int[] { x, z }); // Add it to the list of initial available grid coordinates
                    currentlyAvailableGridCoordinates.Add(new int[] { x, z }); // Add it to the list of currently available grid coordinates
                }
            }
        }

        // A helper function to check if a grid coordinate is available
        bool CoordinateIsAvailable(int x, int z)
        {
            // If the coordinate is not a floor, then it is not available
            if (LevelPlatform.grid[x, z] != LevelPlatform.CoordinateType.FLOOR)
            {
                return false;
            }

            // If the coordinate is adjacent to a grid coordinate that is not a floor, then it is not available
            // This is done to prevent an agent or destination node from being generated right next to the exterior 
            // or an obstacle because if it is, then there may be no bitangent edges that can reach it
            if (LevelPlatform.grid[x + 1, z] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x, z + 1] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x + 1, z + 1] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x - 1, z] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x, z - 1] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x - 1, z - 1] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x + 1, z - 1] != LevelPlatform.CoordinateType.FLOOR ||
                LevelPlatform.grid[x - 1, z + 1] != LevelPlatform.CoordinateType.FLOOR
                )
            {
                return false;
            }

            return true; // Otherwise, the coordinate is available
        }
    }

    // Add colors to the list of distinct colors if it does not have 
    // at least one color for every agent to generate
    private void DistinctColorsInit()
    {
        int n = numberOfAgents; // The number of distinct colors to generate

        if (distinctColors.Count >= n)
        {
            return; // Do not reinstantiate the list
        }

        // A maximum of 360 distinct colors can be generated
        if (n > 360)
        {
            n = 360;
        }

        float hue;
        float saturation;
        float value;

        // Generate a number of colors equal to numberOfAgentsToGenerate
        for (int i = 0; i < 360; i += 360 / n)
        {
            hue = i / 360f; // Compute the hue
            saturation = (float)random.NextDouble(); // Compute the saturation
            value = (float)random.NextDouble(); // Compute the value
            distinctColors.Add(Color.HSVToRGB(hue, saturation, value)); // Convert the HSV to a Color object and add it to the list
        }
    }

    // Instantiate an agent at grid coordinates (x, z) with a particular color and id
    private void InstantiateAgent(int x, int z, Color color, int id)
    {
        // Create an empty parent object for the agent
        GameObject newParent = new GameObject();
        newParent.name = "Agent " + id + " container";
        newParent.transform.parent = transform;

        int[] position = LevelPlatform.ConvertCoordinatesToPosition(new int[] { x, z }); // Get the Unity position
        Vector3 vectorPosition = new Vector3(position[0], ReducedVisibilityGraphGenerator.VERTEX_Y, position[1]);
        GameObject newAgent = Instantiate(agent, vectorPosition, transform.rotation) as GameObject; // Instatiate agent
        newAgent.name = "Agent " + id;
        newAgent.transform.parent = newParent.transform; // Make the obstacle block a child of the parent game object created above
        newAgent.GetComponent<Agent>().SetAgentColor(color); // Set the color of the agent
        newAgent.GetComponent<Agent>().SetInitialStartPosition(new Vector3(vectorPosition.x, vectorPosition.y, vectorPosition.z)); // Set the agent's initial start coordinates
        newAgent.GetComponent<Agent>().SetAgentId(id); // Set the agent's id

        // Copy the list of initialAvailableGridCoordinates
        List<int[]> copy = new List<int[]>();

        foreach (int[] c in initialAvailableGridCoordinates)
        {
            copy.Add(c);
        }

        newAgent.GetComponent<Agent>().SetAvailableGridCoordinates(copy); // Set the agent's list of available grid coordinates

        agents.Add(newAgent); // Add the agent to the list of agents
    }
}
