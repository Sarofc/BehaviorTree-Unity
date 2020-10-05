
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
//using Bonsai.Core;
using UnityEngine;

namespace Bonsai.Designer
{
    /// <summary>
    /// The canvas holds the nodes of the behaviour tree.
    /// </summary>
    public class BonsaiCanvas
    {
        private readonly List<BonsaiNode> nodes = new List<BonsaiNode>();
        public IReadOnlyList<BonsaiNode> Nodes { get { return nodes; } }
        public BonsaiNode Root { get; private set; }
        public BehaviorTree Tree { get; }

        /// <summary>
        /// Builds the canvas given the behaviour tree.
        /// </summary>
        /// <param name="treeBehaviours"></param>
        public BonsaiCanvas(BehaviorTree tree)
        {
            Tree = tree;
            var nodeMap = ReconstructEditorNodes(tree.Nodes.Concat(tree.unusedNodes));
            ReconstructEditorConnections(nodeMap);
            Root = nodes.FirstOrDefault(n => n.Behaviour == tree.Root);
        }

        public BonsaiNode CreateRootNode()
        {
            var rootNode = CreateNode(typeof(Selector));
            SetRoot(rootNode);
            return rootNode;
        }

        /// <summary>
        /// Create a node and its behaviour from the type.
        /// </summary>
        /// <param name="nodeTypename"></param>
        public BonsaiNode CreateNode(Type behaviourType)
        {
            var behaviour = ScriptableObject.CreateInstance(behaviourType) as BTNode;
            return CreateNode(behaviour);
        }

        /// <summary>
        /// Create a node from an existing behaviour.
        /// </summary>
        /// <param name="behaviour"></param>
        /// <returns></returns>
        public BonsaiNode CreateNode(BTNode behaviour)
        {
            var node = CreateEditorNode(behaviour.GetType());
            node.Behaviour = behaviour;
            return node;
        }

        public void PushToEnd(BonsaiNode node)
        {
            bool bRemoved = nodes.Remove(node);
            if (bRemoved)
            {
                nodes.Add(node);
            }
        }

        public void Remove(BonsaiNode node)
        {
            if (nodes.Remove(node))
            {
                // Clear root since it was removed.
                if (node == Root)
                {
                    Root = null;
                }

                node.Destroy();
            }
        }

        public void Remove(Predicate<BonsaiNode> match)
        {
            List<BonsaiNode> nodesToDestroy = nodes.FindAll(match);
            nodes.RemoveAll(match);

            // Clear root if removed.
            if (nodesToDestroy.Contains(Root))
            {
                Root = null;
            }

            foreach (BonsaiNode node in nodesToDestroy)
            {
                node.Destroy();
            }
        }

        public void SetRoot(BonsaiNode newRoot)
        {
            if (newRoot.Parent == null)
            {
                Root = newRoot;
                //Undo.RecordObject(Root, "Set Root");
            }
            else
            {
                Debug.LogWarning("Root cannot be a child.");
            }
        }

        public void AddChild(BonsaiNode parent, BonsaiNode child)
        {
            if (child == Root)
            {
                Debug.LogWarning("A root cannot be a child.");
            }

            else if (parent.Contains(child))
            {
                Debug.LogWarning("Already added.");
            }

            else if (DetectCycle(parent, child))
            {
                Debug.LogWarning("Cycle detected.");
            }

            else
            {
                child.SetParent(parent);
            }
        }

        public void SortNodes()
        {
            foreach (BonsaiNode node in nodes)
            {
                node.SortChildren();
            }
        }

        // Creates an editor node.
        private BonsaiNode CreateEditorNode(Type behaviourType)
        {
            var prop = BonsaiEditor.GetNodeTypeProperties(behaviourType);
            var tex = BonsaiPreferences.Texture(prop.texName);
            var node = AddEditorNode(prop.hasOutput, tex);
            return node;
        }

        // Creates and adds an editor node to the canvas.
        private BonsaiNode AddEditorNode(bool hasOutput, Texture icon = null)
        {
            var node = new BonsaiNode(hasOutput, icon);
            nodes.Add(node);
            return node;
        }

        /// <summary>
        /// Test for a cycle in the tree.
        /// </summary>
        /// <param name="child">The child that could cause a cycle.</param>
        /// <returns>True if adding the child causes a cycle.</returns>
        private bool DetectCycle(BonsaiNode parent, BonsaiNode child)
        {
            var currentNode = parent;

            while (currentNode != null)
            {
                // Cycle detected.
                if (child == currentNode)
                {
                    return true;
                }

                // Move up the tree.
                else
                {
                    currentNode = currentNode.Parent;
                }
            }

            // No cycle detected.
            return false;
        }

        // Reconstruct editor nodes from the tree.
        private Dictionary<BTNode, BonsaiNode> ReconstructEditorNodes(IEnumerable<BTNode> behaviours)
        {
            var nodeMap = new Dictionary<BTNode, BonsaiNode>();

            foreach (BTNode behaviour in behaviours)
            {
                BonsaiNode node = ReconstructEditorNode(behaviour);
                nodeMap.Add(behaviour, node);
            }

            return nodeMap;
        }

        private BonsaiNode ReconstructEditorNode(BTNode behaviour)
        {
            BonsaiNode node = CreateNode(behaviour);
            node.Behaviour = behaviour;
            node.Position = behaviour.nodePosition;
            return node;
        }

        // Reconstruct the editor connections from the tree.
        private void ReconstructEditorConnections(Dictionary<BTNode, BonsaiNode> nodeMap)
        {
            // Create the connections
            foreach (BonsaiNode node in nodes)
            {
                int childCount = node.Behaviour.ChildCount();
                for (int i = 0; i < childCount; i++)
                {
                    BTNode child = node.Behaviour.GetChildAt(i);
                    nodeMap[child].SetParent(node);
                }
            }
        }

    }
}
