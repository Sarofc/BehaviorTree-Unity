
namespace Saro.BT.Designer
{
    public interface IUndoableAction
    {
        void Undo();
        void Redo();
    }
}
