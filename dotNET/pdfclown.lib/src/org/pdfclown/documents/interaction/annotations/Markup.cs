/*
  Copyright 2015 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.documents.interaction.annotations
{
  /**
    <summary>Markup annotation [PDF:1.6:8.4.5].</summary>
    <remarks>It represents text-based annotations used primarily to mark up documents.</remarks>
  */
  [PDF(VersionEnum.PDF11)]
  public abstract class Markup
    : Annotation
  {
    #region dynamic
    #region constructors
    protected Markup(
      Page page,
      PdfName subtype,
      RectangleF box,
      string text
      ) : base(page, subtype, box, text)
    {CreationDate = DateTime.Now;}
  
    protected Markup(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets the annotation editor. It is displayed as a text label in the title bar of
      the annotation's pop-up window when open and active. By convention, it identifies the user who
      added the annotation.</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public virtual string Author
    {
      get
      {return (string)PdfSimpleObject<object>.GetValue(BaseDataObject[PdfName.T]);}
      set
      {
        BaseDataObject[PdfName.T] = PdfTextString.Get(value);
        ModificationDate = DateTime.Now;
      }
    }

    /**
      <summary>Gets/Sets the date and time when the annotation was created.</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public virtual DateTime? CreationDate
    {
      get
      {
        PdfDirectObject creationDateObject = BaseDataObject[PdfName.CreationDate];
        return creationDateObject is PdfDate ? (DateTime?)((PdfDate)creationDateObject).Value : null;
      }
      set
      {BaseDataObject[PdfName.CreationDate] = PdfDate.Get(value);}
    }

    /**
      <summary>Gets/Sets the pop-up annotation associated with this one.</summary>
      <exception cref="InvalidOperationException">If pop-up annotations can't be associated with
      this markup.</exception>
    */
    [PDF(VersionEnum.PDF13)]
    public virtual Popup Popup
    {
      get
      {return (Popup)Annotation.Wrap(BaseDataObject[PdfName.Popup]);}
      set
      {
        value.Markup = this;
        BaseDataObject[PdfName.Popup] = value.BaseObject;
      }
    }

    /**
      <summary>Gets/Sets the annotation subject.</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public virtual string Subject
    {
      get
      {return (string)PdfSimpleObject<object>.GetValue(BaseDataObject[PdfName.Subj]);}
      set
      {
        BaseDataObject[PdfName.Subj] = PdfTextString.Get(value);
        ModificationDate = DateTime.Now;
      }
    }
    #endregion

    #region protected
    protected PdfName TypeBase
    {
      get
      {return (PdfName)BaseDataObject[PdfName.IT];}
      set
      {BaseDataObject[PdfName.IT] = value;}
    }
    #endregion
    #endregion
    #endregion
  }
}