using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Bonsai
{
    public abstract class BTComposite : BTNode
    {
        [SerializeField]
        private BTNode[] children;

        public BTNode[] Children => children;

        protected EStatus lastChildExitStatus;

        public int CurrentChildIndex { get; private set; } = 0;

        public virtual BTNode CurrentChild()
        {
            if (CurrentChildIndex < children.Length)
            {
                return children[CurrentChildIndex];
            }

            return null;
        }

        public override void OnEnter()
        {
            CurrentChildIndex = 0;
            var next = CurrentChild();
            if (next != null)
            {
                Iterator.Traverse(next);
            }
        }


        public void SetChildren(BTNode[] nodes)
        {
            children = nodes;
            for (int i = 0; i < children.Length; i++)
            {
                children[i].childOrder = i;
                children[i].Parent = this;
            }
        }

        public override void OnAbort(int childIndex)
        {
            CurrentChildIndex = childIndex;
        }

        public override void OnChildExit(int childIndex, EStatus status)
        {
            CurrentChildIndex++;
            lastChildExitStatus = status;
        }

        public sealed override int MaxChildCount()
        {
            return int.MaxValue;
        }

        public sealed override int ChildCount()
        {
            return children.Length;
        }

        public sealed override BTNode GetChildAt(int index)
        {
            return children[index];
        }

        public override bool IsValid()
        {
            return children != null && children.Length > 0;
        }

        public override string OnError(StringBuilder text)
        {
            return "children length should greater than zero.";
        }
    }

}

