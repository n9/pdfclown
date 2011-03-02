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

package org.pdfclown.objects;

import java.io.ByteArrayOutputStream;

import org.pdfclown.bytes.IOutputStream;
import org.pdfclown.tokens.Encoding;
import org.pdfclown.util.ConvertUtils;
import org.pdfclown.util.IDataWrapper;
import org.pdfclown.util.NotImplementedException;

/**
  PDF string object [PDF:1.6:3.2.3].

  <p>A string object consists of a series of bytes.</p>
  <p>String objects can be serialized in two ways:</p>
  <ul>
    <li>as a sequence of literal characters (plain form)</li>
    <li>as a sequence of hexadecimal digits (hexadecimal form)</li>
  </ul>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.6
  @version 0.1.0
*/
public class PdfString
  extends PdfAtomicObject<byte[]>
  implements IDataWrapper
{
  /*
    NOTE: String objects are internally represented as unescaped sequences of bytes.
    Escaping is applied on serialization only.
  */
  // <class>
  // <classes>
  /**
    String serialization mode.
  */
  public enum SerializationModeEnum
  {
    /**
      Plain form.
    */
    Literal,
    /**
      Hexadecimal form.
    */
    Hex
  };
  // </classes>

  // <static>
  // <fields>
  private static final byte BackspaceCode = 8;
  private static final byte CarriageReturnCode = 13;
  private static final byte FormFeedCode = 12;
  private static final byte HorizontalTabCode = 9;
  private static final byte LineFeedCode = 10;

  private static final byte HexLeftDelimiterCode = 60;
  private static final byte HexRightDelimiterCode = 62;
  private static final byte LiteralEscapeCode = 92;
  private static final byte LiteralLeftDelimiterCode = 40;
  private static final byte LiteralRightDelimiterCode = 41;
  // </fields>
  // </static>

  // <dynamic>
  // <fields>
  private SerializationModeEnum serializationMode = SerializationModeEnum.Literal;
  // </fields>

  // <constructors>
  public PdfString(
    )
  {}

  public PdfString(
    byte[] value
    )
  {setRawValue(value);}

  public PdfString(
    String value
    )
  {setValue(value);}

  public PdfString(
    byte[] value,
    SerializationModeEnum serializationMode
    )
  {
    setSerializationMode(serializationMode);
    setRawValue(value);
  }

  public PdfString(
    String value,
    SerializationModeEnum serializationMode
    )
  {
    setSerializationMode(serializationMode);
    setValue(value);
  }
  // </constructors>

  // <interface>
  // <public>
  @Override
  public int compareTo(
    PdfDirectObject object
    )
  {
    if(!(object instanceof PdfString))
      throw new IllegalArgumentException("Object MUST be a PdfString");

    return ((String)getValue()).compareTo((String)((PdfString)object).getValue());
  }

  /**
    Gets the serialization mode.
  */
  public SerializationModeEnum getSerializationMode(
    )
  {return serializationMode;}

  @Override
  public Object getValue(
    )
  {
    switch(serializationMode)
    {
      case Literal:
        return Encoding.decode(getRawValue());
      case Hex:
        return ConvertUtils.byteArrayToHex(getRawValue());
      default:
        throw new NotImplementedException(serializationMode + " serialization mode is not implemented.");
    }
  }

  /**
    @see #getSerializationMode()
  */
  public void setSerializationMode(
    SerializationModeEnum value
    )
  {serializationMode = value;}

  @Override
  public void setValue(
    Object value
    )
  {
    switch(serializationMode)
    {
      case Literal:
        setRawValue(Encoding.encode((String)value));
        break;
      case Hex:
        setRawValue(ConvertUtils.hexToByteArray((String)value));
        break;
      default:
        throw new NotImplementedException(serializationMode + " serialization mode is not implemented.");
    }
  }

  @Override
  public byte[] toByteArray(
    )
  {return getRawValue().clone();}

  @Override
  public void writeTo(
    IOutputStream stream
    )
  {
    ByteArrayOutputStream buffer = new ByteArrayOutputStream();
    {
      byte[] rawValue = getRawValue();
      switch(serializationMode)
      {
        case Literal:
          buffer.write(LiteralLeftDelimiterCode);
          /*
            NOTE: Literal lexical conventions prescribe that the following reserved characters
            are to be escaped when placed inside string character sequences:
              - \n Line feed (LF)
              - \r Carriage return (CR)
              - \t Horizontal tab (HT)
              - \b Backspace (BS)
              - \f Form feed (FF)
              - \( Left parenthesis
              - \) Right parenthesis
              - \\ Backslash
          */
          for(
            int index = 0;
            index < rawValue.length;
            index++
            )
          {
            byte valueByte = rawValue[index];
            switch(valueByte)
            {
              case LineFeedCode:
                buffer.write(LiteralEscapeCode); valueByte = 110; break;
              case CarriageReturnCode:
                buffer.write(LiteralEscapeCode); valueByte = 114; break;
              case HorizontalTabCode:
                buffer.write(LiteralEscapeCode); valueByte = 116; break;
              case BackspaceCode:
                buffer.write(LiteralEscapeCode); valueByte = 98; break;
              case FormFeedCode:
                buffer.write(LiteralEscapeCode); valueByte = 102; break;
              case LiteralLeftDelimiterCode:
              case LiteralRightDelimiterCode:
              case LiteralEscapeCode:
                buffer.write(LiteralEscapeCode); break;
            }
            buffer.write(valueByte);
          }
          buffer.write(LiteralRightDelimiterCode);
          break;
        case Hex:
          buffer.write(HexLeftDelimiterCode);
          byte[] value = Encoding.encode(ConvertUtils.byteArrayToHex(rawValue));
          buffer.write(value,0,value.length);
          buffer.write(HexRightDelimiterCode);
          break;
        default:
          throw new NotImplementedException();
      }
    }
    stream.write(buffer.toByteArray());
  }
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}