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
      ) : base(
        baseObject,
        null // NO container (info MUST be an indirect object [PDF:1.6:3.4.4]).
        )
    {}
    #endregion

    #region interface
    #region public
    public string Author
    {
      get
      {return (string)GetEntry<byte[],PdfTextString>(PdfName.Author);}
      set
      {SetEntry<byte[],PdfTextString>(PdfName.Author,value);}
    }

    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    public DateTime? CreationDate
    {
      get
      {return (DateTime?)GetEntry<byte[],PdfDate>(PdfName.CreationDate);}
      set
      {SetEntry<byte[],PdfDate>(PdfName.CreationDate,value);}
    }

    public string Creator
    {
      get
      {return (string)GetEntry<byte[],PdfTextString>(PdfName.Creator);}
      set
      {SetEntry<byte[],PdfTextString>(PdfName.Creator,value);}
    }

    [PDF(VersionEnum.PDF11)]
    public string Keywords
    {
      get
      {return (string)GetEntry<byte[],PdfTextString>(PdfName.Keywords);}
      set
      {SetEntry<byte[],PdfTextString>(PdfName.Keywords,value);}
    }

    [PDF(VersionEnum.PDF11)]
    public DateTime? ModificationDate
    {
      get
      {return (DateTime?)GetEntry<byte[],PdfDate>(PdfName.ModDate);}
      set
      {SetEntry<byte[],PdfDate>(PdfName.ModDate,value);}
    }

    public string Producer
    {
      get
      {return (string)GetEntry<byte[],PdfTextString>(PdfName.Producer);}
      set
      {SetEntry<byte[],PdfTextString>(PdfName.Producer,value);}
    }

    [PDF(VersionEnum.PDF11)]
    public string Subject
    {
      get
      {return (string)GetEntry<byte[],PdfTextString>(PdfName.Subject);}
      set
      {SetEntry<byte[],PdfTextString>(PdfName.Subject,value);}
    }

    [PDF(VersionEnum.PDF11)]
    public string Title
    {
      get
      {return (string)GetEntry<byte[],PdfTextString>(PdfName.Title);}
      set
      {SetEntry<byte[],PdfTextString>(PdfName.Title,value);}
    }
    #endregion

    #region private
    private object GetEntry<T,TPdf>(
      PdfName key
      ) where TPdf : PdfAtomicObject<T>
    {
      TPdf entry = (TPdf)BaseDataObject.Resolve(key);
      return entry == null ? null : entry.Value;
    }

    private void SetEntry<T,TPdf>(
      PdfName key,
      object value
      ) where TPdf : PdfAtomicObject<T>, new()
    {
      if(value == null)
      {BaseDataObject.Remove(key);}
      else
      {
        if(!BaseDataObject.ContainsKey(key))
        {BaseDataObject[key] = new TPdf();}
        ((TPdf)BaseDataObject.Resolve(key)).Value = value;
      }
    }
    #endregion
    #endregion
    #endregion
  }
}