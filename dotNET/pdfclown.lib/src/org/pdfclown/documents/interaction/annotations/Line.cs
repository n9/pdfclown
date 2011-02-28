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

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.documents.interaction.annotations
{
  /**
    <summary>Line annotation [PDF:1.6:8.4.5].</summary>
    <remarks>It displays displays a single straight line on the page.
    When opened, it displays a pop-up window containing the text of the associated note.</remarks>
  */
  [PDF(VersionEnum.PDF13)]
  public sealed class Line
    : Annotation
  {
    #region types
    /**
      <summary>Line ending style [PDF:1.6:8.4.5].</summary>
    */
    public enum LineEndStyleEnum
    {
      /**
        Square.
      */
      Square,
      /**
        Circle.
      */
      Circle,
      /**
        Diamond.
      */
      Diamond,
      /**
        Open arrow.
      */
      OpenArrow,
      /**
        Closed arrow.
      */
      ClosedArrow,
      /**
        None.
      */
      None,
      /**
        Butt.
      */
      Butt,
      /**
        Reverse open arrow.
      */
      ReverseOpenArrow,
      /**
        Reverse closed arrow.
      */
      ReverseClosedArrow,
      /**
        Slash.
      */
      Slash
    };
    #endregion

    #region static
    #region fields
    private static readonly double DefaultLeaderLineExtensionLength = 0;
    private static readonly double DefaultLeaderLineLength = 0;
    private static readonly LineEndStyleEnum DefaultLineEndStyle = LineEndStyleEnum.None;

    private static readonly Dictionary<LineEndStyleEnum,PdfName> LineEndStyleEnumCodes;
    #endregion

    #region constructors
    static Line()
    {
      LineEndStyleEnumCodes = new Dictionary<LineEndStyleEnum,PdfName>();
      LineEndStyleEnumCodes[LineEndStyleEnum.Square] = PdfName.Square;
      LineEndStyleEnumCodes[LineEndStyleEnum.Circle] = PdfName.Circle;
      LineEndStyleEnumCodes[LineEndStyleEnum.Diamond] = PdfName.Diamond;
      LineEndStyleEnumCodes[LineEndStyleEnum.OpenArrow] = PdfName.OpenArrow;
      LineEndStyleEnumCodes[LineEndStyleEnum.ClosedArrow] = PdfName.ClosedArrow;
      LineEndStyleEnumCodes[LineEndStyleEnum.None] = PdfName.None;
      LineEndStyleEnumCodes[LineEndStyleEnum.Butt] = PdfName.Butt;
      LineEndStyleEnumCodes[LineEndStyleEnum.ReverseOpenArrow] = PdfName.ROpenArrow;
      LineEndStyleEnumCodes[LineEndStyleEnum.ReverseClosedArrow] = PdfName.RClosedArrow;
      LineEndStyleEnumCodes[LineEndStyleEnum.Slash] = PdfName.Slash;
    }
    #endregion

    #region interface
    #region private
    /**
      <summary>Gets the code corresponding to the given value.</summary>
    */
    private static PdfName ToCode(
      LineEndStyleEnum value
      )
    {return LineEndStyleEnumCodes[value];}

    /**
      <summary>Gets the line ending style corresponding to the given value.</summary>
    */
    private static LineEndStyleEnum ToLineEndStyleEnum(
      PdfName value
      )
    {
      foreach(KeyValuePair<LineEndStyleEnum,PdfName> style in LineEndStyleEnumCodes)
      {
        if(style.Value.Equals(value))
          return style.Key;
      }
      return DefaultLineEndStyle;
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public Line(
      Page page,
      PointF startPoint,
      PointF endPoint
      ) : base(
        page.Document,
        PdfName.Line,
        new RectangleF(
          startPoint.X,
          startPoint.Y,
          endPoint.X-startPoint.X,
          endPoint.Y-startPoint.Y
          ),
        page
        )
    {
      BaseDataObject[PdfName.L] = new PdfArray(new PdfDirectObject[]{new PdfReal(0),new PdfReal(0),new PdfReal(0),new PdfReal(0)});
      StartPoint = startPoint;
      EndPoint = endPoint;
    }

    public Line(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      ) : base(baseObject,container)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets whether the contents should be shown as a caption.</summary>
    */
    public bool CaptionVisible
    {
      get
      {
        /*
          NOTE: 'Cap' entry may be undefined.
        */
        PdfBoolean captionVisibleObject = (PdfBoolean)BaseDataObject[PdfName.Cap];
        if(captionVisibleObject == null)
          return false;

        return captionVisibleObject.RawValue;
      }
      set
      {BaseDataObject[PdfName.Cap] = PdfBoolean.Get(value);}
    }

    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets the ending coordinates.</summary>
    */
    public PointF EndPoint
    {
      get
      {
        /*
          NOTE: 'L' entry MUST be defined.
        */
        PdfArray coordinatesObject = (PdfArray)BaseDataObject[PdfName.L];

        return new PointF(
          (float)((IPdfNumber)coordinatesObject[2]).RawValue,
          (float)((IPdfNumber)coordinatesObject[3]).RawValue
          );
      }
      set
      {
        PdfArray coordinatesObject = (PdfArray)BaseDataObject[PdfName.L];
        coordinatesObject[2] = new PdfReal(value.X);
        coordinatesObject[3] = new PdfReal(Page.Box.Height-value.Y);
      }
    }

    /**
      <summary>Gets/Sets the style of the ending line ending.</summary>
    */
    public LineEndStyleEnum EndStyle
    {
      get
      {
        /*
          NOTE: 'LE' entry may be undefined.
        */
        PdfArray endstylesObject = (PdfArray)BaseDataObject[PdfName.LE];
        if(endstylesObject == null)
          return DefaultLineEndStyle;

        return ToLineEndStyleEnum((PdfName)endstylesObject[1]);
      }
      set
      {EnsureLineEndStylesObject()[1] = ToCode(value);}
    }

    /**
      <summary>Gets/Sets the color with which to fill the interior of the annotation's line endings.</summary>
    */
    public DeviceRGBColor FillColor
    {
      get
      {
        /*
          NOTE: 'IC' entry may be undefined.
        */
        PdfArray fillColorObject = (PdfArray)BaseDataObject[PdfName.IC];
        if(fillColorObject == null)
          return null;
//TODO:use baseObject constructor!!!
        return new DeviceRGBColor(
          ((IPdfNumber)fillColorObject[0]).RawValue,
          ((IPdfNumber)fillColorObject[1]).RawValue,
          ((IPdfNumber)fillColorObject[2]).RawValue
          );
      }
      set
      {BaseDataObject[PdfName.IC] = (PdfDirectObject)value.BaseDataObject;}
    }

    /**
      <summary>Gets/Sets the length of leader line extensions that extend
      in the opposite direction from the leader lines.</summary>
    */
    public double LeaderLineExtensionLength
    {
      get
      {
        /*
          NOTE: 'LLE' entry may be undefined.
        */
        IPdfNumber leaderLineExtensionLengthObject = (IPdfNumber)BaseDataObject[PdfName.LLE];
        if(leaderLineExtensionLengthObject == null)
          return DefaultLeaderLineExtensionLength;

        return leaderLineExtensionLengthObject.RawValue;
      }
      set
      {
        BaseDataObject[PdfName.LLE] = new PdfReal(value);
        /*
          NOTE: If leader line extension entry is present, leader line MUST be too.
        */
        if(!BaseDataObject.ContainsKey(PdfName.LL))
        {LeaderLineLength = DefaultLeaderLineLength;}
      }
    }

    /**
      <summary>Gets/Sets the length of leader lines that extend from each endpoint
      of the line perpendicular to the line itself.</summary>
      <remarks>A positive value means that the leader lines appear in the direction
      that is clockwise when traversing the line from its starting point
      to its ending point; a negative value indicates the opposite direction.</remarks>
    */
    public double LeaderLineLength
    {
      get
      {
        /*
          NOTE: 'LL' entry may be undefined.
        */
        IPdfNumber leaderLineLengthObject = (IPdfNumber)BaseDataObject[PdfName.LL];
        if(leaderLineLengthObject == null)
          return DefaultLeaderLineLength;

        return -leaderLineLengthObject.RawValue;
      }
      set
      {BaseDataObject[PdfName.LL] = new PdfReal(-value);}
    }

    /**
      <summary>Gets/Sets the starting coordinates.</summary>
    */
    public PointF StartPoint
    {
      get
      {
        /*
          NOTE: 'L' entry MUST be defined.
        */
        PdfArray coordinatesObject = (PdfArray)BaseDataObject[PdfName.L];

        return new PointF(
          (float)((IPdfNumber)coordinatesObject[0]).RawValue,
          (float)((IPdfNumber)coordinatesObject[1]).RawValue
          );
      }
      set
      {
        PdfArray coordinatesObject = (PdfArray)BaseDataObject[PdfName.L];
        coordinatesObject[0] = new PdfReal(value.X);
        coordinatesObject[1] = new PdfReal(Page.Box.Height-value.Y);
      }
    }

    /**
      <summary>Gets/Sets the style of the starting line ending.</summary>
    */
    public LineEndStyleEnum StartStyle
    {
      get
      {
        /*
          NOTE: 'LE' entry may be undefined.
        */
        PdfArray endstylesObject = (PdfArray)BaseDataObject[PdfName.LE];
        if(endstylesObject == null)
          return DefaultLineEndStyle;

        return ToLineEndStyleEnum((PdfName)endstylesObject[0]);
      }
      set
      {EnsureLineEndStylesObject()[0] = ToCode(value);}
    }
    #endregion

    #region private
    private PdfArray EnsureLineEndStylesObject(
      )
    {
      PdfArray endStylesObject = (PdfArray)BaseDataObject[PdfName.LE];
      if(endStylesObject == null)
      {
        BaseDataObject[PdfName.LE] = endStylesObject = new PdfArray(
          new PdfDirectObject[]
          {
            ToCode(DefaultLineEndStyle),
            ToCode(DefaultLineEndStyle)
          }
          );
      }

      return endStylesObject;
    }
    #endregion
    #endregion
    #endregion
  }
}