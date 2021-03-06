﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace whiteMath.General
{
    public class BinomialHeap<T>
    {
        /// <summary>
        /// Returns the comparer for elements in the heap.
        /// </summary>
        public IComparer<T> Comparer { get; private set; }
        
        /// <summary>
        /// Returns the overall element count in the heap.
        /// Takes O(N) time to perform.
        /// </summary>
        public int Count { get; private set; }

        private List<TreeNode<T>> roots;

        // ----------------------------------

        private bool less(T one, T two)
        {
            return Comparer.Compare(one, two) < 0;
        }

        // ----------------------------------

        /// <summary>
        /// Сливает два биномиальных дерева и возвращает ссылку на нового родителя.
        /// </summary>
        private TreeNode<T> mergeSubtrees(TreeNode<T> p, TreeNode<T> q)
        {
            if (less(p.Value, q.Value))
            {
                q.AddChild(p);
                return q;
            }
            else
            {
                p.AddChild(q);
                return p;
            }
        }
        
        /*
        private BinaryHeap<T> mergeHeaps(BinaryHeap<T> one, BinaryHeap<T> two)
        {

        }*/
    }

    // -----------------------

    public class TreeNodeSmart<T>: ITreeNode<T>
    {
        public int Height { get; private set; }
        public int ChildrenCount { get { return this.children.Count; } }

        public bool HasParent { get { return this.Parent != null; } }
        public bool HasChildren { get { return this.children.Count > 0; } }

        /// <summary>
        /// Gets the amount of total descendants of the current root, excluding the current.
        /// The time of getting is O(1).
        /// </summary>
        public int DescendantsCount
        {
            get;
            private set;
        }

        // ----------------------------------
        // ----------- EVENTS ---------------
        // ----------------------------------

        /// <summary>
        /// Событие, вызываемое для родителя при добавлении потомка.
        /// </summary>
        private void descendantAdded()
        {
            this.DescendantsCount++;

            if (Parent != null)
                Parent.descendantAdded();
        }

        /// <summary>
        /// Событие, вызываемое для родителя при удалении потомка.
        /// </summary>
        private void descendantRemoved()
        {
            this.DescendantsCount--;

            if (Parent != null)
                Parent.descendantRemoved();
        }

        /// <summary>
        /// Событие, вызываемое для родителя при изменении высоты ребенка.
        /// </summary>
        /// <param name="oldOrder">Старая высота ребенка</param>
        /// <param name="newOrder">Новая высота ребенка</param>
        private void childOrderChanged(int oldOrder, int newOrder)
        {
            // Если старая высота + 1 равна текущей,
            // возможно, такой высоты больше нет и надо искать новую.

            if (newOrder + 1 > this.Height)
            {
                int thisOld = this.Height;

                this.Height = newOrder + 1;

                if (Parent != null)
                    Parent.childOrderChanged(thisOld, this.Height);
            }
            else if (oldOrder + 1 == this.Height)
            {
                int thisOld = this.Height;

                this.Height = children.Max(delegate(TreeNodeSmart<T> obj) { return obj.Height; }) + 1;

                if (thisOld < this.Height && Parent != null)
                    Parent.childOrderChanged(thisOld, this.Height);
            }
        }

        // -------------------------------
        // -------- ANOTHER --------------
        // -------------------------------

        private List<TreeNodeSmart<T>> children = null;

        /// <summary>
        /// Gets the parent node for the current node.
        /// If no parent node is present, null value is returned.
        /// </summary>
        public TreeNodeSmart<T> Parent          { get; private set; }
               ITreeNode<T> ITreeNode<T>.Parent { get { return this.Parent; } }
        
        /// <summary>
        /// Gets the value stored in the node.
        /// </summary>
        public T Value { get; private set; }

        public void AddChild(TreeNodeSmart<T> child)
        {
            AddChild(child, children.Count);
        }

        public void AddChild(TreeNodeSmart<T> child, int index)
        {
            this.DescendantsCount++;

            children.Insert(index, child);
            child.Parent = this;

            // Высота

            if (child.Height + 1 > this.Height)
            {
                int old = this.Height;
                this.Height = child.Height + 1;

                if (this.Parent != null)
                    Parent.childOrderChanged(old, this.Height);
            }

            if (this.Parent != null)
                Parent.descendantAdded();
        }

        ITreeNode<T> ITreeNode<T>.GetChildAt(int index)
        {
            return this.GetChildAt(index);
        }

        public TreeNodeSmart<T> GetChildAt(int index)
        {
            return children[index];
        }

        public virtual void RemoveChildAt(int index)
        {
            TreeNodeSmart<T> child = children[index];

            this.DescendantsCount--;

            children.RemoveAt(index);

            // Высота

            if (child.Height + 1 == this.Height)
            {
                int oldOrder = this.Height;

                if (children.Count > 0)
                    this.Height = children.Max(delegate(TreeNodeSmart<T> obj) { return obj.Height; }) + 1;
                else
                    this.Height = 0;

                if (oldOrder != this.Height && Parent != null)
                    Parent.childOrderChanged(oldOrder, this.Height);
            }

            if (Parent != null)
                Parent.descendantRemoved();
        }

        public TreeNodeSmart(T value)
        {
            this.Value = value;
            this.Height = 0;

            this.children = new List<TreeNodeSmart<T>>();
        }

        // ----------- root finding ----------

        /// <summary>
        /// Returns the value determining if the current node is a root node for 
        /// some tree. It is true if the node does not have a parent.
        /// </summary>
        public bool IsRoot
        {
            get { return this.Parent == null; }
        }

        /// <summary>
        /// Gets the tree root which could be either this node - if it has no parents -
        /// or the the highest grandparent who has no parent node.
        /// </summary>
        public TreeNodeSmart<T> TreeRoot
        {
            get
            {
                if (this.Parent == null) return this;
                else
                    return this.Parent.TreeRoot;
            }
        }

        // -----------------------------------
        // ----------- ToString --------------
        // -----------------------------------

        public override string ToString()
        {
            return Value.ToString();
        }

        // -----------------------------------
        // ----------- conversion ------------
        // -----------------------------------

        public static implicit operator TreeNodeSmart<T>(T value)
        {
            return new TreeNodeSmart<T>(value);
        }

        // -----------------------------------
        // ----------- swapping --------------
        // -----------------------------------

        /// <summary>
        /// Swaps the node with its child of index <paramref name="i"/>.
        /// </summary>
        /// <param name="i">The number of child index to swap with.</param>
        public void SwapWithChild(int i)
        {
            if (i < 0 || i >= this.children.Count)
                throw new ArgumentException("There is no child with such index.");

            TreeNodeSmart<T> child = this.children[i];

            // Каждому из детей ребенка говорим, что теперь я - ваш родитель.

            foreach (TreeNodeSmart<T> childChild in child.children)
                childChild.Parent = this;

            // Каждому из собственных детей говорим, что теперь ребенок - ваш родитель.

            for (int j = 0; j < children.Count; j++)
                if (i != j)
                    this.children[j].Parent = child;

            // Делаем себя своим ребенком вместо child.

            this.children[i] = this;

            // Меняемся детьми

            List<TreeNodeSmart<T>> childChildren = child.children;

            child.children = this.children;
            this.children = childChildren;

            // Родителями

            if (this.Parent != null)
            {
                this.Parent.children.Remove(this);
                this.Parent.children.Add(child);
            }

            child.Parent = this.Parent;
            this.Parent = child;

            // Высотами

            int tmp = child.Height;
            
            child.Height = this.Height;
            this.Height = tmp;

            // Количеством потомков

            tmp = child.DescendantsCount;

            child.DescendantsCount = this.DescendantsCount;
            this.DescendantsCount = tmp;

            return;
        }
    }
}
