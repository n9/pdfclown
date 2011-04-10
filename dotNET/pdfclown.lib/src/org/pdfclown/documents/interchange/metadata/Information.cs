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
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Reflection;

namespace org.pdfclown.documents.interchange.metadata
{
  /**
    <summary>Document information [PDF:1.6:10.2.1].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class Information
    : PdfObjectWrapper<PdfDictionary>
  {
    #region dynamic
    #region constructors
    public Information(
      Document context
      ) : base(
        context.File,
        new PdfDictionary()
        )
    {
      string assemblyTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title;
      string assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
      Producer = assemblyTitle + " " + assemblyVersion;
    }

    internal Information(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    public string Author
    {
      get
      {return (string)Get(PdfName.Author);}
      set
      {BaseDataObject[PdfName.Author] = PdfTextString.Get(value);}
    }

    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    public DateTime? CreationDate
    {
      get
      {return (DateTime?)Get(PdfName.CreationDate);}
      set
      {BaseDataObject[PdfName.CreationDate] = PdfDate.Get(value);}
    }

    public string Creator
    {
      get
      {return (string)Get(PdfName.Creator);}
      set
      {BaseDataObject[PdfName.Creator] = PdfTextString.Get(value);}
    }

    [PDF(VersionEnum.PDF11)]
    public string Keywords
    {
      get
      {return (string)Get(PdfName.Keywords);}
      set
      {BaseDataObject[PdfName.Keywords] = PdfTextString.Get(value);}
    }

    [PDF(VersionEnum.PDF11)]
    public DateTime? ModificationDate
    {
      get
      {return (DateTime?)Get(PdfName.ModDate);}
      set
      {BaseDataObject[PdfName.ModDate] = PdfDate.Get(value);}
    }

    public string Producer
    {
      get
      {return (string)Get(PdfName.Producer);}
      set
      {BaseDataObject[PdfName.Producer] = PdfTextString.Get(value);}
    }

    [PDF(VersionEnum.PDF11)]
    public string Subject
    {
      get
      {return (string)Get(PdfName.Subject);}
      set
      {BaseDataObject[PdfName.Subject] = PdfTextString.Get(value);}
    }

    [PDF(VersionEnum.PDF11)]
    public string Title
    {
      get
      {return (string)Get(PdfName.Title);}
      set
      {BaseDataObject[PdfName.Title] = PdfTextString.Get(value);}
    }
    #endregion

    #region private
    private object Get(
      PdfName key
      )
    {return PdfSimpleObject<object>.GetValue(BaseDataObject[key]);}
    #endregion
    #endregion
    #endregion
  }
}