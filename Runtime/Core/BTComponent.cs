using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bonsai
{
    public class BTComponent : MonoBehaviour
    {
        [SerializeField]
        private BehaviorTree treeAsset;

        private BehaviorTree treeIntance;

        private void Awake()
        {
            if (treeAsset)
            {
                treeIntance = BehaviorTree.Clone(treeAsset);
                treeIntance.actor = gameObject;
            }
            else
            {
                Debug.LogError("null tree set for.");
            }
        }

        private void Start()
        {
            treeIntance.Start();
            treeIntance.BeginTraversal();
        }

        private void Update()
        {
            treeIntance.Tick();
        }

        private void OnDestroy()
        {
            Destroy(treeIntance);
        }

        public BehaviorTree Tree => treeIntance;
    }

}