/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using drawing = System.Drawing;
using System.Drawing.Drawing2D;

namespace org.pdfclown.documents.contents.xObjects
{
  /**
    <summary>Form external object [PDF:1.6:4.9].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class FormXObject
    : XObject,
      IContentContext
  {
    #region dynamic
    #region constructors
    /**
      <summary>Creates a new form within the given document context, using default resources.</summary>
    */
    public FormXObject(
      Document context
      ) : this(context, null)
    {}

    /**
      <summary>Creates a new form within the given document context, using custom resources.</summary>
    */
    public FormXObject(
      Document context,
      Resources resources
      ) : base(context)
    {
      PdfDictionary header = BaseDataObject.Header;
      header[PdfName.Subtype] = PdfName.Form;
      header[PdfName.BBox] = new Rectangle(0,0,0,0).BaseDataObject;

      // No resources collection?
      /* NOTE: Resources collection is mandatory. */
      if(resources == null)
      {resources = new Resources(context);}
      header[PdfName.Resources] = resources.BaseObject;
    }

    internal FormXObject(
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

    public override Matrix Matrix
    {
      get
      {
        /*
          NOTE: Form-space-to-user-space matrix is identity [1 0 0 1 0 0] by default,
          but may be adjusted by setting the Matrix entry in the form dictionary [PDF:1.6:4.9].
        */
        PdfArray matrix = (PdfArray)BaseDataObject.Header.Resolve(PdfName.Matrix);
        if(matrix == null)
          return new Matrix();
        else
          return new Matrix(
            ((IPdfNumber)matrix[0]).RawValue,
            ((IPdfNumber)matrix[1]).RawValue,
            ((IPdfNumber)matrix[2]).RawValue,
            ((IPdfNumber)matrix[3]).RawValue,
            ((IPdfNumber)matrix[4]).RawValue,
            ((IPdfNumber)matrix[5]).RawValue
            );
      }
    }

    public override drawing::SizeF Size
    {
      get
      {
        PdfArray box = (PdfArray)BaseDataObject.Header.Resolve(PdfName.BBox);
        return new drawing::SizeF(
          ((IPdfNumber)box[2]).RawValue,
          ((IPdfNumber)box[3]).RawValue
          );
      }
      set
      {
        PdfArray boxObject = (PdfArray)BaseDataObject.Header.Resolve(PdfName.BBox);
        boxObject[2] = new PdfReal(value.Width);
        boxObject[3] = new PdfReal(value.Height);
      }
    }
    #endregion

    #region internal
    #region IContentContext
    public drawing::RectangleF Box
    {
      get
      {
        PdfArray box = (PdfArray)BaseDataObject.Header.Resolve(PdfName.BBox);
        return new drawing::RectangleF(
          (float)((IPdfNumber)box[0]).RawValue,
          (float)((IPdfNumber)box[1]).RawValue,
          (float)((IPdfNumber)box[2]).RawValue,
          (float)((IPdfNumber)box[3]).RawValue
          );
      }
    }

    public Contents Contents
    {
      get
      {return new Contents(BaseObject, this);}
    }

    public void Render(
      drawing::Graphics context,
      drawing::SizeF size
      )
    {
      ContentScanner scanner = new ContentScanner(Contents);
      scanner.Render(context, size);
    }

    public Resources Resources
    {
      get
      {return Resources.Wrap(BaseDataObject.Header[PdfName.Resources]);}
      set
      {BaseDataObject.Header[PdfName.Resources] = value.BaseObject;}
    }

    public RotationEnum Rotation
    {
      get
      {return RotationEnum.Downward;}
    }

    #region IContentEntity
    public ContentObject ToInlineObject(
      PrimitiveComposer composer
      )
    {throw new NotImplementedException();}

    public XObject ToXObject(
      Document context
      )
    {return (XObject)Clone(context);}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion
  }
}