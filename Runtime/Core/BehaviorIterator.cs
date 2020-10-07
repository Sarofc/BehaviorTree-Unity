//#define BT_DEBUG_ENABLE

using System;
using System.Collections.Generic;
using Saro.BT.Utility;

namespace Saro.BT
{

    public sealed class BehaviorIterator
    {
        private readonly FixedSizeStack<int> traversal;
        private readonly Queue<int> requestedTraversals;

        private readonly BehaviorTree tree;

        public Action onDone;

        public bool IsRunning => traversal.Count != 0;

        public int CurrentIndex => traversal.Count == 0 ? BTNode.kInvalidOrder : traversal.Peek();

        public int LevelOffset { get; }

        public BTNode.EStatus? LastChildExitStatus { get; private set; }
        public BTNode.EStatus LastExecutedStatus { get; private set; }

        public int FirstInTraversal => traversal.GetValueAt(0);

        public BehaviorIterator(BehaviorTree tree, int levelOffset)
        {
            this.tree = tree;
            LevelOffset = levelOffset;

            var maxTraversalLen = this.tree.Height + 1;
            traversal = new FixedSizeStack<int>(maxTraversalLen);
            requestedTraversals = new Queue<int>(maxTraversalLen);
        }

        public void Tick()
        {
            CallOnEnterOnQueuedNodes();
            var index = traversal.Peek();
            var node = tree.Nodes[index];
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

            if (traversal.Count == 0)
            {
                onDone?.Invoke();

                __debug("iterator done!");
            }
        }

        private BTNode PopNode()
        {
            var index = traversal.Pop();
            var node = tree.Nodes[index];

            if (node.IsComposite())
            {
                for (int i = 0; i < node.ChildCount(); i++)
                {
                    node.GetChildAt(i).OnCompositeParentExit();
                }
            }

            node.OnExit();

            __debug($"exit *{LastExecutedStatus}* <color=green>{node.name}: {node.preOrderIndex}</color>");
            __debug($"------traversal pop: {string.Join(",", traversal)}");

            return node;
        }

        private void CallOnEnterOnQueuedNodes()
        {
            while (requestedTraversals.Count != 0)
            {
                var i = requestedTraversals.Dequeue();
                var node = tree.Nodes[i];
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
            traversal.Push(i);
            requestedTraversals.Enqueue(i);

#if UNITY_EDITOR
            child.StatusEditorResult = BTNode.EStatusEditor.Running;
#endif
        }

        public void AbortRunningChildBranch(BTNode parent, int abortBranchIndex)
        {
            if (IsRunning && parent != null)
            {
                var terminatingIndex = parent.preOrderIndex;

                while (traversal.Count != 0 && traversal.Peek() != terminatingIndex)
                {
                    StepBackAbort();
                }

                if (parent.IsComposite())
                {
                    parent.OnAbort(abortBranchIndex);
                }

                requestedTraversals.Clear();

                Traverse(parent.GetChildAt(abortBranchIndex));

                __debug($"<color=red>abort</color> *{parent.name}: {parent.preOrderIndex}* branch index: {abortBranchIndex}");
                __debug($"------traversal abort: {string.Join(",", traversal)}");
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
                var parentIndex = subtree.Parent != null ? subtree.Parent.preOrderIndex : BTNode.kInvalidOrder;
                while (traversal.Count != 0 && traversal.Peek() != parentIndex)
                {
                    var node = PopNode();
#if UNITY_EDITOR
                    node.StatusEditorResult = BTNode.EStatusEditor.Interruption;
#endif
                }
                requestedTraversals.Clear();
            }
        }


        [System.Diagnostics.Conditional("BT_DEBUG_ENABLE")]
        private static void __debug(string msg)
        {
            UnityEngine.Debug.LogError("[BT] " + msg);
        }

    }

}
