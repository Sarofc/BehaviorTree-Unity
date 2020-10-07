
using System.Collections.Generic;

namespace Saro.BT.Designer
{
    /// <summary>
    /// View of the selection.
    /// </summary>
    public interface IReadOnlySelection
    {
        IReadOnlyList<BonsaiNode> SelectedNodes { get; }
        BonsaiNode SingleSelectedNode { get; }
        //IReadOnlyList<BTNode> Referenced { get; }

        bool IsNodeSelected(BonsaiNode node);
        //bool IsReferenced(BonsaiNode node);

        int SelectedCount { get; }
        bool IsEmpty { get; }
        bool IsSingleSelection { get; }
        bool IsMultiSelection { get; }
    }
}
