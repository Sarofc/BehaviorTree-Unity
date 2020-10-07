
namespace Saro.BT
{

    public interface IIterableNode<T>
    {
        T GetChildAt(int index);

        /// <summary>
        /// current child count
        /// </summary>
        /// <returns></returns>
        int ChildCount();
    }
}