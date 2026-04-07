// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Modified to support generic TPriority type

#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Collections.Generic
{
    /// <summary>
    ///  Represents a min priority queue with generic priority type support.
    /// </summary>
    /// <typeparam name="TElement">Specifies the type of elements in the queue.</typeparam>
    /// <typeparam name="TPriority">Specifies the type of priority associated with enqueued elements.</typeparam>
    public class PriorityQueue<TElement, TPriority>
    {
        /// <summary>
        /// Represents an implicit heap-ordered complete d-ary tree, stored as an array.
        /// </summary>
        private (TElement Element, TPriority Priority)[] _nodes;

        /// <summary>
        /// Custom comparer used to order the heap.
        /// </summary>
        private readonly IComparer<TPriority>? _comparer;

        /// <summary>
        /// Lazily-initialized collection used to expose the contents of the queue.
        /// </summary>
        private UnorderedItemsCollection? _unorderedItems;

        /// <summary>
        /// The number of nodes in the heap.
        /// </summary>
        private int _size;

        /// <summary>
        /// Version updated on mutation to help validate enumerators operate on a consistent state.
        /// </summary>
        private int _version;

        /// <summary>
        /// Specifies the arity of the d-ary heap, which here is quaternary.
        /// It is assumed that this value is a power of 2.
        /// </summary>
        private const int Arity = 4;

        /// <summary>
        /// The binary logarithm of <see cref="Arity" />.
        /// </summary>
        private const int Log2Arity = 2;

        /// <summary>
        ///  Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class.
        /// </summary>
        public PriorityQueue()
        {
            _nodes = Array.Empty<(TElement, TPriority)>();
            _comparer = InitializeComparer(null);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
        ///  with the specified initial capacity.
        /// </summary>
        public PriorityQueue(int initialCapacity)
            : this(initialCapacity, comparer: null)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
        ///  with the specified custom priority comparer.
        /// </summary>
        public PriorityQueue(IComparer<TPriority>? comparer)
        {
            _nodes = Array.Empty<(TElement, TPriority)>();
            _comparer = InitializeComparer(comparer);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="PriorityQueue{TElement, TPriority}"/> class
        ///  with the specified initial capacity and custom priority comparer.
        /// </summary>
        public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer)
        {
            _nodes = new (TElement, TPriority)[initialCapacity];
            _comparer = InitializeComparer(comparer);
        }

        /// <summary>
        ///  Gets the number of elements contained in the <see cref="PriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public int Count => _size;

        /// <summary>
        ///  Gets the priority comparer used by the <see cref="PriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public IComparer<TPriority> Comparer => _comparer ?? Comparer<TPriority>.Default;

        /// <summary>
        /// Gets a collection that enumerates the elements of the queue in an unordered manner.
        /// </summary>
        public UnorderedItemsCollection UnorderedItems => _unorderedItems ??= new UnorderedItemsCollection(this);

        /// <summary>
        /// Gets the index of an element's parent.
        /// </summary>
        private static int GetParentIndex(int index) => (index - 1) >> Log2Arity;

        /// <summary>
        /// Gets the index of the first child of an element.
        /// </summary>
        private static int GetFirstChildIndex(int index) => (index << Log2Arity) + 1;

        /// <summary>
        ///  Adds the specified element with associated priority to the <see cref="PriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public void Enqueue(TElement element, TPriority priority)
        {
            int currentSize = _size;
            _version++;

            if (_nodes.Length == currentSize)
            {
                Grow(currentSize + 1);
            }

            _size = currentSize + 1;

            if (_comparer == null)
            {
                MoveUpDefaultComparer((element, priority), currentSize);
            }
            else
            {
                MoveUpCustomComparer((element, priority), currentSize);
            }
        }

        /// <summary>
        ///  Removes and returns the minimal element from the <see cref="PriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public TElement Dequeue()
        {
            if (_size == 0)
            {
                throw new System.Exception("The queue is empty");
            }

            TElement element = _nodes[0].Element;
            RemoveRootNode();
            return element;
        }

        /// <summary>
        ///  Clears all elements from the <see cref="PriorityQueue{TElement, TPriority}"/>.
        /// </summary>
        public void Clear()
        {
            if (RuntimeHelpers.IsReferenceOrContainsReferences<(TElement, TPriority)>())
            {
                Array.Clear(_nodes, 0, _size);
            }

            _size = 0;
            _version++;
        }

        /// <summary>
        /// Check if queue is empty
        /// </summary>
        public bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        /// Grows the priority queue to match the specified min capacity.
        /// </summary>
        private void Grow(int minCapacity)
        {
            Debug.Assert(_nodes.Length < minCapacity);

            const int GrowFactor = 2;
            const int MinimumGrow = 4;

            int newcapacity = GrowFactor * _nodes.Length;
            newcapacity = Math.Max(newcapacity, _nodes.Length + MinimumGrow);

            if (newcapacity < minCapacity) newcapacity = minCapacity;

            Array.Resize(ref _nodes, newcapacity);
        }

        /// <summary>
        /// Removes the node from the root of the heap
        /// </summary>
        private void RemoveRootNode()
        {
            int lastNodeIndex = --_size;
            _version++;

            if (lastNodeIndex > 0)
            {
                (TElement Element, TPriority Priority) lastNode = _nodes[lastNodeIndex];
                if (_comparer == null)
                {
                    MoveDownDefaultComparer(lastNode, 0);
                }
                else
                {
                    MoveDownCustomComparer(lastNode, 0);
                }
            }

            if (RuntimeHelpers.IsReferenceOrContainsReferences<(TElement, TPriority)>())
            {
                _nodes[lastNodeIndex] = default;
            }
        }

        /// <summary>
        /// Moves a node up in the tree to restore heap order.
        /// </summary>
        private void MoveUpDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
        {
            Debug.Assert(_comparer is null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            (TElement Element, TPriority Priority)[] nodes = _nodes;

            while (nodeIndex > 0)
            {
                int parentIndex = GetParentIndex(nodeIndex);
                (TElement Element, TPriority Priority) parent = nodes[parentIndex];

                if (Comparer<TPriority>.Default.Compare(node.Priority, parent.Priority) < 0)
                {
                    nodes[nodeIndex] = parent;
                    nodeIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }

            nodes[nodeIndex] = node;
        }

        /// <summary>
        /// Moves a node up in the tree to restore heap order.
        /// </summary>
        private void MoveUpCustomComparer((TElement Element, TPriority Priority) node, int nodeIndex)
        {
            Debug.Assert(_comparer is not null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            IComparer<TPriority> comparer = _comparer;
            (TElement Element, TPriority Priority)[] nodes = _nodes;

            while (nodeIndex > 0)
            {
                int parentIndex = GetParentIndex(nodeIndex);
                (TElement Element, TPriority Priority) parent = nodes[parentIndex];

                if (comparer.Compare(node.Priority, parent.Priority) < 0)
                {
                    nodes[nodeIndex] = parent;
                    nodeIndex = parentIndex;
                }
                else
                {
                    break;
                }
            }

            nodes[nodeIndex] = node;
        }

        /// <summary>
        /// Moves a node down in the tree to restore heap order.
        /// </summary>
        private void MoveDownDefaultComparer((TElement Element, TPriority Priority) node, int nodeIndex)
        {
            Debug.Assert(_comparer is null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            (TElement Element, TPriority Priority)[] nodes = _nodes;
            int size = _size;

            int i;
            while ((i = GetFirstChildIndex(nodeIndex)) < size)
            {
                (TElement Element, TPriority Priority) minChild = nodes[i];
                int minChildIndex = i;

                int childIndexUpperBound = Math.Min(i + Arity, size);
                while (++i < childIndexUpperBound)
                {
                    (TElement Element, TPriority Priority) nextChild = nodes[i];
                    if (Comparer<TPriority>.Default.Compare(nextChild.Priority, minChild.Priority) < 0)
                    {
                        minChild = nextChild;
                        minChildIndex = i;
                    }
                }

                if (Comparer<TPriority>.Default.Compare(node.Priority, minChild.Priority) <= 0)
                {
                    break;
                }

                nodes[nodeIndex] = minChild;
                nodeIndex = minChildIndex;
            }

            nodes[nodeIndex] = node;
        }

        /// <summary>
        /// Moves a node down in the tree to restore heap order.
        /// </summary>
        private void MoveDownCustomComparer((TElement Element, TPriority Priority) node, int nodeIndex)
        {
            Debug.Assert(_comparer is not null);
            Debug.Assert(0 <= nodeIndex && nodeIndex < _size);

            IComparer<TPriority> comparer = _comparer;
            (TElement Element, TPriority Priority)[] nodes = _nodes;
            int size = _size;

            int i;
            while ((i = GetFirstChildIndex(nodeIndex)) < size)
            {
                (TElement Element, TPriority Priority) minChild = nodes[i];
                int minChildIndex = i;

                int childIndexUpperBound = Math.Min(i + Arity, size);
                while (++i < childIndexUpperBound)
                {
                    (TElement Element, TPriority Priority) nextChild = nodes[i];
                    if (comparer.Compare(nextChild.Priority, minChild.Priority) < 0)
                    {
                        minChild = nextChild;
                        minChildIndex = i;
                    }
                }

                if (comparer.Compare(node.Priority, minChild.Priority) <= 0)
                {
                    break;
                }

                nodes[nodeIndex] = minChild;
                nodeIndex = minChildIndex;
            }

            nodes[nodeIndex] = node;
        }

        /// <summary>
        /// Initializes the custom comparer to be used internally by the heap.
        /// </summary>
        private static IComparer<TPriority>? InitializeComparer(IComparer<TPriority>? comparer)
        {
            if (typeof(TPriority).IsValueType)
            {
                if (comparer == Comparer<TPriority>.Default)
                {
                    return null;
                }

                return comparer;
            }
            else
            {
                return comparer ?? Comparer<TPriority>.Default;
            }
        }

        /// <summary>
        ///  Enumerates the contents of a <see cref="PriorityQueue{TElement, TPriority}"/>, without any ordering guarantees.
        /// </summary>
        public sealed class UnorderedItemsCollection : IReadOnlyCollection<(TElement Element, TPriority Priority)>, ICollection
        {
            internal readonly PriorityQueue<TElement, TPriority> _queue;

            internal UnorderedItemsCollection(PriorityQueue<TElement, TPriority> queue) => _queue = queue;

            public int Count => _queue._size;
            object ICollection.SyncRoot => this;
            bool ICollection.IsSynchronized => false;

            void ICollection.CopyTo(Array array, int index)
            {
                throw new System.Exception("Null Array");
            }

            /// <summary>
            ///  Enumerates the element and priority pairs of a <see cref="PriorityQueue{TElement, TPriority}"/>,
            ///  without any ordering guarantees.
            /// </summary>
            public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>
            {
                private readonly PriorityQueue<TElement, TPriority> _queue;
                private readonly int _version;
                private int _index;
                private (TElement, TPriority) _current;

                internal Enumerator(PriorityQueue<TElement, TPriority> queue)
                {
                    _queue = queue;
                    _index = 0;
                    _version = queue._version;
                    _current = default;
                }

                public void Dispose() { }

                public bool MoveNext()
                {
                    PriorityQueue<TElement, TPriority> localQueue = _queue;

                    if (_version == localQueue._version && ((uint)_index < (uint)localQueue._size))
                    {
                        _current = localQueue._nodes[_index];
                        _index++;
                        return true;
                    }

                    return MoveNextRare();
                }

                private bool MoveNextRare()
                {
                    if (_version != _queue._version)
                    {
                        throw new System.Exception("SR.InvalidOperation_EnumFailedVersion");
                    }

                    _index = _queue._size + 1;
                    _current = default;
                    return false;
                }

                public (TElement Element, TPriority Priority) Current => _current;
                object IEnumerator.Current => _current;

                void IEnumerator.Reset()
                {
                    if (_version != _queue._version)
                    {
                        throw new System.Exception("SR.InvalidOperation_EnumFailedVersion");
                    }

                    _index = 0;
                    _current = default;
                }
            }

            public Enumerator GetEnumerator() => new Enumerator(_queue);

#pragma warning disable 8603
            IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator() =>
                _queue.Count == 0 ? null :
                GetEnumerator();
#pragma warning restore 8603

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<(TElement Element, TPriority Priority)>)this).GetEnumerator();
        }
    }
}
