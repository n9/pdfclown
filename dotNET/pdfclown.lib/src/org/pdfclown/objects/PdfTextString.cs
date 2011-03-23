/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

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

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace org.pdfclown.objects
{
  /**
    <summary>PDF text string object [PDF:1.6:3.8.1].</summary>
    <remarks>Text strings are meaningful only as part of the document hierarchy;
    they cannot appear within content streams.
    They represent information that is intended to be human-readable.</remarks>
  */
  public sealed class PdfTextString
    : PdfString
  {
    #region static
    #region interface
    #region public
    /**
      <summary>Gets the object equivalent to the given value.</summary>
    */
    public static PdfTextString Get(
      string value
      )
    {return value != null ? new PdfTextString(value) : null;}
    #endregion
    #endregion
    #endregion

    /*
      NOTE: Text strings are string objects encoded in either
      PDFDocEncoding (superset of the ISO Latin 1 encoding [PDF:1.6:D])
      or 16-bit big-endian Unicode character encoding (see [UCS:4]).
    */
    #region dynamic
    #region fields
    private Encoding encoding;
    #endregion

    #region constructors
    public PdfTextString(
      )
    {}

    public PdfTextString(
      byte[] value
      ) : base(value)
    {}

    public PdfTextString(
      string value
      ) : base(value)
    {}

    public PdfTextString(
      byte[] value,
      SerializationModeEnum serializationMode
      ) : base(value, serializationMode)
    {}

    public PdfTextString(
      String value,
      SerializationModeEnum serializationMode
      ) : base(value, serializationMode)
    {}
    #endregion

    #region interface
    #region public
    public Encoding Encoding
    {
      get
      {return encoding;}
    }

    public override byte[] RawValue
    {
      protected set
      {
        if(value.Length > 2
          && value[0] == (byte)254
          && value[1] == (byte)255) // Multi-byte (Unicode).
        {encoding = tokens.Encoding.UTF16BE;}
        else // Single byte (PDFDocEncoding).
        {encoding = tokens.Encoding.ISO88591;}
        base.RawValue = value;
      }
    }

    public override object Value
    {
      get
      {
        byte[] valueBytes = RawValue;
        byte[] buffer;
        if(encoding == tokens.Encoding.UTF16BE)
        {
          // Excluding UTF marker...
          buffer = new byte[valueBytes.Length - 2];
          Array.Copy(valueBytes, 2, buffer, 0, buffer.Length);
        }
        else
        {buffer = valueBytes;}
        return encoding.GetString(buffer);
      }
      protected set
      {
        switch(SerializationMode)
        {
          case SerializationModeEnum.Literal:
          {
            byte[] buffer;
            {
              // Prepending UTF marker...
              byte[] valueBytes = tokens.Encoding.UTF16BE.GetBytes((string)value);
              buffer = new byte[valueBytes.Length + 2];
              buffer[0] = (byte)254; buffer[1] = (byte)255;
              Array.Copy(valueBytes, 0, buffer, 2, valueBytes.Length);
            }
            RawValue = buffer;
          }
            break;
          case SerializationModeEnum.Hex:
            base.Value = value;
            break;
        }
      }
    }
    #endregion
    #endregion
    #endregion
  }
}