using System;
using System.Collections.Generic;
using System.Text;

namespace Bonsai
{
    public enum AbortType { None, LowerPriority, Self, Both }

    public abstract class BTConditionalAbort : BTDecorator
    {
        public AbortType abortType = AbortType.None;

        public bool IsObserving { get; protected set; } = false;
        protected bool IsActive { get; set; } = false;

        public abstract bool Condition();

        public virtual void OnObserverBegin() { }

        public virtual void OnObserverEnd() { }

        public override void OnEnter()
        {
            IsActive = true;

            if (abortType != AbortType.None)
            {
                if (!IsObserving)
                {
                    IsObserving = true;
                    OnObserverBegin();
                }
            }

            if (Condition())
            {
                base.OnEnter();
            }
        }

        public override void OnExit()
        {
            if (abortType == AbortType.None || abortType == AbortType.Self)
            {
                if (IsObserving)
                {
                    IsObserving = false;
                    OnObserverEnd();
                }
            }

            IsActive = false;
        }

        public sealed override void OnCompositeParentExit()
        {
            if (IsObserving)
            {
                IsObserving = false;
                OnObserverEnd();
            }

            base.OnCompositeParentExit();
        }

        public override EStatus Run()
        {
            return Iterator.LastChildExitStatus.GetValueOrDefault(EStatus.Failure);
        }

        protected void Evaluate()
        {
            var result = Condition();

            if (IsActive && !result)
            {
                AbortCurrentBranch();
            }
            else
            {
                AbortLowerPriorityBranch();
            }
        }

        private void AbortLowerPriorityBranch()
        {
            if (abortType == AbortType.LowerPriority || abortType == AbortType.Both)
            {
                GetCompositeParent(this, out BTNode compositeParent, out int branchIndex);
                if (compositeParent != null)
                {
                    if (compositeParent is BTComposite _composite)
                    {
                        bool isLowerPriority = _composite.CurrentChildIndex > branchIndex;
                        if (isLowerPriority)
                        {
                            Iterator.AbortRunningChildBranch(compositeParent, branchIndex);
                        }
                    }
                }
            }
        }

        private void AbortCurrentBranch()
        {
            if (abortType == AbortType.Self || abortType == AbortType.Both)
            {
                Iterator.AbortRunningChildBranch(Parent, ChildOrder);
            }
        }

        /// <summary>
        /// find parent composite node
        /// </summary>
        /// <param name="child"></param>
        /// <param name="compositeParent"></param>
        /// <param name="branchIndex"></param>
        private void GetCompositeParent(BTNode child, out BTNode compositeParent, out int branchIndex)
        {
            compositeParent = child.Parent;
            branchIndex = child.childOrder;

            while (compositeParent != null && !compositeParent.IsComposite())
            {
                branchIndex = compositeParent.childOrder;
                compositeParent = compositeParent.Parent;
            }
        }

        public override void Description(StringBuilder builder)
        {
            builder.AppendFormat("Aborts {0}", abortType.ToString());
        }
    }

}