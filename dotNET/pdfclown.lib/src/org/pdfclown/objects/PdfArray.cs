/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown;
using org.pdfclown.bytes;
using org.pdfclown.files;
using org.pdfclown.tokens;
using org.pdfclown.util.collections.generic;

using System;
using System.Collections;
using System.Collections.Generic;
using text = System.Text;

namespace org.pdfclown.objects
{
  /**
    <summary>PDF array object, that is a one-dimensional collection of (possibly-heterogeneous)
    objects arranged sequentially [PDF:1.7:3.2.5].</summary>
  */
  public sealed class PdfArray
    : PdfDirectObject,
      IList<PdfDirectObject>
  {
    #region static
    #region fields
    private static readonly byte[] BeginArrayChunk = Encoding.Encode(Keyword.BeginArray);
    private static readonly byte[] EndArrayChunk = Encoding.Encode(Keyword.EndArray);
    #endregion
    #endregion

    #region dynamic
    #region fields
    private List<PdfDirectObject> items;

    private PdfObject parent;
    private bool updated;
    private bool virtual_;
    #endregion

    #region constructors
    public PdfArray(
      ) : this(10)
    {}

    public PdfArray(
      int capacity
      )
    {items = new List<PdfDirectObject>(capacity);}

    public PdfArray(
      params PdfDirectObject[] items
      ) : this(items.Length)
    {
      this.AddAll(items);
      Ready();
    }

    public PdfArray(
      IList<PdfDirectObject> items
      ) : this(items.Count)
    {
      this.AddAll(items);
      Ready();
    }
    #endregion

    #region interface
    #region public
    public override object Clone(
      File context
      )
    {
      PdfArray clone = (PdfArray)base.Clone(context);
      {
        clone.items = new List<PdfDirectObject>(items.Count);
        foreach(PdfDirectObject item in items)
        {clone.Add((PdfDirectObject)PdfObject.Clone(item, context));}
      }
      return clone;
    }

    public override int CompareTo(
      PdfDirectObject obj
      )
    {throw new NotImplementedException();}

    public override PdfIndirectObject Container
    {
      get
      {return Root;}
    }

    /**
      <summary>Gets the value corresponding to the given index, forcing its instantiation in case
      of missing item.</summary>
      <param name="index">Index of item to return.</param>
      <param name="valueClass Class to use for instantiating the value in case of missing entry.
      @since 0.1.1
    */
    public PdfDirectObject Ensure<T>(
      int index
      ) where T : PdfDirectObject, new()
    {
      PdfDirectObject item;
      if(index == Count
        || (item = this[index]) == null
        || !item.GetType().Equals(typeof(T)))
      {
        /*
          NOTE: The null-object placeholder MUST NOT perturb the existing structure; therefore:
            - it MUST be marked as virtual in order not to unnecessarily serialize it;
            - it MUST be put into this array without affecting its update status.
        */
        try
        {
          item = (PdfDirectObject)Include(new T());
          if(index == Count)
          {items.Add(item);}
          else if(item == null)
          {items[index] = item;}
          else
          {items.Insert(index, item);}
          item.Virtual = true;
        }
        catch(Exception e)
        {throw new Exception(typeof(T).Name + " failed to instantiate.", e);}
      }
      return item;
    }

    public override bool Equals(
      object obj
      )
    {
      return obj != null
        && obj.GetType().Equals(GetType())
        && ((PdfArray)obj).items.Equals(items);
    }

    public override int GetHashCode(
      )
    {return items.GetHashCode();}

    public override PdfObject Parent
    {
      get
      {return parent;}
      internal set
      {parent = value;}
    }

    /**
      <summary>Gets the dereferenced value corresponding to the given index.</summary>
      <remarks>This method takes care to resolve the value returned by <see cref="this[int]">this[int]</see>.</remarks>
      <param name="index">Index of item to return.</param>
    */
    public PdfDataObject Resolve(
      int index
      )
    {return File.Resolve(this[index]);}

    public override PdfIndirectObject Root
    {
      get
      {return parent != null ? parent.Root : null;}
    }

    public override string ToString(
      )
    {
      text::StringBuilder buffer = new text::StringBuilder();
      {
        // Begin.
        buffer.Append("[ ");
        // Elements.
        foreach(PdfDirectObject item in items)
        {buffer.Append(PdfDirectObject.ToString(item)).Append(" ");}
        // End.
        buffer.Append("]");
      }
      return buffer.ToString();
    }

    public override void WriteTo(
      IOutputStream stream
      )
    {
      // Begin.
      stream.Write(BeginArrayChunk);
      // Elements.
      foreach(PdfDirectObject item in items)
      {
        if(item.Virtual)
          continue;

        PdfDirectObject.WriteTo(stream,item); stream.Write(Chunk.Space);
      }
      // End.
      stream.Write(EndArrayChunk);
    }

    #region IList
    public int IndexOf(
      PdfDirectObject item
      )
    {return items.IndexOf(item);}

    public void Insert(
      int index,
      PdfDirectObject item
      )
    {
      items.Insert(index, (PdfDirectObject)Include(item));
      Update();
    }

    public void RemoveAt(
      int index
      )
    {
      PdfDirectObject oldItem = items[index];
      items.RemoveAt(index);
      Exclude(oldItem);
      Update();
    }

    public PdfDirectObject this[
      int index
      ]
    {
      get
      {return items[index];}
      set
      {
        PdfDirectObject oldItem = items[index];
        items[index] = (PdfDirectObject)Include(value);
        Exclude(oldItem);
        Update();
      }
    }

    #region ICollection
    public void Add(
      PdfDirectObject item
      )
    {
      items.Add((PdfDirectObject)Include(item));
      Update();
    }

    public void Clear(
      )
    {
      foreach(PdfDirectObject item in items)
      {Remove(item);}
    }

    public bool Contains(
      PdfDirectObject item
      )
    {return items.Contains(item);}

    public void CopyTo(
      PdfDirectObject[] items,
      int index
      )
    {this.items.CopyTo(items, index);}

    public int Count
    {
      get
      {return items.Count;}
    }

    public bool IsReadOnly
    {
      get
      {return false;}
    }

    public bool Remove(
      PdfDirectObject item
      )
    {
      if(!items.Remove(item))
        return false;

      Exclude((PdfDirectObject)item);
      Update();
      return true;
    }

    #region IEnumerable<PdfDirectObject>
    public IEnumerator<PdfDirectObject> GetEnumerator(
      )
    {return items.GetEnumerator();}

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {return ((IEnumerable<PdfDirectObject>)this).GetEnumerator();}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion

    #region protected
    protected internal override bool Updated
    {
      get
      {return updated;}
      set
      {updated = value;}
    }

    protected internal override bool Virtual
    {
      get
      {return virtual_;}
      set
      {virtual_ = value;}
    }
    #endregion
    #endregion
    #endregion
  }
}