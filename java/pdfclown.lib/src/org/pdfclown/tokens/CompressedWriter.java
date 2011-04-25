/*
  Copyright 2010-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library"
  (the Program): see the accompanying README files for more info.

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

package org.pdfclown.tokens;

import org.pdfclown.bytes.IOutputStream;
import org.pdfclown.files.File;
import org.pdfclown.files.IndirectObjects;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.util.NotImplementedException;

/**
  PDF file writer implementing compressed cross-reference stream [PDF:1.6:3.4.7].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 04/25/11
*/
final class CompressedWriter
  extends Writer
{
  // <class>
  // <dynamic>
  // <constructors>
  CompressedWriter(
    File file,
    IOutputStream stream
    )
  {super(file, stream);}
  // </constructors>

  // <interface>
  // <protected>
  @Override
  protected void writeIncremental(
    )
  {
    // 1. Original content (head, body and previous trailer).
    FileParser parser = file.getReader().getParser();
    stream.write(parser.getStream());

    // 2. Body update (modified indirect objects insertion).
    XRefEntry xrefStreamEntry;
    {
      // 2.1. Content indirect objects.
      IndirectObjects indirectObjects = file.getIndirectObjects();

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
        xrefStreamEntry = new XRefEntry(indirectObjects.size(), 0, 0, XRefEntry.UsageEnum.InUse)
        );

      XRefEntry prevFreeEntry = null;
      for(PdfIndirectObject indirectObject : indirectObjects.getModifiedObjects().values())
      {
        prevFreeEntry = addXRefEntry(
          indirectObject.getXrefEntry(),
          indirectObject,
          xrefStream,
          prevFreeEntry
          );
      }
      if(prevFreeEntry != null)
      {prevFreeEntry.setOffset(0);} // Linking back to the first free object. NOTE: The first entry in the table (object number 0) is always free.

      // 2.2. XRef stream.
      xrefStream.getHeader().put(PdfName.Prev,new PdfInteger((int)parser.retrieveXRefOffset()));
      addXRefEntry(
        xrefStreamEntry,
        xrefStreamIndirectObject,
        xrefStream,
        null
        );
    }

    // 3. Tail.
    writeTail(xrefStreamEntry.getOffset());
  }

  @Override
  protected void writeLinearized(
    )
  {throw new NotImplementedException();}

  @Override
  protected void writeStandard(
    )
  {
    // 1. Header [PDF:1.6:3.4.1].
    writeHeader();

    // 2. Body [PDF:1.6:3.4.2,3,7].
    XRefEntry xrefStreamEntry;
    {
      // 2.1. Content indirect objects.
      IndirectObjects indirectObjects = file.getIndirectObjects();

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
        xrefStreamEntry = new XRefEntry(indirectObjects.size(), 0, 0, XRefEntry.UsageEnum.InUse)
        );

      XRefEntry prevFreeEntry = null;
      for(PdfIndirectObject indirectObject : indirectObjects)
      {
        if(indirectObject.getDataObject() instanceof XRefStream)
        {
          /*
            NOTE: Existing xref streams MUST be suppressed,
            temporarily replacing them with free entries.
          */
          indirectObject = new PdfIndirectObject(
            file,
            null,
            new XRefEntry(
              indirectObject.getReference().getObjectNumber(),
              XRefEntry.GenerationUnreusable,
              0,
              XRefEntry.UsageEnum.Free
              )
            );
        }

        try
        {
          prevFreeEntry = addXRefEntry(
            indirectObject.getXrefEntry().clone(), // NOTE: Xref entry is cloned to preserve the original one.
            indirectObject,
            xrefStream,
            prevFreeEntry
            );
        }
        catch(CloneNotSupportedException e)
        {throw new RuntimeException("XRef entry addition failed.", e);}
      }
      prevFreeEntry.setOffset(0); // Linking back to the first free object. NOTE: The first entry in the table (object number 0) is always free.

      // 2.2. XRef stream.
      addXRefEntry(
        xrefStreamEntry,
        xrefStreamIndirectObject,
        xrefStream,
        null
        );
    }

    // 3. Tail.
    writeTail(xrefStreamEntry.getOffset());
  }
  // </protected>

  // <private>
  /**
    Adds an indirect object entry to the specified xref stream.

    @param xrefEntry Indirect object's xref entry.
    @param indirectObject Indirect object.
    @param xrefStream XRef stream.
    @param prevFreeEntry Previous free xref entry.
    @return Current free xref entry.
  */
  private XRefEntry addXRefEntry(
    XRefEntry xrefEntry,
    PdfIndirectObject indirectObject,
    XRefStream xrefStream,
    XRefEntry prevFreeEntry
    )
  {
    xrefStream.put(xrefEntry.getNumber(),xrefEntry);

    switch(xrefEntry.getUsage())
    {
      case InUse:
        // Set entry content's offset!
        xrefEntry.setOffset((int)stream.getLength());
        // Add entry content!
        indirectObject.writeTo(stream);
        break;
      case InUseCompressed:
        /* NOOP: Serialization is delegated to the containing object stream. */
        break;
      case Free:
        if(prevFreeEntry != null)
        {prevFreeEntry.setOffset(xrefEntry.getNumber());} // Object number of the next free object.

        prevFreeEntry = xrefEntry;
        break;
      default:
        throw new UnsupportedOperationException();
    }
    return prevFreeEntry;
  }
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}