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
        private string m_Typename;

        //private Action<Blackboard.KeyEvent> OnBlackboardChanged;

        public override void OnStart()
        {
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
            if (type != null) m_Typename = type.AssemblyQualifiedName;
            else m_Typename = "";
        }

        public void OnAfterDeserialize()
        {
            if (string.IsNullOrEmpty(m_Typename)) type = null;
            else type = Type.GetType(m_Typename);
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

        private void OnBlackboardChanged(Blackboard.KeyEvent evt)
        {
            if (evt.Key == key)
            {
                Evaluate();
            }
        }
    }

}