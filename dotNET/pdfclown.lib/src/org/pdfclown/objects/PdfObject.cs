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

using org.pdfclown;
using org.pdfclown.bytes;
using org.pdfclown.files;

using System;

namespace org.pdfclown.objects
{
  /**
    <summary>Abstract PDF object.</summary>
  */
  public abstract class PdfObject
  {
    #region static
    /**
      <summary>Gets the clone of the specified object, registered inside the specified file context.</summary>
      <param name="object">Object to clone into the specified file context.</param>
      <param name="context">File context of the cloning.</param>
    */
    public static object Clone(
      PdfObject obj,
      File context
      )
    {return obj == null ? null : obj.Clone(context);}
    #endregion

    #region dynamic
    #region constructors
    protected PdfObject(
      )
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the clone of this object, registered inside the specified file context.</summary>
    */
    public abstract object Clone(
      File context
      );

    /**
      <summary>Serializes this object to the specified stream.</summary>
    */
    public abstract void WriteTo(
      IOutputStream stream
      );
    #endregion
    #endregion
    #endregion
  }
}