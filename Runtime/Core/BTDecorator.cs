using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bonsai
{
    public abstract class BTDecorator : BTNode
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
    }

}