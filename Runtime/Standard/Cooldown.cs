using System;
using System.Collections.Generic;
using System.Text;

namespace Bonsai
{
    [BTNode("Decorator/", "Editor_Condition")]
    public sealed class Cooldown : BTConditionalAbort
    {
        [BTRunTimeValue]
        public Timer timer = new Timer();

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (abortType == AbortType.Self || abortType == AbortType.Both)
                abortType = AbortType.LowerPriority;
        }
#endif

        public override void OnStart()
        {
            if (abortType == AbortType.Self || abortType == AbortType.Both)
                abortType = AbortType.LowerPriority;

            timer.OnTimeout += RemoveOnTimeout;
        }

        public override void OnEnter()
        {
            IsActive = true;

            if (timer.IsDone) Iterator.Traverse(child);

            if (abortType != AbortType.None)
            {
                if (!IsObserving)
                {
                    IsObserving = true;
                    OnObserverBegin();
                }
            }
        }

        public override void OnExit()
        {
            if (timer.IsDone)
            {
                Tree.AddTimer(timer);
                timer.Start();
            }

            IsActive = false;
        }

        public override void OnObserverBegin()
        {
            timer.OnTimeout += Evaluate;
        }

        public override void OnObserverEnd()
        {
            timer.OnTimeout -= Evaluate;
        }

        #region OLD

        //public override void OnEnter()
        //{
        //    if (timer.IsDone) Iterator.Traverse(child);
        //}

        //public override void OnExit()
        //{
        //    if (timer.IsDone)
        //    {
        //        Tree.AddTimer(timer);
        //        timer.Start();
        //    }
        //}

        #endregion

        public override bool Condition()
        {
            return timer.IsDone;
        }

        public override EStatus Run()
        {
            if (timer.IsRunning) return EStatus.Failure;

            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        private void RemoveOnTimeout()
        {
            Tree.RemoveTimer(timer);
        }


        public override void Description(StringBuilder builder)
        {
            base.Description(builder);
            builder.AppendLine();
            builder.AppendFormat("Lock execution for {0:0.00}s", timer.interval);
        }
    }

}