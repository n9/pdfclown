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

package org.pdfclown.documents.interchange.metadata;

import java.util.Date;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.objects.PdfDate;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfSimpleObject;
import org.pdfclown.objects.PdfTextString;
import org.pdfclown.util.NotImplementedException;

/**
  Document information [PDF:1.6:10.2.1].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 11/01/11
*/
@PDF(VersionEnum.PDF10)
public final class Information
  extends PdfObjectWrapper<PdfDictionary>
{
  // <class>
  // <dynamic>
  // <constructors>
  public Information(
    Document context
    )
  {
    super(context, new PdfDictionary());
    try
    {
      Package package_ = getClass().getPackage();
      setProducer(
        package_.getSpecificationTitle() + " "
          + package_.getSpecificationVersion()
        );
    }
    catch(Exception e)
    {/* NOOP */}
  }

  /**
    <span style="color:red">For internal use only.</span>
  */
  public Information(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Information clone(
    Document context
    )
  {throw new NotImplementedException();}

  public String getAuthor(
  )
  {return (String)get(PdfName.Author);}

  public Date getCreationDate(
    )
  {return (Date)get(PdfName.CreationDate);}

  public String getCreator(
    )
  {return (String)get(PdfName.Creator);}

  @PDF(VersionEnum.PDF11)
  public String getKeywords(
    )
  {return (String)get(PdfName.Keywords);}

  @PDF(VersionEnum.PDF11)
  public Date getModificationDate(
    )
  {return (Date)get(PdfName.ModDate);}

  public String getProducer(
    )
  {return (String)get(PdfName.Producer);}

  @PDF(VersionEnum.PDF11)
  public String getSubject(
    )
  {return (String)get(PdfName.Subject);}

  @PDF(VersionEnum.PDF11)
  public String getTitle(
    )
  {return (String)get(PdfName.Title);}

  public void setAuthor(
    String value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.Author, PdfTextString.get(value));
  }

  public void setCreationDate(
    Date value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.CreationDate, PdfDate.get(value));
  }

  public void setCreator(
    String value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.Creator, PdfTextString.get(value));
  }

  public void setKeywords(
    String value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.Keywords, PdfTextString.get(value));
  }

  public void setModificationDate(
    Date value
    )
  {getBaseDataObject().put(PdfName.ModDate, PdfDate.get(value));}

  public void setProducer(
    String value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.Producer, PdfTextString.get(value));
  }

  public void setSubject(
    String value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.Subject, PdfTextString.get(value));
  }

  public void setTitle(
    String value
    )
  {
    onChange();
    getBaseDataObject().put(PdfName.Title, PdfTextString.get(value));
  }
  // </public>

  // <private>
  private Object get(
    PdfName key
    )
  {return PdfSimpleObject.getValue(getBaseDataObject().get(key));}

  //TODO: Listen to baseDataObject's onChange notification?
  private void onChange(
    )
  {
    if(!getBaseDataObject().isUpdated())
    {setModificationDate(new Date());}
  }
  // </private>
  // </interface>
  // </class>
}