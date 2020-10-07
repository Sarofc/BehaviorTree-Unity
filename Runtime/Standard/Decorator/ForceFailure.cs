using System;
using System.Collections.Generic;
using System.Text;

namespace Saro.BT
{
    [BTNode("Decorator/", "Editor_SmallCross")]
    public sealed class ForceFailure : BTDecorator
    {
        public override EStatus Run()
        {
            return EStatus.Failure;
        }

        public override void Description(StringBuilder builder)
        {
            builder.Append("Always fail");
        }
    }
}
