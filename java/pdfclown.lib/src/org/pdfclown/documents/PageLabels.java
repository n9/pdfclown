/*
  Copyright 2012 Stefano Chizzolini. http://www.pdfclown.org

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
import org.pdfclown.documents.interaction.navigation.page.PageLabel;
import org.pdfclown.objects.NumberTree;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.util.NotImplementedException;

/**
  Page label ranges [PDF:1.6:3.6.1].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.2
  @version 0.1.2, 02/15/12
*/
@PDF(VersionEnum.PDF13)
public final class PageLabels
  extends NumberTree<PageLabel>
{
  // <class>
  // <static>
  // <interface>
  // <public>
  /**
    Gets existing page label ranges.

    @param baseObject Base object to wrap.
  */
  public static PageLabels wrap(
    PdfDirectObject baseObject
    )
  {return baseObject != null ? new PageLabels(baseObject) : null;}
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  public PageLabels(
    Document context
    )
  {super(context);}

  private PageLabels(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public PageLabels clone(
    Document context
    )
  {throw new NotImplementedException();}
  // </public>

  // <protected>
  @Override
  protected PageLabel wrap(
    PdfDirectObject baseObject,
    PdfInteger keyObject
    )
  {return PageLabel.wrap(baseObject);}
  // </protected>
  // </interface>
  // </dynamic>
  // </class>
}
