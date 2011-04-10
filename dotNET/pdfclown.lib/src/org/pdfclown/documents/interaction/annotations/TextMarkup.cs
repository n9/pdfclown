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

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.documents.interaction.annotations
{
  /**
    <summary>Text markup annotation [PDF:1.6:8.4.5].</summary>
    <remarks>It displays highlights, underlines, strikeouts, or jagged ("squiggly") underlines
    in the text of a document.</remarks>
  */
  [PDF(VersionEnum.PDF13)]
  public sealed class TextMarkup
    : Annotation
  {
    #region types
    /**
      <summary>Markup type [PDF:1.6:8.4.5].</summary>
    */
    public enum MarkupTypeEnum
    {
      /**
        <summary>Highlight.</summary>
      */
      [PDF(VersionEnum.PDF13)]
      Highlight,
      /**
        <summary>Squiggly.</summary>
      */
      [PDF(VersionEnum.PDF14)]
      Squiggly,
      /**
        <summary>StrikeOut.</summary>
      */
      [PDF(VersionEnum.PDF13)]
      StrikeOut,
      /**
        <summary>Underline.</summary>
      */
      [PDF(VersionEnum.PDF13)]
      Underline
    };
    #endregion

    #region static
    #region fields
    private static readonly Dictionary<MarkupTypeEnum,PdfName> MarkupTypeEnumCodes;
    #endregion

    #region constructors
    static TextMarkup()
    {
      MarkupTypeEnumCodes = new Dictionary<MarkupTypeEnum,PdfName>();
      MarkupTypeEnumCodes[MarkupTypeEnum.Highlight] = PdfName.Highlight;
      MarkupTypeEnumCodes[MarkupTypeEnum.Squiggly] = PdfName.Squiggly;
      MarkupTypeEnumCodes[MarkupTypeEnum.StrikeOut] = PdfName.StrikeOut;
      MarkupTypeEnumCodes[MarkupTypeEnum.Underline] = PdfName.Underline;
    }
    #endregion

    #region interface
    #region private
    /**
      <summary>Gets the code corresponding to the given value.</summary>
    */
    private static PdfName ToCode(
      MarkupTypeEnum value
      )
    {return MarkupTypeEnumCodes[value];}

    /**
      <summary>Gets the markup type corresponding to the given value.</summary>
    */
    private static MarkupTypeEnum ToMarkupTypeEnum(
      PdfName value
      )
    {
      foreach(KeyValuePair<MarkupTypeEnum,PdfName> markupType in MarkupTypeEnumCodes)
      {
        if(markupType.Value.Equals(value))
          return markupType.Key;
      }
      throw new Exception("Invalid markup type.");
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public TextMarkup(
      Page page,
      RectangleF box,
      MarkupTypeEnum type
      ) : base(
        page.Document,
        ToCode(type),
        box,
        page
        )
    {}

    public TextMarkup(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets the quadrilaterals encompassing a word or group of contiguous words
      in the text underlying the annotation.</summary>
    */
    public IList<RectangleF> Boxes
    {
      get
      {
        PdfArray quadPointsObject = (PdfArray)BaseDataObject[PdfName.QuadPoints];
        IList<RectangleF> boxes = new List<RectangleF>();
        double pageHeight = Page.Box.Height;
        for(
          int index = 0,
            length = quadPointsObject.Count;
          index < length;
          index += 8
          )
        {
          double x = ((IPdfNumber)quadPointsObject[index+6]).RawValue;
          double y = pageHeight - ((IPdfNumber)quadPointsObject[index+7]).RawValue;
          double width = ((IPdfNumber)quadPointsObject[index+2]).RawValue - ((IPdfNumber)quadPointsObject[index]).RawValue;
          double height = ((IPdfNumber)quadPointsObject[index+3]).RawValue - ((IPdfNumber)quadPointsObject[index+1]).RawValue;
          boxes.Add(
            new RectangleF((float)x,(float)y,(float)width,(float)height)
            );
        }
        return boxes;
      }
      set
      {
        PdfArray quadPointsObject = new PdfArray();
        double pageHeight = Page.Box.Height;
        foreach(RectangleF box in value)
        {
          quadPointsObject.Add(new PdfReal(box.X)); // x1.
          quadPointsObject.Add(new PdfReal(pageHeight-(box.Y+box.Height))); // y1.
          quadPointsObject.Add(new PdfReal(box.X+box.Width)); // x2.
          quadPointsObject.Add(quadPointsObject[1]); // y2.
          quadPointsObject.Add(quadPointsObject[2]); // x3.
          quadPointsObject.Add(new PdfReal(pageHeight-box.Y)); // y3.
          quadPointsObject.Add(quadPointsObject[0]); // x4.
          quadPointsObject.Add(quadPointsObject[5]); // y4.
        }

        BaseDataObject[PdfName.QuadPoints] = quadPointsObject;
      }
    }

    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets the markup type.</summary>
    */
    public MarkupTypeEnum MarkupType
    {
      get
      {return ToMarkupTypeEnum((PdfName)BaseDataObject[PdfName.Subtype]);}
      set
      {BaseDataObject[PdfName.Subtype] = ToCode(value);}
    }
    #endregion
    #endregion
    #endregion
  }
}