/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents;

import java.awt.Dimension;
import java.awt.geom.Dimension2D;
import java.awt.geom.Rectangle2D;
import java.awt.print.Pageable;
import java.awt.print.Paper;
import java.awt.print.Printable;
import java.util.ArrayList;
import java.util.Collection;

import org.pdfclown.PDF;
import org.pdfclown.Version;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.contents.Resources;
import org.pdfclown.documents.interaction.forms.Form;
import org.pdfclown.documents.interaction.navigation.document.Bookmarks;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.documents.interaction.viewer.ViewerPreferences;
import org.pdfclown.documents.interchange.metadata.Information;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfNumber;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfString;
import org.pdfclown.objects.Rectangle;
import org.pdfclown.util.NotImplementedException;

/**
  PDF document [PDF:1.6:3.6.1].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
*/
@PDF(VersionEnum.PDF10)
public final class Document
  extends PdfObjectWrapper<PdfDictionary>
  implements Pageable
{
  // <class>
  // <classes>
  /**
    Document configuration.

    @author Stefano Chizzolini (http://www.stefanochizzolini.it)
    @since 0.1.0
  */
  public static final class Configuration
  {
    /**
      Version compatibility mode.
    */
    public enum CompatibilityModeEnum
    {
      /**
        Document's conformance version is ignored;
        any feature is accepted without checking its compatibility.
      */
      Passthrough,
      /**
        Document's conformance version is automatically updated
        to support used features.
      */
      Loose,
      /**
        Document's conformance version is mandatory;
        any unsupported feature is forbidden and causes an exception
        to be thrown in case of attempted use.
      */
      Strict
    }

    /**
      Cross-reference mode [PDF:1.6:3.4].
    */
    public enum XRefModeEnum
    {
      /**
        Cross-reference table [PDF:1.6:3.4.3].
      */
      @PDF(VersionEnum.PDF10)
      Plain,
      /**
        Cross-reference stream [PDF:1.6:3.4.7].
      */
      @PDF(VersionEnum.PDF15)
      Compressed
    }

    private CompatibilityModeEnum compatibilityMode = CompatibilityModeEnum.Loose;
    private XRefModeEnum xrefMode = XRefModeEnum.Plain;

    private Document document;

    Configuration(
      Document document
      )
    {this.document = document;}

    /**
      Gets the document's version compatibility mode.
    */
    public CompatibilityModeEnum getCompatibilityMode(
      )
    {return compatibilityMode;}

    /**
      Gets the document associated with this configuration.
    */
    public Document getDocument(
      )
    {return document;}

    /**
      Gets the document's cross-reference mode.
    */
    public XRefModeEnum getXrefMode(
      )
    {return xrefMode;}

    /**
      @see #getCompatibilityMode()
    */
    public void setCompatibilityMode(
      CompatibilityModeEnum value
      )
    {compatibilityMode = value;}

    /**
      @see #getXrefMode()
    */
    public void setXrefMode(
      XRefModeEnum value
      )
    {document.checkCompatibility(xrefMode = value);}
  }

  public enum PageLayoutEnum
  {
    SinglePage,
    OneColumn,
    TwoColumns
  }

  public enum PageModeEnum
  {
    /**
      Neither document outline nor thumbnail images visible.
    */
    Simple,
    /**
      Document outline visible.
    */
    Bookmarks,
    /**
      Thumbnail images visible.
    */
    Thumbnails,
    /**
      Full-screen mode, with no menu bar, window controls, or any other window visible.
    */
    FullScreen,
    /**
      Optional content group panel visible.
    */
    @PDF(VersionEnum.PDF15)
    OCG,
    /**
      Attachments panel visible.
    */
    @PDF(VersionEnum.PDF16)
    Attachments
  }
  // </classes>

  // <static>
  // <interface>
  // <public>
  @SuppressWarnings("unchecked")
  public static <T extends PdfObjectWrapper<?>> T resolve(
    Class<T> type,
    PdfDirectObject baseObject,
    PdfIndirectObject container
    )
  {
    if(Destination.class.isAssignableFrom(type))
      return (T)Destination.wrap(baseObject,container,null);
    else
      throw new UnsupportedOperationException("Type '" + type.getName() + "' wrapping is not supported.");
  }

  /**
    Forces a named base object to be expressed as its corresponding
    high-level representation.
  */
  public static <T extends PdfObjectWrapper<?>> T resolveName(
    Class<T> type,
    PdfDirectObject namedBaseObject,
    PdfIndirectObject container
    )
  {
    if(namedBaseObject instanceof PdfString) // Named destination.
      return container.getFile().getDocument().getNames().resolve(type,(PdfString)namedBaseObject);
    else // Explicit destination.
      return resolve(type,namedBaseObject,container);
  }
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <fields>
  /**
    For internal use only.
  */
  public java.util.Hashtable<PdfReference,Object> cache = new java.util.Hashtable<PdfReference,Object>();

  private Configuration configuration = new Configuration(this);
  // </fields>

  // <constructors>
  /**
    For internal use only.
  */
  public Document(
    File context
    )
  {
    super(
      context,
      new PdfDictionary(
        new PdfName[]{PdfName.Type},
        new PdfDirectObject[]{PdfName.Catalog}
        ) // Document catalog [PDF:1.6:3.6.1].
      );

    /*
      NOTE: Here it is just a minimal initialization;
      any further customization is upon client's responsibility.
    */
    // Link the document to the file!
    context.getTrailer().put(PdfName.Root,getBaseObject()); // Attaches the catalog reference to the file trailer.

    // Initialize the pages collection (page-tree root node)!
    setPages(new Pages(this)); // NOTE: The page-tree root node is required [PDF:1.6:3.6.1].

    // Default page size.
    setPageSize(PageFormat.getSize());

    // Default resources collection.
    setResources(new Resources(this));
  }

  /**
    For internal use only.
  */
  public Document(
    PdfDirectObject baseObject // Catalog.
    )
  {
    super(
      baseObject,
      null // NO container (catalog MUST be an indirect object [PDF:1.6:3.4.4]).
      );
  }
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Document clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Clones the object within this document context.
  */
  public Object contextualize(
    PdfObjectWrapper<?> object
    )
  {
    if(object.getFile() == getFile())
      return object;

    return object.clone(this);
  }

  /**
    Clones the collection objects within this document context.
  */
  public Collection<? extends PdfObjectWrapper<?>> contextualize(
    Collection<? extends PdfObjectWrapper<?>> objects
    )
  {
    ArrayList<PdfObjectWrapper<?>> contextualizedObjects = new ArrayList<PdfObjectWrapper<?>>(objects.size());
    for(PdfObjectWrapper<?> object : objects)
    {contextualizedObjects.add((PdfObjectWrapper<?>)contextualize(object));}

    return contextualizedObjects;
  }

  /**
    Drops the object from this document context.
  */
  public void decontextualize(
    PdfObjectWrapper<?> object
    )
  {
    if(object.getFile() != getFile())
      return;

    object.delete();
  }

  /**
    Drops the collection's objects from this document context.
  */
  public void decontextualize(
    Collection<? extends PdfObjectWrapper<?>> objects
    )
  {
    for(PdfObjectWrapper<?> object : objects)
    {decontextualize(object);}
  }

  /**
    Gets the document's behavior in response to trigger events.

    @since 0.0.7
  */
  @PDF(VersionEnum.PDF14)
  public DocumentActions getActions(
    )
  {
    PdfDirectObject actionsObject = getBaseDataObject().get(PdfName.AA);
    if(actionsObject == null)
      return null;

    return new DocumentActions(actionsObject, getContainer());
  }

  /**
    Gets the bookmark collection [PDF:1.6:8.2.2].
  */
  public Bookmarks getBookmarks(
    )
  {
    PdfDirectObject bookmarksObject = getBaseDataObject().get(PdfName.Outlines);
    if(bookmarksObject == null)
      return null;

    return new Bookmarks(bookmarksObject);
  }

  /**
    Gets the configuration of this document.
  */
  public final Configuration getConfiguration(
    )
  {return configuration;}

  /**
    Gets the interactive form (AcroForm) [PDF:1.6:8.6.1].

    @since 0.0.7
  */
  @PDF(VersionEnum.PDF12)
  public Form getForm(
    )
  {
    PdfDirectObject formObject = getBaseDataObject().get(PdfName.AcroForm);
    if(formObject == null)
      return null;

    return new Form(formObject, getContainer());
  }

  /**
    Gets the document information dictionary [PDF:1.6:10.2.1].
  */
  public Information getInformation(
    )
  {
    PdfDirectObject informationObject = getFile().getTrailer().get(PdfName.Info);
    if(informationObject == null)
      return null;

    return new Information(informationObject);
  }

  /**
    Gets the name dictionary [PDF:1.6:3.6.3].
  */
  @PDF(VersionEnum.PDF12)
  public Names getNames(
    )
  {
    PdfDirectObject namesObject = getBaseDataObject().get(PdfName.Names);
    if(namesObject == null)
      return null;

    return new Names(namesObject, getContainer());
  }

  /**
    Gets the page layout to be used when the document is opened.
  */
  public PageLayoutEnum getPageLayout(
    )
  {
    PdfName value = (PdfName)getBaseDataObject().get(PdfName.PageLayout);
    if(value.equals(PdfName.OneColumn))
      return PageLayoutEnum.OneColumn;
    else if(value.equals(PdfName.TwoColumnLeft))
      return PageLayoutEnum.TwoColumns;
    else
      return PageLayoutEnum.SinglePage;
  }

  /**
    Gets the page mode, that is how the document should be displayed when is opened.
  */
  public PageModeEnum getPageMode(
    )
  {
    PdfName value = (PdfName)getBaseDataObject().get(PdfName.PageMode);
    if(value == null
      || value.equals(PdfName.UseNone))
      return PageModeEnum.Simple;
    else if(value.equals(PdfName.UseOutlines))
      return PageModeEnum.Bookmarks;
    else if(value.equals(PdfName.UseThumbs))
      return PageModeEnum.Thumbnails;
    else if(value.equals(PdfName.FullScreen))
      return PageModeEnum.FullScreen;
    else if(value.equals(PdfName.UseOC))
      return PageModeEnum.OCG;
    else if(value.equals(PdfName.UseAttachments))
      return PageModeEnum.Attachments;
    else
      throw new UnsupportedOperationException("Page mode unknown: " + value);
  }

  /**
    Gets the page collection [PDF:1.6:3.6.2].
  */
  public Pages getPages(
    )
  {return new Pages(getBaseDataObject().get(PdfName.Pages));}

  /**
    Gets the default page size [PDF:1.6:3.6.2].
  */
  public Dimension2D getPageSize(
    )
  {
    /*
      NOTE: Due to the contract,
      we cannot force the existence of the default media box at document level.
    */
    PdfArray mediaBox = getMediaBox();
    if(mediaBox == null)
      return null;

    return new Dimension(
      ((PdfNumber<?>)mediaBox.get(2)).getValue().intValue(),
      ((PdfNumber<?>)mediaBox.get(3)).getValue().intValue()
      );
  }

  /**
    Gets the <b>default resource collection</b> [PDF:1.6:3.6.2].
    <p>The default resource collection is used as last resort by every page
    that doesn't reference one explicitly (and doesn't reference an intermediate one
    implicitly).</p>
  */
  public Resources getResources(
    )
  {
    PdfReference pagesReference = (PdfReference)getBaseDataObject().get(PdfName.Pages);
    return Resources.wrap(
      ((PdfDictionary)File.resolve(pagesReference)).get(PdfName.Resources),
      pagesReference.getIndirectObject()
      );
  }

  /**
    Gets the document size, that is the maximum page dimensions across the whole document.
  */
  public Dimension2D getSize(
    )
  {
    double height = 0, width = 0;
    for(Page page : getPages())
    {
      Dimension2D pageSize = page.getSize();
      height = Math.max(height,pageSize.getHeight());
      width = Math.max(width,pageSize.getWidth());
    }
    return new org.pdfclown.util.math.geom.Dimension(width,height);
  }

  /**
    Gets the version of the PDF specification this document conforms to [PDF:1.6:3.6.1].
  */
  @PDF(VersionEnum.PDF14)
  public Version getVersion(
    )
  {
    /*
      NOTE: If the header specifies a later version, or if this entry is absent,
      the document conforms to the version specified in the header.
    */
    Version fileVersion = getFile().getVersion();

    PdfName versionObject = (PdfName)getBaseDataObject().get(PdfName.Version);
    if(versionObject == null)
      return fileVersion;

    Version version = Version.get(versionObject.getRawValue());
    if(getFile().getReader() == null)
      return version;

    return (version.compareTo(fileVersion) > 0 ? version : fileVersion);
  }

  /**
    Gets the way the document is to be presented [PDF:1.6:8.1].
  */
  @PDF(VersionEnum.PDF12)
  public ViewerPreferences getViewerPreferences(
    )
  {
    PdfDirectObject viewerPreferencesObject = getBaseDataObject().get(PdfName.ViewerPreferences);
    if(viewerPreferencesObject == null)
      return null;

    return new ViewerPreferences(viewerPreferencesObject, getContainer());
  }

  /**
    @see #getActions()
  */
  public void setActions(
    DocumentActions value
    )
  {getBaseDataObject().put(PdfName.AA, value.getBaseObject());}

  /**
    @see #getBookmarks()
  */
  public void setBookmarks(
    Bookmarks value
    )
  {getBaseDataObject().put(PdfName.Outlines,value.getBaseObject());}

  /**
    @see #getConfiguration()
    @since 0.1.0
  */
  public void setConfiguration(
    Configuration value
    )
  {configuration = value;}

  /**
    @see #getForm()
    @since 0.0.7
  */
  public void setForm(
    Form value
    )
  {getBaseDataObject().put(PdfName.AcroForm,value.getBaseObject());}

  /**
    @see #getInformation()
  */
  public void setInformation(
    Information value
    )
  {getFile().getTrailer().put(PdfName.Info,value.getBaseObject());}

  /**
    @see #getNames()
    @since 0.0.4
  */
  public void setNames(
    Names value
    )
  {getBaseDataObject().put(PdfName.Names,value.getBaseObject());}

  /**
    @see #getPageLayout()
  */
  public void setPageLayout(
    PageLayoutEnum value
    )
  {
    switch(value)
    {
      case SinglePage:
        getBaseDataObject().put(PdfName.PageLayout,PdfName.SinglePage);
        break;
      case OneColumn:
        getBaseDataObject().put(PdfName.PageLayout,PdfName.OneColumn);
        break;
      case TwoColumns:
        getBaseDataObject().put(PdfName.PageLayout,PdfName.TwoColumnLeft);
        break;
    }
  }

  /**
    @see #getPageMode()
  */
  public void setPageMode(
    PageModeEnum value
    )
  {
    PdfName valueObject;
    switch(value)
    {
      case Simple:
        valueObject = PdfName.UseNone;
        break;
      case Bookmarks:
        valueObject = PdfName.UseOutlines;
        break;
      case Thumbnails:
        valueObject = PdfName.UseThumbs;
        break;
      case FullScreen:
        valueObject = PdfName.FullScreen;
        break;
      case OCG:
        valueObject = PdfName.UseOC;
        break;
      case Attachments:
        valueObject = PdfName.UseAttachments;
        break;
      default:
        throw new UnsupportedOperationException("Page mode unknown: " + value);
    }
    getBaseDataObject().put(PdfName.PageMode, valueObject);
  }

  /**
    @see #getPages()
  */
  public void setPages(
    Pages value
    )
  {getBaseDataObject().put(PdfName.Pages,value.getBaseObject());}

  /**
    @see #getPageSize()
  */
  public void setPageSize(
    Dimension2D value
    )
  {
    PdfArray mediaBox = getMediaBox();
    if(mediaBox == null)
    {
      // Create default media box!
      mediaBox = new Rectangle(0,0,0,0).getBaseDataObject();
      // Assign the media box to the document!
      ((PdfDictionary)getBaseDataObject().resolve(PdfName.Pages)).put(PdfName.MediaBox,mediaBox);
    }
    mediaBox.set(2,new PdfReal(value.getWidth()));
    mediaBox.set(3,new PdfReal(value.getHeight()));
  }

  /**
    @see #getResources()
  */
  public void setResources(
    Resources value
    )
  {
    PdfReference pages = (PdfReference)getBaseDataObject().get(PdfName.Pages);
    ((PdfDictionary)File.resolve(pages)).put(PdfName.Resources, value.getBaseObject());
  }

  /**
    @see #getVersion()
  */
  public void setVersion(
    Version value
    )
  {
    if(value == null)
    {getBaseDataObject().remove(PdfName.Version);}
    else
    {
      checkCompatibility("version");
      getBaseDataObject().put(PdfName.Version, new PdfName(value.toString()));
    }
  }

  /**
    @see #getViewerPreferences()
  */
  public void setViewerPreferences(
    ViewerPreferences value
    )
  {getBaseDataObject().put(PdfName.ViewerPreferences, value.getBaseObject());}

  // <Pageable>
  @Override
  public int getNumberOfPages(
    )
  {return getPages().size();}

  @Override
  public java.awt.print.PageFormat getPageFormat(
    int pageIndex
    ) throws IndexOutOfBoundsException
  {
    java.awt.print.PageFormat pageFormat = new java.awt.print.PageFormat();
    {
      Page page = getPages().get(pageIndex);
      Paper paper = new Paper();
      {
        Rectangle2D pageBox = page.getBox();
        paper.setSize(pageBox.getWidth(),pageBox.getHeight());
      }
      pageFormat.setPaper(paper);
    }
    return pageFormat;
  }

  @Override
  public Printable getPrintable(
    int pageIndex
    ) throws IndexOutOfBoundsException
  {return getPages().get(pageIndex);}
  // </Pageable>
  // </public>

  // <private>
  /**
    Gets the default media box.
  */
  private PdfArray getMediaBox(
    )
  {
    /*
      NOTE: Document media box MUST be associated with the page-tree root node
      in order to be inheritable by all the pages.
    */
    return (PdfArray)((PdfDictionary)getBaseDataObject().resolve(PdfName.Pages)).resolve(PdfName.MediaBox);
  }
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}