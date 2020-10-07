using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Saro.BT
{
    [BTNode("Decorator/")]
    public class IsValueOfType : BTDecorator, ISerializationCallbackReceiver
    {
        [Tooltip("the key of the value to test its type.")]
        public string key;

        public Type type;

        [SerializeField]
        private string typename;

        private Action<Blackboard.KeyEvent> OnBlackboardChanged;

        public override void OnStart()
        {
            OnBlackboardChanged = delegate (Blackboard.KeyEvent e)
            {
                if (e.Key == key)
                {
                    Evaluate();
                }
            };
        }

        public override bool Condition()
        {
            if (type == null || !Blackboard.Contains(key)) return false;

            var value = Blackboard.Get(key);

            if (value == null) return false;

            return value.GetType() == type;
        }

        public void OnBeforeSerialize()
        {
            if (type != null) typename = type.AssemblyQualifiedName;
            else typename = "";
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(typename)) type = null;
            else type = Type.GetType(typename);
        }

        public override void OnObserverBegin()
        {
            Blackboard.AddObserver(OnBlackboardChanged);
        }

        public override void OnObserverEnd()
        {
            Blackboard.RemoveObserver(OnBlackboardChanged);
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);
            builder.AppendLine();

            if (key == null || key.Length == 0)
            {
                builder.Append("No key is set to check");
            }
            else
            {
                builder.AppendFormat("Blackboard key {0} is {1}", key, Saro.BT.Utility.TypeExtensions.NiceName(type));
            }
        }
    }

}