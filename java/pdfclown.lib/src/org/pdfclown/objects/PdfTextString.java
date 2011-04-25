/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

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

import java.io.UnsupportedEncodingException;

import org.pdfclown.tokens.CharsetName;

/**
  PDF text string object [PDF:1.6:3.8.1].
  <p>Text strings are meaningful only as part of the document hierarchy;
  they cannot appear within content streams.
  They represent information that is intended to be human-readable.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.6
  @version 0.1.1, 04/25/11
*/
public final class PdfTextString
  extends PdfString
{
  // <class>
  // <static>
  // <interface>
  // <public>
  /**
    Gets the object equivalent to the given value.
  */
  public static PdfTextString get(
    String value
    )
  {return value == null ? null : new PdfTextString(value);}
  // </public>
  // </interface>
  // </static>

  /*
    NOTE: Text strings are string objects encoded in either
    PDFDocEncoding (superset of the ISO Latin 1 encoding [PDF:1.6:D])
    or 16-bit big-endian Unicode character encoding (see [UCS:4]).
  */
  // <dynamic>
  // <fields>
  private String encoding;
  // </fields>

  // <constructors>
  public PdfTextString(
    )
  {}

  public PdfTextString(
    byte[] value
    )
  {super(value);}

  public PdfTextString(
    String value
    )
  {super(value);}

  public PdfTextString(
    byte[] value,
    SerializationModeEnum serializationMode
    )
  {super(value, serializationMode);}

  public PdfTextString(
    String value,
    SerializationModeEnum serializationMode
    )
  {super(value, serializationMode);}
  // </constructors>

  // <interface>
  // <public>
  public String getEncoding(
    )
  {return encoding;}

  @Override
  public String getValue(
    )
  {
    try
    {
      byte[] valueBytes = getRawValue();
      byte[] buffer;
      if(encoding == CharsetName.UTF16BE)
      {
        // Excluding UTF marker...
        buffer = new byte[valueBytes.length - 2];
        System.arraycopy(valueBytes, 2, buffer, 0, buffer.length);
      }
      else
      {buffer = valueBytes;}
      return new String(buffer, encoding);
    }
    catch(UnsupportedEncodingException e)
    {throw new RuntimeException(e);} // NOTE: It should NEVER happen.
  }
  // </public>

  // <protected>
  @Override
  protected void setRawValue(
    byte[] value
    )
  {
    if(value.length > 2
      && value[0] == (byte)254
      && value[1] == (byte)255) // Multi-byte (Unicode).
    {encoding = CharsetName.UTF16BE;}
    else // Single byte (PDFDocEncoding).
    {encoding = CharsetName.ISO88591;}
    super.setRawValue(value);
  }

  @Override
  protected void setValue(
    Object value
    )
  {
    switch(getSerializationMode())
    {
      case Literal:
      {
        byte[] buffer;
        try
        {
          // Prepending UTF marker...
          byte[] valueBytes = ((String)value).getBytes(CharsetName.UTF16BE);
          buffer = new byte[valueBytes.length + 2];
          buffer[0] = (byte)254; buffer[1] = (byte)255;
          System.arraycopy(valueBytes, 0, buffer, 2, valueBytes.length);
        }
        catch(UnsupportedEncodingException e)
        {throw new RuntimeException(e);}
        setRawValue(buffer);
      }
        break;
      case Hex:
        super.setValue(value);
        break;
    }
  }
  // </protected>
  // </interface>
  // </dynamic>
  // </class>
}