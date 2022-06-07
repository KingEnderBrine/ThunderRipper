using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThunderClassGenerator.Utilities
{
    public interface IParent<T>
    {
        IEnumerable<T> Children { get; }
    }

    public interface IChild<T>
    {
        T Parent { get; }
    }

    public struct RecursionItem<T>
    {
        public T Parent { get; init; }
        public T Child { get; init; }

        public RecursionItem(T parent, T child)
        {
            Parent = parent;
            Child = child;
        }
    }

    public static class Recursion
    {
        public static IEnumerable<T> Upwards<T>(T child, Func<T, T> parentFunc, bool includeSelf)
        {
            var current = includeSelf ? child : parentFunc(child);
            while (current != null)
            {
                yield return current;
                current = parentFunc(current);
            }
        }

        public static IEnumerable<T> Downwards<T>(T child, Func<T, T> parentFunc, bool includeSelf = true)
        {
            var itemStack = new Stack<T>();
            var current = includeSelf ? child : parentFunc(child);
            while (current != null)
            {
                itemStack.Push(current);
                current = parentFunc(current);
            }

            while (itemStack.Count > 0)
            {
                yield return itemStack.Pop();
            }
        }

        public static IEnumerable<T> DepthFirst<T>(T parent, Func<T, IEnumerable<T>> childrenFunc, bool includeSelf = true)
        {
            var enumeratorStack = new Stack<IEnumerator<T>>();
            var itemStack = new Stack<T>();

            enumeratorStack.Push(childrenFunc(parent)?.GetEnumerator() ?? new EmptyEnumerator<T>());
            itemStack.Push(parent);

            while (itemStack.Count > 0)
            {
                var enumerator = enumeratorStack.Peek();
                if (enumerator.MoveNext())
                {
                    var nextItem = enumerator.Current;
                    enumeratorStack.Push(childrenFunc(nextItem)?.GetEnumerator() ?? new EmptyEnumerator<T>());
                    itemStack.Push(nextItem);
                    continue;
                }

                enumeratorStack.Pop();
                var item = itemStack.Pop();
                if (itemStack.Count > 0 || includeSelf)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<RecursionItem<T>> DepthFirstParented<T>(T parent, Func<T, IEnumerable<T>> childrenFunc)
        {
            var enumeratorStack = new Stack<IEnumerator<T>>();
            var itemStack = new Stack<T>();

            enumeratorStack.Push(childrenFunc(parent)?.GetEnumerator() ?? new EmptyEnumerator<T>());
            itemStack.Push(parent);

            while (itemStack.Count > 0)
            {
                var enumerator = enumeratorStack.Peek();
                if (enumerator.MoveNext())
                {
                    var nextItem = enumerator.Current;
                    enumeratorStack.Push(childrenFunc(nextItem)?.GetEnumerator() ?? new EmptyEnumerator<T>());
                    itemStack.Push(nextItem);
                    continue;
                }

                enumeratorStack.Pop();
                var item = itemStack.Pop();
                if (itemStack.Count > 0)
                {
                    yield return new RecursionItem<T>(itemStack.Peek(), item);
                }
            }
        }

        public static IEnumerable<T> Simple<T>(T parent, Func<T, IEnumerable<T>> childrenFunc)
        {
            var stack = new Stack<T>();
            stack.Push(parent);

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                foreach (var child in childrenFunc(item) ?? Array.Empty<T>())
                {
                    stack.Push(child);
                }
                yield return item;
            }
        }
    }

    public static class ChildExtensions
    {
        public static IEnumerable<T> RecursionUpwards<T>(this T child, bool includeSelf = true) where T : IChild<T>
        {
            var current = includeSelf ? child : child.Parent;
            while (current != null)
            {
                yield return current;
                current = current.Parent;
            }
        }

        public static IEnumerable<T> RecursionDownwards<T>(this T child, bool includeSelf = true) where T : IChild<T>
        {
            var itemStack = new Stack<T>();
            var current = includeSelf ? child : child.Parent;
            while (current != null)
            {
                itemStack.Push(current);
                current = current.Parent;
            }

            while (itemStack.Count > 0)
            {
                yield return itemStack.Pop();
            }
        }
    }

    public static class ParentExtensions
    {
        public static IEnumerable<T> RecursionDepthFirst<T>(this T parent, bool includeSelf = true) where T : IParent<T>
        {
            var enumeratorStack = new Stack<IEnumerator<T>>();
            var itemStack = new Stack<T>();

            enumeratorStack.Push(parent.Children?.GetEnumerator() ?? new EmptyEnumerator<T>());
            itemStack.Push(parent);

            while (itemStack.Count > 0)
            {
                var enumerator = enumeratorStack.Peek();
                if (enumerator.MoveNext())
                {
                    var nextItem = enumerator.Current;
                    enumeratorStack.Push(nextItem.Children?.GetEnumerator() ?? new EmptyEnumerator<T>());
                    itemStack.Push(nextItem);
                    continue;
                }

                enumeratorStack.Pop();
                var item = itemStack.Pop();
                if (itemStack.Count > 0 || includeSelf)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<RecursionItem<T>> RecursionDepthFirstParented<T>(this T parent) where T : IParent<T>
        {
            var enumeratorStack = new Stack<IEnumerator<T>>();
            var itemStack = new Stack<T>();

            enumeratorStack.Push(parent.Children?.GetEnumerator() ?? new EmptyEnumerator<T>());
            itemStack.Push(parent);

            while (itemStack.Count > 0)
            {
                var enumerator = enumeratorStack.Peek();
                if (enumerator.MoveNext())
                {
                    var nextItem = enumerator.Current;
                    enumeratorStack.Push(nextItem.Children?.GetEnumerator() ?? new EmptyEnumerator<T>());
                    itemStack.Push(nextItem);
                    continue;
                }

                enumeratorStack.Pop();
                var item = itemStack.Pop();
                if (itemStack.Count > 0)
                {
                    yield return new RecursionItem<T>(itemStack.Peek(), item);
                }
            }
        }

        public static IEnumerable<T> RecursionSimple<T>(this T parent) where T: IParent<T>
        {
            var stack = new Stack<T>();
            stack.Push(parent);

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                foreach (var child in item.Children ?? Array.Empty<T>())
                {
                    stack.Push(child);
                }
                yield return item;
            }
        }
    }

    /*public class Item : IParent<Item>
    {
        private readonly List<Item> children = new List<Item>();
        public IEnumerable<Item> Children => children;
        public string Name { get; set; }

        public Item(string name) 
        {
            Name = name;
        }

        public Item(string name, IEnumerable<Item> children) : this(name)
        {
            this.children.AddRange(children);
        }
    }*/
}
