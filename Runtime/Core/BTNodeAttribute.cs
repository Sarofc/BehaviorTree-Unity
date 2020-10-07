using System;
using System.Collections.Generic;

namespace Saro.BT
{

    [AttributeUsage(AttributeTargets.Class)]
    public class BTNodeAttribute : Attribute
    {
        public readonly string menuPath, texturePath;

        public BTNodeAttribute(string menuPath, string texturePath = null)
        {
            this.menuPath = menuPath;
            this.texturePath = texturePath;
        }
    }

}