using System;
using System.Collections.Generic;

namespace Saro.BT
{
    [BTNode("Composites/", "Editor_Arrow")]
    public class Sequence : BTComposite
    {
        public override EStatus Run()
        {
            if (m_LastChildExitStatus == EStatus.Failure) return EStatus.Failure;

            var next = CurrentChild();

            if (next == null)
            {
                return EStatus.Success;
            }
            else
            {
                Iterator.Traverse(next);
                return EStatus.Running;
            }
        }
    }
}
