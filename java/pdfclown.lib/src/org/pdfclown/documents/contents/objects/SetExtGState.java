/*
  Copyright 2009-2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.contents.objects;

import java.util.List;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.contents.ContentScanner;
import org.pdfclown.documents.contents.ExtGState;
import org.pdfclown.documents.contents.ExtGStateResources;
import org.pdfclown.documents.contents.IContentContext;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;

/**
  'Set the specified graphics state parameters' operation [PDF:1.6:4.3.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.0
*/
@PDF(VersionEnum.PDF12)
public final class SetExtGState
  extends Operation
{
  // <class>
  // <static>
  // <fields>
  public static final String Operator = "gs";
  // </fields>
  // </static>

  // <dynamic>
  // <constructors>
  public SetExtGState(
    PdfName name
    )
  {super(Operator, name);}

  public SetExtGState(
    List<PdfDirectObject> operands
    )
  {super(Operator, operands);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public void scan(
    ContentScanner.GraphicsState state
    )
  {
    ExtGState extGState = state.getScanner().getContentContext().getResources().getExtGStates().get(getName());
    extGState.applyTo(state);
  }

  /**
    Gets the name of the {@link ExtGState graphics state parameters} resource to be set.

    @see #getExtGState(IContentContext)
    @see ExtGStateResources
  */
  public PdfName getName(
    )
  {return (PdfName)operands.get(0);}

  /**
    Gets the {@link ExtGState graphics state parameters} resource to be set.

    @param context Content context.
  */
  public ExtGState getExtGState(
    IContentContext context
    )
  {return context.getResources().getExtGStates().get(getName());}

  /**
    @see #getName()
  */
  public void setName(
    PdfName value
    )
  {operands.set(0,value);}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}