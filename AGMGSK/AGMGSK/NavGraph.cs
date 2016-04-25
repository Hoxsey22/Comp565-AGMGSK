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
    public class NavGraph : DrawableGameComponent
    {

        private Dictionary<String, NavNode> graph;
        private List<NavNode> open, closed, path;
        private List<NavNode> aStarPath;
        private Stage stage;
        private bool aStarCompleted = false;

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

        // Get the Number of NavNodes in the Graph
        public int Count
        {
            get { return graph.Count; }
        }

        // Get or Set a NavNode in the graph
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

        // Get the Number of NavNodes in the Open Set
        public int OpenSetCount
        {
            get { return open.Count; }
        }


        // Get the Number of NavNodes in the Closed Set
        public int ClosedSetCount
        {
            get { return closed.Count; }
        }


        // # of NavNodes in the Path Set
        public int PathSetCount
        {
            get { return path.Count; }
        }


        // Returns a String representation of the coordinates of
        private String stringKey(int x, int z)
        {
            return String.Format("{0}::{1}", x, z);
        }

        //Get a NavNode in the graph that is at
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


        public void addNavNode(NavNode newNode)
        {
            String nodeKey = stringKey((int)newNode.X, (int)newNode.Z);
            if (!graph.ContainsKey(nodeKey)) { graph.Add(nodeKey, newNode); }
        }


        public void createAdjacent()
        {
            float distanceBetweenNodes = 0.0f;

            // Loop Through the entire graph
            foreach (KeyValuePair<String, NavNode> node in graph)
            {
                foreach (KeyValuePair<String, NavNode> nNode in graph)
                {
                    distanceBetweenNodes = Vector3.Distance(node.Value.Translation, nNode.Value.Translation);

                    if ((node.Key != nNode.Key) && (distanceBetweenNodes <= node.Value.Offset) && (distanceBetweenNodes <= nNode.Value.Offset))
                    {
                        node.Value.addAdjacentNode(nNode.Value);
                    }
                }
            }
        }


        // finds the closest NavNode from the given location.
        public NavNode findClosestNavNodeInGraph(Vector3 location)
        {
            NavNode closestNode = new NavNode(new Vector3(0));
            float shortestDistance = 100000.0f;
            float distance = 0.0f;
            SortedDictionary<String, NavNode> sortedList = new SortedDictionary<string, NavNode>(graph);


            foreach (KeyValuePair<String, NavNode> node in sortedList)
            {
                distance = Vector3.Distance(node.Value.Translation, location);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestNode = node.Value;
                }
            }

            return closestNode;
        }


        // A *
        public List<NavNode> aStarPathFinding(NavNode source, NavNode destination)
        {
            open = new List<NavNode>();
            closed = new List<NavNode>();
            path = new List<NavNode>();

            NavNode current = source;

            current.Cost = 0;

            open.Add(current);

            open.Sort(delegate(NavNode n1, NavNode n2)
            {
                return n1.Cost.CompareTo(n2.Cost);
            });

            while (open.Count != 0)
            {
                current = open.First<NavNode>();
                open.Remove(open.First<NavNode>());


                if (current.Translation == destination.Translation)
                {
                    break;
                }

                closed.Add(current);
                current.Navigatable = NavNode.NavNodeEnum.CLOSED;


                foreach (NavNode adjacent in current.Adjacent)
                {

                    if (!open.Contains(adjacent) && !closed.Contains(adjacent))
                    {

                        adjacent.PredecessorNode = current;


                        adjacent.DistanceFromSource = current.DistanceFromSource +
                            Vector3.Distance(current.Translation, adjacent.Translation);


                        adjacent.DistanceToGoal =
                            Vector3.Distance(current.Translation, adjacent.Translation) +
                            Vector3.Distance(adjacent.Translation, destination.Translation);


                        adjacent.Cost = adjacent.DistanceFromSource + adjacent.DistanceToGoal;

  
                        open.Add(adjacent);
                        adjacent.Navigatable = NavNode.NavNodeEnum.OPEN;
                    }
                }


                open.Sort(delegate(NavNode n1, NavNode n2)
                {
                    return n1.Cost.CompareTo(n2.Cost);
                });
            }


            while (Vector3.Distance(current.Translation, source.Translation) != 0.0)
            {
                current.Navigatable = NavNode.NavNodeEnum.PATH;
                path.Add(current);
                current = current.PredecessorNode;
            }

            aStarCompleted = true;
            path.Reverse();

            return path;
        }
    }
}