using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Saro.BT.Utility;

namespace Saro.BT
{

    [BTNode("Tasks/", "Editor_Timer")]
    public sealed class Wait : Task
    {
        [BTRunTimeValue]
        public Timer timer = new Timer();

        public override void OnEnter()
        {
            timer.Start();
        }

        public override EStatus Run()
        {
            timer.Tick(Time.deltaTime);

            return timer.IsDone ? EStatus.Success : EStatus.Running;
        }

        public override void Description(StringBuilder builder)
        {
            builder.AppendFormat("Wait for {0:0.00}s", timer.GetIntervalInfo());
        }
    }

}