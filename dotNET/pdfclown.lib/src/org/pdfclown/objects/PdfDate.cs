/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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
using System.Globalization;
using System.Text;

namespace org.pdfclown.objects
{
  /**
    <summary>PDF date object [PDF:1.6:3.8.3].</summary>
  */
  public sealed class PdfDate
    : PdfString
  {
    #region static
    #region interface
    #region public
    public static DateTime ToDate(
      string value
      )
    {
      //TODO:IMPL this code is quite ugly... is there a more elegant solution?
      // Normalize datetime value.
      // Cut leading "D:" tag!
      value = value.Substring(2);
      int length = value.Length;
      switch(length)
      {
        case 8: // Date only.
          value += "000000+00:00";
          break;
        case 14: // Datetime without timezone.
          value += "+00:00";
          break;
        case 15: // Datetime at UT timezone ("Z" tag).
          value = value.Substring(0,length-1) + "+00:00";
          break;
        case 18: // Datetime at non-UT timezone without minutes.
          value = value.Substring(0,length-1) + "00";
          break;
        case 21: // Datetime at non-UT full timezone ("'mm'" PDF timezone-minutes format).
          value = value.Substring(0,length-1).Replace('\'',':');
          break;
      }

      // Parse datetime value!
      return DateTime.ParseExact(
        value,
        "yyyyMMddHHmmsszzz",
        new CultureInfo("en-US")
        );
    }
    #endregion

    #region private
    private static string Format(
      DateTime value
      )
    {return ("D:" + value.ToString("yyyyMMddHHmmsszzz").Replace(':','\'') + "'");}
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public PdfDate(
      )
    {}

    public PdfDate(
      DateTime value
      )
    {Value = value;}
    #endregion

    #region interface
    #region public
    public override object Value
    {
      get
      {return ToDate(tokens.Encoding.Decode(RawValue));}
      set
      {RawValue = tokens.Encoding.Encode(Format((DateTime)value));}
    }
    #endregion
    #endregion
    #endregion
  }
}