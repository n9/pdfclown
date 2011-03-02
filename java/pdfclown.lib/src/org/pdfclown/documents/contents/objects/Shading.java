/*
  Copyright 2007-2010 Stefano Chizzolini. http://www.pdfclown.org

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

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.contents.IContentContext;
import org.pdfclown.documents.contents.ShadingResources;
import org.pdfclown.objects.PdfName;

/**
  Shading object [PDF:1.6:4.6.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.4
  @version 0.1.0
*/
@PDF(VersionEnum.PDF13)
public final class Shading
  extends GraphicsObject
{
  // <class>
  // <static>
  // <fields>
  public static final String BeginOperator = PaintShading.Operator;
  public static final String EndOperator = BeginOperator;
  // </fields>
  // </static>

  // <dynamic>
  // <constructors>
  public Shading(
    PaintShading operation
    )
  {super(operation);}
  // </constructors>

  // <interface>
  // <public>
  /**
    Gets the {@link org.pdfclown.documents.contents.colorSpaces.Shading shading} resource name.

    @see #getResource(IContentContext)
    @see ShadingResources
  */
  public PdfName getName(
    )
  {return ((PaintShading)getObjects().get(0)).getName();}

  /**
    Gets the {@link org.pdfclown.documents.contents.colorSpaces.Shading shading} resource.

    @param context Content context.
  */
  public org.pdfclown.documents.contents.colorSpaces.Shading<?> getResource(
    IContentContext context
    )
  {return ((PaintShading)getObjects().get(0)).getShading(context);}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}