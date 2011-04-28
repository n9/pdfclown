/*
  Copyright 2006-2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.contents.xObjects;

import java.awt.Graphics2D;
import java.awt.geom.AffineTransform;
import java.awt.geom.Dimension2D;
import java.awt.geom.Rectangle2D;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.ContentScanner;
import org.pdfclown.documents.contents.Contents;
import org.pdfclown.documents.contents.IContentContext;
import org.pdfclown.documents.contents.Resources;
import org.pdfclown.documents.contents.RotationEnum;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.objects.ContentObject;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfNumber;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.objects.Rectangle;
import org.pdfclown.util.NotImplementedException;
import org.pdfclown.util.math.geom.Dimension;

/**
  Form external object [PDF:1.6:4.9].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 04/28/11
*/
@PDF(VersionEnum.PDF10)
public final class FormXObject
  extends XObject
  implements IContentContext
{
  // <class>
  // <dynamic>
  // <constructors>
  /**
    Creates a new form within the given document context, using default resources.
  */
  public FormXObject(
    Document context
    )
  {this(context, null);}

  /**
    Creates a new form within the given document context, using custom resources.
  */
  public FormXObject(
    Document context,
    Resources resources
    )
  {
    super(context);

    PdfDictionary header = getBaseDataObject().getHeader();
    header.put(PdfName.Subtype,PdfName.Form);
    header.put(PdfName.BBox,new Rectangle(0,0,0,0).getBaseDataObject());

    // No resources collection?
    /* NOTE: Resources collection is mandatory. */
    if(resources == null)
    {resources = new Resources(context);}
    header.put(PdfName.Resources,resources.getBaseObject());
  }

  /**
    For internal use only.
  */
  public FormXObject(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public FormXObject clone(
    Document context
    )
  {throw new NotImplementedException();}

  @Override
  public AffineTransform getMatrix(
    )
  {
    /*
      NOTE: Form-space-to-user-space matrix is identity [1 0 0 1 0 0] by default,
      but may be adjusted by setting the Matrix entry in the form dictionary [PDF:1.6:4.9].
    */
    PdfArray matrix = (PdfArray)getBaseDataObject().getHeader().resolve(PdfName.Matrix);
    if(matrix == null)
      return new AffineTransform();
    else
      return new AffineTransform(
        ((PdfNumber<?>)matrix.get(0)).getNumberValue(),
        ((PdfNumber<?>)matrix.get(1)).getNumberValue(),
        ((PdfNumber<?>)matrix.get(2)).getNumberValue(),
        ((PdfNumber<?>)matrix.get(3)).getNumberValue(),
        ((PdfNumber<?>)matrix.get(4)).getNumberValue(),
        ((PdfNumber<?>)matrix.get(5)).getNumberValue()
        );
  }

  /**
    Gets the form size.
  */
  @Override
  public Dimension2D getSize(
    )
  {
    PdfArray box = (PdfArray)getBaseDataObject().getHeader().resolve(PdfName.BBox);
    return new Dimension(
      ((PdfNumber<?>)box.get(2)).getNumberValue(),
      ((PdfNumber<?>)box.get(3)).getNumberValue()
      );
  }

  /**
    Sets the resources associated to the form.
  */
  public void setResources(
    Resources value
    )
  {
    getBaseDataObject().getHeader().put(
      PdfName.Resources,
      value.getBaseObject()
      );
  }

  /**
    Sets the form size.
  */
  @Override
  public void setSize(
    Dimension2D value
    )
  {
    PdfArray boxObject = (PdfArray)getBaseDataObject().getHeader().resolve(PdfName.BBox);
    boxObject.set(2, new PdfReal(value.getWidth()));
    boxObject.set(3, new PdfReal(value.getHeight()));
  }

  // <IContentContext>
  @Override
  public Rectangle2D getBox(
    )
  {
    PdfArray box = (PdfArray)getBaseDataObject().getHeader().resolve(PdfName.BBox);
    return new Rectangle2D.Double(
      ((PdfNumber<?>)box.get(0)).getNumberValue(),
      ((PdfNumber<?>)box.get(1)).getNumberValue(),
      ((PdfNumber<?>)box.get(2)).getNumberValue(),
      ((PdfNumber<?>)box.get(3)).getNumberValue()
      );
  }

  @Override
  public Contents getContents(
    )
  {return new Contents(getBaseObject(), this);}

  @Override
  public Resources getResources(
    )
  {return Resources.wrap(getBaseDataObject().getHeader().get(PdfName.Resources));}

  @Override
  public RotationEnum getRotation(
    )
  {return RotationEnum.Downward;}

  @Override
  public void render(
    Graphics2D context,
    Dimension2D size
    )
  {
    ContentScanner scanner = new ContentScanner(getContents());
    scanner.render(context,size);
  }

  // <IContentEntity>
  @Override
  public ContentObject toInlineObject(
    PrimitiveComposer composer
    )
  {throw new NotImplementedException();}

  @Override
  public XObject toXObject(
    Document context
    )
  {return clone(context);}
  // </IContentEntity>
  // </IContentContext>
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}