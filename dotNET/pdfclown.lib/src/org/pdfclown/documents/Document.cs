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

using org.pdfclown;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.interaction.forms;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.documents.interchange.metadata;
using org.pdfclown.documents.interaction.viewer;
using org.pdfclown.files;
using org.pdfclown.objects;
using org.pdfclown.tokens;

using System;
using System.Collections.Generic;
using drawing = System.Drawing;
using System.Drawing.Printing;

namespace org.pdfclown.documents
{
  /**
    <summary>PDF document [PDF:1.6::3.6.1].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class Document
    : PdfObjectWrapper<PdfDictionary>
  {
    #region types
    /**
      <summary>Document configuration.</summary>
    */
    public sealed class Config
    {
      /**
        <summary>Version compatibility mode.</summary>
      */
      public enum CompatibilityModeEnum
      {
        /**
          <summary>Document's conformance version is ignored;
          any feature is accepted without checking its compatibility.</summary>
        */
        Passthrough,
        /**
          <summary>Document's conformance version is automatically updated
          to support used features.</summary>
        */
        Loose,
        /**
          <summary>Document's conformance version is mandatory;
          any unsupported feature is forbidden and causes an exception
          to be thrown in case of attempted use.</summary>
        */
        Strict
      }

      /**
        <summary>Cross-reference mode [PDF:1.6:3.4].</summary>
      */
      public enum XRefModeEnum
      {
        /**
          <summary>Cross-reference table [PDF:1.6:3.4.3].</summary>
        */
        [PDF(VersionEnum.PDF10)]
        Plain,
        /**
          <summary>Cross-reference stream [PDF:1.6:3.4.7].</summary>
        */
        [PDF(VersionEnum.PDF15)]
        Compressed
      }

      private CompatibilityModeEnum compatibilityMode = CompatibilityModeEnum.Loose;
      private XRefModeEnum xrefMode = XRefModeEnum.Plain;

      private Document document;

      internal Config(
        Document document
        )
      {this.document = document;}

      /**
        <summary>Gets the document's version compatibility mode.</summary>
      */
      public CompatibilityModeEnum CompatibilityMode
      {
        get
        {return compatibilityMode;}
        set
        {compatibilityMode = value;}
      }

      /**
        <summary>Gets the document associated with this configuration.</summary>
      */
      public Document Document
      {
        get
        {return document;}
      }

      /**
        <summary>Gets the document's cross-reference mode.</summary>
      */
      public XRefModeEnum XrefMode
      {
        get
        {return xrefMode;}
        set
        {document.CheckCompatibility(xrefMode = value);}
      }
    }

    public enum PageLayoutEnum
    {
      SinglePage,
      OneColumn,
      TwoColumns
    };
    
    public enum PageModeEnum
    {
      /**
        <summary>Neither document outline nor thumbnail images visible.</summary>
      */
      Simple,
      /**
        <summary>Document outline visible.</summary>
      */
      Bookmarks,
      /**
        <summary>Thumbnail images visible.</summary>
      */
      Thumbnails,
      /**
        <summary>Full-screen mode, with no menu bar, window controls, or any other window visible.</summary>
      */
      FullScreen,
      /**
        <summary>Optional content group panel visible.</summary>
      */
      [PDF(VersionEnum.PDF15)]
      OCG,
      /**
        <summary>Attachments panel visible.</summary>
      */
      [PDF(VersionEnum.PDF16)]
      Attachments
    };
    #endregion

    #region static
    #region interface
    #region public
    public static T Resolve<T>(
      PdfDirectObject baseObject
      ) where T : PdfObjectWrapper
    {
      if(typeof(Destination).IsAssignableFrom(typeof(T)))
        return Destination.Wrap(baseObject, null) as T;
      else
        throw new NotSupportedException("Type '" + typeof(T).Name + "' wrapping is not supported.");
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region fields
    internal Dictionary<PdfReference,object> Cache = new Dictionary<PdfReference,object>();

    private Config configuration;
    #endregion

    #region constructors
    internal Document(
      File context
      ) : base(
        context,
        new PdfDictionary(
          new PdfName[1]{PdfName.Type},
          new PdfDirectObject[1]{PdfName.Catalog}
          ) // Document catalog [PDF:1.6:3.6.1].
        )
    {
      configuration = new Config(this);

      /*
        NOTE: Here it is just a minimal initialization;
        any further customization is upon client's responsibility.
      */
      // Link the document to the file!
      context.Trailer[PdfName.Root] = BaseObject; // Attaches the catalog reference to the file trailer.

      // Initialize the pages collection (page-tree root node)!
      this.Pages = new Pages(this); // NOTE: The page-tree root node is required [PDF:1.6:3.6.1].

      // Default page size.
      PageSize = PageFormat.GetSize();

      // Default resources collection.
      Resources = new Resources(this);
    }

    internal Document(
      PdfDirectObject baseObject // Catalog.
      ) : base(baseObject)
    {configuration = new Config(this);}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets the document's behavior in response to trigger events.</summary>
    */
    [PDF(VersionEnum.PDF14)]
    public DocumentActions Actions
    {
      get
      {
        PdfDirectObject actionsObject = BaseDataObject[PdfName.AA];
        return actionsObject != null ? new DocumentActions(actionsObject) : null;
      }
      set
      {BaseDataObject[PdfName.AA] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the bookmark collection [PDF:1.6:8.2.2].</summary>
    */
    public Bookmarks Bookmarks
    {
      get
      {
        PdfDirectObject bookmarksObject = BaseDataObject[PdfName.Outlines];
        return bookmarksObject != null ? new Bookmarks(bookmarksObject) : null;
      }
      set
      {BaseDataObject[PdfName.Outlines] = value.BaseObject;}
    }

    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets the configuration of this document.</summary>
    */
    public Config Configuration
    {
      get
      {return configuration;}
      set
      {configuration = value;}
    }

    /**
      <summary>Drops the object from this document context.</summary>
    */
    public void Exclude(
      PdfObjectWrapper obj
      )
    {
      if(obj.File != File)
        return;

      obj.Delete();
    }

    /**
      <summary>Drops the collection's objects from this document context.</summary>
    */
    public void Exclude<T>(
      ICollection<T> objs
      ) where T : PdfObjectWrapper
    {
      foreach(T obj in objs)
      {Exclude(obj);}
    }

    /**
      <summary>Gets/Sets the interactive form (AcroForm) [PDF:1.6:8.6.1].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public Form Form
    {
      get
      {
        PdfDirectObject formObject = BaseDataObject[PdfName.AcroForm];
        return formObject != null ? new Form(formObject) : null;
      }
      set
      {BaseDataObject[PdfName.AcroForm] = value.BaseObject;}
    }

    /**
      <summary>Gets the document size, that is the maximum page dimensions across
      the whole document.</summary>
    */
    public drawing::SizeF GetSize(
      )
    {
      float height = 0, width = 0;
      foreach(Page page in Pages)
      {
        drawing::SizeF pageSize = page.Size;
        height = Math.Max(height,pageSize.Height);
        width = Math.Max(width,pageSize.Width);
      }
      return new drawing::SizeF(width,height);
    }

    /**
      <summary>Clones the object within this document context.</summary>
    */
    public PdfObjectWrapper Include(
      PdfObjectWrapper obj
      )
    {
      if(obj.File == File)
        return obj;

      return (PdfObjectWrapper)obj.Clone(this);
    }

    /**
      <summary>Clones the collection objects within this document context.</summary>
    */
    public ICollection<T> Include<T>(
      ICollection<T> objs
      ) where T : PdfObjectWrapper
    {
      List<T> includedObjects = new List<T>(objs.Count);
      foreach(T obj in objs)
      {includedObjects.Add((T)Include(obj));}

      return (ICollection<T>)includedObjects;
    }

    /**
      <summary>Gets/Sets the document information dictionary [PDF:1.6:10.2.1].</summary>
    */
    public Information Information
    {
      get
      {
        PdfDirectObject informationObject = File.Trailer[PdfName.Info];
        return informationObject != null ? new Information(informationObject) : null;
      }
      set
      {File.Trailer[PdfName.Info] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the name dictionary [PDF:1.6:3.6.3].</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public Names Names
    {
      get
      {
        PdfDirectObject namesObject = BaseDataObject[PdfName.Names];
        return namesObject != null ? new Names(namesObject) : null;
      }
      set
      {BaseDataObject[PdfName.Names] = value.BaseObject;}
    }

    /**
      <summary>Gets the page layout to be used when the document is opened.</summary>
    */
    public PageLayoutEnum PageLayout
    {
      get
      {
        PdfName value = (PdfName)BaseDataObject[PdfName.PageLayout];
        if(value.Equals(PdfName.OneColumn))
          return PageLayoutEnum.OneColumn;
        else if(value.Equals(PdfName.TwoColumnLeft))
          return PageLayoutEnum.TwoColumns;
        else
          return PageLayoutEnum.SinglePage;
      }
      set
      {
        switch(value)
        {
          case PageLayoutEnum.SinglePage:
            BaseDataObject[PdfName.PageLayout] = PdfName.SinglePage;
            break;
          case PageLayoutEnum.OneColumn:
            BaseDataObject[PdfName.PageLayout] = PdfName.OneColumn;
            break;
          case PageLayoutEnum.TwoColumns:
            BaseDataObject[PdfName.PageLayout] = PdfName.TwoColumnLeft;
            break;
        }
      }
    }

    /**
      <summary>Gets the page mode, that is how the document should be displayed when is opened.</summary>
    */
    public PageModeEnum PageMode
    {
      get
      {
        PdfName value = (PdfName)BaseDataObject[PdfName.PageMode];
        if(value == null
          || value.Equals(PdfName.UseNone))
          return PageModeEnum.Simple;
        else if(value.Equals(PdfName.UseOutlines))
          return PageModeEnum.Bookmarks;
        else if(value.Equals(PdfName.UseThumbs))
          return PageModeEnum.Thumbnails;
        else if(value.Equals(PdfName.FullScreen))
          return PageModeEnum.FullScreen;
        else if(value.Equals(PdfName.UseOC))
          return PageModeEnum.OCG;
        else if(value.Equals(PdfName.UseAttachments))
          return PageModeEnum.Attachments;
        else
          throw new NotSupportedException("Page mode unknown: " + value);
      }
      set
      {
        PdfName valueObject;
        switch(value)
        {
          case PageModeEnum.Simple:
            valueObject = PdfName.UseNone;
            break;
          case PageModeEnum.Bookmarks:
            valueObject = PdfName.UseOutlines;
            break;
          case PageModeEnum.Thumbnails:
            valueObject = PdfName.UseThumbs;
            break;
          case PageModeEnum.FullScreen:
            valueObject = PdfName.FullScreen;
            break;
          case PageModeEnum.OCG:
            valueObject = PdfName.UseOC;
            break;
          case PageModeEnum.Attachments:
            valueObject = PdfName.UseAttachments;
            break;
          default:
            throw new NotSupportedException("Page mode unknown: " + value);
        }
        BaseDataObject[PdfName.PageMode] = valueObject;
      }
    }

    /**
      <summary>Gets/Sets the page collection [PDF:1.6:3.6.2].</summary>
    */
    public Pages Pages
    {
      get
      {return new Pages(BaseDataObject[PdfName.Pages]);}
      set
      {BaseDataObject[PdfName.Pages] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the default page size [PDF:1.6:3.6.2].</summary>
    */
    public drawing::Size? PageSize
    {
      get
      {
        PdfArray mediaBox = MediaBox;
        return mediaBox != null
          ? new drawing::Size(
            (int)((IPdfNumber)mediaBox[2]).RawValue,
            (int)((IPdfNumber)mediaBox[3]).RawValue
            )
          : (drawing::Size?)null;
      }
      set
      {
        PdfArray mediaBox = MediaBox;
        if(mediaBox == null)
        {
          // Create default media box!
          mediaBox = new Rectangle(0,0,0,0).BaseDataObject;
          // Assign the media box to the document!
          ((PdfDictionary)BaseDataObject.Resolve(PdfName.Pages))[PdfName.MediaBox] = mediaBox;
        }
        mediaBox[2] = new PdfReal(value.Value.Width);
        mediaBox[3] = new PdfReal(value.Value.Height);
      }
    }

    /**
      <summary>Forces a named base object to be expressed as its corresponding
      high-level representation.</summary>
    */
    public T ResolveName<T>(
      PdfDirectObject namedBaseObject
      ) where T : PdfObjectWrapper
    {
      if(namedBaseObject is PdfString) // Named object.
        return Names.Resolve<T>((PdfString)namedBaseObject);
      else // Explicit object.
        return Resolve<T>(namedBaseObject);
    }

    /**
      <summary>Gets/Sets the default resource collection [PDF:1.6:3.6.2].</summary>
      <remarks>The default resource collection is used as last resort by every page
      that doesn't reference one explicitly (and doesn't reference an intermediate one
      implicitly).</remarks>
    */
    public Resources Resources
    {
      get
      {
        PdfReference pagesReference = (PdfReference)BaseDataObject[PdfName.Pages];
        return Resources.Wrap(((PdfDictionary)File.Resolve(pagesReference))[PdfName.Resources]);
      }
      set
      {
        PdfReference pages = (PdfReference)BaseDataObject[PdfName.Pages];
        ((PdfDictionary)File.Resolve(pages))[PdfName.Resources] = value.BaseObject;
      }
    }

    /**
      <summary>Gets/Sets the version of the PDF specification this document conforms to [PDF:1.6:3.6.1].</summary>
    */
    [PDF(VersionEnum.PDF14)]
    public Version Version
    {
      get
      {
        /*
          NOTE: If the header specifies a later version, or if this entry is absent,
          the document conforms to the version specified in the header.
        */
        Version fileVersion = File.Version;

        PdfName versionObject = (PdfName)BaseDataObject[PdfName.Version];
        if(versionObject == null)
          return fileVersion;

        Version version = Version.Get(versionObject.RawValue);
        if(File.Reader == null)
          return version;

        return (version.CompareTo(fileVersion) > 0 ? version : fileVersion);
      }
      set
      {BaseDataObject[PdfName.Version] = new PdfName(value.ToString());}
    }

    /**
      <summary>Gets the way the document is to be presented [PDF:1.6:8.1].</summary>
    */
    public ViewerPreferences ViewerPreferences
    {
      get
      {
        PdfDirectObject viewerPreferencesObject = BaseDataObject[PdfName.ViewerPreferences];
        return viewerPreferencesObject != null ? new ViewerPreferences(viewerPreferencesObject) : null;
      }
      set
      {BaseDataObject[PdfName.ViewerPreferences] = value.BaseObject;}
    }
    #endregion

    #region private
    /**
      <summary>Gets the default media box.</summary>
    */
    private PdfArray MediaBox
    {
      get
      {
        /*
          NOTE: Document media box MUST be associated with the page-tree root node
          in order to be inheritable by all the pages.
        */
        return (PdfArray)((PdfDictionary)BaseDataObject.Resolve(PdfName.Pages)).Resolve(PdfName.MediaBox);
      }
    }
    #endregion
    #endregion
    #endregion
  }
}