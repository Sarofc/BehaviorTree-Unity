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
        BehaviorIterator[] branchIterator;
        EStatus[] childrenStatus;

        [SerializeField]
        EFinishMode finishMode = EFinishMode.Immediate;

        enum EFinishMode : byte
        {
            Immediate = 1,
            Delayed = 2
        }

        public override void OnStart()
        {
            if (ChildCount() != 2) throw new System.Exception("SimpleParallel's children should equal to 2");

            var count = ChildCount();
            childrenStatus = new EStatus[count];
            branchIterator = new BehaviorIterator[count];
            for (int i = 0; i < count; i++)
            {
                branchIterator[i] = new BehaviorIterator(Tree, levelOrder + 1);

                foreach (var node in TreeTraversal.PreOrderSkipChildren(GetChildAt(i), n => n is SimpleParallel))
                {
                    node.Iterator = branchIterator[i];
                }
            }
        }

        public override void OnEnter()
        {
            for (int i = 0; i < ChildCount(); i++)
            {
                childrenStatus[i] = EStatus.Running;
                branchIterator[i].Traverse(Children[i]);
            }
        }

        public override void OnExit()
        {
            for (int i = 0; i < branchIterator.Length; i++)
            {
                if (branchIterator[i].IsRunning)
                {
                    branchIterator[i].Interrupt(Children[i]);
                }
            }
        }

        public override void OnChildExit(int childIndex, EStatus status)
        {
            childrenStatus[childIndex] = status;
        }

        public override EStatus Run()
        {
            if (childrenStatus[0] == EStatus.Failure) return EStatus.Failure;

            if (finishMode == EFinishMode.Immediate)
            {
                if (childrenStatus[0] != EStatus.Running)
                {
                    return childrenStatus[0];
                }
            }
            else
            {
                if (childrenStatus[0] == EStatus.Success &&
                    childrenStatus[1] != EStatus.Running)
                {
                    return EStatus.Success;
                }
            }

            for (int i = 0; i < branchIterator.Length; i++)
            {
                if (branchIterator[i].IsRunning)
                {
                    branchIterator[i].Tick();
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
            builder.Append("FinishMode: ").Append(finishMode).AppendLine();
        }

    }
}