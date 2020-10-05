

namespace Bonsai
{
    [BTNode("Composites/", "Editor_Question")]
    public class Selector : BTComposite
    {
        public override EStatus Run()
        {
            if (lastChildExitStatus == EStatus.Success)
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
    }

}