/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.objects;

using System;
using System.IO;

namespace org.pdfclown.documents.fileSpecs
{
  /**
    <summary>Reference to the contents of another file (file specification) [PDF:1.6:3.10.2].</summary>
  */
  [PDF(VersionEnum.PDF11)]
  public sealed class FileSpec
    : PdfNamedObjectWrapper<PdfDictionary>
  {
    #region dynamic
    #region constructors
    public FileSpec(
      Document context
      ) : base(
        context.File,
        new PdfDictionary(
          new PdfName[]
          {PdfName.Type},
          new PdfDirectObject[]
          {PdfName.Filespec}
          )
        )
    {}

    public FileSpec(
      EmbeddedFile embeddedFile,
      string filename
      ) : this(embeddedFile.Document)
    {
      Filename = filename;
      EmbeddedFile = embeddedFile;
    }

    internal FileSpec(
      PdfDirectObject baseObject,
      PdfString name
      ) : base(
        baseObject,
        name
        )
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets the related files.</summary>
    */
    public RelatedFiles Dependencies
    {
      get
      {return GetDependencies(PdfName.F);}
      set
      {SetDependencies(PdfName.F,value);}
    }

    /**
      <summary>Gets/Sets the description of the file.</summary>
    */
    public string Description
    {
      get
      {
        PdfTextString descriptionObject = (PdfTextString)BaseDataObject[PdfName.Desc];
        return descriptionObject != null ? (string)descriptionObject.Value : null;
      }
      set
      {BaseDataObject[PdfName.Desc] = new PdfTextString(value);}
    }

    /**
      <summary>Gets/Sets the embedded file.</summary>
    */
    public EmbeddedFile EmbeddedFile
    {
      get
      {return GetEmbeddedFile(PdfName.F);}
      set
      {SetEmbeddedFile(PdfName.F,value);}
    }

    /**
      <summary>Gets/Sets the file name.</summary>
    */
    public string Filename
    {
      get
      {return GetFilename(PdfName.F);}
      set
      {SetFilename(PdfName.F,value);}
    }

    /**
      <summary>Gets/Sets the Mac OS-specific related files.</summary>
    */
    public RelatedFiles MacDependencies
    {
      get
      {return GetDependencies(PdfName.Mac);}
      set
      {SetDependencies(PdfName.Mac,value);}
    }

    /**
      <summary>Gets/Sets the Mac OS-specific embedded file.</summary>
    */
    public EmbeddedFile MacEmbeddedFile
    {
      get
      {return GetEmbeddedFile(PdfName.Mac);}
      set
      {SetEmbeddedFile(PdfName.Mac,value);}
    }

    /**
      <summary>Gets/Sets the Mac OS-specific file name.</summary>
    */
    public string MacFilename
    {
      get
      {return GetFilename(PdfName.Mac);}
      set
      {SetFilename(PdfName.Mac,value);}
    }

    /**
      <summary>Gets/Sets the Unix-specific related files.</summary>
    */
    public RelatedFiles UnixDependencies
    {
      get
      {return GetDependencies(PdfName.Unix);}
      set
      {SetDependencies(PdfName.Unix,value);}
    }

    /**
      <summary>Gets/Sets the Unix-specific embedded file.</summary>
    */
    public EmbeddedFile UnixEmbeddedFile
    {
      get
      {return GetEmbeddedFile(PdfName.Unix);}
      set
      {SetEmbeddedFile(PdfName.Unix,value);}
    }

    /**
      <summary>Gets/Sets the Unix-specific file name.</summary>
    */
    public string UnixFilename
    {
      get
      {return GetFilename(PdfName.Unix);}
      set
      {SetFilename(PdfName.Unix,value);}
    }

    /**
      <summary>Gets/Sets the Windows-specific related files.</summary>
    */
    public RelatedFiles WinDependencies
    {
      get
      {return GetDependencies(PdfName.DOS);}
      set
      {SetDependencies(PdfName.DOS,value);}
    }

    /**
      <summary>Gets/Sets the Windows-specific embedded file.</summary>
    */
    public EmbeddedFile WinEmbeddedFile
    {
      get
      {return GetEmbeddedFile(PdfName.DOS);}
      set
      {SetEmbeddedFile(PdfName.DOS,value);}
    }

    /**
      <summary>Gets/Sets the Windows-specific file name.</summary>
    */
    public string WinFilename
    {
      get
      {return GetFilename(PdfName.DOS);}
      set
      {SetFilename(PdfName.DOS,value);}
    }
    #endregion

    #region private
    /**
      <summary>Gets the related files associated to the given key.</summary>
    */
    private RelatedFiles GetDependencies(
      PdfName key
      )
    {
      PdfDictionary dependenciesObject = (PdfDictionary)BaseDataObject[PdfName.RF];
      if(dependenciesObject == null)
        return null;

      PdfReference dependencyFilesObject = (PdfReference)dependenciesObject[key];
      return dependencyFilesObject != null ? new RelatedFiles(dependencyFilesObject) : null;
    }

    /**
      <summary>Gets the embedded file associated to the given key.</summary>
    */
    private EmbeddedFile GetEmbeddedFile(
      PdfName key
      )
    {
      PdfDictionary embeddedFilesObject = (PdfDictionary)BaseDataObject[PdfName.EF];
      if(embeddedFilesObject == null)
        return null;

      PdfReference embeddedFileObject = (PdfReference)embeddedFilesObject[key];
      return embeddedFileObject != null ? new EmbeddedFile(embeddedFileObject) : null;
    }

    /**
      <summary>Gets the file name associated to the given key.</summary>
    */
    private string GetFilename(
      PdfName key
      )
    {
      PdfString nameObject = (PdfString)BaseDataObject[key];
      return nameObject != null ? (string)nameObject.Value : null;
    }

    private void SetDependencies(
      PdfName key,
      RelatedFiles value
      )
    {
      /*
        NOTE: 'RF' entry may be undefined.
      */
      PdfDictionary dependenciesObject = (PdfDictionary)BaseDataObject[PdfName.RF];
      if(dependenciesObject == null)
      {
        dependenciesObject = new PdfDictionary();
        BaseDataObject[PdfName.RF] = dependenciesObject;
      }

      dependenciesObject[key] = value.BaseObject;
    }

    private void SetEmbeddedFile(
      PdfName key,
      EmbeddedFile value
      )
    {
      /*
        NOTE: 'EF' entry may be undefined.
      */
      PdfDictionary embeddedFilesObject = (PdfDictionary)BaseDataObject[PdfName.EF];
      if(embeddedFilesObject == null)
      {
        embeddedFilesObject = new PdfDictionary();
        BaseDataObject[PdfName.EF] = embeddedFilesObject;
      }

      embeddedFilesObject[key] = value.BaseObject;
    }

    private void SetFilename(
      PdfName key,
      string value
      )
    {BaseDataObject[key] = new PdfString(value);}
    #endregion
    #endregion
    #endregion
  }
}