//#define BT_DEBUG_ENABLE

using System;
using System.Collections.Generic;
using Saro.BT.Utility;

namespace Saro.BT
{

    public sealed class BehaviorIterator
    {
        private readonly FixedSizeStack<int> m_Traversal;
        private readonly Queue<int> m_RequestedTraversals;

        private readonly BehaviorTree m_Tree;

        public event Action OnCompleted;

        public bool IsRunning => m_Traversal.Count != 0;

        public int CurrentIndex => m_Traversal.Count == 0 ? BTNode.k_InvalidOrder : m_Traversal.Peek();

        public int LevelOffset { get; }

        public BTNode.EStatus? LastChildExitStatus { get; private set; }
        public BTNode.EStatus LastExecutedStatus { get; private set; }

        public int FirstInTraversal => m_Traversal.GetValueAt(0);

        public BehaviorIterator(BehaviorTree tree, int levelOffset)
        {
            this.m_Tree = tree;
            LevelOffset = levelOffset;

            var maxTraversalLen = this.m_Tree.Height + 1;
            m_Traversal = new FixedSizeStack<int>(maxTraversalLen);
            m_RequestedTraversals = new Queue<int>(maxTraversalLen);
        }

        public void Tick()
        {
            CallOnEnterOnQueuedNodes();
            var index = m_Traversal.Peek();
            var node = m_Tree.Nodes[index];
            var status = node.Run();

            LastExecutedStatus = status;

#if UNITY_EDITOR
            node.StatusEditorResult = (BTNode.EStatusEditor)status;
#endif

            if (status != BTNode.EStatus.Running)
            {
                PopNode();
                OnChildExit(node, status);
            }

            if (m_Traversal.Count == 0)
            {
                OnCompleted?.Invoke();

                __debug("iterator done!");
            }
        }

        private BTNode PopNode()
        {
            var index = m_Traversal.Pop();
            var node = m_Tree.Nodes[index];

            if (node.IsComposite())
            {
                for (int i = 0; i < node.ChildCount(); i++)
                {
                    node.GetChildAt(i).OnCompositeParentExit();
                }
            }

            node.OnExit();

            __debug($"exit *{LastExecutedStatus}* <color=green>{node.name}: {node.preOrderIndex}</color>");
            __debug($"------traversal pop: {string.Join(",", m_Traversal)}");

            return node;
        }

        private void CallOnEnterOnQueuedNodes()
        {
            while (m_RequestedTraversals.Count != 0)
            {
                var i = m_RequestedTraversals.Dequeue();
                var node = m_Tree.Nodes[i];
                node.OnEnter();

                __debug($"enter <color=green>{node.name}: {node.preOrderIndex}</color>");

#if UNITY_EDITOR
                node.OnBreakpoint();
#endif
                OnChildEnter(node);
            }
        }

        private void OnChildEnter(BTNode node)
        {
            if (node.Parent != null)
            {
                LastChildExitStatus = null;
                node.Parent.OnChildEnter(node.childOrder);
            }
        }

        private void OnChildExit(BTNode node, BTNode.EStatus status)
        {
            if (node.Parent != null)
            {
                node.Parent.OnChildExit(node.childOrder, status);
                LastChildExitStatus = status;
            }
        }

        public void Traverse(BTNode child)
        {
            var i = child.preOrderIndex;
            m_Traversal.Push(i);
            m_RequestedTraversals.Enqueue(i);

#if UNITY_EDITOR
            child.StatusEditorResult = BTNode.EStatusEditor.Running;
#endif
        }

        public void AbortRunningChildBranch(BTNode parent, int abortBranchIndex)
        {
            if (IsRunning && parent != null)
            {
                var terminatingIndex = parent.preOrderIndex;

                while (m_Traversal.Count != 0 && m_Traversal.Peek() != terminatingIndex)
                {
                    StepBackAbort();
                }

                if (parent.IsComposite())
                {
                    parent.OnAbort(abortBranchIndex);
                }

                m_RequestedTraversals.Clear();

                Traverse(parent.GetChildAt(abortBranchIndex));

                __debug($"<color=red>abort</color> *{parent.name}: {parent.preOrderIndex}* branch index: {abortBranchIndex}");
                __debug($"------traversal abort: {string.Join(",", m_Traversal)}");
            }
        }

        private void StepBackAbort()
        {
            var node = PopNode();

#if UNITY_EDITOR
            node.StatusEditorResult = BTNode.EStatusEditor.Aborted;
#endif
        }

        internal void Interrupt(BTNode subtree)
        {
            if (subtree != null)
            {
                var parentIndex = subtree.Parent != null ? subtree.Parent.preOrderIndex : BTNode.k_InvalidOrder;
                while (m_Traversal.Count != 0 && m_Traversal.Peek() != parentIndex)
                {
                    var node = PopNode();
#if UNITY_EDITOR
                    node.StatusEditorResult = BTNode.EStatusEditor.Interruption;
#endif
                }
                m_RequestedTraversals.Clear();
            }
        }


        [System.Diagnostics.Conditional("BT_DEBUG_ENABLE")]
        private static void __debug(string msg)
        {
            UnityEngine.Debug.LogError("[BT] " + msg);
        }

    }

}
