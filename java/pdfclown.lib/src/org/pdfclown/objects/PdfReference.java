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

import org.pdfclown.bytes.IOutputStream;
import org.pdfclown.files.File;
import org.pdfclown.tokens.FileParser;
import org.pdfclown.tokens.Symbol;
import org.pdfclown.util.NotImplementedException;

/**
  PDF indirect reference object [PDF:1.6:3.2.9].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 07/05/11
*/
public final class PdfReference
  extends PdfDirectObject
  implements IPdfIndirectObject
{
  // <class>
  // <dynamic>
  // <fields>
  private PdfIndirectObject indirectObject;

  private final int generationNumber;
  private final int objectNumber;

  private File file;
  // </fields>

  // <constructors>
  PdfReference(
    PdfIndirectObject indirectObject,
    int objectNumber,
    int generationNumber
    )
  {
    this.indirectObject = indirectObject;
    this.objectNumber = objectNumber;
    this.generationNumber = generationNumber;
  }

  /**
    For internal use only.

    <p>This is a necessary hack because indirect objects are unreachable on parsing bootstrap
    (see File(IInputStream) constructor).</p>
  */
  public PdfReference(
    FileParser.Reference reference,
    File file
    )
  {
    this.objectNumber = reference.getObjectNumber();
    this.generationNumber = reference.getGenerationNumber();
    this.file = file;
  }
  // </constructors>

  // <interface>
  // <public>
  @Override
  public int compareTo(
    PdfDirectObject object
    )
  {throw new NotImplementedException();}

  @Override
  public boolean equals(
    Object object
    )
  {
    return object != null
      && object.getClass().equals(getClass())
      && ((PdfReference)object).getId().equals(getId());
  }

  @Override
  public PdfIndirectObject getContainer(
    )
  {return getIndirectObject();}

  /**
    Gets the generation number.
  */
  public int getGenerationNumber(
    )
  {return generationNumber;}

  /**
    Gets the object identifier.
    <p>This corresponds to the serialized representation of an object identifier within a PDF file.</p>
  */
  public String getId(
    )
  {return ("" + objectNumber + Symbol.Space + generationNumber);}

  /**
    Gets the object reference.
    <p>This corresponds to the serialized representation of a reference within a PDF file.</p>
  */
  public String getIndirectReference(
    )
  {return (getId() + Symbol.Space + Symbol.CapitalR);}

  /**
    Gets the object number.
  */
  public int getObjectNumber(
    )
  {return objectNumber;}

  @Override
  public PdfObject getParent(
    )
  {return null;} // NOTE: As references are immutable, no parent can be associated.

  @Override
  public PdfIndirectObject getRoot(
    )
  {return null;} // NOTE: As references are immutable, no root can be associated.

  @Override
  public boolean isUpdateable(
    )
  {return false;}

  @Override
  public int hashCode(
    )
  {return getIndirectObject().hashCode();}

  @Override
  public void setUpdateable(
    boolean value
    )
  {/* NOOP: As references are immutable, no update can be done. */}

  @Override
  public String toString(
    )
  {return getIndirectReference();}

  @Override
  public void writeTo(
    IOutputStream stream
    )
  {stream.write(getIndirectReference());}

  // <IPdfIndirectObject>
  @Override
  public PdfReference clone(
    File context
    )
  {
    /*
      NOTE: Local cloning keeps the same reference as it's immutable;
      conversely, alien cloning generates a new reference in the new file context.
    */
    return context == null || context == file
      ? this // Local clone (immutable).
      : getIndirectObject().clone(context).getReference(); // Alien clone.
  }

  @Override
  public void delete(
    )
  {getIndirectObject().delete();}

  @Override
  public PdfDataObject getDataObject(
    )
  {return getIndirectObject().getDataObject();}

  @Override
  public PdfIndirectObject getIndirectObject(
    )
  {
    if(indirectObject == null)
    {indirectObject = file.getIndirectObjects().get(objectNumber);}

    return indirectObject;
  }

  @Override
  public PdfReference getReference(
    )
  {return this;}

  @Override
  public void setDataObject(
    PdfDataObject value
    )
  {getIndirectObject().setDataObject(value);}
  // </IPdfIndirectObject>
  // </public>

  // <protected>
  @Override
  protected boolean isUpdated(
    )
  {return false;}

  @Override
  protected boolean isVirtual(
    )
  {return false;}

  @Override
  protected void setUpdated(
    boolean value
    )
  {/* NOOP: As references are immutable, no update can be done. */}

  @Override
  protected void setVirtual(
    boolean value
    )
  {/* NOOP */}
  // </protected>

  // <internal>
  @Override
  void setParent(
    PdfObject value
    )
  {/* NOOP: As references are immutable, no parent can be associated. */}
  // </internal>
  // </interface>
  // </dynamic>
  // </class>
}