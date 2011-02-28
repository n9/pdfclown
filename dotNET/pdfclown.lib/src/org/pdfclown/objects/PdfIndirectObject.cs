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

namespace org.pdfclown.objects
{
  /**
    <summary>PDF indirect object [PDF:1.6:3.2.9].</summary>
  */
  public sealed class PdfIndirectObject
    : PdfObject,
      IPdfIndirectObject
  {
    #region static
    #region fields
    private static readonly byte[] BeginIndirectObjectChunk = Encoding.Encode(Symbol.Space + Keyword.BeginIndirectObject + Symbol.LineFeed);
    private static readonly byte[] EndIndirectObjectChunk = Encoding.Encode(Symbol.LineFeed + Keyword.EndIndirectObject + Symbol.LineFeed);
    #endregion
    #endregion

    #region dynamic
    #region fields
    private PdfDataObject dataObject;
    private File file;
    private bool original;
    private PdfReference reference;
    private XRefEntry xrefEntry;
    #endregion

    #region constructors
    /**
      <param name="file">Associated file.</param>
      <param name="dataObject">
        <para>Data object associated to the indirect object.</para>
        <list type="bullet">
          <item>It MUST be null if the indirect object is original (i.e. coming from an existing file)
          or free.</item>
          <item>It MUST be NOT null if the indirect object is new and in-use.</item>
        </list>
      </param>
      <param name="xrefEntry">
        <para>Cross-reference entry associated to the indirect object.</para>
        <list type="bullet">
          <item>If the indirect object is new, its offset field MUST be set to 0 (zero).</item>
        </list>
      </param>
    */
    internal PdfIndirectObject(
      File file,
      PdfDataObject dataObject,
      XRefEntry xrefEntry
      )
    {
      this.file = file;
      this.dataObject = dataObject;
      this.xrefEntry = xrefEntry;

      this.original = (xrefEntry.Offset != 0);
      this.reference = new PdfReference(
        this,
        xrefEntry.Number,
        xrefEntry.Generation
        );
    }
    #endregion

    #region interface
    #region public
    /**
      <summary>Adds the <see cref="DataObject">data object</see> to the specified object stream
      [PDF:1.6:3.4.6].</summary>
      <param name="objectStreamIndirectObject">Target object stream.</param>
     */
    public void Compress(
      PdfIndirectObject objectStreamIndirectObject
      )
    {
      if(objectStreamIndirectObject == null)
      {Uncompress();}
      else
      {
        PdfDataObject objectStreamDataObject = objectStreamIndirectObject.DataObject;
        if(!(objectStreamDataObject is ObjectStream))
          throw new ArgumentException("MUST contain an ObjectStream instance.","objectStreamIndirectObject");

        // Ensure removal from previous object stream!
        Uncompress();

        // Add to the object stream!
        ObjectStream objectStream = (ObjectStream)objectStreamDataObject;
        objectStream[xrefEntry.Number] = DataObject;
        // Update its xref entry!
        xrefEntry.Usage = XRefEntry.UsageEnum.InUseCompressed;
        xrefEntry.StreamNumber = objectStreamIndirectObject.Reference.ObjectNumber;
        xrefEntry.Offset = -1; // Internal object index unknown (to set on object stream serialization -- see ObjectStream).
      }
    }

    public File File
    {get{return file;}}

    public override int GetHashCode(
      )
    {
      /*
        NOTE: Uniqueness should be achieved XORring the (local) reference hashcode
        with the (global) file hashcode.
        NOTE: Do NOT directly invoke reference.GetHashCode() method here as
        it would trigger an infinite loop, as it conversely relies on this method.
      */
      return reference.Id.GetHashCode() ^ file.GetHashCode();
    }

    /**
      <summary>Gets whether it's compressed within an object stream [PDF:1.6:3.4.6].</summary>
    */
    public bool IsCompressed(
      )
    {return xrefEntry.Usage == XRefEntry.UsageEnum.InUseCompressed;}

    /**
      <summary>Gets whether it contains a data object.</summary>
    */
    public bool IsInUse(
      )
    {return (xrefEntry.Usage == XRefEntry.UsageEnum.InUse);}

    /**
      <summary>Gets whether it hasn't been modified.</summary>
    */
    public bool IsOriginal(
      )
    {return original;}

    /**
      <summary>Removes the <see cref="DataObject">data object</see> from its object stream [PDF:1.6:3.4.6].</summary>
    */
    public void Uncompress(
      )
    {
      if(!IsCompressed())
        return;

      // Remove from its object stream!
      ObjectStream oldObjectStream = (ObjectStream)file.IndirectObjects[xrefEntry.StreamNumber].DataObject;
      oldObjectStream.Remove(xrefEntry.Number);
      // Update its xref entry!
      xrefEntry.Usage = XRefEntry.UsageEnum.InUse;
      xrefEntry.StreamNumber = -1; // No object stream.
      xrefEntry.Offset = -1; // Offset unknown (to set on file serialization -- see CompressedWriter).
    }

    public void Update(
      )
    {
      if(original)
      {
        /*
          NOTE: It's expected that DropOriginal() is invoked by IndirectObjects indexer;
          such an action is delegated because clients may invoke directly the indexer skipping
          this method.
        */
        file.IndirectObjects.Update(this);
      }
    }

    public override void WriteTo(
      IOutputStream stream
      )
    {
      // Header.
      stream.Write(reference.Id); stream.Write(BeginIndirectObjectChunk);
      // Body.
      DataObject.WriteTo(stream);
      // Tail.
      stream.Write(EndIndirectObjectChunk);
    }

    public XRefEntry XrefEntry
    {
      get
      {return xrefEntry;}
    }

    #region IPdfIndirectObject
    public override object Clone(
      File context
      )
    {return context.IndirectObjects.Add(this);}

    public PdfDataObject DataObject
    {
      get
      {
        if(dataObject == null)
        {
          try
          {
            switch(xrefEntry.Usage)
            {
              // Free entry (no data object at all).
              case XRefEntry.UsageEnum.Free:
                break;
              // In-use entry (late-bound data object).
              case XRefEntry.UsageEnum.InUse:
              {
                Parser parser = file.Reader.Parser;
                // Retrieve the associated data object among the original objects!
                parser.Seek(xrefEntry.Offset);
                // Get the indirect data object!
                dataObject = parser.ParsePdfObject(4); // NOTE: Skips the indirect-object header.
                break;
              }
              case XRefEntry.UsageEnum.InUseCompressed:
              {
                // Get the object stream where its data object is stored!
                ObjectStream objectStream = (ObjectStream)file.IndirectObjects[xrefEntry.StreamNumber].DataObject;
                // Get the indirect data object!
                dataObject = objectStream[xrefEntry.Number];
                break;
              }
            }
          }
          catch(Exception e)
          {throw new Exception("Data object resolution failed.",e);}
        }
        return dataObject;
      }
      set
      {
        if(xrefEntry.Generation == XRefEntry.GenerationUnreusable)
          throw new Exception("Unreusable entry.");

        dataObject = value;
        xrefEntry.Usage = XRefEntry.UsageEnum.InUse;
        Update();
      }
    }

    public void Delete(
      )
    {
      if(file == null)
        return;

      /*
        NOTE: It's expected that DropFile() is invoked by IndirectObjects.Remove() method;
        such an action is delegated because clients may invoke directly Remove() method,
        skipping this method.
      */
      file.IndirectObjects.RemoveAt(xrefEntry.Number);
    }

    public PdfIndirectObject IndirectObject
    {get{return this;}}

    public PdfReference Reference
    {get{return reference;}}
    #endregion
    #endregion

    #region internal
    internal void DropFile(
      )
    {
      Uncompress();
      file = null;
    }

    internal void DropOriginal(
      )
    {original = false;}
    #endregion
    #endregion
    #endregion
  }
}