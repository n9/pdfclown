/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.documents;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;

namespace org.pdfclown.documents.contents
{
  /**
    <summary>Resources collection [PDF:1.6:3.7.2].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class Resources
    : PdfObjectWrapper<PdfDictionary>
  {
/*
  TODO:create a mechanism for parameterizing resource retrieval/assignment
  based on the tuple <Class resourceClass, PdfName resourceName> -- see Names class!
*/
    #region static
    #region interface
    public static Resources Wrap(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      )
    {
      return baseObject == null
        ? null
        : new Resources(baseObject, container);
    }
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public Resources(
      Document context
      ) : base(context.File, new PdfDictionary())
    {}

    private Resources(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      ) : base(baseObject, container)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    public ColorSpaceResources ColorSpaces
    {
      get
      {return ColorSpaceResources.Wrap(BaseDataObject[PdfName.ColorSpace], Container);}
      set
      {BaseDataObject[PdfName.ColorSpace] = value.BaseObject;}
    }

    public ExtGStateResources ExtGStates
    {
      get
      {return ExtGStateResources.Wrap(BaseDataObject[PdfName.ExtGState], Container);}
      set
      {BaseDataObject[PdfName.ExtGState] = value.BaseObject;}
    }

    public FontResources Fonts
    {
      get
      {return FontResources.Wrap(BaseDataObject[PdfName.Font], Container);}
      set
      {BaseDataObject[PdfName.Font] = value.BaseObject;}
    }

    public PatternResources Patterns
    {
      get
      {return PatternResources.Wrap(BaseDataObject[PdfName.Pattern], Container);}
      set
      {BaseDataObject[PdfName.Pattern] = value.BaseObject;}
    }

    [PDF(VersionEnum.PDF12)]
    public PropertyListResources PropertyLists
    {
      get
      {return PropertyListResources.Wrap(BaseDataObject[PdfName.Properties], Container);}
      set
      {
        CheckCompatibility("PropertyLists");
        BaseDataObject[PdfName.Properties] = value.BaseObject;
      }
    }

    [PDF(VersionEnum.PDF13)]
    public ShadingResources Shadings
    {
      get
      {return ShadingResources.Wrap(BaseDataObject[PdfName.Shading], Container);}
      set
      {BaseDataObject[PdfName.Shading] = value.BaseObject;}
    }

    public XObjectResources XObjects
    {
      get
      {return XObjectResources.Wrap(BaseDataObject[PdfName.XObject], Container);}
      set
      {BaseDataObject[PdfName.XObject] = value.BaseObject;}
    }
  }
  #endregion
  #endregion
  #endregion
}