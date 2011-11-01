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

using org.pdfclown.documents;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.interaction.navigation.document
{
  /**
    <summary>Remote interaction target [PDF:1.6:8.2.1].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class RemoteDestination
    : Destination
  {
    #region dynamic
    #region constructors
    public RemoteDestination(
      Document context,
      int pageIndex
      ) : this(
        context,
        pageIndex,
        ModeEnum.Fit,
        null
        )
    {}

    public RemoteDestination(
      Document context,
      int pageIndex,
      ModeEnum mode,
      double?[] viewParams
      ) : base(
        context,
        new PdfInteger(pageIndex),
        mode,
        viewParams
        )
    {}

    internal RemoteDestination(
      PdfDirectObject baseObject,
      PdfString name
      ) : base(baseObject, name)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    public int PageIndex
    {get{return ((PdfInteger)BaseDataObject[0]).RawValue;}}

    public override object PageRef
    {get{return PageIndex;}}
    #endregion
    #endregion
    #endregion
  }
}