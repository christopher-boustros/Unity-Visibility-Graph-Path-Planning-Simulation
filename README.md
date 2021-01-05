# Unity Visibility Graph Path Planning Simulation
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/christopher-boustros/Unity-Visibility-Graph-Path-Planning-Simulation)

A simulation made with Unity in which agents perform path planning with a dynamically generated reduced visibility graph and the A\* algorithm. This was made as part of a course assignment for COMP 521 Modern Computer Games in fall 2020 at McGill University.

No third-party assets are used in this project.

You can run the simulation on GitHub Pages [**HERE**](https://christopher-boustros.github.io/Unity-Visibility-Graph-Path-Planning-Simulation/)!

![Alt text](/Screenshot.png?raw=true "Screenshot")
<br></br>

## How to run the simulation

### Requirements

You must have Unity version 2019.4.9f1 installed on your computer. Other versions of Unity may have compatibility issues.

### Running the simulation in Unity

Clone the master branch of this repository with `git clone --single-branch https://github.com/christopher-boustros/Unity-Visibility-Graph-Path-Planning-Simulation.git`, or alternatively, download and extract the ZIP archive of the master branch. 

Open the Unity Hub, click on the Projects tab, click on the ADD button, and select the root directory of this repository.

Click on the project to open it in Unity.

In the Project window, double click on the `MainScene.unity` file from the `Assets/Scenes` folder to replace the sample scene.

Click on the play button to play the simulation.
<br></br>

## Simulation mechanics

### Choosing the number of agents
In Unity, click on the `Agents` game object from the Hierarchy tab. Then, in the Inspector tab, there will be an input field named `Number Of Agents`, which will let you choose the number of agents that will be generated when the simulation is played. 

Each agent is represented as a sphere with a distinct color, and each agent has a destination represented as a cube of the same color. The line segments from an agent's start position to its destination represents the dynamically generated path along the visibility graph that the agent will move along to reach its destination. Once an agent reaches its destination, it will randomly choose a new destination and generate a new path to its destination. 

### L-Shaped obstacles
When the simulation starts, between 4 and 8 obstacles are spawned at random locations. The obstacles are dynamically generated to have a random L-shape. 

### Reduced visibility graph
A reduced visibility graph is dynamically generated when the simulation starts. The blue spheres are the vertices and the pink lines are the edges of the graph. This graph allows the agents to plan paths that avoid obstacles.

A **visibility graph** is a graph that connects each obstacle vertex to every other obstacle vertex with an edge. So, each vertex of the graph represents a location that an agent can move to, and each edge represents a visible path between two vertices. This type of graph is useful for path planning as every path in the graph is visible, meaning it does not intersect an obstacle. However, a visibility graph may have edges that are not necessary for path planning as they are not part of a shortest path between any two locations. 

A **reduced visibility graph** is a visibility graph that only contains reflex vertices and bitangent edges, so it eliminates many unnecessary edges. A **reflex vertex** is a vertex of a polygon that has an exterior angle of less than 180 degrees. A **bitangent edge** is an edge between two reflex vertices that does not intersect an obstacle, even when slightly extended from either end. An example of an edge of a visibility graph that is not bitangent, and thus would not be part of a reduced visibility graph, is an edge that goes directly into an obstacle. This type of edge would be unnecessary for path planning because edges that go directly into an obstacle as opposed to scraping the corner of an obstacle are never part of an optimal, shortest path.

### Path finding with the A\* algorithm
An agent finds an optimal path from its start position to its destination by searching the reduced visibility graph for a shortest path using the A\* algorithm. The weight of an edge used by the A\* algorithm is the Euclidean distance between the two vertices of the edge.

### Collision avoidance
The agents detect and avoid collisions with each other using a simple replanning strategy. When two or more agents are about to collide with each other, they will all stop moving and each agent will wait between 0.1 and 1.1 seconds before replanning its path. An agent replans its path by simply moving back to its previous vertex and then resuming the path towards its destination. When an agent is still unable to move to its destination after three consecutive replannings, it will choose a new destination.
<br></br>

## References

- The `AStarAlgorithm.cs` file is transformed from and built upon the pseudocode [here](https://en.wikipedia.org/wiki/A*_search_algorithm#Pseudocode). Since that pseudocode was released on Wikipedia, it is licensed for use under the [CC BY-SA 3.0 License](https://creativecommons.org/licenses/by-sa/3.0/) and the author has agreed that a hyperlink or URL is sufficient attribution under that license.

- The implementation of the `UpdateOrthographicCameraSize` method in `CameraScaler.cs` is inspired by [this source](https://pressstart.vip/tutorials/2018/06/14/37/understanding-orthographic-size.html). No code in this repository is copied from that source.
<br></br>

## License

- All contents of this repository excluding the `Assets/Scripts/AStarAlgorithm.cs` file are released under the [MIT License](https://opensource.org/licenses/MIT) (see LICENSE).

- The `Assets/Scripts/AStarAlgorithm.cs` file is released under the [CC BY-SA 4.0 License](https://creativecommons.org/licenses/by-sa/4.0/) (see LICENSE).
