
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    [BTNode("Tasks/", "Editor_TreeIcon")]
    public sealed class RunSubtree : Task
    {
        [Tooltip("The sub-tree to run when this task executes.")]
        public BehaviorTree subtreeAsset;

        public BehaviorTree RunningSubTree { get; private set; }

        public override void OnStart()
        {
            if (subtreeAsset)
            {
                RunningSubTree = BehaviorTree.Clone(subtreeAsset);
                RunningSubTree.actor = Actor;
                RunningSubTree.Start();
            }
        }

        public override void OnEnter()
        {
            RunningSubTree.BeginTraversal();
        }

        public override void OnExit()
        {
            if (RunningSubTree.IsRunning())
            {
                RunningSubTree.Interrupt();
            }
        }

        public override EStatus Run()
        {
            if (RunningSubTree != null)
            {
                RunningSubTree.Tick();
                return RunningSubTree.IsRunning()
                  ? EStatus.Running
                  : RunningSubTree.LastStatus();
            }

            // No tree was included. Just fail.
            return EStatus.Failure;
        }

        public override void Description(StringBuilder builder)
        {
            if (subtreeAsset != null)
            {
                builder.AppendFormat("Include {0}", subtreeAsset.name);
            }
            else
            {
                builder.Append("Tree not set");
            }
        }
    }
}