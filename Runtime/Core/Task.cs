using System;
using System.Collections.Generic;

namespace Saro.BT
{
    public abstract class Task : BTNode
    {
        public sealed override void OnAbort(int childIndex) { }

        public sealed override void OnChildEnter(int childIndex) { }

        public sealed override void OnChildExit(int childIndex, EStatus status) { }

        public sealed override int ChildCount() { return 0; }

        public sealed override BTNode GetChildAt(int index) { return null; }

        public sealed override int MaxChildCount() { return 0; }
    }
}

