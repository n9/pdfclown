/*
  Copyright 2009-2010 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.contents
{
  /**
    <summary>Graphics state parameters [PDF:1.6:4.3.4].</summary>
  */
  [PDF(VersionEnum.PDF12)]
  public sealed class ExtGState
    : PdfObjectWrapper<PdfDictionary>
  {
    #region static
    #region interface
    #region public
    /**
      <summary>Wraps a reference into a graphics state parameter dictionary object.</summary>
      <param name="reference">Reference to a graphics state parameter dictionary object.</param>
      <returns>Graphics state parameter dictionary object corresponding to the reference.</returns>
    */
    public static ExtGState Wrap(
      PdfReference reference
      )
    {return Wrap(reference, null);}

    /**
      <summary>Wraps the specified base object into a graphics state parameter dictionary object.</summary>
      <param name="baseObject">Base object of a graphics state parameter dictionary object.</param>
      <param name="container">Indirect object possibly containing the graphics state parameter dictionary base object.</param>
      <returns>Graphics state parameter dictionary object corresponding to the base object.</returns>
    */
    public static ExtGState Wrap(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      )
    {
      return baseObject == null
        ? null
        : new ExtGState(baseObject, container);
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public ExtGState(
      Document context,
      PdfDictionary baseDataObject
      ) : base(context.File, baseDataObject)
    {}

    internal ExtGState(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      ) : base(baseObject, container)
    {}
    #endregion

    #region interface
    #region public
    public void ApplyTo(
      ContentScanner.GraphicsState state
      )
    {
      foreach(PdfName parameterName in BaseDataObject.Keys)
      {
        if(parameterName.Equals(PdfName.Font))
        {
          state.Font = Font;
          state.FontSize = FontSize.Value;
        }
        else if(parameterName.Equals(PdfName.LC))
        {state.LineCap = LineCap.Value;}
        else if(parameterName.Equals(PdfName.D))
        {state.LineDash = LineDash;}
        else if(parameterName.Equals(PdfName.LJ))
        {state.LineJoin = LineJoin.Value;}
        else if(parameterName.Equals(PdfName.LW))
        {state.LineWidth = LineWidth.Value;}
        else if(parameterName.Equals(PdfName.ML))
        {state.MiterLimit = MiterLimit.Value;}
        //TODO:extend supported parameters!!!
      }
    }

    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    [PDF(VersionEnum.PDF13)]
    public Font Font
    {
      get
      {
        PdfArray fontObject = (PdfArray)BaseDataObject[PdfName.Font];
        return fontObject == null
          ? null
          : Font.Wrap((PdfReference)fontObject[0]);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public float? FontSize
    {
      get
      {
        PdfArray fontObject = (PdfArray)BaseDataObject[PdfName.Font];
        return (fontObject == null
          ? (float?)null
          : ((IPdfNumber)fontObject[1]).RawValue);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public LineCapEnum? LineCap
    {
      get
      {
        PdfInteger lineCapObject = (PdfInteger)BaseDataObject[PdfName.LC];
        return (lineCapObject == null
          ? (LineCapEnum?)null
          : (LineCapEnum)lineCapObject.RawValue);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public LineDash LineDash
    {
      get
      {
        PdfArray lineDashObject = (PdfArray)BaseDataObject[PdfName.D];
        if(lineDashObject == null)
          return null;

        float[] dashArray;
        {
          PdfArray baseDashArray = (PdfArray)lineDashObject[0];
          dashArray = new float[baseDashArray.Count];
          for(
            int index = 0,
              length = dashArray.Length;
            index < length;
            index++
            )
          {dashArray[index] = ((IPdfNumber)baseDashArray[index]).RawValue;}
        }
        float dashPhase = ((IPdfNumber)lineDashObject[1]).RawValue;
        return new LineDash(dashArray,dashPhase);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public LineJoinEnum? LineJoin
    {
      get
      {
        PdfInteger lineJoinObject = (PdfInteger)BaseDataObject[PdfName.LJ];
        return (lineJoinObject == null
          ? (LineJoinEnum?)null
          : (LineJoinEnum)lineJoinObject.RawValue);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public float? LineWidth
    {
      get
      {
        IPdfNumber lineWidthObject = (IPdfNumber)BaseDataObject[PdfName.LW];
        return (lineWidthObject == null
          ? (float?)null
          : lineWidthObject.RawValue);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public float? MiterLimit
    {
      get
      {
        IPdfNumber miterLimitObject = (IPdfNumber)BaseDataObject[PdfName.ML];
        return (miterLimitObject == null
          ? (float?)null
          : miterLimitObject.RawValue);
      }
    }
  }
  #endregion
  #endregion
  #endregion
}