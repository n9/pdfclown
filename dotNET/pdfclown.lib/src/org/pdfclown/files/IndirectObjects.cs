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

using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.objects;
using org.pdfclown.tokens;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace org.pdfclown.files
{
  /**
    <summary>Collection of the <b>alive indirect objects</b> available inside the
    file.</summary>
    <remarks>
      <para>According to the PDF spec, <i>indirect object entries may be free
      (no data object associated) or in-use (data object associated)</i>.</para>
      <para>We can effectively subdivide indirect objects in two possibly-overriding
      collections: the <b>original indirect objects</b> (coming from the associated
      preexisting file) and the <b>newly-registered indirect objects</b> (coming
      from new data objects or original indirect objects manipulated during the
      current session).</para>
      <para><i>To ensure that the modifications applied to an original indirect object
      are committed to being persistent</i> is critical that the modified original
      indirect object is newly-registered (practically overriding the original
      indirect object).</para>
      <para><b>Alive indirect objects</b> encompass all the newly-registered ones plus
      not-overridden original ones.</para>
    </remarks>
  */
  public sealed class IndirectObjects
    : IList<PdfIndirectObject>
  {
    #region dynamic
    #region fields
    /**
      <summary>Associated file.</summary>
    */
    private File file;

    /**
      <summary>Map of matching references of imported indirect objects.</summary>
      <remarks>
        <para>This collection is used to prevent duplications among imported
        indirect objects.</para>
        <para><code>Key</code> is the external indirect object hashcode, <code>Value</code> is the
        matching internal indirect object.</para>
      </remarks>
    */
    private Dictionary<int,PdfIndirectObject> importedObjects = new Dictionary<int,PdfIndirectObject>();
    /**
      <summary>Collection of newly-registered indirect objects.</summary>
    */
    private SortedDictionary<int,PdfIndirectObject> modifiedObjects = new SortedDictionary<int,PdfIndirectObject>();
    /**
      <summary>Collection of instantiated original indirect objects.</summary>
      <remarks>This collection is used as a cache to avoid unconsistent parsing duplications.</remarks>
    */
    private SortedDictionary<int,PdfIndirectObject> wokenObjects = new SortedDictionary<int,PdfIndirectObject>();

    /**
      <summary>Object counter.</summary>
    */
    private int lastObjectNumber;
    /**
      <summary>Offsets of the original indirect objects inside the associated file
      (to say: implicit collection of the original indirect objects).</summary>
      <remarks>This information is vital to randomly retrieve the indirect-object
      persistent representation inside the associated file.</remarks>
    */
    private SortedDictionary<int,XRefEntry> xrefEntries;
    #endregion

    #region constructors
    internal IndirectObjects(
      File file,
      SortedDictionary<int,XRefEntry> xrefEntries
      )
    {
      this.file = file;
      this.xrefEntries = xrefEntries;
      if(this.xrefEntries == null) // No original indirect objects.
      {
        // Register the leading free-object!
        /*
          NOTE: Mandatory head of the linked list of free objects
          at object number 0 [PDF:1.6:3.4.3].
        */
        lastObjectNumber = 0;
        modifiedObjects[lastObjectNumber] = new PdfIndirectObject(
          this.file,
          null,
          new XRefEntry(
            lastObjectNumber,
            XRefEntry.GenerationUnreusable,
            0,
            XRefEntry.UsageEnum.Free
            )
          );
      }
      else
      {
        // Adjust the object counter!
        lastObjectNumber = xrefEntries.Keys.Last();
      }
    }
    #endregion

    #region interface
    #region public
    public File File
    {
      get
      {return file;}
    }

    /**
      <summary>Register an <b>internal data object</b>.</summary>
      <remarks>
        Alternatives:
        <list type="bullet">
          <item>To register a <b>modified internal indirect object</b>, use
          <see cref="this"> indexer</see>.</item>
          <item>To register an <b>external indirect object</b>, use
          <see cref="Add(PdfIndirectObject)"/>.</item>
        </list>
      </remarks>
    */
    public PdfIndirectObject Add(
      PdfDataObject obj
      )
    {
      // Register a new indirect object wrapping the data object inside!
      lastObjectNumber++;
      PdfIndirectObject indirectObject = modifiedObjects[lastObjectNumber] = new PdfIndirectObject(
        file,
        obj,
        new XRefEntry(lastObjectNumber, 0)
        );
      return indirectObject;
    }

    /**
      <summary>Registers and gets an <b>external indirect object</b>.</summary>
      <remarks>
        <para>External indirect objects come from alien PDF files. <i>This
        is a powerful way to import the content of one file into another</i>.</para>
        <para>Alternatives:</para>
        <list type="bullet">
          <item>To register a <b>modified internal indirect object</b>, use
          <see cref="this"> indexer</see>.</item>
          <item>To register an <b>internal data object</b>, use
          <see cref="Add(PdfDataObject)"/>.</item>
        </list>
      </remarks>
    */
    public PdfIndirectObject Add(
      PdfIndirectObject obj
      )
    {
      PdfIndirectObject indirectObject;
      // Hasn't the external indirect object been imported yet?
      if(!importedObjects.TryGetValue(obj.GetHashCode(),out indirectObject))
      {
        // Keep track of the imported indirect object!
        importedObjects.Add(
          obj.GetHashCode(),
          indirectObject = Add((PdfDataObject)obj.DataObject.Clone(file)) // Registers the clone of the data object corresponding to the external indirect object.
          );
      }
      return indirectObject;
    }

    public bool IsEmpty(
      )
    {
      /*
        NOTE: Semantics of the indirect objects collection imply that the collection is considered
        empty in any case no in-use object is available.
      */
      foreach(PdfIndirectObject obj in this)
      {
        if(obj.IsInUse())
          return false;
      }
      return true;
    }

    #region IList
    public int IndexOf(
      PdfIndirectObject obj
      )
    {
      // Is this indirect object associated to this file?
      if(obj.File != file)
        return -1;

      return obj.Reference.ObjectNumber;
    }

    public void Insert(
      int index,
      PdfIndirectObject obj
      )
    {throw new NotSupportedException();}

    public void RemoveAt(
      int index
      )
    {
      /*
        NOTE: Acrobat 6.0 and later (PDF 1.5+) DO NOT use the free list to recycle object numbers;
        new objects are assigned new numbers [PDF:1.6:H.3:16].
        According to such an implementation note, we simply mark the removed object as 'not-reusable'
        newly-freed entry, neglecting both to add it to the linked list of free entries
        and to increment by 1 its generation number.
      */
      Update(
        new PdfIndirectObject(
          file,
          null,
          new XRefEntry(
            index,
            XRefEntry.GenerationUnreusable,
            0,
            XRefEntry.UsageEnum.Free
            )
          )
        );
    }

    /**
      <summary>Gets/Sets an indirect object with the specified object number.</summary>
      <remarks>
        <para>This indexer's setter is currently limited to <b>internal indirect
        objects</b>: <i>use it to register modified internal indirect objects only</i>.
        If you need to register alternative-type objects, consider the following
        methods:</para>
        <list type="bullet">
          <item>to register an <b>internal data object</b>, use <see cref="Add(PdfDataObject)"/>.</item>
          <item>to register an <b>external indirect object</b>, use <see cref="Add(PdfIndirectObject)"/>.</item>
        </list>
      </remarks>
      <returns>A <see cref="org.pdfclown.objects.PdfIndirectObject"/>.</returns>
    */
    public PdfIndirectObject this[
      int index
      ]
    {
      get
      {
        PdfIndirectObject obj;
        if(!modifiedObjects.TryGetValue(index, out obj))
        {
          if(!wokenObjects.TryGetValue(index, out obj))
          {
            XRefEntry xrefEntry;
            if(!xrefEntries.TryGetValue(index, out xrefEntry))
            {
              if(index > lastObjectNumber)
                return null;

              /*
                NOTE: The cross-reference table (comprising the original cross-reference section and all update sections)
                MUST contain one entry for each object number from 0 to the maximum object number used in the file, even
                if one or more of the object numbers in this range do not actually occur in the file.
                However, for resilience purposes missing entries are treated as free ones.
              */
              xrefEntries[index] = xrefEntry = new XRefEntry(
                index,
                XRefEntry.GenerationUnreusable,
                0,
                XRefEntry.UsageEnum.Free
                );
            }

            // Awake the object!
            /*
              NOTE: This operation allows to keep a consistent state across the whole session,
              avoiding multiple incoherent instantiations of the same original indirect object.
            */
            wokenObjects[index] = obj = new PdfIndirectObject(file, null, xrefEntry);
          }
        }
        return obj;
      }
      set
      {throw new NotSupportedException();}
    }

    #region ICollection
    /**
      <summary>Registers an <b>external indirect object</b>.</summary>
      <remarks>
        <para>External indirect objects come from alien PDF files. <i>This is
        a powerful way to import the content of one file into another</i>.</para>
        <para>Alternatives:</para>
        <list type="bullet">
          <item>To register and get an <b>external indirect object</b>, use <see cref="Add(PdfIndirectObject)"/>.</item>
          <item>To register a <b>modified internal indirect object</b>, use <see cref="this"> indexer</see>.</item>
          <item>To register an <b>internal data object</b>, use <see cref="Add(PdfDataObject)"/>.</item>
        </list>
      </remarks>
    */
    void ICollection<PdfIndirectObject>.Add(
      PdfIndirectObject obj
      )
    {Add(obj);}

    public void Clear(
      )
    {throw new NotSupportedException();}

    public bool Contains(
      PdfIndirectObject obj
      )
    {throw new NotSupportedException();}

    public void CopyTo(
      PdfIndirectObject[] objs,
      int index
      )
    {throw new NotSupportedException();}

    /**
      <summary>Gets the number of entries available (both in-use and free) in the
      collection.</summary>
      <returns>The number of entries available in the collection.</returns>
    */
    public int Count
    {
      get
      {return (lastObjectNumber + 1);}
    }

    public bool IsReadOnly
    {
      get
      {return false;}
    }

    public bool Remove(
      PdfIndirectObject obj
      )
    {
      try
      {
        RemoveAt(
          ((PdfIndirectObject)obj).Reference.ObjectNumber
          );
        return true;
      }
      catch
      {return false;}
    }

    #region IEnumerable<ContentStream>
    public IEnumerator<PdfIndirectObject> GetEnumerator(
      )
    {
      for(int index = 0; index < this.Count; index++)
      {yield return this[index];}
    }

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator(
      )
    {return this.GetEnumerator();}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion

    #region internal
    internal SortedDictionary<int,PdfIndirectObject> ModifiedObjects
    {
      get
      {return modifiedObjects;}
    }

    internal PdfIndirectObject Update(
      PdfIndirectObject obj
      )
    {
      int index = obj.Reference.ObjectNumber;

      // Get the old indirect object to be replaced!
      PdfIndirectObject old = this[index];
      if(old != obj)
      {old.DropFile();} // Disconnect the old indirect object.

      // Insert the new indirect object into the modified objects collection!
      modifiedObjects[index] = obj;
      // Remove old indirect object from cache!
      wokenObjects.Remove(index);
      // Mark the new indirect object as modified!
      obj.DropOriginal();

      return old;
    }
    #endregion
    #endregion
    #endregion
  }
}