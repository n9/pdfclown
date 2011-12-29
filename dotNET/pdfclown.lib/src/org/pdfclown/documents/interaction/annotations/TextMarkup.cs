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
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.objects;
using org.pdfclown.util.math.geom;

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
    /**
      <summary>Creates a new text markup on the specified page, making it printable by default.
      </summary>
      <param name="page">Page to annotate.</param>
      <param name="markupType">Markup type.</param>
      <param name="markupBox">Quadrilateral encompassing a word or group of contiguous words in the
      text underlying the annotation.</param>
    */
    public TextMarkup(
      Page page,
      MarkupTypeEnum markupType,
      Quad markupBox
      ) : this(
        page,
        markupType,
        new List<Quad>(){markupBox}
        )
    {}

    /**
      <summary>Creates a new text markup on the specified page, making it printable by default.
      </summary>
      <param name="page">Page to annotate.</param>
      <param name="markupType">Markup type.</param>
      <param name="markupBoxes">Quadrilaterals encompassing a word or group of contiguous words in
      the text underlying the annotation.</param>
    */
    public TextMarkup(
      Page page,
      MarkupTypeEnum markupType,
      IList<Quad> markupBoxes
      ) : this(
        page,
        markupBoxes[0].GetBounds(),
        markupType,
        markupBoxes
        )
    {}

    /**
      <summary>Creates a new text markup on the specified page, making it printable by default.
      </summary>
      <param name="page">Page to annotate.</param>
      <param name="box">Annotation location on the page.</param>
      <param name="markupType">Markup type.</param>
      <param name="markupBoxes">Quadrilaterals encompassing a word or group of contiguous words in
      the text underlying the annotation.</param>
    */
    public TextMarkup(
      Page page,
      RectangleF box,
      MarkupTypeEnum markupType,
      IList<Quad> markupBoxes
      ) : base(
        page.Document,
        ToCode(markupType),
        box,
        page
        )
    {
      MarkupType = markupType;
      MarkupBoxes = markupBoxes;
      Printable = true;
    }

    public TextMarkup(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets the quadrilaterals encompassing a word or group of contiguous words
      in the text underlying the annotation.</summary>
    */
    public IList<Quad> MarkupBoxes
    {
      get
      {
        PdfArray quadPointsObject = (PdfArray)BaseDataObject[PdfName.QuadPoints];
        IList<Quad> markupBoxes = new List<Quad>();
        float pageHeight = Page.Box.Height;
        for(
          int index = 0,
            length = quadPointsObject.Count;
          index < length;
          index += 8
          )
        {
          /*
            NOTE: Despite the spec prescription, Point 3 and Point 4 MUST be inverted.
          */
          markupBoxes.Add(
            new Quad(
              new PointF(
                ((IPdfNumber)quadPointsObject[index]).FloatValue,
                pageHeight - ((IPdfNumber)quadPointsObject[index + 1]).FloatValue
                ),
              new PointF(
                ((IPdfNumber)quadPointsObject[index + 2]).FloatValue,
                pageHeight - ((IPdfNumber)quadPointsObject[index + 3]).FloatValue
                ),
              new PointF(
                ((IPdfNumber)quadPointsObject[index + 6]).FloatValue,
                pageHeight - ((IPdfNumber)quadPointsObject[index + 7]).FloatValue
                ),
              new PointF(
                ((IPdfNumber)quadPointsObject[index + 4]).FloatValue,
                pageHeight - ((IPdfNumber)quadPointsObject[index + 5]).FloatValue
                )
              )
            );
        }
        return markupBoxes;
      }
      set
      {
        PdfArray quadPointsObject = new PdfArray();
        double pageHeight = Page.Box.Height;
        foreach(Quad markupBox in value)
        {
          /*
            NOTE: Despite the spec prescription, Point 3 and Point 4 MUST be inverted.
          */
          PointF[] markupBoxPoints = markupBox.Points;
          quadPointsObject.Add(new PdfReal(markupBoxPoints[0].X)); // x1.
          quadPointsObject.Add(new PdfReal(pageHeight - markupBoxPoints[0].Y)); // y1.
          quadPointsObject.Add(new PdfReal(markupBoxPoints[1].X)); // x2.
          quadPointsObject.Add(new PdfReal(pageHeight - markupBoxPoints[1].Y)); // y2.
          quadPointsObject.Add(new PdfReal(markupBoxPoints[3].X)); // x4.
          quadPointsObject.Add(new PdfReal(pageHeight - markupBoxPoints[3].Y)); // y4.
          quadPointsObject.Add(new PdfReal(markupBoxPoints[2].X)); // x3.
          quadPointsObject.Add(new PdfReal(pageHeight - markupBoxPoints[2].Y)); // y3.
        }
        BaseDataObject[PdfName.QuadPoints] = quadPointsObject;
      }
    }

    /**
      <summary>Gets/Sets the markup type.</summary>
    */
    public MarkupTypeEnum MarkupType
    {
      get
      {return ToMarkupTypeEnum((PdfName)BaseDataObject[PdfName.Subtype]);}
      set
      {
        BaseDataObject[PdfName.Subtype] = ToCode(value);
        switch(value)
        {
          case MarkupTypeEnum.Highlight:
            Color = new DeviceRGBColor(1, 1, 0);
            break;
          case MarkupTypeEnum.Squiggly:
            Color = new DeviceRGBColor(1, 0, 0);
            break;
          default:
            Color = new DeviceRGBColor(0, 0, 0);
            break;
        }
      }
    }
    #endregion
    #endregion
    #endregion
  }
}