using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace AGMGSKv7
{
    /// <summary>
    /// This class has all nav points for the A* Path Algoirthm to find the fastest path. 
    /// </summary>
    public class NavGraph : DrawableGameComponent
    {

        private Dictionary<String, NavNode> graph;
        private List<NavNode> open, closed, path;
        private List<NavNode> aStarPath;
        private Stage stage;
        private bool aStarCompleted = false;

        /// <summary>
        /// Constructor Method
        /// </summary>
        /// <param name="theStage"></param>
        public NavGraph(Stage theStage)
            : base(theStage)
        {
            graph = new Dictionary<string, NavNode>();
            open = new List<NavNode>();
            closed = new List<NavNode>();
            path = new List<NavNode>();
            aStarPath = new List<NavNode>();
            stage = theStage;
        }

        ///////////////////////////////////////////////////////////////////
        /// Properties

        /// <summary>
        /// Get the Number of NavNodes in the Graph
        /// </summary>
        public int Count
        {
            get { return graph.Count; }
        }

        /// <summary>
        /// Get or Set a NavNode in the graph
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public NavNode this[int x, int z]
        {
            get
            {
                NavNode node = null;
                try
                {
                    node = graph[stringKey(x, z)];
                    return node;
                }
                catch (KeyNotFoundException)
                {
                    return node;
                }
            }
            set { graph[stringKey(x, z)] = value; }

        }

        /// <summary>
        /// Get the Number of NavNodes in the Open Set
        /// </summary>
        public int OpenSetCount
        {
            get { return open.Count; }
        }

        /// <summary>
        /// Get the Number of NavNodes in the Closed Set
        /// </summary>
        public int ClosedSetCount
        {
            get { return closed.Count; }
        }

        /// <summary>
        /// Get the Number of NavNodes in the Path Set
        /// </summary>
        public int PathSetCount
        {
            get { return path.Count; }
        }

        ////////////////////////////////////////////////////////////////////
        /// Methods

        /// <summary>
        /// Method that returns a String representation of the coordinates of
        /// a NavNode. This string is used as a key for the values in the graph
        /// </summary>
        /// <param name="x"> x coordinate</param>
        /// <param name="z"> z coordinate</param>
        /// <returns></returns>
        private String stringKey(int x, int z)
        {
            return String.Format("{0}::{1}", x, z);
        }

        /// <summary>
        /// Method to get a NavNode in the graph that is at
        /// coordinates (x, z). The method converts the x and
        /// z coordinates given into a key that is used to retrieve
        /// the NavNode in the graph.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns>A NAvNode if one exists, otherwise null</returns>
        public NavNode getNavNode(int x, int z)
        {
            String nodeKey = stringKey(x, z);
            if (graph.ContainsKey(nodeKey))
            {
                return graph[nodeKey];
            }
            else
            {
                Console.WriteLine("Node With Key: {0} Not Found", nodeKey);
                return null;
            }
        }

        /// <summary>
        /// Method to add a NavNode into the graph. The new NavNode
        /// is added only if it does not exists in the graph.
        /// </summary>
        /// <param name="newNode">NavNode to be added to the graph</param>
        public void addNavNode(NavNode newNode)
        {
            String nodeKey = stringKey((int)newNode.X, (int)newNode.Z);
            if (!graph.ContainsKey(nodeKey)) { graph.Add(nodeKey, newNode); }
        }

        /// <summary>
        /// Method that goes through the entire graph connecting NavNodes.
        /// A NavNode is given connected to an adjacent NavNode if both nodes
        /// offset are less than or equal to their distance.
        /// </summary>
        public void setAdjacents()
        {
            float distanceBetweenNodes = 0.0f;

            // Loop Through the entire graph
            foreach (KeyValuePair<String, NavNode> node in graph)
            {
                foreach (KeyValuePair<String, NavNode> nNode in graph)
                {
                    distanceBetweenNodes = Vector3.Distance(node.Value.Translation, nNode.Value.Translation);

                    // Check the distance between NavNodes
                    if ((node.Key != nNode.Key) && (distanceBetweenNodes <= node.Value.Offset) && (distanceBetweenNodes <= nNode.Value.Offset))
                    {
                        node.Value.addAdjacentNode(nNode.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Method that finds the closest NavNode from the given location.
        /// The NavNode is search from the existing NavNodes in the graph.
        /// </summary>
        /// <param name="location"> (x, y, z) location</param>
        /// <returns></returns>
        public NavNode findClosestNavNodeInGraph(Vector3 location)
        {
            NavNode closestNode = new NavNode(new Vector3(0));
            float shortestDistance = 100000.0f;
            float distance = 0.0f;
            SortedDictionary<String, NavNode> sortedList = new SortedDictionary<string, NavNode>(graph);

            // Loop through all the nodes in the graph
            foreach (KeyValuePair<String, NavNode> node in sortedList)
            {
                distance = Vector3.Distance(node.Value.Translation, location);

                // Compare to min distance, if smaller, update min distance
                // and update closest NavNode
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestNode = node.Value;
                }
            }

            return closestNode;
        }


        /// <summary>
        /// Method that implements the A* Algorithm using the current graph
        /// of NavNodes.
        /// </summary>
        /// <param name="source"> Start NavNode of the path</param>
        /// <param name="destination">Destination NavNode of the path</param>
        /// <returns></returns>
        public List<NavNode> aStarPathFinding(NavNode source, NavNode destination)
        {
            open = new List<NavNode>();
            closed = new List<NavNode>();
            path = new List<NavNode>();

            // Grab the source Node
            NavNode current = source;

            // No cost at the start
            current.Cost = 0;

            open.Add(current);

            // Sorting implementation of the open set
            // Sort with respect to cost, lowest cost nodes
            // are prioritized
            open.Sort(delegate(NavNode n1, NavNode n2)
            {
                return n1.Cost.CompareTo(n2.Cost);
            });

            // Loop through until the open set is empty
            while (open.Count != 0)
            {
                current = open.First<NavNode>();
                open.Remove(open.First<NavNode>());

                // If the current node is the destination node,
                // Path is complete, break out of loop
                if (current.Translation == destination.Translation)
                {
                    break;
                }

                // Add the current node to the set and change is Navigability
                closed.Add(current);
                current.Navigatable = NavNode.NavNodeEnum.CLOSED;

                // Go through all the current nodes adjacency list
                foreach (NavNode adjacent in current.Adjacent)
                {
                    // if the adjacent node has not been processed, process the node
                    if (!open.Contains(adjacent) && !closed.Contains(adjacent))
                    {
                        // Set the adjacents previous node to the current node
                        adjacent.PathPredecessor = current;

                        // calculate the distance of the adjacent node from the source
                        adjacent.DistanceFromSource = current.DistanceFromSource +
                            Vector3.Distance(current.Translation, adjacent.Translation);

                        // Calculate the heuristic distance of the adjacent node to the goal
                        adjacent.DistanceToGoal =
                            Vector3.Distance(current.Translation, adjacent.Translation) +
                            Vector3.Distance(adjacent.Translation, destination.Translation);

                        // Set the total cost of the adjacent node
                        adjacent.Cost = adjacent.DistanceFromSource + adjacent.DistanceToGoal;

                        // Add the node the the open set, and change its Navigability
                        open.Add(adjacent);
                        adjacent.Navigatable = NavNode.NavNodeEnum.OPEN;
                    }
                }

                // Sort the list 
                open.Sort(delegate(NavNode n1, NavNode n2)
                {
                    return n1.Cost.CompareTo(n2.Cost);
                });
            }

            // Traverse the Path from the destination to the source
            while (Vector3.Distance(current.Translation, source.Translation) != 0.0)
            {
                // Set the Navigability of the nodes in the Path and add them to the path list
                current.Navigatable = NavNode.NavNodeEnum.PATH;
                path.Add(current);
                current = current.PathPredecessor;
            }

            // Signal that A* has been completed
            aStarCompleted = true;

            // Path needs to be reverse in order to have proper traversal from source to destination
            path.Reverse();

            return path;
        }
    }
}