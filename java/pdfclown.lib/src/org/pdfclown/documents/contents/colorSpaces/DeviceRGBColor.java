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

package org.pdfclown.documents.contents.colorSpaces;

import java.util.Arrays;
import java.util.List;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.util.NotImplementedException;

/**
  Device Red-Green-Blue color value [PDF:1.6:4.5.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
*/
@PDF(VersionEnum.PDF11)
public final class DeviceRGBColor
  extends DeviceColor
{
  // <class>
  // <static>
  // <fields>
  public static final DeviceRGBColor Black = new DeviceRGBColor(0,0,0);
  public static final DeviceRGBColor White = new DeviceRGBColor(1,1,1);

  public static final DeviceRGBColor Default = Black;
  // </fields>
  
  // <interface>
  // <public>
  /**
  	Gets the color corresponding to the specified components.
  	
  	@param components Color components to convert.
  	@since 0.1.0
   */
  public static DeviceRGBColor get(
  	PdfArray components
  	)
  {
	  return (components != null
	  	? new DeviceRGBColor(components)
	  	: Default
	  	);
  }
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  public DeviceRGBColor(
    float r,
    float g,
    float b
    )
  {
  	this(
  		Arrays.asList(
    		new PdfReal(normalizeComponent(r)),
    		new PdfReal(normalizeComponent(g)),
    		new PdfReal(normalizeComponent(b))
  			)
  		);
  }
  
  DeviceRGBColor(
  	List<? extends PdfDirectObject> components
    )
  {
    super(
    	DeviceRGBColorSpace.Default,
    	new PdfArray(components)
    	);
  }
  // </constructors>

  // <interface>
  // <public>
	@Override
	public Object clone(
		Document context
		)
	{throw new NotImplementedException();}

  /**
    Gets the blue component.
  */
  public float getB(
    )
  {return getComponentValue(2);}

  /**
    Gets the green component.
  */
  public float getG(
    )
  {return getComponentValue(1);}

  /**
    Gets the red component.
  */
  public float getR(
    )
  {return getComponentValue(0);}

  /**
  	@see #getB()
  */
  public void setB(
    float value
    )
  {setComponentValue(2, value);}

  /**
  	@see #getG()
  */
  public void setG(
    float value
    )
  {setComponentValue(1, value);}

  /**
  	@see #getR()
  */
  public void setR(
    float value
    )
  {setComponentValue(0, value);}
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}