/*
  Copyright 2010-2011 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace org.pdfclown.tokens
{
  /**
    <summary>PDF file writer implementing compressed cross-reference stream [PDF:1.6:3.4.7].</summary>
  */
  internal sealed class CompressedWriter
    : Writer
  {
    #region dynamic
    #region constructors
    internal CompressedWriter(
      files.File file,
      IOutputStream stream
      ) : base(file, stream)
    {}
    #endregion

    #region interface
    #region protected
    protected override void WriteIncremental(
      )
    {
      // 1. Original content (head, body and previous trailer).
      FileParser parser = file.Reader.Parser;
      stream.Write(parser.Stream);

      // 2. Body update (modified indirect objects insertion).
      XRefEntry xrefStreamEntry;
      {
        // 2.1. Content indirect objects.
        IndirectObjects indirectObjects = file.IndirectObjects;

        // Create the xref stream indirect object!
        /*
          NOTE: Incremental xref table comprises multiple sections each one composed by multiple subsections;
          this update adds a new section.
        */
        /*
          NOTE: This xref stream indirect object is purposely temporary (i.e. not registered into the file's
          indirect objects collection).
        */
        XRefStream xrefStream;
        PdfIndirectObject xrefStreamIndirectObject = new PdfIndirectObject(
          file,
          xrefStream = new XRefStream(file),
          xrefStreamEntry = new XRefEntry(indirectObjects.Count, 0, 0, XRefEntry.UsageEnum.InUse)
          );

        XRefEntry prevFreeEntry = null;
        foreach(PdfIndirectObject indirectObject in indirectObjects.ModifiedObjects.Values)
        {
          prevFreeEntry = AddXRefEntry(
            indirectObject.XrefEntry,
            indirectObject,
            xrefStream,
            prevFreeEntry
            );
        }
        if(prevFreeEntry != null)
        {prevFreeEntry.Offset = 0;} // Linking back to the first free object. NOTE: The first entry in the table (object number 0) is always free.

        // 2.2. XRef stream.
        xrefStream.Header[PdfName.Prev] = new PdfInteger((int)parser.RetrieveXRefOffset());
        AddXRefEntry(
          xrefStreamEntry,
          xrefStreamIndirectObject,
          xrefStream,
          null
          );
      }

      // 3. Tail.
      WriteTail(xrefStreamEntry.Offset);
    }

    protected override void WriteLinearized(
      )
    {throw new NotImplementedException();}

    protected override void WriteStandard(
      )
    {
      // 1. Header [PDF:1.6:3.4.1].
      WriteHeader();

      // 2. Body [PDF:1.6:3.4.2,3,7].
      XRefEntry xrefStreamEntry;
      {
        // 2.1. Content indirect objects.
        IndirectObjects indirectObjects = file.IndirectObjects;

        // Create the xref stream indirect object!
        /*
          NOTE: A standard xref stream comprises just one section composed by just one subsection.
          The xref stream is generated on-the-fly and kept volatile not to interfere with the existing
          file structure.
        */
        /*
          NOTE: This xref stream indirect object is purposely temporary (i.e. not registered into the file's
          indirect objects collection).
        */
        XRefStream xrefStream;
        PdfIndirectObject xrefStreamIndirectObject = new PdfIndirectObject(
          file,
          xrefStream = new XRefStream(file),
          xrefStreamEntry = new XRefEntry(indirectObjects.Count, 0, 0, XRefEntry.UsageEnum.InUse)
          );

        XRefEntry prevFreeEntry = null;
        foreach(PdfIndirectObject indirectObject in indirectObjects)
        {
          PdfIndirectObject actualIndirectObject;
          if(indirectObject.DataObject is XRefStream)
          {
            /*
              NOTE: Existing xref streams MUST be suppressed,
              temporarily replacing them with free entries.
            */
            actualIndirectObject = new PdfIndirectObject(
              file,
              null,
              new XRefEntry(
                indirectObject.Reference.ObjectNumber,
                XRefEntry.GenerationUnreusable,
                0,
                XRefEntry.UsageEnum.Free
                )
              );
          }
          else
          {actualIndirectObject = indirectObject;}

          prevFreeEntry = AddXRefEntry(
            (XRefEntry)actualIndirectObject.XrefEntry.Clone(), // NOTE: Xref entry is cloned to preserve the original one.
            actualIndirectObject,
            xrefStream,
            prevFreeEntry
            );
        }
        prevFreeEntry.Offset = 0; // Linking back to the first free object. NOTE: The first entry in the table (object number 0) is always free.

        // 2.2. XRef stream.
        AddXRefEntry(
          xrefStreamEntry,
          xrefStreamIndirectObject,
          xrefStream,
          null
          );
      }

      // 3. Tail.
      WriteTail(xrefStreamEntry.Offset);
    }
    #endregion

    #region private
    /**
      <summary>Adds an indirect object entry to the specified xref stream.</summary>
      <param name="xrefEntry">Indirect object's xref entry.</param>
      <param name="indirectObject">Indirect object.</param>
      <param name="xrefStream">XRef stream.</param>
      <param name="prevFreeEntry">Previous free xref entry.</param>
      <returns>Current free xref entry.</returns>
    */
    private XRefEntry AddXRefEntry(
      XRefEntry xrefEntry,
      PdfIndirectObject indirectObject,
      XRefStream xrefStream,
      XRefEntry prevFreeEntry
      )
    {
      xrefStream[xrefEntry.Number] = xrefEntry;

      switch(xrefEntry.Usage)
      {
        case XRefEntry.UsageEnum.InUse:
          // Set entry content's offset!
          xrefEntry.Offset = (int)stream.Length;
          // Add entry content!
          indirectObject.WriteTo(stream);
          break;
        case XRefEntry.UsageEnum.InUseCompressed:
          /* NOOP: Serialization is delegated to the containing object stream. */
          break;
        case XRefEntry.UsageEnum.Free:
          if(prevFreeEntry != null)
          {prevFreeEntry.Offset = xrefEntry.Number;} // Object number of the next free object.

          prevFreeEntry = xrefEntry;
          break;
        default:
          throw new NotSupportedException();
      }
      return prevFreeEntry;
    }
    #endregion
    #endregion
    #endregion
  }
}