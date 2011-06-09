/*
  Copyright 2007-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using org.pdfclown.documents;
using org.pdfclown.files;

using System;
using System.Collections;
using System.Collections.Generic;

namespace org.pdfclown.objects
{
  /**
    <summary>Name tree [PDF:1.6:3.8.5].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public abstract class NameTree<TValue>
    : PdfObjectWrapper<PdfDictionary>,
      IDictionary<PdfString,TValue>
    where TValue : PdfObjectWrapper
  {
    #region types
    #region public
    public class Enumerator
      : IEnumerator<KeyValuePair<PdfString,TValue>>
    {
      #region dynamic
      #region fields
      /**
        <summary>Current named object.</summary>
      */
      private KeyValuePair<PdfString,TValue>? current;

      /**
        <summary>Current level index.</summary>
      */
      private int levelIndex = 0;
      /**
        <summary>Stacked levels.</summary>
      */
      private Stack<object[]> levels = new Stack<object[]>();

      /**
        <summary>Current child tree nodes.</summary>
      */
      private PdfArray kids;
      /**
        <summary>Current names.</summary>
      */
      private PdfArray names;
      /**
        <summary>Current container.</summary>
      */
      private PdfIndirectObject container;

      /**
        <summary>Name tree.</summary>
      */
      private NameTree<TValue> nameTree;
      #endregion

      #region constructors
      internal Enumerator(
        NameTree<TValue> nameTree
        )
      {
        this.nameTree = nameTree;

        container = nameTree.Container;
        PdfDictionary rootNode = nameTree.BaseDataObject;
        PdfDirectObject kidsObject =  rootNode[PdfName.Kids];
        if(kidsObject == null) // Leaf node.
        {
          PdfDirectObject namesObject = rootNode[PdfName.Names];
          if(namesObject is PdfReference)
          {container = ((PdfReference)namesObject).IndirectObject;}
          names = (PdfArray)File.Resolve(namesObject);
        }
        else // Intermediate node.
        {
          if(kidsObject is PdfReference)
          {container = ((PdfReference)kidsObject).IndirectObject;}
          kids = (PdfArray)File.Resolve(kidsObject);
        }
      }
      #endregion

      #region interface
      #region public
      #region IEnumerator<KeyValuePair<PdfString,TValue>>
      KeyValuePair<PdfString,TValue> IEnumerator<KeyValuePair<PdfString,TValue>>.Current
      {
        get
        {return current.Value;}
      }

      #region IEnumerator
      public object Current
      {
        get
        {return ((IEnumerator<KeyValuePair<PdfString,TValue>>)this).Current;}
      }

      public bool MoveNext(
        )
      {return (current = GetNext()) != null;}

      public void Reset(
        )
      {throw new NotSupportedException();}
      #endregion

      #region IDisposable
      public void Dispose(
        )
      {}
      #endregion
      #endregion
      #endregion

      #region private
      private KeyValuePair<PdfString,TValue>? GetNext(
        )
      {
        /*
          NOTE: Algorithm:
          1. [Vertical, down] We have to go downward the name tree till we reach
          a names collection (leaf node).
          2. [Horizontal] Then we iterate across the names collection.
          3. [Vertical, up] When leaf-nodes scan is complete, we go upward solving
          parent nodes, repeating step 1.
        */
        while(true)
        {
          if(names == null)
          {
            if(kids == null
              || kids.Count == levelIndex) // Kids subtree complete.
            {
              if(levels.Count == 0)
                return null;

              // 3. Go upward one level.
              // Restore current level!
              object[] level = levels.Pop();
              container = (PdfIndirectObject)level[0];
              kids = (PdfArray)level[1];
              levelIndex = ((int)level[2]) + 1; // Next node (partially scanned level).
            }
            else // Kids subtree incomplete.
            {
              // 1. Go downward one level.
              // Save current level!
              levels.Push(new object[]{container,kids,levelIndex});

              // Move downward!
              PdfReference kidReference = (PdfReference)kids[levelIndex];
              container = kidReference.IndirectObject;
              PdfDictionary kid = (PdfDictionary)kidReference.DataObject;
              PdfDirectObject kidsObject = kid[PdfName.Kids];
              if(kidsObject == null) // Leaf node.
              {
                PdfDirectObject namesObject = kid[PdfName.Names];
                if(namesObject is PdfReference)
                {container = ((PdfReference)namesObject).IndirectObject;}
                names = (PdfArray)File.Resolve(namesObject);
                kids = null;
              }
              else // Intermediate node.
              {
                if(kidsObject is PdfReference)
                {container = ((PdfReference)kidsObject).IndirectObject;}
                kids = (PdfArray)File.Resolve(kidsObject);
              }
              levelIndex = 0; // First node (new level).
            }
          }
          else
          {
            if(names.Count == levelIndex) // Names complete.
            {names = null;}
            else // Names incomplete.
            {
              // 2. Object found.
              PdfString key = (PdfString)names[levelIndex];
              TValue value = nameTree.Wrap(names[levelIndex + 1], key);
              levelIndex+=2;

              return new KeyValuePair<PdfString,TValue>(key, value);
            }
          }
        }
      }
      #endregion
      #endregion
      #endregion
    }
    #endregion

    #region private
    private interface IFiller<TObject>
    {
      void Add(
        PdfArray names,
        int offset
        );

      ICollection<TObject> Collection
      {
        get;
      }
    }

    private class KeysFiller
      : IFiller<PdfString>
    {
      private ICollection<PdfString> keys = new List<PdfString>();

      public void Add(
        PdfArray names,
        int offset
        )
      {
        keys.Add(
          (PdfString)names[offset]
          );
      }

      public ICollection<PdfString> Collection
      {
        get
        {return keys;}
      }
    }

    private class ValuesFiller
      : IFiller<TValue>
    {
      private NameTree<TValue> nameTree;
      private ICollection<TValue> values = new List<TValue>();

      internal ValuesFiller(
        NameTree<TValue> nameTree
        )
      {this.nameTree = nameTree;}

      public void Add(
        PdfArray names,
        int offset
        )
      {
        values.Add(
          nameTree.Wrap(
            names[offset + 1],
            (PdfString)names[offset]
            )
          );
      }

      public ICollection<TValue> Collection
      {
        get
        {return values;}
      }
    }
    #endregion
    #endregion

    #region static
    #region fields
    /**
      Minimum number of children for each node.
    */
    private const int NodeMinSize = 5;
    /**
      Maximum number of children for each node.
    */
    private const int TreeOrder = NodeMinSize * 2;

    /**
      Minimum number of name/value items for each node.
    */
    private const int NameNodeMinSize = NodeMinSize * 2; // NOTE: Name collections are arrays of name/value pairs.
    /**
      Maximum number of name/value items for each node.
    */
    private const int NameOrder = NameNodeMinSize * 2;

    private static readonly int[] ChildrenOrders = new int[]{NameOrder, TreeOrder};
    private static readonly PdfName[] ChildrenTypeNames = new PdfName[]{PdfName.Names, PdfName.Kids};
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public NameTree(
      Document context
      ) : base(
        context,
        new PdfDictionary(
          new PdfName[]
          {PdfName.Names},
          new PdfDirectObject[]
          {new PdfArray()}
          ) // NOTE: Initial root is by-definition a leaf node.
        )
    {}

    public NameTree(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    #region IDictionary
    public void Add(
      PdfString key,
      TValue value
      )
    {Add(key,value,false);}

    public bool ContainsKey(
      PdfString key
      )
    {
      /*
        NOTE: Here we assume that any named entry has a non-null value.
      */
      return this[key] != null;
    }

    public ICollection<PdfString> Keys
    {
      get
      {
        KeysFiller filler = new KeysFiller();
        Fill(
          filler,
          (PdfReference)BaseObject
          );

        return filler.Collection;
      }
    }

    public bool Remove(
      PdfString key
      )
    {throw new NotImplementedException();}

    public TValue this[
      PdfString key
      ]
    {
      get
      {
        PdfDictionary parent = BaseDataObject;
        while(true)
        {
          PdfArray names = (PdfArray)parent.Resolve(PdfName.Names);
          if(names == null) // Intermediate node.
          {
            PdfArray kids = (PdfArray)File.Resolve(parent[PdfName.Kids]);
            int low = 0, high = kids.Count - 1;
            while(true)
            {
              if(low > high)
                return null;

              int mid = (low + high) / 2;
              PdfDictionary kid = (PdfDictionary)kids.Resolve(mid);
              PdfArray limits = (PdfArray)kid.Resolve(PdfName.Limits);
              // Compare to the lower limit!
              int comparison = key.CompareTo(limits[0]);
              if(comparison < 0)
              {high = mid - 1;}
              else
              {
                // Compare to the upper limit!
                comparison = key.CompareTo(limits[1]);
                if(comparison > 0)
                {low = mid + 1;}
                else
                {
                  // Go down one level!
                  parent = kid;
                  break;
                }
              }
            }
          }
          else // Leaf node.
          {
            int low = 0, high = names.Count;
            while(true)
            {
              if(low > high)
                return null;

              int mid = (mid = ((low + high) / 2)) - (mid % 2);
              int comparison = key.CompareTo(names[mid]);
              if(comparison < 0)
              {high = mid - 2;}
              else if(comparison > 0)
              {low = mid + 2;}
              else
              {
                // We got it!
                return Wrap(
                  names[mid + 1],
                  (PdfString)names[mid]
                  );
              }
            }
          }
        }
      }
      set
      {Add(key,value,true);}
    }

    public bool TryGetValue(
      PdfString key,
      out TValue value
      )
    {
      value = this[key];
      return value != null;
    }

    public ICollection<TValue> Values
    {
      get
      {
        ValuesFiller filler = new ValuesFiller(this);
        Fill(
          filler,
          (PdfReference)BaseObject
          );

        return filler.Collection;
      }
    }

    #region ICollection
    void ICollection<KeyValuePair<PdfString,TValue>>.Add(
      KeyValuePair<PdfString,TValue> keyValuePair
      )
    {Add(keyValuePair.Key,keyValuePair.Value);}

    public void Clear(
      )
    {Clear(BaseDataObject);}

    bool ICollection<KeyValuePair<PdfString,TValue>>.Contains(
      KeyValuePair<PdfString,TValue> keyValuePair
      )
    {return keyValuePair.Value.Equals(this[keyValuePair.Key]);}

    public void CopyTo(
      KeyValuePair<PdfString,TValue>[] keyValuePairs,
      int index
      )
    {throw new NotImplementedException();}

    public int Count
    {
      get
      {return GetCount(BaseDataObject);}
    }

    public bool IsReadOnly
    {
      get
      {return false;}
    }

    public bool Remove(
      KeyValuePair<PdfString,TValue> keyValuePair
      )
    {throw new NotSupportedException();}

    #region IEnumerable<KeyValuePair<PdfString,TValue>>
    public IEnumerator<KeyValuePair<PdfString,TValue>> GetEnumerator(
      )
    {return new Enumerator(this);}

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator(
      )
    {return this.GetEnumerator();}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion

    #region protected
    /**
      <summary>Wraps a base object within its corresponding high-level representation.</summary>
    */
    protected abstract TValue Wrap(
      PdfDirectObject baseObject,
      PdfString name
      );
    #endregion

    #region private
    /**
      <summary>Adds an entry into the tree.</summary>
      <param name="key">New entry's key.</param>
      <param name="value">New entry's value.</param>
      <param name="overwrite">Whether the entry is allowed to replace an existing one having the same key.</param>
    */
    private void Add(
      PdfString key,
      TValue value,
      bool overwrite
      )
    {
      // Get the root node!
      PdfReference rootReference = (PdfReference)BaseObject; // NOTE: Nodes MUST be indirect objects.
      PdfDictionary root = (PdfDictionary)rootReference.DataObject;

      // Ensuring the root node isn't full...
      {
        PdfName rootChildrenTypeName;
        int rootChildrenOrder;
        PdfArray rootChildren = GetChildren(root, out rootChildrenTypeName, out rootChildrenOrder);
        if(rootChildren.Count >= rootChildrenOrder) // Root node full.
        {
          // Insert the old root under the new one!
          PdfDataObject oldRootDataObject = rootReference.DataObject;
          rootReference.DataObject = root = new PdfDictionary(
              new PdfName[]
              { PdfName.Kids },
              new PdfDirectObject[]
              {
                new PdfArray(
                  new PdfDirectObject[]{File.Register(oldRootDataObject)}
                  )
              }
              );
          // Split the old root!
          SplitFullNode(
            (PdfArray)root[PdfName.Kids],
            0, // Old root's position within new root's kids.
            rootChildrenTypeName
            );
        }
      }

      // Set the entry under the root node!
      Add(key, value, overwrite, root);
    }

    /**
      <summary>Adds an entry under the given tree node.</summary>
      <param name="key">New entry's key.</param>
      <param name="value">New entry's value.</param>
      <param name="overwrite">Whether the entry is allowed to replace an existing one having the same key.</param>
      <param name="node">Current node.</param>
    */
    private void Add(
      PdfString key,
      TValue value,
      bool overwrite,
      PdfDictionary node
      )
    {
      PdfArray children = (PdfArray)File.Resolve(node[PdfName.Names]);
      if(children == null) // Intermediate node.
      {
        children = (PdfArray)File.Resolve(node[PdfName.Kids]);
        int low = 0, high = children.Count - 1;
        while(true)
        {
          bool matched = false;
          int mid = (low + high) / 2;
          PdfDictionary kid = (PdfDictionary)File.Resolve(children[mid]);
          PdfArray limits = (PdfArray)File.Resolve(kid[PdfName.Limits]);
          if(key.CompareTo(limits[0]) < 0) // Before the lower limit.
          {high = mid - 1;}
          else if(key.CompareTo(limits[1]) > 0) // After the upper limit.
          {low = mid + 1;}
          else // Limit range matched.
          {matched = true;}

          if(matched // Limit range matched.
            || low > high) // No limit range match.
          {
            PdfName kidChildrenTypeName;
            int kidChildrenOrder;
            PdfArray kidChildren = GetChildren(kid, out kidChildrenTypeName, out kidChildrenOrder);
            if(kidChildren.Count >= kidChildrenOrder) // Current node is full.
            {
              // Split the node!
              SplitFullNode(
                children,
                mid,
                kidChildrenTypeName
                );
              // Is the key before the splitted node?
              if(key.CompareTo(((PdfArray)File.Resolve(kid[PdfName.Limits]))[0]) < 0)
              {kid = (PdfDictionary)File.Resolve(children[mid]);}
            }

            Add(key, value, overwrite, kid);
            // Update the key limits!
            UpdateNodeLimits(node, children, PdfName.Kids);
            break;
          }
        }
      }
      else // Leaf node.
      {
        int childrenCount = children.Count;
        int low = 0, high = childrenCount;
        while(true)
        {
          int mid = (mid = ((low + high) / 2)) - (mid % 2);
          if(mid >= childrenCount)
          {
            // Append the entry!
            children.Add(key);
            children.Add(value.BaseObject);
            break;
          }

          int comparison = key.CompareTo(children[mid]);
          if(comparison < 0) // Before.
          {high = mid - 2;}
          else if(comparison > 0) // After.
          {low = mid + 2;}
          else // Matching entry.
          {
            if(!overwrite)
              throw new ArgumentException("Key '" + key + "' already exists.", "key");

            // Overwrite the entry!
            children[mid] = key;
            children[++mid] = value.BaseObject;
            break;
          }
          if(low > high)
          {
            // Insert the entry!
            children.Insert(low,key);
            children.Insert(++low,value.BaseObject);
            break;
          }
        }
        // Update the key limits!
        UpdateNodeLimits(node, children, PdfName.Names);
      }
    }

    /**
      <summary>Removes all the given node's children.</summary>
      <remarks>Removal affects only tree nodes: referenced objects are preserved
      to avoid inadvertently breaking possible references to them from somewhere else.</remarks>
      <param name="node">Current node.</param>
    */
    private void Clear(
      PdfDictionary node
      )
    {
      PdfName childrenTypeName;
      int childrenOrder;
      PdfArray children = GetChildren(node, out childrenTypeName, out childrenOrder);
      if(childrenTypeName.Equals(PdfName.Kids))
      {
        foreach(PdfDirectObject child in children)
        {
          Clear((PdfDictionary)File.Resolve(child));
          File.Unregister((PdfReference)child);
        }

        node[PdfName.Names] = node[childrenTypeName];
        node.Remove(childrenTypeName);
      }
      children.Clear();
      node.Remove(PdfName.Limits);
    }

    private void Fill<TObject>(
      IFiller<TObject> filler,
      PdfReference nodeReference
      )
    {
      PdfDictionary node = (PdfDictionary)nodeReference.DataObject;
      File.ResolvedObject<PdfArray> kidsObject = File.Resolve<PdfArray>(
        node[PdfName.Kids],
        nodeReference
        );
      if(kidsObject == null) // Leaf node.
      {
        File.ResolvedObject<PdfArray> namesObject = File.Resolve<PdfArray>(
          node[PdfName.Names],
          nodeReference
          );
        for(
          int index = 0,
            length = namesObject.DataObject.Count;
          index < length;
          index += 2
          )
        {
          filler.Add(
            namesObject.DataObject,
            index
            );
        }
      }
      else // Intermediate node.
      {
        foreach(PdfDirectObject kidObject in kidsObject.DataObject)
        {Fill(filler,(PdfReference)kidObject);}
      }
    }

    /**
      <summary>Gets the given node's children.</summary>
      <param name="node">Parent node.</param>
      <param name="childrenTypeName">Node's children type.</param>
      <param name="childrenOrder">Node's children order (that is maximum number of items allowed).</param>
    */
    private PdfArray GetChildren(
      PdfDictionary node,
      out PdfName childrenTypeName,
      out int childrenOrder
      )
    {
      PdfArray children = null;
      childrenTypeName = null;
      childrenOrder = 0;
      for(
        int index = 0,
          length = ChildrenTypeNames.Length;
        index < length;
        index++
        )
      {
        childrenTypeName = ChildrenTypeNames[index];
        children = (PdfArray)File.Resolve(node[childrenTypeName]);
        if(children == null)
          continue;

        childrenOrder = ChildrenOrders[index];
        break;
      }
      return children;
    }

    /**
      <summary>Gets the given node's entries count.</summary>
      <param name="node">Current node.</param>
    */
    private int GetCount(
      PdfDictionary node
      )
    {
      PdfArray children = (PdfArray)File.Resolve(node[PdfName.Names]);
      if(children == null) // Intermediate node.
      {
        children = (PdfArray)File.Resolve(node[PdfName.Kids]);

        int count = 0;
        foreach(PdfDirectObject child in children)
        {count += GetCount((PdfDictionary)File.Resolve(child));}

        return count;
      }
      else // Leaf node.
      {return (children.Count / 2);}
    }

    /**
      <summary>Splits a full node.</summary>
      <remarks>A new node is inserted at the full node's position, receiving the lower half of its children.</remarks>
      <param name="nodes">Parent nodes.</param>
      <param name="fullNodeIndex">Full node's position among the parent nodes.</param>
      <param name="childrenTypeName">Full node's children type.</param>
    */
    private void SplitFullNode(
      PdfArray nodes,
      int fullNodeIndex,
      PdfName childrenTypeName
      )
    {
      // Get the full node!
      PdfDictionary fullNode = (PdfDictionary)File.Resolve(nodes[fullNodeIndex]);
      PdfArray fullNodeChildren = (PdfArray)File.Resolve(fullNode[childrenTypeName]);

      // Create a new (sibling) node!
      PdfDictionary newNode = new PdfDictionary();
      PdfArray newNodeChildren = new PdfArray();
      newNode[childrenTypeName] = newNodeChildren;
      // Insert the new node just before the full!
      nodes.Insert(fullNodeIndex,File.Register(newNode)); // NOTE: Nodes MUST be indirect objects.

      // Transferring exceeding children to the new node...
      {
        int index = 0;
        int length;
        if(childrenTypeName.Equals(PdfName.Kids))
        {length = NodeMinSize;}
        else if(childrenTypeName.Equals(PdfName.Names))
        {length = NameNodeMinSize;}
        else // NOTE: Should NEVER happen.
        {throw new NotSupportedException(childrenTypeName + " is NOT a supported child type.");}
        while(index++ < length)
        {
          newNodeChildren.Add(fullNodeChildren[0]);
          fullNodeChildren.RemoveAt(0);
        }
      }

      // Update the key limits!
      UpdateNodeLimits(newNode, newNodeChildren, childrenTypeName);
      UpdateNodeLimits(fullNode, fullNodeChildren, childrenTypeName);
    }

    /**
      <summary>Sets the key limits of the given node.</summary>
      <param name="node">Node to update.</param>
      <param name="children">Node children.</param>
      <param name="childrenTypeName">Node's children type.</param>
    */
    private void UpdateNodeLimits(
      PdfDictionary node,
      PdfArray children,
      PdfName childrenTypeName
      )
    {
      if(childrenTypeName.Equals(PdfName.Kids))
      {
        node[PdfName.Limits] = new PdfArray(
          new PdfDirectObject[]
          {
            ((PdfArray)((PdfDictionary)File.Resolve(children[0]))[PdfName.Limits])[0],
            ((PdfArray)((PdfDictionary)File.Resolve(children[children.Count-1]))[PdfName.Limits])[1]
          }
          );
      }
      else if(childrenTypeName.Equals(PdfName.Names))
      {
        node[PdfName.Limits] = new PdfArray(
          new PdfDirectObject[]
          {
            children[0],
            children[children.Count-2]
          }
          );
      }
      else // NOTE: Should NEVER happen.
      {throw new NotSupportedException(childrenTypeName + " is NOT a supported child type.");}
    }
    #endregion
    #endregion
    #endregion
  }
}