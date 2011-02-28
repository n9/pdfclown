/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.util.NotImplementedException;

/**
  Resources collection [PDF:1.6:3.7.2].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
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
		PdfDirectObject baseObject,
		PdfIndirectObject container
		)
	{
    return baseObject == null
	  	? null
			: new Resources(baseObject, container);
	}
  // </interface>
  // </static>
	
  // <dynamic>
  // <constructors>
  public Resources(
    Document context
    )
  {super(context.getFile(), new PdfDictionary());}

  private Resources(
    PdfDirectObject baseObject,
    PdfIndirectObject container
    )
  {super(baseObject, container);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Resources clone(
    Document context
    )
  {throw new NotImplementedException();}

  public ColorSpaceResources getColorSpaces(
    )
  {return ColorSpaceResources.wrap(getBaseDataObject().get(PdfName.ColorSpace), getContainer());}

  public ExtGStateResources getExtGStates(
    )
  {return ExtGStateResources.wrap(getBaseDataObject().get(PdfName.ExtGState), getContainer());}

  public FontResources getFonts(
    )
  {return FontResources.wrap(getBaseDataObject().get(PdfName.Font), getContainer());}

  public PatternResources getPatterns(
    )
  {return PatternResources.wrap(getBaseDataObject().get(PdfName.Pattern), getContainer());}

  @PDF(VersionEnum.PDF12)
  public PropertyListResources getPropertyLists(
    )
  {return PropertyListResources.wrap(getBaseDataObject().get(PdfName.Properties), getContainer());}

  @PDF(VersionEnum.PDF13)
  public ShadingResources getShadings(
	  )
	{return ShadingResources.wrap(getBaseDataObject().get(PdfName.Shading), getContainer());}

  public XObjectResources getXObjects(
    )
  {return XObjectResources.wrap(getBaseDataObject().get(PdfName.XObject), getContainer());}

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