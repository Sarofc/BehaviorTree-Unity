using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Saro.BT
{
    public static class TreeTraversal
    {
        /// <summary>
        /// 前序遍历
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<T> PreOrder<T>(T root) where T : IIterableNode<T>
        {
            var stack = new Stack<T>();
            if (root != null) stack.Push(root);
            var count = 0;
            while (stack.Count != 0)
            {
                var top = stack.Pop();
                //UnityEngine.Debug.LogError(count + "" + top);
                count++;
                yield return top;
                for (int i = top.ChildCount() - 1; i >= 0; i--)
                {
                    var child = top.GetChildAt(i);
                    stack.Push(child);
                }
            }
        }

        public static IEnumerable<T> PreOrderSkipChildren<T>(T root, Predicate<T> skip) where T : IIterableNode<T>
        {
            var stack = new Stack<T>();
            if (root != null) stack.Push(root);

            while (stack.Count != 0)
            {
                var top = stack.Pop();
                yield return top;

                if (!skip(top))
                {
                    for (int i = top.ChildCount() - 1; i >= 0; i--)
                    {
                        var child = top.GetChildAt(i);
                        stack.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// 后序遍历
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<T> PostOrder<T>(T root) where T : IIterableNode<T>
        {
            if (root != null)
            {
                var visited = new HashSet<T>();
                var stack = new Stack<T>();
                stack.Push(root);

                while (stack.Count != 0)
                {
                    var current = stack.Peek();

                    while (!visited.Contains(current) && current.ChildCount() != 0)
                    {
                        for (int i = current.ChildCount() - 1; i >= 0; i--)
                        {
                            var child = current.GetChildAt(i);
                            stack.Push(child);
                        }

                        visited.Add(current);
                        current = stack.Peek();
                    }

                    yield return stack.Pop();
                }
            }
        }

        /// <summary>
        /// 层序遍历
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <returns></returns>
        public static IEnumerable<ValueTuple<T, int>> LevelOrder<T>(T root) where T : IIterableNode<T>
        {
            var currentLevel = -1;
            var queueNodeCount = 0;
            var queue = new Queue<T>();

            if (root != null)
            {
                queue.Enqueue(root);
            }

#if UNITY_EDITOR
            int times = 0;
#endif

            while (queue.Count != 0)
            {
                if (queueNodeCount > 0)
                {
                    queueNodeCount -= 1;
                }
                if (queueNodeCount == 0)
                {
                    queueNodeCount = queue.Count;
                    currentLevel += 1;
                }

#if UNITY_EDITOR
                times++;
                if (times > 1000) throw new Exception("time out...");
#endif

                var top = queue.Dequeue();

                yield return new ValueTuple<T, int>(top, currentLevel);

                for (int i = 0; i < top.ChildCount(); i++)
                {
                    var child = top.GetChildAt(i);
                    queue.Enqueue(child);
                }
            }
        }
    }

}