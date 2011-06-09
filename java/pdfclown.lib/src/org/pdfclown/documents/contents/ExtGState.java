/*
  Copyright 2009-2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.contents;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.ContentScanner.GraphicsState;
import org.pdfclown.documents.contents.fonts.Font;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfNumber;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.util.NotImplementedException;

/**
  Graphics state parameters [PDF:1.6:4.3.4].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.1, 06/08/11
*/
@PDF(VersionEnum.PDF12)
public final class ExtGState
  extends PdfObjectWrapper<PdfDictionary>
{
  // <class>
  // <static>
  // <interface>
  // <public>
  /**
    Wraps the specified base object into a graphics state parameter dictionary object.

    @param baseObject Base object of a graphics state parameter dictionary object.
    @param container Indirect object possibly containing the graphics state parameter dictionary base object.
    @return Graphics state parameter dictionary object corresponding to the base object.
  */
  public static ExtGState wrap(
    PdfDirectObject baseObject
    )
  {return baseObject != null ? new ExtGState(baseObject) : null;}
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  public ExtGState(
    Document context,
    PdfDictionary baseDataObject
    )
  {super(context, baseDataObject);}

  ExtGState(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  public void applyTo(
    GraphicsState state
    )
  {
    for(PdfName parameterName : getBaseDataObject().keySet())
    {
      if(parameterName.equals(PdfName.Font))
      {
        state.setFont(getFont());
        state.setFontSize(getFontSize());
      }
      else if(parameterName.equals(PdfName.LC))
      {state.setLineCap(getLineCap());}
      else if(parameterName.equals(PdfName.D))
      {state.setLineDash(getLineDash());}
      else if(parameterName.equals(PdfName.LJ))
      {state.setLineJoin(getLineJoin());}
      else if(parameterName.equals(PdfName.LW))
      {state.setLineWidth(getLineWidth());}
      else if(parameterName.equals(PdfName.ML))
      {state.setMiterLimit(getMiterLimit());}
      //TODO:extend supported parameters!!!
    }
  }

  @Override
  public ExtGState clone(
    Document context
    )
  {throw new NotImplementedException();}

  @PDF(VersionEnum.PDF13)
  public Font getFont(
    )
  {
    PdfArray fontObject = (PdfArray)getBaseDataObject().get(PdfName.Font);
    return fontObject != null ? Font.wrap(fontObject.get(0)) : null;
  }

  @PDF(VersionEnum.PDF13)
  public Float getFontSize(
    )
  {
    PdfArray fontObject = (PdfArray)getBaseDataObject().get(PdfName.Font);
    return fontObject != null ? ((PdfNumber<?>)fontObject.get(1)).getNumberValue() : null;
  }

  @PDF(VersionEnum.PDF13)
  public LineCapEnum getLineCap(
    )
  {
    PdfInteger lineCapObject = (PdfInteger)getBaseDataObject().get(PdfName.LC);
    return lineCapObject != null ? LineCapEnum.valueOf(lineCapObject.getRawValue()) : null;
  }

  @PDF(VersionEnum.PDF13)
  public LineDash getLineDash(
    )
  {
    PdfArray lineDashObject = (PdfArray)getBaseDataObject().get(PdfName.D);
    if(lineDashObject == null)
      return null;

    float[] dashArray;
    {
      PdfArray baseDashArray = (PdfArray)lineDashObject.get(0);
      dashArray = new float[baseDashArray.size()];
      for(
        int index = 0,
          length = dashArray.length;
        index < length;
        index++
        )
      {dashArray[index] = ((PdfNumber<?>)baseDashArray.get(index)).getNumberValue();}
    }
    float dashPhase = ((PdfNumber<?>)lineDashObject.get(1)).getNumberValue();
    return new LineDash(dashArray, dashPhase);
  }

  @PDF(VersionEnum.PDF13)
  public LineJoinEnum getLineJoin(
    )
  {
    PdfInteger lineJoinObject = (PdfInteger)getBaseDataObject().get(PdfName.LJ);
    return lineJoinObject != null ? LineJoinEnum.valueOf(lineJoinObject.getRawValue()) : null;
  }

  @PDF(VersionEnum.PDF13)
  public Float getLineWidth(
    )
  {
    PdfNumber<?> lineWidthObject = (PdfNumber<?>)getBaseDataObject().get(PdfName.LW);
    return lineWidthObject != null ? lineWidthObject.getNumberValue() : null;
  }

  @PDF(VersionEnum.PDF13)
  public Float getMiterLimit(
    )
  {
    PdfNumber<?> miterLimitObject = (PdfNumber<?>)getBaseDataObject().get(PdfName.ML);
    return miterLimitObject != null ? miterLimitObject.getNumberValue() : null;
  }
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}