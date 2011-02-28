/*
  Copyright 2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.interaction.actions;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.fileSpecs.FileSpec;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfName;

/**
  Abstract go-to-nonlocal-destination action.
  
  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.0
*/
@PDF(VersionEnum.PDF11)
public abstract class GoToNonLocal<T extends Destination>
  extends GoToDestination<T>
{
  // <class>
  // <constructors>
  protected GoToNonLocal(
    Document context,
    PdfName actionType,
    FileSpec fileSpec,
    T destination
    )
  {
    super(context,actionType,destination);
    setFileSpec(fileSpec);
  }

  protected GoToNonLocal(
    PdfDirectObject baseObject,
    PdfIndirectObject container
    )
  {super(baseObject,container);}
  // </constructors>

  // <interface>
  // <public>
  /**
    Gets the file in which the destination is located.
  */
  public FileSpec getFileSpec(
    )
  {
    PdfDirectObject fileSpecObject = getBaseDataObject().get(PdfName.F);
    if(fileSpecObject == null)
      return null;

    return new FileSpec(fileSpecObject,getContainer(),null);
  }
  
  /**
    @see #getFileSpec()
  */
  public void setFileSpec(
    FileSpec value
    )
  {
    if(value == null)
    {getBaseDataObject().remove(PdfName.F);}
    else
    {getBaseDataObject().put(PdfName.F, value.getBaseObject());}
  }
  // </public>
  // </interface>
  // </class>
}
