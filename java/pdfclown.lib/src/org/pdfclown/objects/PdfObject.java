/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.objects;

import org.pdfclown.bytes.IOutputStream;
import org.pdfclown.files.File;

/**
  Abstract PDF object.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
*/
public abstract class PdfObject
  implements Cloneable
{
  // <class>
  // <static>
  /**
    Gets the clone of the specified object, registered inside the specified file context.

    @param object Object to clone into the specified file context.
    @param context File context of the cloning.
  */
  public static final Object clone(
    PdfObject object,
    File context
    )
  {return object == null ? null : object.clone(context);}
  // </static>

  // <dynamic>
  // <constructors>
  protected PdfObject(
    )
  {}
  // </constructors>

  // <interface>
  // <public>
  /**
    Gets the clone of this object, registered inside the specified file context.
  */
  public abstract Object clone(
    File context
    );

  /**
    Serializes this object to the specified stream.
  */
  public abstract void writeTo(
    IOutputStream stream
    );
  // </public>

  // <protected>
  @Override
  protected Object clone(
    )
  {
    try
    {return super.clone();}
    catch(CloneNotSupportedException e)
    {throw new RuntimeException("Unable to clone.",e);}
  }
  // </protected>
  // </interface>
  // </dynamic>
  // </class>
}