/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

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

import org.pdfclown.files.File;

/**
  Abstract PDF simple object.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 07/05/11
*/
public abstract class PdfSimpleObject<TValue>
  extends PdfDirectObject
{
  // <class>
  // <static>
  // <interface>
  // <public>
  /**
    Gets the value corresponding to the given object.

    @param object Object to extract the value from.
  */
  public static Object getValue(
    PdfObject object
    )
  {return getValue(object, null);}

  /**
    Gets the value corresponding to the given object.

    @param object Object to extract the value from.
    @param defaultValue Value returned in case the object's one is undefined.
  */
  public static Object getValue(
    PdfObject object,
    Object defaultValue
    )
  {
    Object value = ((object = File.resolve(object)) instanceof PdfSimpleObject<?>
      ? ((PdfSimpleObject<?>)object).getValue()
      : null);
    return (value != null ? value : defaultValue);
  }
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <fields>
  private TValue value;
  // </fields>

  // <constructors>
  public PdfSimpleObject(
    )
  {}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public final PdfSimpleObject<TValue> clone(
    File context
    )
  {return this;} // NOTE: Simple objects are immutable.

  @Override
  public boolean equals(
    Object object
    )
  {
    return object != null
      && object.getClass().equals(getClass())
      && ((PdfSimpleObject<?>)object).getRawValue().equals(getRawValue());
  }

  @Override
  public final PdfIndirectObject getContainer(
    )
  {return null;} // NOTE: As simple objects are immutable, no container can be associated.

  @Override
  public final PdfObject getParent(
    )
  {return null;} // NOTE: As simple objects are immutable, no parent can be associated.

  /**
    Gets the low-level representation of the value.
  */
  public TValue getRawValue(
    )
  {return value;}

  @Override
  public final PdfIndirectObject getRoot(
    )
  {return null;} // NOTE: As simple objects are immutable, no root can be associated.

  /**
    Gets the high-level representation of the value.
  */
  public Object getValue(
    )
  {return value;}

  @Override
  public int hashCode(
    )
  {return getRawValue().hashCode();}

  @Override
  public final boolean isUpdateable(
    )
  {return false;} // NOTE: Simple objects are immutable.

  @Override
  public final void setUpdateable(
    boolean value
    )
  {/* NOOP: As simple objects are immutable, no update can be done. */}

  @Override
  public String toString(
    )
  {return getValue().toString();}
  // </public>

  // <protected>
  @Override
  protected final boolean isUpdated(
    )
  {return false;} // NOTE: Simple objects are immutable.

  @Override
  protected boolean isVirtual(
    )
  {return false;}

  /**
    @see #getRawValue()
  */
  protected void setRawValue(
    TValue value
    )
  {this.value = value;}

  @Override
  protected final void setUpdated(
    boolean value
    )
  {/* NOOP: As simple objects are immutable, no update can be done. */}

  /**
    @see #getValue()
  */
  @SuppressWarnings("unchecked")
  protected void setValue(
    Object value
    )
  {this.value = (TValue)value;}

  @Override
  protected void setVirtual(
    boolean value
    )
  {/* NOOP */}
  // </protected>

  // <internal>
  @Override
  final void setParent(
    PdfObject value
    )
  {/* NOOP: As simple objects are immutable, no parent can be associated. */}
  // </internal>
  // </interface>
  // </dynamic>
  // </class>
}