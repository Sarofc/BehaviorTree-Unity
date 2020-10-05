using System;
using System.Collections.Generic;
using System.Text;

namespace Bonsai
{
    [BTNode("Decorator/", "Editor_SmallCross")]
    public class ForceSuccess : BTDecorator
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
