/*
  Copyright 2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.documents.contents.composition;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;

namespace org.pdfclown.documents.interaction
{
  /**
    <summary>Text justification [PDF:1.6:8.4.5,8.6.2].</summary>
  */
  public enum JustificationEnum
  {
    /**
      <summary>Left.</summary>
    */
    Left,
    /**
      <summary>Center.</summary>
    */
    Center,
    /**
      <summary>Right.</summary>
    */
    Right
  }

  internal static class JustificationEnumExtension
  {
    private static readonly Dictionary<JustificationEnum,PdfInteger> JustificationEnumCodes;

    static JustificationEnumExtension(
      )
    {
      JustificationEnumCodes = new Dictionary<JustificationEnum,PdfInteger>();
      JustificationEnumCodes[JustificationEnum.Left] = new PdfInteger(0);
      JustificationEnumCodes[JustificationEnum.Center] = new PdfInteger(1);
      JustificationEnumCodes[JustificationEnum.Right] = new PdfInteger(2);
    }

    public static AlignmentXEnum ToAlignmentX(
      this JustificationEnum value
      )
    {
      switch(value)
      {
        case JustificationEnum.Left:
          return AlignmentXEnum.Left;
        case JustificationEnum.Center:
          return AlignmentXEnum.Center;
        case JustificationEnum.Right:
          return AlignmentXEnum.Right;
        default:
          throw new NotSupportedException();
      }
    }

    /**
      <summary>Gets the code corresponding to the given value.</summary>
    */
    public static PdfInteger ToCode(
      this JustificationEnum value
      )
    {return JustificationEnumCodes[value];}

    /**
      <summary>Gets the justification corresponding to the given value.</summary>
    */
    public static JustificationEnum ToEnum(
      PdfInteger value
      )
    {
      if(value == null)
        return JustificationEnum.Left;

      foreach(KeyValuePair<JustificationEnum,PdfInteger> justification in JustificationEnumCodes)
      {
        if(justification.Value.Equals(value))
          return justification.Key;
      }
      throw new ArgumentException(value.ToString() + " is NOT a valid justification code.");
    }
  }
}