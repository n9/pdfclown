/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.fileSpecs;

import java.io.FileNotFoundException;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.bytes.Buffer;
import org.pdfclown.bytes.FileInputStream;
import org.pdfclown.bytes.IInputStream;
import org.pdfclown.documents.Document;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfStream;
import org.pdfclown.util.NotImplementedException;

/**
  Embedded file [PDF:1.6:3.10.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 06/08/11
*/
@PDF(VersionEnum.PDF13)
public final class EmbeddedFile
  extends PdfObjectWrapper<PdfStream>
{
  // <class>
  // <static>
  // <interface>
  // <public>
  public static EmbeddedFile get(
    Document context,
    String path
    )
  {
    try
    {
      return new EmbeddedFile(
        context,
        new FileInputStream(
          new java.io.RandomAccessFile(path,"r")
          )
        );
    }
    catch(FileNotFoundException e)
    {throw new RuntimeException(e);}
  }

  public static EmbeddedFile get(
    Document context,
    java.io.File file
    )
  {return get(context,file.getPath());}
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  /**
    Creates a new embedded file inside the document.
  */
  public EmbeddedFile(
    Document context,
    IInputStream stream
    )
  {
    super(
      context,
      new PdfStream(
        new PdfDictionary(
          new PdfName[]{PdfName.Type},
          new PdfDirectObject[]{PdfName.EmbeddedFile}
          ),
        new Buffer(stream.toByteArray())
        )
      );
  }

  /**
    Instantiates an existing embedded file.
  */
  public EmbeddedFile(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public EmbeddedFile clone(
    Document context
    )
  {throw new NotImplementedException();}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}