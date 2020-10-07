using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    public abstract class BTAuxiliary : BTNode
    {
        [SerializeField]
        protected BTNode child;

        protected BTNode Child => child;

        public void SetChild(BTNode node)
        {
            child = node;
            if (child != null)
            {
                child.Parent = this;
                child.childOrder = 0;
            }
        }

        public override void OnEnter()
        {
            if (child) Iterator.Traverse(child);
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
            if (child != null && child.IsDecorator())
            {
                child.OnCompositeParentExit();
            }
        }

        public sealed override BTNode GetChildAt(int index)
        {
            return child;
        }

        public sealed override int ChildCount()
        {
            return child != null ? 1 : 0;
        }

        public sealed override int MaxChildCount()
        {
            return 1;
        }

        public override bool IsValid()
        {
            return child != null;
        }

        public override void Error(StringBuilder builder)
        {
            builder.AppendLine("<color=red>Children must equal to 1.</color>");
        }
    }

}