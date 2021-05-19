using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Saro.BT.Utility;

namespace Saro.BT
{

    public class BehaviorTree : ScriptableObject
    {
        private BehaviorIterator m_MainIterator;

        private UpdateList<Timer> m_ActiveTimers;

        private bool m_IsTreeInitialized = false;

        public Blackboard blackboard;

        public BTNode[] Nodes => m_Nodes;
        [SerializeField]
        private BTNode[] m_Nodes = { };

        /// <summary>
        /// tree owner
        /// </summary>
        public object actor;
        public int Height { get; internal set; } = 0;

        public BTNode Root => m_Nodes.Length == 0 ? null : m_Nodes[0];

        public void Start()
        {
            if (Root == null)
            {
                throw new System.Exception("null root node.");
            }

            // TODO !!! loop all nodes 4 times
            PreProcess();

            foreach (var node in m_Nodes)
            {
                node.OnStart();
            }

            m_IsTreeInitialized = true;
        }

        public void Tick()
        {
            if (m_IsTreeInitialized && m_MainIterator.IsRunning)
            {
                TickTimers();
                m_MainIterator.Tick();
            }
        }

        public void BeginTraversal()
        {
            if (m_IsTreeInitialized && !m_MainIterator.IsRunning)
            {
                m_MainIterator.Traverse(Root);
            }
        }

        public void SetNodes(BTNode root)
        {
            SetNodes(TreeTraversal.PreOrder(root));
        }

        public void SetNodes(IEnumerable<BTNode> nodes)
        {
            this.m_Nodes = nodes.ToArray();
            int preOrderIndex = 0;
            foreach (var node in this.m_Nodes)
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
            m_ActiveTimers.Add(timer);
        }

        public void RemoveTimer(Timer timer)
        {
            m_ActiveTimers.Remove(timer);
        }

        private void TickTimers()
        {
            var timers = m_ActiveTimers.Data;
            var count = m_ActiveTimers.Data.Count;
            for (int i = 0; i < count; i++)
            {
                timers[i].Tick(Time.deltaTime);
            }

            m_ActiveTimers.AddAndRemoveQueued();
        }

        public int ActiveTimerCount => m_ActiveTimers.Data.Count;

        public IEnumerable<T> GetNodes<T>() where T : BTNode
        {
            return m_Nodes.Select(node => node as T).Where(casted => casted != null);
        }

        private void PreProcess()
        {
            SetLevelOrders();
            m_MainIterator = new BehaviorIterator(this, 0);
            m_ActiveTimers = new UpdateList<Timer>();

            SetRootIteratorReferences();
        }

        private void SetRootIteratorReferences()
        {
            //foreach (var item in TreeTraversal.PreOrderSkipChildren(Root, n => n is ParallelComposite))
            foreach (var node in TreeTraversal.PreOrder(Root))
            {
                node.Iterator = m_MainIterator;
            }
        }

        private void SetLevelOrders()
        {
            //int postOrderIndex = 0;
            //foreach (var node in TreeTraversal.PostOrder(Root))
            //{
            //    node.postOrderIndex = postOrderIndex++;
            //}

            foreach ((BTNode node, int level) in TreeTraversal.LevelOrder(Root))
            {
                node.levelOrder = level;
                Height = level;
            }
        }

        public bool IsRunning()
        {
            return m_MainIterator != null && m_MainIterator.IsRunning;
        }

        public bool IsInitialized()
        {
            return m_IsTreeInitialized;
        }

        public BTNode.EStatus LastStatus()
        {
            return m_MainIterator.LastExecutedStatus;
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
            return copy.m_Nodes[index];
        }

        public static BehaviorTree Clone(BehaviorTree src)
        {
            var clone = CreateInstance<BehaviorTree>();
            clone.name = src.name;
            if (src.blackboard)
            {
                clone.blackboard = Instantiate(src.blackboard);
            }
            clone.SetNodes(src.m_Nodes.Select(n => Instantiate(n)));

            //foreach (var item in clone.Nodes)
            //{
            //    Debug.LogError(item.ToString());
            //}
            //throw new Exception();

            int maxCloneNodeCount = clone.m_Nodes.Length;
            for (int i = 0; i < maxCloneNodeCount; i++)
            {
                var nodeSrc = src.m_Nodes[i];
                var nodeCopy = GetIntanceVersion(clone, nodeSrc);
                if (nodeCopy is BTComposite _composite)
                {
                    _composite.SetChildren(
                    Enumerable.Range(0, nodeSrc.ChildCount())
                    .Select(childIndex => GetIntanceVersion(clone, nodeSrc.GetChildAt(childIndex)))
                    .ToArray());
                }
                else if (nodeCopy is BTAuxiliary _decorator)
                {
                    _decorator.SetChild(GetIntanceVersion(clone, nodeSrc.GetChildAt(0)));
                }
            }

            return clone;
        }

        public void ClearStructure()
        {
            foreach (var node in m_Nodes)
            {
                ClearChildrenStructure(node);
                node.preOrderIndex = BTNode.k_InvalidOrder;
                node.childOrder = 0;
                node.Parent = null;
                node.treeOwner = null;
            }

            m_Nodes = new BTNode[0];
        }

        private void ClearChildrenStructure(BTNode node)
        {
            if (node is BTComposite _composite)
            {
                _composite.SetChildren(new BTNode[0]);
            }
            else if (node is BTAuxiliary _auxiliary)
            {
                _auxiliary.SetChild(null);
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
