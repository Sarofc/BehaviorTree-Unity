using System;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Bonsai
{
    public abstract class BTNode : ScriptableObject, IIterableNode<BTNode>
    {
        public enum EStatus : byte
        {
            Success = 1,
            Failure,
            Running
        }

        public const int kInvalidOrder = -1;

        internal BehaviorTree treeOwner = null;

        internal int preOrderIndex = kInvalidOrder;

        internal int postOrderIndex = 0;
        internal int levelOrder = 0;

        public BTNode Parent { get; internal set; }
        public BehaviorIterator Iterator { get; internal set; }

        /// <summary>
        /// the order of the node relative to its parent.
        /// </summary>
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

        public virtual void OnAbort(int childIndex) { }

        public virtual void OnChildEnter(int childIndex) { }

        public virtual void OnChildExit(int childIndex, EStatus status) { }

        public virtual void OnCompositeParentExit() { }

        public virtual void OnCopy() { }

        public virtual bool IsValid() { return true; }

        public virtual string OnError(StringBuilder builder) { return null; }

        public virtual BTNode[] GetReferencedNodes() { return null; }

        public BehaviorTree Tree => treeOwner;

        public int ChildOrder => childOrder;

        public int PreOrderIndex => preOrderIndex;

        public int LevelOrder => levelOrder;

        protected Blackboard Blackboard => treeOwner.blackboard;

        protected object Actor => treeOwner.actor;

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

        public virtual void Description(StringBuilder builder)
        {

        }

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

        public bool BreakPoint { get => breakPoint; set => breakPoint = value; }
        [SerializeField] private bool breakPoint;

        public void OnBreakpoint()
        {
            if (breakPoint) UnityEditor.EditorApplication.isPaused = breakPoint;
        }

#endif
    }

}