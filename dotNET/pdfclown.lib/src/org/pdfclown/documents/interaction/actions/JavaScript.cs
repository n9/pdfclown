/*
  Copyright 2008-2010 Stefano Chizzolini. http://www.pdfclown.org

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

using bytes = org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.interaction.actions
{
  /**
    <summary>'Cause a script to be compiled and executed by the JavaScript interpreter'
    action [PDF:1.6:8.6.4].</summary>
  */
  [PDF(VersionEnum.PDF13)]
  public sealed class JavaScript
    : Action
  {
    #region dynamic
    #region constructors
    /**
      <summary>Creates a new action within the given document context.</summary>
    */
    public JavaScript(
      Document context,
      string script
      ) : base(context, PdfName.JavaScript)
    {BaseDataObject[PdfName.JS] = new PdfTextString(script);}

    internal JavaScript(
      PdfDirectObject baseObject,
      PdfIndirectObject container,
      PdfString name
      ) : base(baseObject, container, name)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    public string Script
    {
      get
      {
        /*
          NOTE: 'JS' entry MUST be defined.
        */
        PdfDataObject scriptObject = BaseDataObject[PdfName.JS];
        if(scriptObject is PdfTextString)
        {return (string)((PdfTextString)scriptObject).Value;}
        else
        {
          bytes::IBuffer scriptBuffer = ((PdfStream)scriptObject).Body;
          return scriptBuffer.GetString(0,(int)scriptBuffer.Length);
        }
      }
      set
      {
        /*
          NOTE: 'JS' entry MUST be defined.
        */
        PdfDataObject scriptObject = BaseDataObject[PdfName.JS];
        if(scriptObject is PdfTextString)
        {((PdfTextString)scriptObject).Value = value;}
        else
        {
          bytes::IBuffer scriptBuffer = ((PdfStream)scriptObject).Body;
          scriptBuffer.SetLength(0);
          scriptBuffer.Append(value);
        }
      }
    }
    #endregion
    #endregion
    #endregion
  }
}