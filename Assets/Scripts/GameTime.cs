/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is not linked to a game object
using UnityEngine;

/*
 * This class defines the constant timestep interval used for the game's physics computations
 * The lower the time interval, the faster the game's overall physics motions will appear
 * 
 * The Agent class is defined to make the agent move by a particular distance unit once every INTERVAL amount of time.
 * So, by decreasing INTERVAL, the agent will move faster.
 */
public static class GameTime
{
    public const float INTERVAL = 0.02f; // 0.02 seconds
    public const float RATE = 1 / INTERVAL; // The framerate equivalent to the interval

    /*
     * This factor determines by how much an agent should move based on the actual time between frames
     * For example, if an agent is set to move by 1 distance unit once every GameTime.INTERVAL (so once every 0.02 seconds) but the time between frames was only 0.01 seconds, 
     * then the timeFactor() will be 0.01/0.02 = 1/2. So for that single frame, the agent will move by 1/2 of the distance unit in order to keep its motion at 1 distance
     * unit every 0.02 seconds.
     */
    public static float TimeFactor()
    {
        return Time.deltaTime * RATE;
    }
}
