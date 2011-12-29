/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library"
  (the Program): see the accompanying README files for more info.

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

package org.pdfclown.documents.interaction.annotations;

import java.awt.geom.Point2D;
import java.awt.geom.Rectangle2D;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.colorSpaces.DeviceRGBColor;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfNumber;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.util.NotImplementedException;
import org.pdfclown.util.math.geom.Quad;

/**
  Text markup annotation [PDF:1.6:8.4.5].
  <p>It displays highlights, underlines, strikeouts, or jagged ("squiggly") underlines
  in the text of a document.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.2, 12/28/11
*/
@PDF(VersionEnum.PDF13)
public final class TextMarkup
  extends Annotation
{
  // <class>
  // <classes>
  /**
    Markup type [PDF:1.6:8.4.5].
  */
  public enum MarkupTypeEnum
  {
    // <class>
    // <static>
    // <fields>
    /**
      Highlight.
    */
    @PDF(VersionEnum.PDF13)
    Highlight(PdfName.Highlight),
    /**
      Squiggly.
    */
    @PDF(VersionEnum.PDF14)
    Squiggly(PdfName.Squiggly),
    /**
      StrikeOut.
    */
    @PDF(VersionEnum.PDF13)
    StrikeOut(PdfName.StrikeOut),
    /**
      Underline.
    */
    @PDF(VersionEnum.PDF13)
    Underline(PdfName.Underline);
    // </fields>

    // <interface>
    // <public>
    /**
      Gets the markup type corresponding to the given value.
    */
    public static MarkupTypeEnum get(
      PdfName value
      )
    {
      for(MarkupTypeEnum markupType : MarkupTypeEnum.values())
      {
        if(markupType.getCode().equals(value))
          return markupType;
      }
      return null;
    }
    // </public>
    // </interface>
    // </static>

    // <dynamic>
    // <fields>
    private final PdfName code;
    // </fields>

    // <constructors>
    private MarkupTypeEnum(
      PdfName code
      )
    {this.code = code;}
    // </constructors>

    // <interface>
    // <public>
    public PdfName getCode(
      )
    {return code;}
    // </public>
    // </interface>
    // </dynamic>
    // </class>
  }
  // </classes>

  // <dynamic>
  // <constructors>
  /**
    Creates a new text markup on the specified page, making it printable by default.

    @param page Page to annotate.
    @param markupType Markup type.
    @param markupBox Quadrilateral encompassing a word or group of contiguous words in the text
      underlying the annotation.
  */
  public TextMarkup(
    Page page,
    MarkupTypeEnum markupType,
    Quad markupBox
    )
  {
    this(
      page,
      markupType,
      Arrays.asList(markupBox)
      );
  }

  /**
    Creates a new text markup on the specified page, making it printable by default.

    @param page Page to annotate.
    @param markupType Markup type.
    @param markupBoxes Quadrilaterals encompassing a word or group of contiguous words in the text
      underlying the annotation.
  */
  public TextMarkup(
    Page page,
    MarkupTypeEnum markupType,
    List<Quad> markupBoxes
    )
  {
    this(
      page,
      markupBoxes.get(0).getBounds2D(),
      markupType,
      markupBoxes
      );
  }

  /**
    Creates a new text markup on the specified page, making it printable by default.

    @param page Page to annotate.
    @param box Annotation location on the page.
    @param markupType Markup type.
    @param markupBoxes Quadrilaterals encompassing a word or group of contiguous words in the text
      underlying the annotation.
  */
  public TextMarkup(
    Page page,
    Rectangle2D box,
    MarkupTypeEnum markupType,
    List<Quad> markupBoxes
    )
  {
    super(
      page.getDocument(),
      markupType.getCode(),
      box,
      page
      );
    setMarkupType(markupType);
    setMarkupBoxes(markupBoxes);
    setPrintable(true);
  }

  public TextMarkup(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public TextMarkup clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Gets the quadrilaterals encompassing a word or group of contiguous words
    in the text underlying the annotation.
  */
  public List<Quad> getMarkupBoxes(
    )
  {
    PdfArray quadPointsObject = (PdfArray)getBaseDataObject().get(PdfName.QuadPoints);
    List<Quad> markupBoxes = new ArrayList<Quad>();
    double pageHeight = getPage().getBox().getHeight();
    for(
      int index = 0,
        length = quadPointsObject.size();
      index < length;
      index += 8
      )
    {
      /*
        NOTE: Despite the spec prescription, Point 3 and Point 4 MUST be inverted.
      */
      markupBoxes.add(
        new Quad(
          new Point2D.Double(
            ((PdfNumber<?>)quadPointsObject.get(index)).getDoubleValue(),
            pageHeight - ((PdfNumber<?>)quadPointsObject.get(index + 1)).getDoubleValue()
            ),
          new Point2D.Double(
            ((PdfNumber<?>)quadPointsObject.get(index + 2)).getDoubleValue(),
            pageHeight - ((PdfNumber<?>)quadPointsObject.get(index + 3)).getDoubleValue()
            ),
          new Point2D.Double(
            ((PdfNumber<?>)quadPointsObject.get(index + 6)).getDoubleValue(),
            pageHeight - ((PdfNumber<?>)quadPointsObject.get(index + 7)).getDoubleValue()
            ),
          new Point2D.Double(
            ((PdfNumber<?>)quadPointsObject.get(index + 4)).getDoubleValue(),
            pageHeight - ((PdfNumber<?>)quadPointsObject.get(index + 5)).getDoubleValue()
            )
          )
        );
    }
    return markupBoxes;
  }

  /**
    Gets the markup type.
  */
  public MarkupTypeEnum getMarkupType(
    )
  {return MarkupTypeEnum.get((PdfName)getBaseDataObject().get(PdfName.Subtype));}

  /**
    @see #getMarkupBoxes()
  */
  public void setMarkupBoxes(
    List<Quad> value
    )
  {
    PdfArray quadPointsObject = new PdfArray();
    double pageHeight = getPage().getBox().getHeight();
    for(Quad markupBox : value)
    {
      /*
        NOTE: Despite the spec prescription, Point 3 and Point 4 MUST be inverted.
      */
      Point2D[] markupBoxPoints = markupBox.getPoints();
      quadPointsObject.add(new PdfReal(markupBoxPoints[0].getX())); // x1.
      quadPointsObject.add(new PdfReal(pageHeight - markupBoxPoints[0].getY())); // y1.
      quadPointsObject.add(new PdfReal(markupBoxPoints[1].getX())); // x2.
      quadPointsObject.add(new PdfReal(pageHeight - markupBoxPoints[1].getY())); // y2.
      quadPointsObject.add(new PdfReal(markupBoxPoints[3].getX())); // x4.
      quadPointsObject.add(new PdfReal(pageHeight - markupBoxPoints[3].getY())); // y4.
      quadPointsObject.add(new PdfReal(markupBoxPoints[2].getX())); // x3.
      quadPointsObject.add(new PdfReal(pageHeight - markupBoxPoints[2].getY())); // y3.
    }
    getBaseDataObject().put(PdfName.QuadPoints, quadPointsObject);
  }

  /**
    @see #getMarkupType()
  */
  public void setMarkupType(
    MarkupTypeEnum value
    )
  {
    getBaseDataObject().put(PdfName.Subtype, value.getCode());
    switch(value)
    {
      case Highlight:
        setColor(new DeviceRGBColor(1, 1, 0));
        break;
      case Squiggly:
        setColor(new DeviceRGBColor(1, 0, 0));
        break;
      default:
        setColor(new DeviceRGBColor(0, 0, 0));
        break;
    }
  }
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}