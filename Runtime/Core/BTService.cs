using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Saro.BT.Utility;

namespace Saro.BT
{
    // TODO
    public abstract class BTService : BTAuxiliary
    {
        [BTRunTimeValue]
        public Timer timer = new Timer();
        //public bool restartTimerOnEnter = true;

        public sealed override void OnStart()
        {
            timer.OnTimeout += ServiceTick;
            timer.AutoRestart = true;
        }

        public sealed override void OnEnter()
        {
            Tree.AddTimer(timer);

            if (timer.IsDone /*|| restartTimerOnEnter*/)
            {
                timer.Start();
            }

            if(child != null) Iterator.Traverse(child);
        }

        public sealed override void OnExit()
        {
            Tree.RemoveTimer(timer);
        }

        public sealed override EStatus Run()
        {
            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        protected abstract void ServiceTick();

        public override bool IsValid()
        {
            return true;
        }

        public override void Description(StringBuilder builder)
        {
            builder.AppendFormat("Tick every {0:0.00}s", timer.GetIntervalInfo()).AppendLine();

            //builder.AppendLine(restartTimerOnEnter ? "Restart timer on enter" : "Resume timer on enter");
        }
    }

}