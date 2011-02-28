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
    <summary>PDF dictionary object [PDF:1.6:3.2.6].</summary>
  */
  public sealed class PdfDictionary
    : PdfDirectObject,
      IDictionary<PdfName,PdfDirectObject>
  {
    #region static
    #region fields
    private static readonly byte[] BeginDictionaryChunk = Encoding.Encode(Keyword.BeginDictionary);
    private static readonly byte[] EndDictionaryChunk = Encoding.Encode(Keyword.EndDictionary);
    #endregion
    #endregion

    #region dynamic
    #region fields
    private Dictionary<PdfName,PdfDirectObject> entries;
    #endregion

    #region constructors
    public PdfDictionary(
      )
    {entries = new Dictionary<PdfName,PdfDirectObject>();}

    public PdfDictionary(
      int capacity
      )
    {entries = new Dictionary<PdfName,PdfDirectObject>(capacity);}

    public PdfDictionary(
      PdfName[] keys,
      PdfDirectObject[] values
      ) : this(values.Length)
    {
      for(
        int index = 0;
        index < values.Length;
        index++
        )
      {this[keys[index]] = values[index];}
    }
    #endregion

    #region interface
    #region public
    public override object Clone(
      File context
      )
    {
      PdfDictionary clone = (PdfDictionary)MemberwiseClone();
      {
        // Deep cloning...
        clone.entries = new Dictionary<PdfName,PdfDirectObject>(entries.Count);
        foreach(KeyValuePair<PdfName,PdfDirectObject> entry in entries)
        {clone[entry.Key] = (PdfDirectObject)PdfObject.Clone(entry.Value, context);}
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
        && ((PdfDictionary)obj).entries.Equals(entries);
    }

    public override int GetHashCode(
      )
    {return entries.GetHashCode();}

    /**
      Gets the key associated to a given value.
    */
    public PdfName GetKey(
      PdfDirectObject value
      )
    {
      /*
        NOTE: Current PdfDictionary implementation doesn't support bidirectional
        maps, to say that the only currently-available way to retrieve a key from a
        value is to iterate the whole map (really poor performance!).
        NOTE: Complex high-level matches are not verified (too expensive!), to say that
        if the searched high-level object (font, xobject, colorspace etc.) has a
        PdfReference base object while some high-level objects in the
        collection have other direct type (PdfName, for example) base objects, they
        won't match in any case (even if they represent the SAME high-level object --
        but that should be a rare case...).
      */
      foreach(KeyValuePair<PdfName,PdfDirectObject> entry in entries)
      {
        if(entry.Value.Equals(value))
          return entry.Key; // Found.
      }
      return null; // Not found.
    }

    /**
      <summary>Gets the dereferenced value corresponding to the given key.</summary>
      <remarks>This method takes care to resolve the value returned by <see cref="this[PdfName]">this[PdfName]</see>.</remarks>
      <param name="key">Key whose associated value is to be returned.</param>
      <returns>null, if the map contains no mapping for this key.</returns>
    */
    public PdfDataObject Resolve(
      PdfName key
      )
    {return File.Resolve(this[key]);}

    public override string ToString(
      )
    {
      text::StringBuilder buffer = new text::StringBuilder();
      {
        // Begin.
        buffer.Append("<< ");
        // Entries.
        foreach(KeyValuePair<PdfName,PdfDirectObject> entry in entries)
        {
          // Entry...
          // ...key.
          buffer.Append(entry.Key.ToString()).Append(" ");
          // ...value.
          buffer.Append(PdfDirectObject.ToString(entry.Value)).Append(" ");
        }
        // End.
        buffer.Append(">>");
      }
      return buffer.ToString();
    }

    public override void WriteTo(
      IOutputStream stream
      )
    {
      // Begin.
      stream.Write(BeginDictionaryChunk);
      // Entries.
      foreach(KeyValuePair<PdfName,PdfDirectObject> entry in entries)
      {
        // Entry...
        // ...key.
        entry.Key.WriteTo(stream); stream.Write(Chunk.Space);
        // ...value.
        PdfDirectObject.WriteTo(stream, entry.Value); stream.Write(Chunk.Space);
      }
      // End.
      stream.Write(EndDictionaryChunk);
    }

    #region IDictionary
    public void Add(
      PdfName key,
      PdfDirectObject value
      )
    {entries.Add(key,value);}

    public bool ContainsKey(
      PdfName key
      )
    {return entries.ContainsKey(key);}

    public ICollection<PdfName> Keys
    {get{return entries.Keys;}}

    public bool Remove(
      PdfName key
      )
    {return entries.Remove(key);}

    public PdfDirectObject this[
      PdfName key
      ]
    {
      get
      {
        /*
          NOTE: This is an intentional violation of the official .NET Framework Class
          Library prescription.
          It's way too idiotic to throw an exception anytime a key is not found,
          as a dictionary is somewhat fuzzier than a list: using lists you have just
          1 boundary to keep in mind and an out-of-range exception is feasible,
          whilst dictionaries have so many boundaries as their entries and using them
          accordingly to the official fashion is a real nightmare!
          My loose implementation prizes client coding smoothness and concision,
          against cumbersome exception handling blocks or ugly TryGetValue()
          invocations: unfound keys are treated happily returning a default (null) value.
          If the user is interested in verifying whether such result represents
          a non-existing key or an actual null object, it suffices to query
          ContainsKey() method (a nice application of the KISS principle ;-)).
        */
        PdfDirectObject value; entries.TryGetValue(key,out value); return value;
      }
      set
      {entries[key] = value;}
    }

    public bool TryGetValue(
      PdfName key,
      out PdfDirectObject value
      )
    {return entries.TryGetValue(key,out value);}

    public ICollection<PdfDirectObject> Values
    {get{return entries.Values;}}

    #region ICollection
    void ICollection<KeyValuePair<PdfName,PdfDirectObject>>.Add(
      KeyValuePair<PdfName,PdfDirectObject> keyValuePair
      )
    {((ICollection<KeyValuePair<PdfName,PdfDirectObject>>)entries).Add(keyValuePair);}

    public void Clear(
      )
    {entries.Clear();}

    bool ICollection<KeyValuePair<PdfName,PdfDirectObject>>.Contains(
      KeyValuePair<PdfName,PdfDirectObject> keyValuePair
      )
    {return ((ICollection<KeyValuePair<PdfName,PdfDirectObject>>)entries).Contains(keyValuePair);}

    public void CopyTo(
      KeyValuePair<PdfName,PdfDirectObject>[] keyValuePairs,
      int index
      )
    {throw new NotImplementedException();}

    public int Count
    {get{return entries.Count;}}

    public bool IsReadOnly
    {get{return false;}}

    public bool Remove(
      KeyValuePair<PdfName,PdfDirectObject> keyValuePair
      )
    {return ((ICollection<KeyValuePair<PdfName,PdfDirectObject>>)entries).Remove(keyValuePair);}

    #region IEnumerable<KeyValuePair<PdfName,PdfDirectObject>>
    IEnumerator<KeyValuePair<PdfName,PdfDirectObject>> IEnumerable<KeyValuePair<PdfName,PdfDirectObject>>.GetEnumerator(
      )
    {return entries.GetEnumerator();}

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator(
      )
    {return ((IEnumerable<KeyValuePair<PdfName,PdfDirectObject>>)this).GetEnumerator();}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
  }
}