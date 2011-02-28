/*
  Copyright 2008-2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.interaction.annotations;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.objects.PdfBoolean;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.util.NotImplementedException;

import java.awt.geom.Rectangle2D;

/**
  Pop-up annotation [PDF:1.6:8.4.5].
  <p>It displays text in a pop-up window for entry and editing.
  It typically does not appear alone but is associated with a markup annotation,
  its parent annotation, and is used for editing the parent's text.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.0
*/
@PDF(VersionEnum.PDF13)
public final class Popup
  extends Annotation
{
  // <class>
  // <dynamic>
  // <constructors>
  protected Popup(
    Page page,
    Rectangle2D box
    )
  {
    super(
      page.getDocument(),
      PdfName.Popup,
      box,
      page
      );
  }

  protected Popup(
    PdfDirectObject baseObject,
    PdfIndirectObject container
    )
  {super(baseObject,container);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Popup clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Gets whether the annotation should initially be displayed open.
  */
  public boolean isOpen(
    )
  {
    /*
      NOTE: 'Open' entry may be undefined.
    */
    PdfBoolean openObject = (PdfBoolean)getBaseDataObject().get(PdfName.Open);
    if(openObject == null)
      return false;

    return ((Boolean)openObject.getValue()).booleanValue();
  }

  /**
    Gets the parent annotation.
  */
  public Annotation getParent(
    )
  {
    /*
      NOTE: 'Parent' entry may be undefined.
    */
    PdfReference parentObject = (PdfReference)getBaseDataObject().get(PdfName.Parent);
    if(parentObject == null)
      return null;

    return Annotation.wrap(parentObject);
  }

  /**
    @see #isOpen()
  */
  public void setOpen(
    boolean value
    )
  {getBaseDataObject().put(PdfName.Open, PdfBoolean.get(value));}

  /**
    @see #getParent()
  */
  public void setParent(
    Annotation value
    )
  {getBaseDataObject().put(PdfName.Parent,value.getBaseObject());}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}