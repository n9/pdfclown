/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

using System;
using System.Collections;
using System.Collections.Generic;
using text = System.Text;

namespace org.pdfclown.objects
{
  /**
    <summary>PDF array object [PDF:1.6:3.2.5].</summary>
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
    {this.items.AddRange(items);}

    public PdfArray(
      IList<PdfDirectObject> items
      ) : this(items.Count)
    {this.items.AddRange(items);}
    #endregion

    #region interface
    #region public
    public override object Clone(
      File context
      )
    {
      PdfArray clone = (PdfArray)MemberwiseClone();
      {
        // Deep cloning...
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

    /**
      <summary>Gets the dereferenced value corresponding to the given index.</summary>
      <remarks>This method takes care to resolve the value returned by <see cref="this[int]">this[int]</see>.</remarks>
      <param name="index">Index of element to return.</param>
    */
    public PdfDataObject Resolve(
      int index
      )
    {return File.Resolve(this[index]);}

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
      {PdfDirectObject.WriteTo(stream,item); stream.Write(Chunk.Space);}
      // End.
      stream.Write(EndArrayChunk);
    }

    #region IList
    public int IndexOf(
      PdfDirectObject obj
      )
    {return items.IndexOf(obj);}

    public void Insert(
      int index,
      PdfDirectObject obj
      )
    {items.Insert(index,obj);}

    public void RemoveAt(
      int index
      )
    {items.RemoveAt(index);}

    public PdfDirectObject this[
      int index
      ]
    {
      get{return items[index];}
      set{items[index] = value;}
    }

    #region ICollection
    public void Add(
      PdfDirectObject obj
      )
    {items.Add(obj);}

    public void Clear(
      )
    {items.Clear();}

    public bool Contains(
      PdfDirectObject obj
      )
    {return items.Contains(obj);}

    public void CopyTo(
      PdfDirectObject[] objs,
      int index
      )
    {items.CopyTo(objs, index);}

    public int Count
    {get{return items.Count;}}

    public bool IsReadOnly
    {get{return false;}}

    public bool Remove(
      PdfDirectObject obj
      )
    {return items.Remove(obj);}

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
    #endregion
    #endregion
  }
}