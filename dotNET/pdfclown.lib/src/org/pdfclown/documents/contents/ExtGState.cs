/*
  Copyright 2009-2011 Stefano Chizzolini. http://www.pdfclown.org

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
      <summary>Wraps the specified base object into a graphics state parameter dictionary object.</summary>
      <param name="baseObject">Base object of a graphics state parameter dictionary object.</param>
      <param name="container">Indirect object possibly containing the graphics state parameter dictionary base object.</param>
      <returns>Graphics state parameter dictionary object corresponding to the base object.</returns>
    */
    public static ExtGState Wrap(
      PdfDirectObject baseObject
      )
    {return baseObject != null ? new ExtGState(baseObject) : null;}
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public ExtGState(
      Document context,
      PdfDictionary baseDataObject
      ) : base(context, baseDataObject)
    {}

    internal ExtGState(
      PdfDirectObject baseObject
      ) : base(baseObject)
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
        return fontObject != null ? Font.Wrap(fontObject[0]) : null;
      }
    }

    [PDF(VersionEnum.PDF13)]
    public double? FontSize
    {
      get
      {
        PdfArray fontObject = (PdfArray)BaseDataObject[PdfName.Font];
        return fontObject != null ? ((IPdfNumber)fontObject[1]).RawValue : (double?)null;
      }
    }

    [PDF(VersionEnum.PDF13)]
    public LineCapEnum? LineCap
    {
      get
      {
        PdfInteger lineCapObject = (PdfInteger)BaseDataObject[PdfName.LC];
        return lineCapObject != null ? (LineCapEnum)lineCapObject.RawValue : (LineCapEnum?)null;
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

        double[] dashArray;
        {
          PdfArray baseDashArray = (PdfArray)lineDashObject[0];
          dashArray = new double[baseDashArray.Count];
          for(
            int index = 0,
              length = dashArray.Length;
            index < length;
            index++
            )
          {dashArray[index] = ((IPdfNumber)baseDashArray[index]).RawValue;}
        }
        double dashPhase = ((IPdfNumber)lineDashObject[1]).RawValue;
        return new LineDash(dashArray,dashPhase);
      }
    }

    [PDF(VersionEnum.PDF13)]
    public LineJoinEnum? LineJoin
    {
      get
      {
        PdfInteger lineJoinObject = (PdfInteger)BaseDataObject[PdfName.LJ];
        return lineJoinObject != null ? (LineJoinEnum)lineJoinObject.RawValue : (LineJoinEnum?)null;
      }
    }

    [PDF(VersionEnum.PDF13)]
    public double? LineWidth
    {
      get
      {
        IPdfNumber lineWidthObject = (IPdfNumber)BaseDataObject[PdfName.LW];
        return lineWidthObject != null ? lineWidthObject.RawValue : (double?)null;
      }
    }

    [PDF(VersionEnum.PDF13)]
    public double? MiterLimit
    {
      get
      {
        IPdfNumber miterLimitObject = (IPdfNumber)BaseDataObject[PdfName.ML];
        return miterLimitObject != null ? miterLimitObject.RawValue : (double?)null;
      }
    }
  }
  #endregion
  #endregion
  #endregion
}