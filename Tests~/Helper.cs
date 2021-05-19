
using System.Collections.Generic;
using UnityEngine;
//using Bonsai.Core;

namespace Saro.BT.Tests
{
    public class TestNode : Task
    {
        public float? Utility { get; set; } = null;
        public EStatus ReturnStatus { get; set; }

        public const string kHistoryKey = "TraverseHistory";

        public override void OnStart()
        {
            if (!Blackboard.Contains(kHistoryKey))
            {
                Blackboard.Set(kHistoryKey, new List<int>());
            }
        }

        public override EStatus Run()
        {
            return ReturnStatus;
        }

        public override float UtilityValue()
        {
            return Utility.GetValueOrDefault(base.UtilityValue());
        }

        public override void OnEnter()
        {
            Blackboard.Get<List<int>>(kHistoryKey).Add(PreOrderIndex);
        }

        public TestNode WithUtility(float utility)
        {
            Utility = utility;
            return this;
        }
    }

    public static class Helper
    {
        public static void StartBehaviourTree(BehaviorTree tree)
        {
            tree.blackboard = ScriptableObject.CreateInstance<Blackboard>();
            tree.Start();
            tree.BeginTraversal();
        }

        public static BTNode.EStatus StepBehaviourTree(BehaviorTree tree)
        {
            if (tree.IsRunning())
            {
                tree.Tick();
            }

            return tree.LastStatus();
        }

        public static BTNode.EStatus RunBehaviourTree(BehaviorTree tree)
        {
            StartBehaviourTree(tree);
            while (tree.IsRunning())
            {
                tree.Tick();
            }

            return tree.LastStatus();
        }

        static public BehaviorTree CreateTree()
        {
            return ScriptableObject.CreateInstance<BehaviorTree>();
        }

        static public T CreateNode<T>() where T : BTNode
        {
            return ScriptableObject.CreateInstance<T>();
        }

        static public TestNode PassNode()
        {
            var node = CreateNode<TestNode>();
            node.ReturnStatus = BTNode.EStatus.Success;
            return node;
        }

        static public TestNode FailNode()
        {
            var node = CreateNode<TestNode>();
            node.ReturnStatus = BTNode.EStatus.Failure;
            return node;
        }
    }
}

