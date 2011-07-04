/*
  Copyright 2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.interaction;

import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.objects.PdfInteger;

/**
  Text justification [PDF:1.7:8.4.5,8.6.2].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.1, 07/05/11
*/
public enum JustificationEnum
{
  // <class>
  // <static>
  // <fields>
  /**
    Left.
  */
  Left(new PdfInteger(0)),
  /**
    Center.
  */
  Center(new PdfInteger(1)),
  /**
    Right.
  */
  Right(new PdfInteger(2));
  // </fields>

  // <interface>
  // <public>
  /**
    Gets the highlighting mode corresponding to the given value.
  */
  public static JustificationEnum get(
    PdfInteger value
    )
  {
    if(value == null)
      return JustificationEnum.Left;

    for(JustificationEnum justification : JustificationEnum.values())
    {
      if(justification.getCode().equals(value))
        return justification;
    }
    throw new IllegalArgumentException(value.toString() + " is NOT a valid justification code.");
  }
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <fields>
  private final PdfInteger code;
  // </fields>

  // <constructors>
  private JustificationEnum(
    PdfInteger code
    )
  {this.code = code;}
  // </constructors>

  // <interface>
  // <public>
  public PdfInteger getCode(
    )
  {return code;}

  public AlignmentXEnum toAlignmentX(
    )
  {
    switch(this)
    {
      case Left:
        return AlignmentXEnum.Left;
      case Center:
        return AlignmentXEnum.Center;
      case Right:
        return AlignmentXEnum.Right;
      default:
        throw new UnsupportedOperationException();
    }
  }
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}
