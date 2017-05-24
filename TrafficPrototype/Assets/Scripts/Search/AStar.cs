using System;
using System.Collections.Generic;
using UnityEngine;

namespace Search {
    public class AStar {
        public delegate Vector3[] NextLocations(Vector3 current);

        private readonly Node startNode;
        private readonly Node endNode;
        private readonly NextLocations adjacentLocations;
        private readonly Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();

        public AStar(Vector3 startLocation, Vector3 endLocation, NextLocations adjacentLocations) {
            startNode = new Node(startLocation, endLocation);
            endNode = new Node(endLocation, endLocation);
            this.adjacentLocations = adjacentLocations;
        }

        public static List<Vector3> FindPath(Vector3 startNode, Vector3 endNode, NextLocations nextLocations) {
            return new AStar(startNode, endNode, nextLocations).FindPath();
        }

        private List<Vector3> FindPath() {
            var path = new List<Vector3>();
            var success = Search(startNode);
            if (success) {
                var node = endNode;
                while (node.ParentNode != null) {
                    path.Add(node.Location);
                    node = node.ParentNode;
                }
                path.Reverse();
            }
            return path;
        }

        private bool Search(Node currentNode) {
            currentNode.State = NodeState.Closed;
            List<Node> nextNodes = GetAdjacentWalkableNodes(currentNode);
            nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
            foreach (var nextNode in nextNodes) {
                if (nextNode.Location == endNode.Location) {
                    return true;
                }
                if (Search(nextNode)) // Note: Recurses back into Search(Node)
                    return true;
            }
            return false;
        }

        private List<Node> GetAdjacentWalkableNodes(Node fromNode) {
            List<Node> walkableNodes = new List<Node>();
            var nextLocations = adjacentLocations(fromNode.Location);

            foreach (var location in nextLocations) {

                Node node;
                if (!nodes.TryGetValue(location, out node)) {
                    node = new Node(location, endNode.Location);
                    nodes[location] = node;
                }

                // Ignore already-closed nodes
                switch (node.State) {
                    case NodeState.Closed:
                        continue;
                        
                    // Already-open nodes are only added to the list if their G-value is lower going via this route.
                    case NodeState.Open:
                        var traversalCost = Node.GetTraversalCost(node.Location, node.ParentNode.Location);
                        var gTemp = fromNode.G + traversalCost;
                        if (gTemp < node.G) {
                            node.ParentNode = fromNode;
                            walkableNodes.Add(node);
                        }
                        break;
                        
                    case NodeState.Untested:
                        // If it's untested, set the parent and flag it as 'Open' for consideration
                        node.ParentNode = fromNode;
                        node.State = NodeState.Open;
                        walkableNodes.Add(node);
                        break;
                        
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                
            }

            return walkableNodes;
        }
    }

    public class Node {
        public Vector3 Location { get; private set; }

        // The length of the path from the start node to this node.
        public float G { get; private set; }

        // The straight-line distance from this node to the end node.
        public float H { get; private set; }

        // Estimated total distance/cost.
        public float F {
            get { return G + H; }
        }

        public NodeState State { get; set; }

        private Node parentNode;

        public Node ParentNode {
            get { return parentNode; }
            set {
                // When setting the parent, also calculate the traversal cost from the start node to here (the 'G' value)
                parentNode = value;
                G = parentNode.G + GetTraversalCost(Location, parentNode.Location);
            }
        }
        
        public Node(Vector3 location, Vector3 endLocation) {
            Location = location;
            State = NodeState.Untested;
            H = GetTraversalCost(Location, endLocation);
            G = 0;
        }

        internal static float GetTraversalCost(Vector3 location, Vector3 otherLocation) {
            var deltaX = otherLocation.x - location.x;
            var deltaY = otherLocation.y - location.y;
            return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
    }

    public enum NodeState {
        Untested,
        Open,
        Closed
    }
}