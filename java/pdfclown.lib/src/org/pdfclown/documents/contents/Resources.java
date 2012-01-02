/*
  Copyright 2006-2012 Stefano Chizzolini. http://www.pdfclown.org

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
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;

/**
  Resources collection [PDF:1.6:3.7.2].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.2, 01/02/12
*/
@PDF(VersionEnum.PDF10)
public final class Resources
  extends PdfObjectWrapper<PdfDictionary>
{
/*
 TODO:create a mechanism for parameterizing resource retrieval/assignment
 based on the tuple <Class resourceClass, PdfName resourceName> -- see Names class!
*/
  // <class>
  // <static>
  // <interface>
  public static Resources wrap(
    PdfDirectObject baseObject
    )
  {return baseObject != null ? new Resources(baseObject) : null;}
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  public Resources(
    Document context
    )
  {super(context, new PdfDictionary());}

  private Resources(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Resources clone(
    Document context
    )
  {return new Resources((PdfDirectObject)getBaseObject().clone(context.getFile()));}

  public ColorSpaceResources getColorSpaces(
    )
  {
    PdfDirectObject colorSpaceObject = getBaseDataObject().get(PdfName.ColorSpace);
    return colorSpaceObject != null ? new ColorSpaceResources(colorSpaceObject) : null;
  }

  public ExtGStateResources getExtGStates(
    )
  {
    PdfDirectObject extGStateObject = getBaseDataObject().get(PdfName.ExtGState);
    return extGStateObject != null ? new ExtGStateResources(extGStateObject) : null;
  }

  public FontResources getFonts(
    )
  {
    PdfDirectObject fontObject = getBaseDataObject().get(PdfName.Font);
    return fontObject != null ? new FontResources(fontObject) : null;
  }

  public PatternResources getPatterns(
    )
  {
    PdfDirectObject patternObject = getBaseDataObject().get(PdfName.Pattern);
    return patternObject != null ? new PatternResources(patternObject) : null;
  }

  @PDF(VersionEnum.PDF12)
  public PropertyListResources getPropertyLists(
    )
  {
    PdfDirectObject propertiesObject = getBaseDataObject().get(PdfName.Properties);
    return propertiesObject != null ? new PropertyListResources(propertiesObject) : null;
  }

  @PDF(VersionEnum.PDF13)
  public ShadingResources getShadings(
    )
  {
    PdfDirectObject shadingObject = getBaseDataObject().get(PdfName.Shading);
    return shadingObject != null ? new ShadingResources(shadingObject) : null;
  }

  public XObjectResources getXObjects(
    )
  {
    PdfDirectObject xObjectObject = getBaseDataObject().get(PdfName.XObject);
    return xObjectObject != null ? new XObjectResources(xObjectObject) : null;
  }

  public void setColorSpaces(
    ColorSpaceResources value
    )
  {getBaseDataObject().put(PdfName.ColorSpace,value.getBaseObject());}

  public void setExtGStates(
    ExtGStateResources value
    )
  {getBaseDataObject().put(PdfName.ExtGState,value.getBaseObject());}

  public void setFonts(
    FontResources value
    )
  {getBaseDataObject().put(PdfName.Font,value.getBaseObject());}

  public void setPatterns(
    PatternResources value
    )
  {getBaseDataObject().put(PdfName.Pattern,value.getBaseObject());}

  public void setPropertyLists(
    PropertyListResources value
    )
  {
    checkCompatibility("propertyLists");
    getBaseDataObject().put(PdfName.Properties,value.getBaseObject());
  }

  public void setShadings(
    ShadingResources value
    )
  {
    checkCompatibility("shadings");
    getBaseDataObject().put(PdfName.Shading,value.getBaseObject());
  }

  public void setXObjects(
    XObjectResources value
    )
  {getBaseDataObject().put(PdfName.XObject,value.getBaseObject());}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}