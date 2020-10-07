using System;
using System.Collections.Generic;
using System.Text;
using Saro.BT.Utility;

namespace Saro.BT
{
    [BTNode("Decorator/", "Editor_Condition")]
    public class TimeLimit : BTDecorator
    {
        [BTRunTimeValue]
        public Timer timer = new Timer();

        private void OnValidate()
        {
            abortType = AbortType.Self;
        }

        public override void OnStart()
        {
            OnValidate();

            timer.OnTimeout += Evaluate;
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
            builder.AppendFormat("About and fail after {0:0.00}s", timer.GetIntervalInfo());
        }
    }

}