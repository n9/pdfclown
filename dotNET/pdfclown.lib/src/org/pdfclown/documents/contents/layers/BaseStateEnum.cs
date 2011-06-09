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

using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.contents.layers
{
  /**
    <summary>Base state used to initialize the states of all the layers in a document when this
    configuration is applied.</summary>
  */
  [PDF(VersionEnum.PDF15)]
  public enum BaseStateEnum
  {
    /**
      <summary>All the layers are visible.</summary>
    */
    On,
    /**
      <summary>All the layers are invisible.</summary>
    */
    Off,
    /**
      <summary>All the layers are left unchanged.</summary>
    */
    Unchanged
  }

  internal static class BaseStateEnumExtension
  {
    public static PdfName GetName(
      this BaseStateEnum state
      )
    {
      switch(state)
      {
        case BaseStateEnum.On:
          return PdfName.ON;
        case BaseStateEnum.Off:
          return PdfName.OFF;
        case BaseStateEnum.Unchanged:
          return PdfName.Unchanged;
        default:
          throw new NotImplementedException();
      }
    }

    public static BaseStateEnum ToEnum(
      PdfName name
      )
    {
      if(name == null || name.Equals(PdfName.ON))
        return BaseStateEnum.On;
      else if(name.Equals(PdfName.OFF))
        return BaseStateEnum.Off;
      else if(name.Equals(PdfName.Unchanged))
        return BaseStateEnum.Unchanged;
      else
        throw new NotSupportedException("Base state unknown: " + name);
    }
  }
}