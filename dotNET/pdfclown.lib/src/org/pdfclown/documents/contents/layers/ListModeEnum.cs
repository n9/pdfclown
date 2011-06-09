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
    <summary>List mode specifying which layers should be displayed to the user.</summary>
  */
  public enum ListModeEnum
  {
    /**
      <summary>All the layers are displayed.</summary>
    */
    AllPages,
    /**
      <summary>Only the layers referenced by one or more visible pages are displayed.</summary>
    */
    VisiblePages
  }

  internal static class ListModeEnumExtension
  {
    public static PdfName GetName(
      this ListModeEnum mode
      )
    {
      switch(mode)
      {
        case ListModeEnum.AllPages:
          return PdfName.AllPages;
        case ListModeEnum.VisiblePages:
          return PdfName.VisiblePages;
        default:
          throw new NotImplementedException();
      }
    }

    public static ListModeEnum ToEnum(
      PdfName name
      )
    {
      if(name == null || name.Equals(PdfName.AllPages))
        return ListModeEnum.AllPages;
      else if(name.Equals(PdfName.VisiblePages))
        return ListModeEnum.VisiblePages;
      else
        throw new NotSupportedException("List mode unknown: " + name);
    }
  }
}