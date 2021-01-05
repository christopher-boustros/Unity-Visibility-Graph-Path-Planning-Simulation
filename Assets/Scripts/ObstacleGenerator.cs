/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the obstacles game object
using System.Collections.Generic;
using UnityEngine;

/*
 * This class is used to generate random L-shaped obstacles at the start of the game.
 * Each obstacle is composed of ObstacleBlock game objects, which are visible, and InvisibleObstacleBlock game objects, which are invisible
 * The InvisibleObstacleBlock game objects are larger versions of the ObstacleBlock game object which overlap each ObstacleBlock.
 * The puprose of the InvisibleObstacleBlock game objects are for raycasting in the ReducedVisibilityGraphGenerator
 * This class also defines a grid of posittions along the MainFloor where obstacle blocks can be placed.
 */
public class ObstacleGenerator : MonoBehaviour
{
    public GameObject obstacleBlock; // The visible obstacle block
    public GameObject invisibleObstacleBlock; // The invisible obstacle block
    private List<int[]> availableCoordinates = new List<int[]>(); // A list of all coordinates on the MainFloor availble to put an obstacle corner block (the corner of the L-shape)
    private int numberOfObstaclesToGenerate; // Number of obstacles to generate (fewer may be generated if the number is too high)
    private int numberOfObstacles = 0; // The actual number of obstacles generated

    public const int MAX_OBSTACLE_BLOCKS = 4; // The maximum number of obstacle blocks used to generate one segment of an L-shaped obstacle
    public const int MAX_NUMBER_OF_OBSTACLES = 8; // The maximum number of obstacles to generate
    public const int MIN_NUMBER_OF_OBSTACLES = 4; // The minimum number of obstacles to generate

    // Awake is called before any other script's Start() method
    // This Awake method is set to execute after the LevelPlatform.cs Awake method in the project settings
    void Awake()
    {
        System.Random random = new System.Random(); // Create an instance of the Random class
        numberOfObstaclesToGenerate = random.Next(MIN_NUMBER_OF_OBSTACLES, MAX_NUMBER_OF_OBSTACLES + 1); // Choose a random number of obstacles to generate

        AvailableCoordinatesInit();

        // For each obstacle that needs to be generated
        for (int i = 0; i < numberOfObstaclesToGenerate; i++)
        {
            if (availableCoordinates.Count == 0)
            { // If no more available coordinates
                return;
            }

            int[] cornerPosition = PickRandomFromList(availableCoordinates, random); // Pick a random position for the corner of the L-shaped obstacle
            int numHorizontalBlocks = random.Next(1, MAX_OBSTACLE_BLOCKS + 1); // Pick the number of horizontal blocks to generate from the corner
            int numVerticalBlocks = random.Next(1, MAX_OBSTACLE_BLOCKS + 1); // Pick the number of the vertical blocks to generate from the corner
            int horizontalBlocksDirection = random.Next(0, 2); // Pick the direction for the horizontal blocks (0 or 1)
            int verticalBlocksDirection = random.Next(0, 2); // Pick the direction for the vertical blocks (0 or 1)

            // Instantiate the corner point
            InstantiateBlock(cornerPosition[0], cornerPosition[1], i, LevelPlatform.CoordinateType.BLOCK_CORNER_VERTEX);

            // For each horizontal block to instantiate
            for (int j = 1; j <= numHorizontalBlocks; j++)
            {
                LevelPlatform.CoordinateType blockType; // The blockType (1 or 2) used when updating obstacleGrid

                if (j == numHorizontalBlocks)
                {
                    blockType = LevelPlatform.CoordinateType.BLOCK_VERTEX;
                }
                else
                {
                    blockType = LevelPlatform.CoordinateType.BLOCK;
                }

                if (horizontalBlocksDirection == 0)
                { // Positive direction
                    InstantiateBlock(cornerPosition[0] + j, cornerPosition[1], i, blockType);
                }
                else
                { // Negative direction
                    InstantiateBlock(cornerPosition[0] - j, cornerPosition[1], i, blockType);
                }
            }

            // For each vertical block to instantiate
            for (int j = 1; j <= numVerticalBlocks; j++)
            {
                LevelPlatform.CoordinateType blockType; // The blockType (1 or 2) used when updating obstacleGrid

                if (j == numVerticalBlocks)
                {
                    blockType = LevelPlatform.CoordinateType.BLOCK_VERTEX;
                }
                else
                {
                    blockType = LevelPlatform.CoordinateType.BLOCK;
                }

                if (verticalBlocksDirection == 0)
                { // Positive direction
                    InstantiateBlock(cornerPosition[0], cornerPosition[1] + j, i, blockType);
                }
                else
                { // Negative direction
                    InstantiateBlock(cornerPosition[0], cornerPosition[1] - j, i, blockType);
                }
            }


            int extraLeft = 0, extraRight = 0, extraUp = 0, extraDown = 0; // The extra amount of surrounding coordinates to remove, based on how many horizontal and vertical blocks were generated

            if (horizontalBlocksDirection == 0)
            { // Positive x direction
                extraRight = numHorizontalBlocks;
            }
            else
            { // Negative x direction
                extraLeft = numHorizontalBlocks;
            }

            if (verticalBlocksDirection == 0)
            { // Positive z direction
                extraUp = numVerticalBlocks;
            }
            else
            { // Negative z direction
                extraDown = numVerticalBlocks;
            }

            // Remove the coordinates on and surrounding the cornerPosition of the generated obstacle
            // This is done so that the next obstacle corner block is not placed on or surrounding the current corner block
            for (int j = -MAX_OBSTACLE_BLOCKS - 2 - extraLeft; j <= MAX_OBSTACLE_BLOCKS + 2 + extraRight; j++)
            {
                for (int k = -MAX_OBSTACLE_BLOCKS - 2 - extraDown; k <= MAX_OBSTACLE_BLOCKS + 2 + extraUp; k++)
                {
                    int[] coordinatesToRemove = new int[] { cornerPosition[0] + j, cornerPosition[1] + k };
                    availableCoordinates.RemoveAll(c => coordinatesToRemove[0] == c[0] && coordinatesToRemove[1] == c[1]); // Remove coordinatesToRemove from the list of available coordinates
                }
            }

            numberOfObstacles++;
        }
    }

    // Returns a random element from the list l
    private int[] PickRandomFromList(List<int[]> l, System.Random random)
    {
        if (l.Count == 0)
        { // If l is empty
            return null;
        }

        int i = random.Next(0, l.Count); // Pick a random index
        return l[i];
    }

    // Instantiate an obstacle block at coordinates (x, z) for obstacle i with coordinate type blockType
    // and update obstacleGrid
    private void InstantiateBlock(int x, int z, int i, LevelPlatform.CoordinateType blockType)
    {
        int[] position = LevelPlatform.ConvertCoordinatesToPosition(new int[] { x, z }); // Get the Unity position
        Vector3 vectorPosition = new Vector3(position[0], ReducedVisibilityGraphGenerator.VERTEX_Y, position[1]);
        GameObject block = Instantiate(obstacleBlock, vectorPosition, transform.rotation) as GameObject; // Instatiate the obstacle block
        GameObject invisibleBlock = Instantiate(invisibleObstacleBlock, vectorPosition, transform.rotation) as GameObject; // Instatiate the invisible obstacle block at the same position as the obstacle block
        block.name = "Obstacle Block " + x + "," + z;
        invisibleBlock.name = "Invisible Obstacle Block " + x + "," + z;

        // Create an empty game object (as a child of the game object Obstacles) for the i-th obstacle, if not already created
        if (transform.childCount <= i)
        {
            GameObject child = new GameObject();
            child.name = "Obstacle " + i;
            child.transform.parent = gameObject.transform;
        }

        block.transform.parent = transform.GetChild(i).transform; // Make the obstacle block a child of the i-th obstacle game object
        invisibleBlock.transform.parent = transform.GetChild(i).transform;

        // Update the grid
        LevelPlatform.grid[x, z] = blockType;
    }

    // Initialize the list of available coordinates to place blocks
    private void AvailableCoordinatesInit()
    {
        // For each coordinate in the grid where a block could be placed, excluding a buffer zone around the edges of the grid
        for (int x = LevelPlatform.MIN_MAIN_FLOOR_GRID_COORDINATES[0] + MAX_OBSTACLE_BLOCKS + 3; x <= LevelPlatform.MAX_MAIN_FLOOR_GRID_COORDINATES[0] - MAX_OBSTACLE_BLOCKS - 3; x++)
        {
            for (int z = LevelPlatform.MIN_MAIN_FLOOR_GRID_COORDINATES[1] + MAX_OBSTACLE_BLOCKS + 3; z <= LevelPlatform.MAX_MAIN_FLOOR_GRID_COORDINATES[1] - MAX_OBSTACLE_BLOCKS - 3; z++)
            {
                int[] coordinates = new int[] { x, z };
                availableCoordinates.Add(coordinates); // Add the coordinate to the list
            }
        }
    }

    public int GetActualNumberOfObstaclesGenerated()
    {
        return numberOfObstacles;
    }
}
