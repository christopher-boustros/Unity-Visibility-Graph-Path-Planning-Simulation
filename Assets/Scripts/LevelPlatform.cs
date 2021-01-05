/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the LevelPlatform game object
using UnityEngine;

/*
 * This class is used to define the position and boundaries of the LevelPlatform
 * and to define a grid for the whole level that contains the coordinates of all possible 
 * positions on the level to place obstacle blocks and agents.
 *
 * The MainFloor is the inner rectangle of the LevelPlatform that does not contain the alcoves.
 * The alcoves are present around the MainFloor. The grid describes the whole floor, which is the rectangle 
 * that contains all of the alcoves and the MainFloor.
 */
public class LevelPlatform : MonoBehaviour
{
    public const float X = 0f; // The X-position of the LevelPlatform
    public const float Y = 0f; // The Y-position of the LevelPlatform
    public const float Z = 0f; // The Z-position of the LevelPlatform

    public const float WHOLE_FLOOR_MAX_X = X + 320f; // The maximum x-position of the main floor
    public const float WHOLE_FLOOR_MIN_X = X - 340f; // The minimum x-position of the main floor
    public const float WHOLE_FLOOR_MAX_Z = Z + 225f; // The maximum z-position of the main floor
    public const float WHOLE_FLOOR_MIN_Z = Z - 235f; // The minimum z-position of the main floor
    public const float WHOLE_FLOOR_WIDTH = WHOLE_FLOOR_MAX_X - WHOLE_FLOOR_MIN_X; // The width of the main floor
    public const float WHOLE_FLOOR_HEIGHT = WHOLE_FLOOR_MAX_Z - WHOLE_FLOOR_MIN_Z; // The height of the main floor

    public const float MAIN_FLOOR_MAX_X = X + 250f; // The maximum x-position of the main floor
    public const float MAIN_FLOOR_MIN_X = X - 250f; // The minimum x-position of the main floor
    public const float MAIN_FLOOR_MAX_Z = Z + 125f; // The maximum z-position of the main floor
    public const float MAIN_FLOOR_MIN_Z = Z - 125f; // The minimum z-position of the main floor
    public const float MAIN_FLOOR_WIDTH = MAIN_FLOOR_MAX_X - MAIN_FLOOR_MIN_X; // The width of the main floor
    public const float MAIN_FLOOR_HEIGHT = MAIN_FLOOR_MAX_Z - MAIN_FLOOR_MIN_Z; // The height of the main floor

    public const float OBSTACLE_BLOCK_LENGTH = 10f; // The length of one side of an obstacle block
    public const float INVISIBLE_OBSTACLE_BLOCK_LENGTH = 19f; // The length of one side of an insivible obstacle block

    /*
     * The matrix "grid" is a grid of coordinates which correspond to Unity positions on the rectangle containing the MainFloor and
     * all the alcoves. The functions convertPositionCoordinate and convertCoordinateToPosition are used to convert between the grid
     * coordinates and the Unity positions. 
     * 
     * grid[x, z] stores a integer enum CoordinateType value that corresponds to what is at that coordinate (empty space, floor, obstacle block, obstacle block vertex, alcove vertex, or agent)
     */
    public static int[] MAX_GRID_COORDINATES = ConvertPositionToCoordinates(new int[] { (int)WHOLE_FLOOR_MAX_X, (int)WHOLE_FLOOR_MAX_Z }); // The maximum x and z grid coordinates. The minimum x and z coordinates are (0, 0).
    public static int[] MAX_MAIN_FLOOR_GRID_COORDINATES = ConvertPositionToCoordinates(new int[] { (int)MAIN_FLOOR_MAX_X, (int)MAIN_FLOOR_MAX_Z }); // The maximum x and z grid coordinates for the MainFloor
    public static int[] MIN_MAIN_FLOOR_GRID_COORDINATES = ConvertPositionToCoordinates(new int[] { (int)MAIN_FLOOR_MIN_X, (int)MAIN_FLOOR_MIN_Z }); // The maximum x and z grid coordinates for the MainFloor
    public enum CoordinateType
    {
        EMPTY,
        FLOOR,
        BLOCK,
        BLOCK_VERTEX,
        BLOCK_CORNER_VERTEX,
        ALCOVE_VERTEX_1,
        ALCOVE_VERTEX_2
    }
    public static CoordinateType[,] grid = new CoordinateType[MAX_GRID_COORDINATES[0] + 1, MAX_GRID_COORDINATES[1] + 1];

    // Awake is called before any other script's Start() method
    // This Awake method is set to execute before the ObstacleGenerator.cs and ReducedVisibilityGraphGenerator.cs Awake method in the project settings
    void Awake()
    {
        transform.position = new Vector3(X, Y, Z); // Set the position of the LevelPlatform

        GridInit();
    }

    // Initializes the grid with the coordinates of the floors and the empty spaces between alcoves
    public static void GridInit()
    {

        // Helper method that fills in floors in the grid 
        // given a range in x and a range in z in Unity positions
        // It does not fill in floors at the edges of the intervals
        // in order to leave a buffer zone around the edges
        void fillInFloor(float minX, float maxX, float minZ, float maxZ)
        {
            int[] minCoordinates = ConvertPositionToCoordinates(new int[] { (int)minX, (int)minZ });
            int[] maxCoordinates = ConvertPositionToCoordinates(new int[] { (int)maxX, (int)maxZ });

            for (int x = minCoordinates[0] + 1; x <= maxCoordinates[0] - 1; x++)
            {
                for (int z = minCoordinates[1] + 1; z <= maxCoordinates[1] - 1; z++)
                {
                    grid[x, z] = CoordinateType.FLOOR;
                }
            }
        }

        // Helper method that marks the Unity position (x, z) as a particular type of coordinate in the corresponding grid position
        void fillInVertexType(float x, float z, CoordinateType type)
        {
            int[] coordinates = ConvertPositionToCoordinates(new int[] { (int)x, (int)z });
            grid[coordinates[0], coordinates[1]] = type;
        }

        // Fill in the grid as all empty space
        for (int x = 0; x <= MAX_GRID_COORDINATES[0]; x++)
        {
            for (int z = 0; z <= MAX_GRID_COORDINATES[1]; z++)
            {
                grid[x, z] = CoordinateType.EMPTY;
            }
        }

        // Fill in the MainFloor and its vertices
        fillInFloor(MAIN_FLOOR_MIN_X, MAIN_FLOOR_MAX_X, MAIN_FLOOR_MIN_Z, MAIN_FLOOR_MAX_Z);

        // Fill in the top-left alcove and its vertices
        fillInFloor(-190f, -130f, MAIN_FLOOR_MAX_Z - OBSTACLE_BLOCK_LENGTH, 205f);
        fillInVertexType(-190f, MAIN_FLOOR_MAX_Z, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(-130f, MAIN_FLOOR_MAX_Z, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the top-mid alcove and its vertices
        fillInFloor(-70f, 20f, MAIN_FLOOR_MAX_Z - OBSTACLE_BLOCK_LENGTH, 175f);
        fillInVertexType(-70f, MAIN_FLOOR_MAX_Z, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(20f, MAIN_FLOOR_MAX_Z, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the top-right alcove and its vertices
        fillInFloor(110f, 210f, MAIN_FLOOR_MAX_Z - OBSTACLE_BLOCK_LENGTH, 225f);
        fillInVertexType(110f, MAIN_FLOOR_MAX_Z, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(210f, MAIN_FLOOR_MAX_Z, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the bottom-left alcove and its vertices
        fillInFloor(-140f, -80f, -175f, MAIN_FLOOR_MIN_Z + OBSTACLE_BLOCK_LENGTH);
        fillInVertexType(-140f, MAIN_FLOOR_MIN_Z, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(-80f, MAIN_FLOOR_MIN_Z, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the bottom-mid alcove and its vertices
        fillInFloor(-20f, 80f, -235f, MAIN_FLOOR_MIN_Z + OBSTACLE_BLOCK_LENGTH);
        fillInVertexType(-20f, MAIN_FLOOR_MIN_Z, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(80f, MAIN_FLOOR_MIN_Z, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the bottom-right alcove and its vertices
        fillInFloor(130f, 200f, -185f, MAIN_FLOOR_MIN_Z + OBSTACLE_BLOCK_LENGTH);
        fillInVertexType(130f, MAIN_FLOOR_MIN_Z, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(200f, MAIN_FLOOR_MIN_Z, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the left-top alcove and its vertices
        fillInFloor(-340f, MAIN_FLOOR_MIN_X + OBSTACLE_BLOCK_LENGTH, 15f, 85f);
        fillInVertexType(MAIN_FLOOR_MIN_X, 15f, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(MAIN_FLOOR_MIN_X, 85f, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the left-bottom alcove and its vertices
        fillInFloor(-280f, MAIN_FLOOR_MIN_X + OBSTACLE_BLOCK_LENGTH, -85f, -25f);
        fillInVertexType(MAIN_FLOOR_MIN_X, -85f, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(MAIN_FLOOR_MIN_X, -25f, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the right-top alcove and its vertices
        fillInFloor(MAIN_FLOOR_MAX_X - OBSTACLE_BLOCK_LENGTH, 300f, 5f, 85f);
        fillInVertexType(MAIN_FLOOR_MAX_X, 5f, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(MAIN_FLOOR_MAX_X, 85f, CoordinateType.ALCOVE_VERTEX_2);

        // Fill in the right-bottom alcove and its vertices
        fillInFloor(MAIN_FLOOR_MAX_X - OBSTACLE_BLOCK_LENGTH, 320f, -95f, -35f);
        fillInVertexType(MAIN_FLOOR_MAX_X, -95f, CoordinateType.ALCOVE_VERTEX_1);
        fillInVertexType(MAIN_FLOOR_MAX_X, -35f, CoordinateType.ALCOVE_VERTEX_2);
    }

    /* 
     * Converts (x, z) coordinates in the grid to its cooresponding position in Unity units on the whole floor
     * This scales up the coordinates by OBSTACLE_BLOCK_LENGTH
     * and converts them from a scale of 0...(LevelPlatform.WHOLE_FLOOR_MAX_X(or Z) - LevelPlatform.WHOLE_FLOOR_MIN_X(or Z))
     * to a scale of LevelPlatform.MAIN_FLOOR_MIN_X(or Z)...LevelPlatform.WHOLE_FLOOR_MAX_X(or Z)
     */
    public static int[] ConvertCoordinatesToPosition(int[] coordinates)
    {
        int x = coordinates[0]; // Get x
        int z = coordinates[1]; // Get z
        x = (int)(x * OBSTACLE_BLOCK_LENGTH); // Scale up x
        z = (int)(z * OBSTACLE_BLOCK_LENGTH); // Scale up z
        x = (int)(x + WHOLE_FLOOR_MIN_X); // Change the scale of x
        z = (int)(z + WHOLE_FLOOR_MIN_Z); // Change the scale of z
        int[] position = new int[] { x, z }; // Create the position to return
        return position;
    }

    /*
     * Converts an (x, z) position in Unity units on the MainFloor to its cooresponding coordinates in the grid
     * This converts (x, z) from a scale of LevelPlatform.WHOLE_FLOOR_MIN_X(or Z)...LevelPlatform.WHOLE_FLOOR_MAX_X(or Z)
     * to a scale of 0...(LevelPlatform.WHOLE_FLOOR_MAX_X(or Z) - LevelPlatform.WHOLE_FLOOR_MIN_X(or Z))
     * and then scales down the coordinates by OBSTACLE_BLOCK_LENGTH
    */
    public static int[] ConvertPositionToCoordinates(int[] position)
    {
        int x = position[0]; // Get x
        int z = position[1]; // Get z
        x = (int)(x - WHOLE_FLOOR_MIN_X); // Change the scale of x
        z = (int)(z - WHOLE_FLOOR_MIN_Z); // Change the scale of z
        x = (int)(x / OBSTACLE_BLOCK_LENGTH); // Scale down x
        z = (int)(z / OBSTACLE_BLOCK_LENGTH); // Scale down z
        int[] coordinates = new int[] { x, z }; // Create the coordinates to return
        return coordinates;
    }
}
