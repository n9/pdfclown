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
    private IDictionary<PdfName,PdfDirectObject> entries;

    private PdfObject parent;
    private bool updateable = true;
    private bool updated;
    private bool virtual_;
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
      Updateable = false;
      for(
        int index = 0;
        index < values.Length;
        index++
        )
      {this[keys[index]] = values[index];}
      Updateable = true;
    }

    public PdfDictionary(
      IDictionary<PdfName,PdfDirectObject> entries
      ) : this(entries.Count)
    {
      Updateable = false;
      foreach(KeyValuePair<PdfName,PdfDirectObject> entry in entries)
      {this[entry.Key] = (PdfDirectObject)Include(entry.Value);}
      Updateable = true;
    }
    #endregion

    #region interface
    #region public
    public override object Clone(
      File context
      )
    {
      PdfDictionary clone = (PdfDictionary)base.Clone(context);
      {
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

    public override PdfIndirectObject Container
    {
      get
      {return Root;}
    }

    /**
      Gets the value corresponding to the given key, forcing its instantiation in case of missing
      entry.
  
      @param key Key whose associated value is to be returned.
      @param valueClass Class to use for instantiating the value in case of missing entry.
      @since 0.1.1
    */
    public PdfDirectObject Ensure<T>(
      PdfName key
      ) where T : PdfDirectObject, new()
    {
      PdfDirectObject value = this[key];
      if(value == null)
      {
        /*
          NOTE: The null-object placeholder MUST NOT perturb the existing structure; therefore:
            - it MUST be marked as virtual in order not to unnecessarily serialize it;
            - it MUST be put into this dictionary without affecting its update status.
        */
        try
        {
          entries[key] = value = (PdfDirectObject)Include(new T());
          value.Virtual = true;
        }
        catch(Exception e)
        {throw new Exception(typeof(T).Name + " failed to instantiate.", e);}
      }
      return value;
    }

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

    public override PdfObject Parent
    {
      get
      {return parent;}
      internal set
      {parent = value;}
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

    public override bool Updateable
    {
      get
      {return updateable;}
      set
      {updateable = value;}
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
        if(entry.Value.Virtual)
          continue;

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
    {
      entries.Add(key, (PdfDirectObject)Include(value));
      Update();
    }

    public bool ContainsKey(
      PdfName key
      )
    {return entries.ContainsKey(key);}

    public ICollection<PdfName> Keys
    {
      get
      {return entries.Keys;}
    }

    public bool Remove(
      PdfName key
      )
    {
      PdfDirectObject oldValue = this[key];
      if(entries.Remove(key))
      {
        Exclude(oldValue);
        Update();
        return true;
      }
      return false;
    }

    public PdfDirectObject this[
      PdfName key
      ]
    {
      get
      {
        /*
          NOTE: This is an intentional violation of the official .NET Framework Class
          Library prescription (no exception is thrown anytime a key is not found --
          a null pointer is returned instead).
        */
        PdfDirectObject value;
        entries.TryGetValue(key,out value);
        return value;
      }
      set
      {
        if(value == null)
        {Remove(key);}
        else
        {
          PdfDirectObject oldValue = this[key];
          entries[key] = (PdfDirectObject)Include(value);
          Exclude(oldValue);
          Update();
        }
      }
    }

    public bool TryGetValue(
      PdfName key,
      out PdfDirectObject value
      )
    {return entries.TryGetValue(key,out value);}

    public ICollection<PdfDirectObject> Values
    {
      get
      {return entries.Values;}
    }

    #region ICollection
    void ICollection<KeyValuePair<PdfName,PdfDirectObject>>.Add(
      KeyValuePair<PdfName,PdfDirectObject> entry
      )
    {Add(entry.Key, entry.Value);}

    public void Clear(
      )
    {
      foreach(PdfName key in entries.Keys)
      {Remove(key);}
    }

    bool ICollection<KeyValuePair<PdfName,PdfDirectObject>>.Contains(
      KeyValuePair<PdfName,PdfDirectObject> entry
      )
    {return ((ICollection<KeyValuePair<PdfName,PdfDirectObject>>)entries).Contains(entry);}

    public void CopyTo(
      KeyValuePair<PdfName,PdfDirectObject>[] entries,
      int index
      )
    {throw new NotImplementedException();}

    public int Count
    {
      get
      {return entries.Count;}
    }

    public bool IsReadOnly
    {
      get
      {return false;}
    }

    public bool Remove(
      KeyValuePair<PdfName,PdfDirectObject> entry
      )
    {
      if(entry.Value.Equals(this[entry.Key]))
        return Remove(entry.Key);
      else
        return false;
    }

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