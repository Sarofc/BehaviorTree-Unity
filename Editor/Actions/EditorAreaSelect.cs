
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Saro.BT.Designer
{
    public static class EditorAreaSelect
    {
        /// <summary>
        /// Returns the nodes under the area.
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="nodes"></param>
        /// <param name="startScreenSpace"></param>
        /// <param name="endScreenSpace"></param>
        /// <returns></returns>
        public static IEnumerable<BonsaiNode> NodesUnderArea(
          IEnumerable<BonsaiNode> nodes,
          Vector2 startCanvasSpace,
          Vector2 endCanvasSpace)
        {
            Rect selectionArea = SelectionArea(startCanvasSpace, endCanvasSpace);
            //if (Mathf.Abs(selectionArea.xMin - selectionArea.xMax) >= 0.5f &&
            //    Mathf.Abs(selectionArea.yMin - selectionArea.yMax) >= 0.5f)
            //{
            //    return nodes.Where(node => selectionArea.Overlaps(node.RectPositon));
            //}
            //return null;

            return nodes.Where(node =>
            {
                return selectionArea.xMax >= node.ContentRectClick.xMax &&
                selectionArea.xMin <= node.ContentRectClick.xMin &&
                selectionArea.yMin <= node.ContentRectClick.yMin &&
                selectionArea.yMax >= node.ContentRectClick.yMax;
            });
        }

        /// <summary>
        /// Returns a rectangle between the endpoints.
        /// </summary>
        /// <returns></returns>
        public static Rect SelectionArea(Vector2 start, Vector2 end)
        {
            // Need to find the proper min and max values to 
            // create a rect without negative width/height values.
            float xmin, xmax;
            float ymin, ymax;

            if (start.x < end.x)
            {
                xmin = start.x;
                xmax = end.x;
            }

            else
            {
                xmax = start.x;
                xmin = end.x;
            }

            if (start.y < end.y)
            {
                ymin = start.y;
                ymax = end.y;
            }

            else
            {
                ymax = start.y;
                ymin = end.y;
            }

            return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
        }
    }
}
