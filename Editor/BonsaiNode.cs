﻿
using System.Collections.Generic;
using System.Text;
//using Bonsai.Core;
using Saro.BT.Utility;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Saro.BT.Designer
{
    public class BonsaiNode : ScriptableObject, IIterableNode<BonsaiNode>
    {
        private static StringBuilder s_Text = new StringBuilder(1024);

        public BonsaiNode Parent { get; private set; }
        private readonly List<BonsaiNode> children = new List<BonsaiNode>();
        public IReadOnlyList<BonsaiNode> Children { get { return children; } }

        private Rect rectPosition;

        /// <summary>
        /// The rect of the node in canvas space.
        /// </summary>
        public Rect RectPositon { get { return rectPosition; } }

        private Rect contentRect;
        public Rect ContentRect { get { return contentRect; } }

        public GUIStyle HeaderStyle { get; private set; } //= CreateHeaderStyle();
        public GUIStyle BodyStyle { get; private set; } //= CreateBodyStyle();
        private GUIStyle preOrderIndexStyle;
        public GUIStyle PreOrderIndexStyle
        {
            get
            {
                if (preOrderIndexStyle == null)
                {
                    return new GUIStyle("DefaultCenteredText");
                }

                return preOrderIndexStyle;
            }
        }

        public GUIContent HeaderContent { get; } = new GUIContent();
        public GUIContent BodyContent { get; } = new GUIContent();

        public bool HasOutput { get; private set; }

        private BTNode behaviour;

        public BTNode Behaviour
        {
            get { return behaviour; }
            set
            {
                behaviour = value;
                UpdateGui();
            }
        }


        public Vector2 Position
        {
            get { return rectPosition.position; }
            set { rectPosition.position = value; }
        }

        public Vector2 Center
        {
            get { return rectPosition.center; }
            set { rectPosition.center = value; }
        }

        public Vector2 Size
        {
            get { return rectPosition.size; }
        }

        public Rect ContentRectClick
        {
            get
            {
                var prefs = BonsaiPreferences.Instance;
                float w = rectPosition.width - 2f * prefs.nodeWidthPadding;
                float h = rectPosition.height - 2f * prefs.portHeight;
                float x = rectPosition.x + (rectPosition.width - w) * 0.5f;
                float y = rectPosition.y + (RectPositon.height - h) * 0.5f;

                return new Rect(x, y, w, h);
            }
        }

        public Rect InputRect
        {
            get
            {
                float w = rectPosition.width - BonsaiPreferences.Instance.portWidthTrim;
                float h = BonsaiPreferences.Instance.portHeight;
                float x = rectPosition.x + (rectPosition.width - w) * 0.5f;
                float y = rectPosition.yMin;
                return new Rect(x, y, w, h);
            }
        }

        public Rect OutputRect
        {
            get
            {
                float w = rectPosition.width - BonsaiPreferences.Instance.portWidthTrim;
                float h = BonsaiPreferences.Instance.portHeight;
                float x = rectPosition.x + (rectPosition.width - w) * 0.5f;
                float y = rectPosition.yMax - h;
                return new Rect(x, y, w, h);
            }
        }

        public Rect BreakpointRect
        {
            get
            {
                var size = BonsaiPreferences.Instance.statusIconSize;
                return new Rect(contentRect.xMin - size, contentRect.yMin - size, size, size);
            }
        }

        public Rect PreOrderIndexRect
        {
            get
            {
                var size = BonsaiPreferences.Instance.statusIconSize * 2f;
                return new Rect(contentRect.xMax - size * 0.4f, contentRect.yMin - size * 0.4f, size, size);
            }
        }

        private void OnEnable()
        {
            HeaderStyle = CreateHeaderStyle();
            BodyStyle = CreateBodyStyle();
        }


        /// <summary>
        /// Create a new node for the first time.
        /// </summary>
        /// <param name="hasOutput">If the node should have an output.</param>
        public static BonsaiNode Create(bool hasOutput, Texture icon = null)
        {
            var node = ScriptableObject.CreateInstance<BonsaiNode>();
            node.HasOutput = hasOutput;

            if (icon)
            {
                node.HeaderContent.image = icon;
            }

            return node;
        }

        public void SetBehaviour(BTNode newBehaviour, Texture icon)
        {
            if (icon)
            {
                HeaderContent.image = icon;
            }

            // Destroy the old behaviour, no longer needed.
            Object.DestroyImmediate(behaviour, true);

            behaviour = newBehaviour;
            UpdateGui();
        }

        public void Destroy()
        {
            // Unregister with previous parent.
            SetParent(null);
            OrphanChildren();
            Object.DestroyImmediate(behaviour, true);
        }

        private void OrphanChildren()
        {
            foreach (BonsaiNode child in children)
            {
                child.Parent = null;
            }

            children.Clear();
        }

        public BonsaiNode GetChildAt(int index)
        {
            return children.Count != 0 ? children[index] : null;
        }

        public int ChildCount()
        {
            return children.Count;
        }

        public int IndexOf(BonsaiNode child)
        {
            return children.IndexOf(child);
        }

        public bool Contains(BonsaiNode child)
        {
            return children.Contains(child);
        }

        public bool IsOrphan()
        {
            return Parent == null;
        }

        public void SetParent(BonsaiNode newParent)
        {
            // Remove from previous parent.
            if (Parent != null)
            {
                Parent.children.Remove(this);
            }

            // Register with new parent.
            if (newParent != null)
            {
                if (newParent.behaviour is BTComposite)
                {
                    newParent.children.Add(this);
                }

                else if (newParent.behaviour is BTAuxiliary)
                {
                    // Replace single child.
                    newParent.OrphanChildren();
                    newParent.children.Add(this);
                }

                // else: Tasks cannot have children added.
            }

            // Set new parent
            Parent = newParent;
        }


        /// <summary>
        /// Sorts child based on their position along the x-axis.
        /// Left most children will come first in the list.
        /// </summary>
        public void SortChildren()
        {
            children.Sort((BonsaiNode left, BonsaiNode right) => left.Center.x.CompareTo(right.Center.x));
        }

        /// <summary>
        /// Returns the y coordinate of the nearest input port on the y axis.
        /// </summary>
        /// <returns></returns>
        public float GetNearestInputY()
        {
            float nearestY = float.MaxValue;
            float nearestDist = float.MaxValue;

            foreach (BonsaiNode child in children)
            {
                Vector2 childPosition = child.RectPositon.position;
                Vector2 toChild = childPosition - Position;

                float yDist = Mathf.Abs(toChild.y);

                if (yDist < nearestDist)
                {
                    nearestDist = yDist;
                    nearestY = childPosition.y;
                }
            }

            return nearestY;
        }

        /// <summary>
        /// Gets the max and min x coordinates between the children and the parent.
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="maxX"></param>
        public void GetBoundsX(out float minX, out float maxX)
        {
            minX = Center.x;
            maxX = Center.x;

            foreach (BonsaiNode child in children)
            {
                float x = child.Center.x;

                if (x < minX)
                {
                    minX = x;
                }

                else if (x > maxX)
                {
                    maxX = x;
                }
            }
        }

        #region Styles and Contents

        public void UpdateGui()
        {
            HeaderContent.text = HeaderText();
            BodyContent.text = BodyText();
            ResizeToFitContent();
        }

        public void SetIcon(Texture icon)
        {
            HeaderContent.image = icon;
        }

        private string HeaderText()
        {
            string text = behaviour.title;

            // Fall back to node name if there is no brief supplied.
            if (string.IsNullOrEmpty(text))
            {
                text = NiceName();
            }

            return text;
        }

        private string BodyText()
        {
            s_Text.Clear();

            behaviour.Description(s_Text);

            if (s_Text.Length == 0)
            {
                s_Text.Append(NiceName());
            }

            if (!string.IsNullOrEmpty(behaviour.comment))
            {
                s_Text.AppendLine();
                s_Text.AppendLine();
                s_Text.Append(behaviour.comment);
            }

            if (!behaviour.IsValid())
            {
                s_Text.AppendLine();
                behaviour.Error(s_Text);
            }

            return s_Text.ToString();
        }

        public string PreOrderIndexText()
        {
            return (behaviour != null ? behaviour.PreOrderIndex : -1).ToString();
        }

        private static GUIStyle CreateHeaderStyle()
        {
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 15;
            style.fontStyle = FontStyle.Bold;
            style.imagePosition = ImagePosition.ImageLeft;
            return style;
        }

        private static GUIStyle CreateBodyStyle()
        {
            var style = new GUIStyle();
            style.normal.textColor = Color.white;
            return style;
        }

        private void ResizeToFitContent()
        {
            var prefs = BonsaiPreferences.Instance;

            float portHeights = 2f * prefs.portHeight;
            Vector2 contentSize = MinimumRequiredContentSize();

            rectPosition.size = contentSize
              + 2f * prefs.nodeSizePadding
              + 2f * Vector2.right * prefs.nodeWidthPadding
              + Vector2.up * portHeights;

            contentRect.width = rectPosition.width - 2f * prefs.nodeWidthPadding;
            contentRect.height = rectPosition.height - portHeights;
            contentRect.x = prefs.nodeWidthPadding;
            contentRect.y = prefs.portHeight;

            // Place content relative to the content rect.
            Vector2 contentOffset = contentRect.position + prefs.nodeSizePadding;
            HeaderStyle.contentOffset = MathExtensions.Round(contentOffset);
            BodyStyle.contentOffset = MathExtensions.Round(contentOffset);

            // Round for UI Sharpness.
            contentRect = MathExtensions.Round(contentRect);
            rectPosition = MathExtensions.Round(rectPosition);

            //Debug.LogError($"contentRect: {ContentRect} rectPosition: {RectPositon} inputRect: {InputRect}");
        }

        private Vector2 MinimumRequiredContentSize()
        {
            Vector2 headerSize = HeaderContentSize();
            Vector2 bodySize = BodyContentSize();
            float maxContentWidth = Mathf.Max(headerSize.x, bodySize.x);
            float totalContentHeight = headerSize.y + bodySize.y;
            return new Vector2(maxContentWidth, totalContentHeight);
        }

        private Vector2 HeaderContentSize()
        {
            // Manually add the icon size specified in preferences.
            // This was done because using CalcSize(HeaderContent) (with the icon set in GUIContent's image)
            // caused the nodes to be incorrectly sized when opening a tree from the inspector.
            // e.g. Clicking on a GameObjects tree asset from Bonsai Tree Component.
            float iconSize = BonsaiPreferences.Instance.iconSize;
            Vector2 size = HeaderStyle.CalcSize(new GUIContent(HeaderText()));
            return new Vector2(size.x + iconSize, Mathf.Max(size.y, iconSize));
        }

        private Vector2 BodyContentSize()
        {
            return BodyStyle.CalcSize(BodyContent);
        }

        private string NiceName()
        {
            return ObjectNames.NicifyVariableName(behaviour.GetType().Name);
        }

        #endregion
    }
}
