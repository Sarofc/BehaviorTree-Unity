using System;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    public abstract partial class BTNode : ScriptableObject, IIterableNode<BTNode>
    {
        /// <summary>
        /// 节点运行状态
        /// </summary>
        public enum EStatus : byte
        {
            Success,
            Failure,
            Running
        }

        public const int k_InvalidOrder = -1;

        /// <summary>
        /// 树
        /// </summary>
        public BehaviorTree Tree => treeOwner;

        /// <summary>
        /// the order of the node relative to its parent.
        /// </summary>
        public int ChildOrder => childOrder;

        public int PreOrderIndex => preOrderIndex;

        public int LevelOrder => levelOrder;

        protected Blackboard Blackboard => treeOwner.blackboard;

        protected object Actor => treeOwner.actor;

        internal BehaviorTree treeOwner = null;

        internal int preOrderIndex = k_InvalidOrder;

        //internal int postOrderIndex = 0;


        internal int levelOrder = 0;

        public BTNode Parent { get; internal set; }

        /// <summary>
        /// node iterator。see
        /// <see cref="BehaviorIterator"/>
        /// <see cref="BehaviorTree"/>
        /// </summary>
        public BehaviorIterator Iterator { get; internal set; }

        protected internal int childOrder = 0;

        /// <summary>
        /// called when the tree is started
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// executes the node
        /// </summary>
        /// <returns></returns>
        public abstract EStatus Run();

        /// <summary>
        /// called when a traversal begins on the node
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// called when a traversal on the node ends
        /// </summary>
        public virtual void OnExit() { }

        public virtual float UtilityValue() { return 0f; }

        /// <summary>
        /// called when iterator abort
        /// </summary>
        /// <param name="childIndex"></param>
        public virtual void OnAbort(int childIndex) { }

        public virtual void OnChildEnter(int childIndex) { }

        public virtual void OnChildExit(int childIndex, EStatus status) { }

        public virtual void OnCompositeParentExit() { }

        /// <summary>
        /// check node is valid or not
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid() { return true; }

        /// <summary>
        /// get error string. will be usefull with 
        /// <see cref="IsValid"/>
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public virtual void Error(StringBuilder builder) { }

        [System.Obsolete("use for guard/Interruptor node, but these nodes are not supported.")]
        public virtual BTNode[] GetReferencedNodes() { return null; }

        public abstract BTNode GetChildAt(int index);

        public abstract int ChildCount();

        /// <summary>
        /// max child count has
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public abstract int MaxChildCount();

        public bool IsComposite()
        {
            return MaxChildCount() > 1;
        }

        public bool IsDecorator()
        {
            return MaxChildCount() == 1;
        }

        public bool IsTask()
        {
            return MaxChildCount() == 0;
        }

        public virtual void Description(StringBuilder builder) { }

        public override string ToString()
        {
            return $"PreOrderIndex: {PreOrderIndex} | {this.GetType()}";
        }

#if UNITY_EDITOR
        public enum EStatusEditor
        {
            Success, Failure, Running, None, Aborted, Interruption
        }

        public EStatusEditor StatusEditorResult { get; set; } = EStatusEditor.None;

        [HideInInspector] public string title;
        [HideInInspector] public string comment;
        [HideInInspector] public Vector2 nodePosition;

        public bool BreakPoint { get => m_BreakPoint; set => m_BreakPoint = value; }
        [SerializeField] private bool m_BreakPoint;

        public void OnBreakpoint()
        {
            if (m_BreakPoint) UnityEditor.EditorApplication.isPaused = m_BreakPoint;
        }

#endif
    }

}