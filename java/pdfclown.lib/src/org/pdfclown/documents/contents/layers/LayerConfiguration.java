/*
  Copyright 2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.contents.layers;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.files.File;
import org.pdfclown.objects.Array;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfSimpleObject;
import org.pdfclown.objects.PdfTextString;
import org.pdfclown.util.NotImplementedException;

/**
  Optional content configuration [PDF:1.7:4.10.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.1, 06/08/11
*/
@PDF(VersionEnum.PDF15)
public class LayerConfiguration
  extends PdfObjectWrapper<PdfDictionary>
  implements ILayerConfiguration
{
  // <class>
  // <dynamic>
  // <constructors>
  public LayerConfiguration(
    Document context
    )
  {super(context, new PdfDictionary());}

  public LayerConfiguration(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Object clone(
    Document context
    )
  {throw new NotImplementedException();}

  // <ILayerConfiguration>
  @Override
  public BaseStateEnum getBaseState(
    )
  {return BaseStateEnum.valueOf((PdfName)getBaseDataObject().get(PdfName.BaseState));}

  @Override
  public String getCreator(
    )
  {return (String)PdfSimpleObject.getValue(getBaseDataObject().get(PdfName.Creator));}

  @Override
  public Layers getLayers(
    )
  {return new Layers(getBaseDataObject().ensure(PdfName.Order, PdfArray.class));}

  @Override
  public ListModeEnum getListMode(
    )
  {return ListModeEnum.valueOf((PdfName)getBaseDataObject().get(PdfName.ListMode));}

  @Override
  public Array<LayerGroup> getOptionGroups(
    )
  {return new Array<LayerGroup>(LayerGroup.class, getBaseDataObject().ensure(PdfName.RBGroups, PdfArray.class));}

  @Override
  public String getTitle(
    )
  {return (String)PdfSimpleObject.getValue(getBaseDataObject().get(PdfName.Name));}

  @Override
  public void setBaseState(
    BaseStateEnum value
    )
  {getBaseDataObject().put(PdfName.BaseState, value.getName());}

  @Override
  public void setCreator(
    String value
    )
  {getBaseDataObject().put(PdfName.Creator, PdfTextString.get(value));}

  @Override
  public void setLayers(
    Layers value
    )
  {getBaseDataObject().put(PdfName.Order, value.getBaseObject());}

  @Override
  public void setListMode(
    ListModeEnum value
    )
  {getBaseDataObject().put(PdfName.ListMode, value.getName());}

  @Override
  public void setTitle(
    String value
    )
  {getBaseDataObject().put(PdfName.Name, PdfTextString.get(value));}
  // </ILayerConfiguration>
  // </public>

  // <internal>
  /**
    Gets the collection of the layer objects whose state is set to OFF.
  */
  PdfArray getOffLayersObject(
    )
  {return (PdfArray)File.resolve(getBaseDataObject().ensure(PdfName.OFF, PdfArray.class));}

  /**
    Gets the collection of the layer objects whose state is set to ON.
  */
  PdfArray getOnLayersObject(
    )
  {return (PdfArray)File.resolve(getBaseDataObject().ensure(PdfName.ON, PdfArray.class));}
  // <internal>
  // </interface>
  // </dynamic>
  // </class>
}
