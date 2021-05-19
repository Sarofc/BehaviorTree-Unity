

using System.Text;

namespace Saro.BT
{
    [BTNode("Composites/", "Editor_Question")]
    public class Selector : BTComposite
    {
        public override EStatus Run()
        {
            if (m_LastChildExitStatus == EStatus.Success)
            {
                return EStatus.Success;
            }

            var nextChild = CurrentChild();

            if (nextChild == null)
            {
                return EStatus.Failure;
            }
            else
            {
                Iterator.Traverse(nextChild);
                return EStatus.Running;
            }
        }

        public override void Description(StringBuilder builder)
        {
            base.Description(builder);
        }

        public override void Error(StringBuilder builder)
        {
            builder.AppendLine("<color=red>Children must greater than 0.</color>");
        }
    }

}