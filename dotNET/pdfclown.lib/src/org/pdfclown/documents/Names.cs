/*
  Copyright 2007-2011 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.documents.fileSpecs;
using org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;

namespace org.pdfclown.documents
{
  /**
    <summary>Name dictionary [PDF:1.6:3.6.3].</summary>
  */
  [PDF(VersionEnum.PDF12)]
  public sealed class Names
    : PdfObjectWrapper<PdfDictionary>
  {
    #region dynamic
    #region constructors
    public Names(
      Document context
      ) : base(context, new PdfDictionary())
    {}

    internal Names(
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

    /**
      <summary>Gets/Sets the named destinations.</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public NamedDestinations Destinations
    {
      get
      {
        PdfDirectObject destinationsObject = BaseDataObject[PdfName.Dests];
        return destinationsObject != null ? new NamedDestinations(destinationsObject) : null;
      }
      set
      {BaseDataObject[PdfName.Dests] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the named embedded files.</summary>
    */
    [PDF(VersionEnum.PDF14)]
    public NamedEmbeddedFiles EmbeddedFiles
    {
      get
      {
        PdfDirectObject embeddedFilesObject = BaseDataObject[PdfName.EmbeddedFiles];
        return embeddedFilesObject != null ? new NamedEmbeddedFiles(embeddedFilesObject) : null;
      }
      set
      {BaseDataObject[PdfName.EmbeddedFiles] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the named JavaScript actions.</summary>
    */
    [PDF(VersionEnum.PDF13)]
    public NamedJavaScripts JavaScripts
    {
      get
      {
        PdfDirectObject javaScriptsObject = BaseDataObject[PdfName.JavaScript];
        return javaScriptsObject != null ? new NamedJavaScripts(javaScriptsObject) : null;
      }
      set
      {BaseDataObject[PdfName.JavaScript] = value.BaseObject;}
    }

    public T Resolve<T>(
      PdfString name
      ) where T : PdfObjectWrapper
    {
      if(typeof(Destination).IsAssignableFrom(typeof(T)))
        return Destinations[name] as T;
      else if(typeof(FileSpec).IsAssignableFrom(typeof(T)))
        return EmbeddedFiles[name] as T;
      else if(typeof(JavaScript).IsAssignableFrom(typeof(T)))
        return JavaScripts[name] as T;
      else
        throw new NotSupportedException("Named type '" + typeof(T).Name + "' is not supported.");
    }
    #endregion
    #endregion
    #endregion
  }
}