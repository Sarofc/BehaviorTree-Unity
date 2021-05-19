using System;
using System.Collections.Generic;
using UnityEngine;

namespace Saro.BT
{
    public class BTComponent : MonoBehaviour
    {
        [SerializeField]
        private BehaviorTree m_TreeAsset;

        private BehaviorTree m_TreeIntance;

        private void Awake()
        {
            if (m_TreeAsset)
            {
                m_TreeIntance = BehaviorTree.Clone(m_TreeAsset);
                m_TreeIntance.actor = gameObject;
            }
            else
            {
                Debug.LogError("null tree set for.");
            }
        }

        private void Start()
        {
            m_TreeIntance.Start();
            m_TreeIntance.BeginTraversal();
        }

        private void Update()
        {
            m_TreeIntance.Tick();
        }

        private void OnDestroy()
        {
            Destroy(m_TreeIntance);
        }

        public BehaviorTree Tree => m_TreeIntance;
    }

}