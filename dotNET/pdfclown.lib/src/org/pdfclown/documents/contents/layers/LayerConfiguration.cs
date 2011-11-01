/*
  Copyright 2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.files;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.contents.layers
{
  /**
    <summary>Optional content configuration [PDF:1.7:4.10.3].</summary>
  */
  [PDF(VersionEnum.PDF15)]
  public class LayerConfiguration
    : PdfObjectWrapper<PdfDictionary>,
      ILayerConfiguration
  {
    #region dynamic
    #region constructors
    public LayerConfiguration(
      Document context
      ) : base(context, new PdfDictionary())
    {}

    public LayerConfiguration(
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

    #region ILayerConfiguration
    public BaseStateEnum BaseState
    {
      get
      {return BaseStateEnumExtension.Get((PdfName)BaseDataObject[PdfName.BaseState]);}
      set
      {BaseDataObject[PdfName.BaseState] = value.GetName();}
    }

    public string Creator
    {
      get
      {return (string)PdfSimpleObject<object>.GetValue(BaseDataObject[PdfName.Creator]);}
      set
      {BaseDataObject[PdfName.Creator] = PdfTextString.Get(value);}
    }

    public Layers Layers
    {
      get
      {return new Layers(BaseDataObject.Ensure<PdfArray>(PdfName.Order));}
      set
      {BaseDataObject[PdfName.Order] = value.BaseObject;}
    }

    public ListModeEnum ListMode
    {
      get
      {return ListModeEnumExtension.Get((PdfName)BaseDataObject[PdfName.ListMode]);}
      set
      {BaseDataObject[PdfName.ListMode] = value.GetName();}
    }

    public Array<LayerGroup> OptionGroups
    {
      get
      {return new Array<LayerGroup>(BaseDataObject.Ensure<PdfArray>(PdfName.RBGroups));}
    }

    public string Title
    {
      get
      {return (string)PdfSimpleObject<object>.GetValue(BaseDataObject[PdfName.Name]);}
      set
      {BaseDataObject[PdfName.Name] = PdfTextString.Get(value);}
    }
    #endregion
    #endregion

    #region internal
    /**
      <summary>Gets the collection of the layer objects whose state is set to OFF.</summary>
    */
    internal PdfArray OffLayersObject
    {
      get
      {return (PdfArray)File.Resolve(BaseDataObject.Ensure<PdfArray>(PdfName.OFF));}
    }

    /**
      <summary>Gets the collection of the layer objects whose state is set to ON.</summary>
    */
    internal PdfArray OnLayersObject
    {
      get
      {return (PdfArray)File.Resolve(BaseDataObject.Ensure<PdfArray>(PdfName.ON));}
    }
    #endregion
    #endregion
    #endregion
  }
}