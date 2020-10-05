using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Bonsai
{

    public class BehaviorTree : ScriptableObject
    {
        private BehaviorIterator mainIterator;

        private UpdateList<Timer> activeTimers;

        private bool isTreeInitialized = false;

        public Blackboard blackboard;

        public BTNode[] Nodes => nodes;
        [SerializeField]
        private BTNode[] nodes = { };

        /// <summary>
        /// tree owner
        /// </summary>
        public object actor;
        public int Height { get; internal set; } = 0;

        public BTNode Root => nodes.Length == 0 ? null : nodes[0];

        public void Start()
        {
            if (Root == null)
            {
                throw new System.Exception("null root node.");
            }

            // TODO !!! loop all nodes 4 times
            PreProcess();

            foreach (var node in nodes)
            {
                node.OnStart();
            }

            isTreeInitialized = true;
        }

        public void Tick()
        {
            if (isTreeInitialized && mainIterator.IsRunning)
            {
                TickTimers();
                mainIterator.Tick();
            }
        }

        public void BeginTraversal()
        {
            if (isTreeInitialized && !mainIterator.IsRunning)
            {
                mainIterator.Traverse(Root);
            }
        }

        public void SetNodes(BTNode root)
        {
            SetNodes(TreeTraversal.PreOrder(root));
        }

        public void SetNodes(IEnumerable<BTNode> nodes)
        {
            this.nodes = nodes.ToArray();
            int preOrderIndex = 0;
            foreach (var node in this.nodes)
            {
                node.treeOwner = this;
                node.preOrderIndex = preOrderIndex++;
            }
        }

        public static void Interrupt(BTNode subroot)
        {
            subroot.Iterator.Interrupt(subroot);
        }

        public void Interrupt()
        {
            Interrupt(Root);
        }

        public void AddTimer(Timer timer)
        {
            activeTimers.Add(timer);
        }

        public void RemoveTimer(Timer timer)
        {
            activeTimers.Remove(timer);
        }

        private void TickTimers()
        {
            var timers = activeTimers.Data;
            var count = activeTimers.Data.Count;
            for (int i = 0; i < count; i++)
            {
                timers[i].Tick(Time.deltaTime);
            }

            activeTimers.AddAndRemoveQueued();
        }

        public int ActiveTimerCount => activeTimers.Data.Count;

        public IEnumerable<T> GetNodes<T>() where T : BTNode
        {
            return nodes.Select(node => node as T).Where(casted => casted != null);
        }

        private void PreProcess()
        {
            SetPostAndLevelOrders();
            mainIterator = new BehaviorIterator(this, 0);
            activeTimers = new UpdateList<Timer>();

            SetRootIteratorReferences();
        }

        private void SetRootIteratorReferences()
        {
            //foreach (var item in TreeTraversal.PreOrderSkipChildren(Root, n => n is ParallelComposite))
            foreach (var node in TreeTraversal.PreOrder(Root))
            {
                node.Iterator = mainIterator;
            }
        }

        private void SetPostAndLevelOrders()
        {
            int postOrderIndex = 0;
            foreach (var node in TreeTraversal.PostOrder(Root))
            {
                node.postOrderIndex = postOrderIndex++;
            }

            foreach ((BTNode node, int level) in TreeTraversal.LevelOrder(Root))
            {
                node.levelOrder = level;
                Height = level;
            }
        }

        public bool IsRunning()
        {
            return mainIterator != null && mainIterator.IsRunning;
        }

        public bool IsInitialized()
        {
            return isTreeInitialized;
        }

        public BTNode.EStatus LastStatus()
        {
            return mainIterator.LastExecutedStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="copy">copy tree</param>
        /// <param name="nodeSrc">node in origin tree</param>
        /// <returns></returns>
        public static BTNode GetIntanceVersion(BehaviorTree copy, BTNode nodeSrc)
        {
            var index = nodeSrc.preOrderIndex;
            return copy.nodes[index];
        }

        public static BehaviorTree Clone(BehaviorTree src)
        {
            var clone = CreateInstance<BehaviorTree>();
            clone.name = src.name;
            if (src.blackboard)
            {
                clone.blackboard = Instantiate(src.blackboard);
            }
            clone.SetNodes(src.nodes.Select(n => Instantiate(n)));

            //foreach (var item in clone.Nodes)
            //{
            //    Debug.LogError(item.ToString());
            //}
            //throw new Exception();

            int maxCloneNodeCount = clone.nodes.Length;
            for (int i = 0; i < maxCloneNodeCount; i++)
            {
                var nodeSrc = src.nodes[i];
                var nodeCopy = GetIntanceVersion(clone, nodeSrc);
                if (nodeCopy is BTComposite _composite)
                {
                    _composite.SetChildren(
                    Enumerable.Range(0, nodeSrc.ChildCount())
                    .Select(childIndex => GetIntanceVersion(clone, nodeSrc.GetChildAt(childIndex)))
                    .ToArray());
                }
                else if (nodeCopy is BTDecorator _decorator)
                {
                    _decorator.SetChild(GetIntanceVersion(clone, nodeSrc.GetChildAt(0)));
                }
            }

            foreach (var node in clone.nodes)
            {
                node.OnCopy();
            }

            return clone;
        }

        public void ClearStructure()
        {
            foreach (var node in nodes)
            {
                ClearChildrenStructure(node);
                node.preOrderIndex = BTNode.kInvalidOrder;
                node.childOrder = 0;
                node.Parent = null;
                node.treeOwner = null;
            }

            nodes = new BTNode[0];
        }

        private void ClearChildrenStructure(BTNode node)
        {
            if (node is BTComposite _composite)
            {
                _composite.SetChildren(new BTNode[0]);
            }
            else if (node is BTDecorator _decorator)
            {
                _decorator.SetChild(null);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Add Blackboard")]
        void AddBlackboardAsset()
        {
            if (blackboard == null && !EditorApplication.isPlaying)
            {
                blackboard = CreateInstance<Blackboard>();
                blackboard.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(blackboard, this);
            }
        }

        [HideInInspector] public Vector2 panPosition = Vector2.zero;
        [HideInInspector] public Vector2 zoomPosition = Vector2.one;
        /*[HideInInspector]*/
        public List<BTNode> unusedNodes = new List<BTNode>();
#endif
    }
}
