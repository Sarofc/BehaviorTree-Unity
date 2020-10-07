

using System.Text;

namespace Saro.BT
{
    [BTNode("Service/")]
    public class TestService : BTService
    {
        protected override void ServiceTick()
        {
            UnityEngine.Debug.Log("test...");
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);

            builder.Append("test service...");
        }
    }
}