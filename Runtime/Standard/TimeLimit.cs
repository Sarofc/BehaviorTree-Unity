using System;
using System.Collections.Generic;
using System.Text;

namespace Bonsai
{
    [BTNode("Decorator/", "Editor_Condition")]
    public class TimeLimit : BTConditionalAbort
    {
        [BTRunTimeValue]
        public Timer timer = new Timer();

#if UNITY_EDITOR
        void OnEnable()
        {
            abortType = AbortType.Self;
        }
#endif

        public override void OnStart()
        {
            timer.OnTimeout += Evaluate;

            abortType = AbortType.Self; // only abort self
        }

        public override void OnEnter()
        {
            Tree.AddTimer(timer);
            timer.Start();
            base.OnEnter();
        }

        public override void OnExit()
        {
            Tree.RemoveTimer(timer);
        }

        public override bool Condition()
        {
            return !timer.IsDone;
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);
            builder.AppendLine();
            builder.AppendFormat("About and fail after {0:0.00}s", timer.interval);
        }
    }

}