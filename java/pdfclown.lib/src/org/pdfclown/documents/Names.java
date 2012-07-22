/*
  Copyright 2007-2012 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.files.FileSpecification;
import org.pdfclown.documents.interaction.actions.JavaScript;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.util.NotImplementedException;

/**
  Name dictionary [PDF:1.6:3.6.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.4
  @version 0.1.2, 01/29/12
*/
@PDF(VersionEnum.PDF12)
public final class Names
  extends PdfObjectWrapper<PdfDictionary>
{
  // <class>
  // <dynamic>
  // <constructors>
  public Names(
    Document context
    )
  {super(context, new PdfDictionary());}

  Names(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Names clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Gets the named destinations.
  */
  @PDF(VersionEnum.PDF12)
  public NamedDestinations getDestinations(
    )
  {
    PdfDirectObject destinationsObject = getBaseDataObject().get(PdfName.Dests);
    return destinationsObject != null ? new NamedDestinations(destinationsObject) : null;
  }

  /**
    Gets the named embedded files.
  */
  @PDF(VersionEnum.PDF14)
  public NamedEmbeddedFiles getEmbeddedFiles(
    )
  {
    PdfDirectObject embeddedFilesObject = getBaseDataObject().get(PdfName.EmbeddedFiles);
    return embeddedFilesObject != null ? new NamedEmbeddedFiles(embeddedFilesObject) : null;
  }

  /**
    Gets the named JavaScript actions.
  */
  @PDF(VersionEnum.PDF13)
  public NamedJavaScripts getJavaScripts(
    )
  {
    PdfDirectObject javaScriptsObject = getBaseDataObject().get(PdfName.JavaScript);
    return javaScriptsObject != null ? new NamedJavaScripts(javaScriptsObject) : null;
  }

  @SuppressWarnings("unchecked")
  public <T extends PdfObjectWrapper<?>> T resolve(
    Class<T> type,
    String name
    )
  {
    if(Destination.class.isAssignableFrom(type))
      return (T)getDestinations().get(name);
    else if(FileSpecification.class.isAssignableFrom(type))
      return (T)getEmbeddedFiles().get(name);
    else if(JavaScript.class.isAssignableFrom(type))
      return (T)getJavaScripts().get(name);
    else
      throw new UnsupportedOperationException("Named type '" + type.getName() + "' is not supported.");
  }

  /**
    @see #getDestinations()
  */
  public void setDestinations(
    NamedDestinations value
    )
  {getBaseDataObject().put(PdfName.Dests,value.getBaseObject());}

  /**
    @see #getEmbeddedFiles()
  */
  public void setEmbeddedFiles(
    NamedEmbeddedFiles value
    )
  {getBaseDataObject().put(PdfName.EmbeddedFiles,value.getBaseObject());}

  /**
    @see #getJavaScripts()
  */
  public void setJavaScripts(
    NamedJavaScripts value
    )
  {getBaseDataObject().put(PdfName.JavaScript,value.getBaseObject());}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}