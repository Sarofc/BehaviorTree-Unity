using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    public abstract class BTComposite : BTNode
    {
        [SerializeField]
        private BTNode[] m_Children;

        public BTNode[] Children => m_Children;

        protected EStatus m_LastChildExitStatus;

        public int CurrentChildIndex { get; private set; } = 0;

        public virtual BTNode CurrentChild()
        {
            if (CurrentChildIndex < m_Children.Length)
            {
                return m_Children[CurrentChildIndex];
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
            m_Children = nodes;
            for (int i = 0; i < m_Children.Length; i++)
            {
                m_Children[i].childOrder = i;
                m_Children[i].Parent = this;
            }
        }

        public override void OnAbort(int childIndex)
        {
            CurrentChildIndex = childIndex;
        }

        public override void OnChildExit(int childIndex, EStatus status)
        {
            CurrentChildIndex++;
            m_LastChildExitStatus = status;
        }

        public sealed override int MaxChildCount()
        {
            return int.MaxValue;
        }

        public sealed override int ChildCount()
        {
            return m_Children.Length;
        }

        public sealed override BTNode GetChildAt(int index)
        {
            return m_Children[index];
        }

        public override bool IsValid()
        {
            return m_Children != null && m_Children.Length > 0;
        }

    }

}

