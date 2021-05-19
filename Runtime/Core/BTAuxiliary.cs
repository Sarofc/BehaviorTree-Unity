using System;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    public abstract class BTAuxiliary : BTNode
    {
        [SerializeField]
        protected BTNode m_Child;

        protected BTNode Child => m_Child;

        public void SetChild(BTNode node)
        {
            m_Child = node;
            if (m_Child != null)
            {
                m_Child.Parent = this;
                m_Child.childOrder = 0;
            }
        }

        public override void OnEnter()
        {
            if (m_Child) Iterator.Traverse(m_Child);
        }

        public sealed override void OnAbort(int childIndex)
        {
        }

        /// <summary>
        /// register event
        /// </summary>
        public virtual void OnObserverBegin() { }

        /// <summary>
        /// unregister event
        /// </summary>
        public virtual void OnObserverEnd() { }

        public override void OnCompositeParentExit()
        {
            if (m_Child != null && m_Child.IsDecorator())
            {
                m_Child.OnCompositeParentExit();
            }
        }

        public sealed override BTNode GetChildAt(int index)
        {
            return m_Child;
        }

        public sealed override int ChildCount()
        {
            return m_Child != null ? 1 : 0;
        }

        public sealed override int MaxChildCount()
        {
            return 1;
        }

        public override bool IsValid()
        {
            return m_Child != null;
        }

        public override void Error(StringBuilder builder)
        {
            builder.AppendLine("<color=red>Children must equal to 1.</color>");
        }
    }

}