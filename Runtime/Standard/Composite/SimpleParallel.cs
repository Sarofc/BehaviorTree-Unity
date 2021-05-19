using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Text;

namespace Saro.BT
{
    [BTNode("Composites/", "Editor_Parallel")]
    public sealed class SimpleParallel : BTComposite
    {
        private BehaviorIterator[] m_BranchIterator;
        private EStatus[] m_ChildrenStatus;

        [SerializeField]
        private EFinishMode m_FinishMode = EFinishMode.Immediate;

        enum EFinishMode : byte
        {
            Immediate = 1,
            Delayed = 2
        }

        public override void OnStart()
        {
            if (ChildCount() != 2) throw new System.Exception("SimpleParallel's children should equal to 2");

            var count = ChildCount();
            m_ChildrenStatus = new EStatus[count];
            m_BranchIterator = new BehaviorIterator[count];
            for (int i = 0; i < count; i++)
            {
                m_BranchIterator[i] = new BehaviorIterator(Tree, levelOrder + 1);

                foreach (var node in TreeTraversal.PreOrderSkipChildren(GetChildAt(i), n => n is SimpleParallel))
                {
                    node.Iterator = m_BranchIterator[i];
                }
            }
        }

        public override void OnEnter()
        {
            for (int i = 0; i < ChildCount(); i++)
            {
                m_ChildrenStatus[i] = EStatus.Running;
                m_BranchIterator[i].Traverse(Children[i]);
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < m_BranchIterator.Length; i++)
            {
                if (m_BranchIterator[i].IsRunning)
                {
                    m_BranchIterator[i].Interrupt(Children[i]);
                }
            }
        }

        public override void OnChildExit(int childIndex, EStatus status)
        {
            m_ChildrenStatus[childIndex] = status;
        }

        public override EStatus Run()
        {
            if (m_ChildrenStatus[0] == EStatus.Failure) return EStatus.Failure;

            if (m_FinishMode == EFinishMode.Immediate)
            {
                if (m_ChildrenStatus[0] != EStatus.Running)
                {
                    return m_ChildrenStatus[0];
                }
            }
            else
            {
                if (m_ChildrenStatus[0] == EStatus.Success &&
                    m_ChildrenStatus[1] != EStatus.Running)
                {
                    return EStatus.Success;
                }
            }

            for (int i = 0; i < m_BranchIterator.Length; i++)
            {
                if (m_BranchIterator[i].IsRunning)
                {
                    m_BranchIterator[i].Tick();
                }
            }

            return EStatus.Running;
        }

        public override bool IsValid()
        {
            return Children != null && Children.Length == 2;
        }

        public override void Error(StringBuilder builder)
        {
            builder.AppendLine("<color=red>Children must equal to 2.</color>");
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);
            builder.Append("FinishMode: ").Append(m_FinishMode).AppendLine();
        }

    }
}