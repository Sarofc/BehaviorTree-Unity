using System;
using System.Collections.Generic;
using System.Text;

namespace Saro.BT
{
    [BTNode("Decorator/", "Editor_SmallCross")]
    public sealed class ForceSuccess : BTDecorator
    {
        public override EStatus Run()
        {
            return EStatus.Success;
        }

        public override void Description(StringBuilder builder)
        {
            builder.Append("Always successful");
        }
    }
}
