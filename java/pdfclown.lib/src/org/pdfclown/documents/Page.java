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

package org.pdfclown.documents;

import java.awt.Graphics;
import java.awt.Graphics2D;
import java.awt.geom.Dimension2D;
import java.awt.geom.Rectangle2D;
import java.awt.print.PageFormat;
import java.awt.print.Printable;
import java.awt.print.PrinterException;
import java.util.Map;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.bytes.IBuffer;
import org.pdfclown.documents.contents.ContentScanner;
import org.pdfclown.documents.contents.Contents;
import org.pdfclown.documents.contents.IContentContext;
import org.pdfclown.documents.contents.Resources;
import org.pdfclown.documents.contents.RotationEnum;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.objects.ContentObject;
import org.pdfclown.documents.contents.xObjects.FormXObject;
import org.pdfclown.documents.contents.xObjects.XObject;
import org.pdfclown.documents.interaction.navigation.page.Transition;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDataObject;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfNumber;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfStream;
import org.pdfclown.objects.Rectangle;
import org.pdfclown.util.NotImplementedException;
import org.pdfclown.util.math.geom.Dimension;

/**
  Document page [PDF:1.6:3.6.2].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.0
  @version 0.1.1, 06/08/11
*/
@PDF(VersionEnum.PDF10)
public final class Page
  extends PdfObjectWrapper<PdfDictionary>
  implements IContentContext,
    Printable
{
  /*
    NOTE: Inheritable attributes are NOT early-collected, as they are NOT part
    of the explicit representation of a page. They are retrieved everytime
    clients call.
  */
  // <class>
  // <classes>
  /**
    Annotations tab order [PDF:1.6:3.6.2].
  */
  @PDF(VersionEnum.PDF15)
  public enum TabOrderEnum
  {
    // <class>
    // <static>
    // <fields>
    /**
      Row order.
    */
    Row(PdfName.R),
    /**
      Column order.
    */
    Column(PdfName.C),
    /**
      Structure order.
    */
    Structure(PdfName.S);
    // </fields>

    // <interface>
    // <public>
    /**
      Gets the tab order corresponding to the given value.
    */
    public static TabOrderEnum get(
      PdfName value
      )
    {
      for(TabOrderEnum tabOrder : TabOrderEnum.values())
      {
        if(tabOrder.getCode().equals(value))
          return tabOrder;
      }
      return null;
    }
    // </public>
    // </interface>
    // </static>

    // <dynamic>
    // <fields>
    private final PdfName code;
    // </fields>

    // <constructors>
    private TabOrderEnum(
      PdfName code
      )
    {this.code = code;}
    // </constructors>

    // <interface>
    // <public>
    public PdfName getCode(
      )
    {return code;}
    // </public>
    // </interface>
    // </dynamic>
    // </class>
  }
  // </classes>

  // <static>
  // <interface>
  // <public>
  public static Page wrap(
    PdfDirectObject baseObject
    )
  {return baseObject == null ? null : new Page(baseObject);}
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  /**
    Creates a new page within the given document context, using default resources.
  */
  public Page(
    Document context
    )
  {
    super(
      context,
      new PdfDictionary(
        new PdfName[]
        {
          PdfName.Type,
          PdfName.Contents
        },
        new PdfDirectObject[]
        {
          PdfName.Page,
          context.getFile().register(
            new PdfStream()
            )
        }
        )
      );
  }

  /**
    Creates a new page within the given document context, using custom resources.
  */
  public Page(
    Document context,
    Dimension2D size,
    Resources resources
    )
  {
    super(
      context,
      new PdfDictionary(
        new PdfName[]
        {
          PdfName.Type,
          PdfName.MediaBox,
          PdfName.Contents,
          PdfName.Resources
        },
        new PdfDirectObject[]
        {
          PdfName.Page,
          new Rectangle(0,0,size.getWidth(),size.getHeight()).getBaseDataObject(),
          context.getFile().register(
            new PdfStream()
            ),
          resources.getBaseObject()
        }
        )
      );
  }

  private Page(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Page clone(
    Document context
    )
  {
    /*
      NOTE: We cannot just delegate the cloning to the base object, as it would
      involve some unwanted objects like those in 'Parent' and 'Annots' entries that may
      cause infinite loops (due to circular references) and may include exceeding contents
      (due to copy propagations to the whole page-tree which this page belongs to).
      TODO: 'Annots' entry must be finely treated to include any non-circular reference.
    */
    // TODO:IMPL deal with inheritable attributes!!!

    File contextFile = context.getFile();
    PdfDictionary clone = new PdfDictionary(getBaseDataObject().size());
    for(Map.Entry<PdfName,PdfDirectObject> entry : getBaseDataObject().entrySet())
    {
      PdfName key = entry.getKey();
      // Is the entry unwanted?
      if(key.equals(PdfName.Parent)
        || key.equals(PdfName.Annots))
        continue;

      // Insert the clone of the entry into the clone of the page dictionary!
      clone.put(
        key,
        (PdfDirectObject)entry.getValue().clone(contextFile)
        );
    }
    return new Page(contextFile.getIndirectObjects().add(clone).getReference());
  }

  /**
    Gets the page's behavior in response to trigger events.
  */
  @PDF(VersionEnum.PDF12)
  public PageActions getActions(
    )
  {
    PdfDirectObject actionsObject = getBaseDataObject().get(PdfName.AA);
    return actionsObject != null ? new PageActions(actionsObject) : null;
  }

  /**
    Gets the annotations associated to the page.
  */
  public PageAnnotations getAnnotations(
    )
  {
    PdfDirectObject annotationsObject = getBaseDataObject().get(PdfName.Annots);
    return annotationsObject != null ? new PageAnnotations(annotationsObject, this) : null;
  }

  /**
    Gets the page's display duration.
    <p>The page's display duration (also called its advance timing)
    is the maximum length of time, in seconds, that the page is displayed
    during presentations before the viewer application automatically advances
    to the next page.</p>
    <p>By default, the viewer does not advance automatically.</p>
  */
  @PDF(VersionEnum.PDF11)
  public float getDuration(
    )
  {
    PdfNumber<?> durationObject = (PdfNumber<?>)getBaseDataObject().get(PdfName.Dur);
    return durationObject == null ? 0 : durationObject.getNumberValue();
  }

  /**
    Gets the index of the page.
  */
  public int getIndex(
    )
  {
    /*
      NOTE: We'll scan sequentially each page-tree level above this page object
      collecting page counts. At each level we'll scan the kids array from the
      lower-indexed item to the ancestor of this page object at that level.
    */
    PdfReference ancestorKidReference = (PdfReference)getBaseObject();
    PdfReference parentReference = (PdfReference)getBaseDataObject().get(PdfName.Parent);
    PdfDictionary parent = (PdfDictionary)File.resolve(parentReference);
    PdfArray kids = (PdfArray)File.resolve(parent.get(PdfName.Kids));
    int index = 0;
    for(
      int i = 0;
      true;
      i++
      )
    {
      PdfReference kidReference = (PdfReference)kids.get(i);
      // Is the current-level counting complete?
      // NOTE: It's complete when it reaches the ancestor at the current level.
      if(kidReference.equals(ancestorKidReference)) // Ancestor node.
      {
        // Does the current level correspond to the page-tree root node?
        if(!parent.containsKey(PdfName.Parent))
        {
          // We reached the top: counting's finished.
          return index;
        }
        // Set the ancestor at the next level!
        ancestorKidReference = parentReference;
        // Move up one level!
        parentReference = (PdfReference)parent.get(PdfName.Parent);
        parent = (PdfDictionary)File.resolve(parentReference);
        kids = (PdfArray)File.resolve(parent.get(PdfName.Kids));
        i = -1;
      }
      else // Intermediate node.
      {
        PdfDictionary kid = (PdfDictionary)File.resolve(kidReference);
        if(kid.get(PdfName.Type).equals(PdfName.Page))
          index++;
        else
          index += ((PdfInteger)kid.get(PdfName.Count)).getRawValue();
      }
    }
  }

  /**
    Gets the page size.
  */
  public Dimension2D getSize(
    )
  {
    Rectangle2D box = getBox();
    return new Dimension(box.getWidth(), box.getHeight());
  }

  /**
    Gets the tab order to be used for annotations on the page.
  */
  @PDF(VersionEnum.PDF15)
  public TabOrderEnum getTabOrder(
    )
  {return TabOrderEnum.get((PdfName)getBaseDataObject().get(PdfName.Tabs));}

  /**
    Gets the transition effect to be used when displaying the page during presentations.
  */
  @PDF(VersionEnum.PDF11)
  public Transition getTransition(
    )
  {
    PdfDirectObject transitionObject = getBaseDataObject().get(PdfName.Trans);
    return transitionObject != null ? new Transition(transitionObject) : null;
  }

  /**
    @see #getActions()
  */
  public void setActions(
    PageActions value
    )
  {getBaseDataObject().put(PdfName.AA, value.getBaseObject());}

  /**
    @see #getAnnotations()
  */
  public void setAnnotations(
    PageAnnotations value
    )
  {getBaseDataObject().put(PdfName.Annots, value.getBaseObject());}

  /**
    @see #getBox()
  */
  public void setBox(
    Rectangle2D value
    )
  {getBaseDataObject().put(PdfName.MediaBox, new Rectangle(value).getBaseDataObject());}

  /**
    @see #getDuration()
  */
  public void setDuration(
    float value
    )
  {getBaseDataObject().put(PdfName.Dur,new PdfReal(value));}

  /**
    @see #getRotation()
  */
  public void setRotation(
    RotationEnum value
    )
  {getBaseDataObject().put(PdfName.Rotate,value.getCode());}

  /**
    @see #getSize()
  */
  public void setSize(
    Dimension2D value
    )
  {
    Rectangle2D box = getBox();
    box.setRect(box.getX(), box.getY(), value.getWidth(), value.getHeight());
    setBox(box);
  }

  /**
    @see #getTabOrder()
  */
  public void setTabOrder(
    TabOrderEnum value
    )
  {getBaseDataObject().put(PdfName.Tabs,value.getCode());}

  /**
    @see #getTransition()
  */
  public void setTransition(
    Transition value
    )
  {getBaseDataObject().put(PdfName.Trans, value.getBaseObject());}

  // <IContentContext>
  @Override
  public Rectangle2D getBox(
    )
  {return new Rectangle(getInheritableAttribute(PdfName.MediaBox)).toRectangle2D();}

  @Override
  public Contents getContents(
    )
  {
    PdfDirectObject contentsObject = getBaseDataObject().get(PdfName.Contents);
    if(contentsObject == null)
    {getBaseDataObject().put(PdfName.Contents, contentsObject = getFile().register(new PdfStream()));}
    return new Contents(contentsObject, this);
  }

  @Override
  public Resources getResources(
    )
  {return Resources.wrap(getInheritableAttribute(PdfName.Resources));}

  @Override
  public RotationEnum getRotation(
    )
  {
    PdfInteger rotationObject = (PdfInteger)getInheritableAttribute(PdfName.Rotate);
    return (rotationObject == null
      ? RotationEnum.Downward
      : RotationEnum.get(rotationObject));
  }

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
  {
    File contextFile = context.getFile();

    FormXObject form = new FormXObject(context);
    PdfStream formStream = form.getBaseDataObject();

    // Header.
    {
      PdfDictionary formHeader = formStream.getHeader();
      // Bounding box.
      formHeader.put(
        PdfName.BBox,
        (PdfDirectObject)getInheritableAttribute(PdfName.MediaBox).clone(contextFile)
        );
      // Resources.
      {
        PdfDirectObject resourcesObject = getInheritableAttribute(PdfName.Resources);
        formHeader.put(
          PdfName.Resources,
          // Same document?
          /* NOTE: Try to reuse the resource dictionary whenever possible. */
          (context.equals(getDocument()) ?
            resourcesObject
            : (PdfDirectObject)resourcesObject.clone(contextFile))
          );
      }
    }

    // Body (contents).
    {
      IBuffer formBody = formStream.getBody();
      PdfDataObject contentsDataObject = getBaseDataObject().resolve(PdfName.Contents);
      if(contentsDataObject instanceof PdfStream)
      {formBody.append(((PdfStream)contentsDataObject).getBody());}
      else
      {
        for(PdfDirectObject contentStreamObject : (PdfArray)contentsDataObject)
        {formBody.append(((PdfStream)File.resolve(contentStreamObject)).getBody());}
      }
    }

    return form;
  }
  // </IContentEntity>
  // </IContentContext>

  // <Printable>
  @Override
  public int print(
    Graphics graphics,
    PageFormat pageFormat,
    int pageIndex
    ) throws PrinterException
  {
    //TODO:verify pageIndex correspondence!
    render(
      (Graphics2D)graphics,
      new Dimension(//TODO:verify page resolution!
        pageFormat.getWidth(),
        pageFormat.getHeight()
        )
      );

    return Printable.PAGE_EXISTS;
  }
  // </Printable>
  // </public>

  // <private>
  private PdfDirectObject getInheritableAttribute(
    PdfName key
    )
  {
    /*
      NOTE: It moves upward until it finds the inherited attribute.
    */
    PdfDictionary dictionary = getBaseDataObject();
    while(true)
    {
      PdfDirectObject entry = dictionary.get(key);
      if(entry != null)
        return entry;

      dictionary = (PdfDictionary)File.resolve(
        dictionary.get(PdfName.Parent)
        );
      if(dictionary == null)
      {
        // Isn't the page attached to the page tree?
        /* NOTE: This condition is illegal. */
        if(getBaseDataObject().get(PdfName.Parent) == null)
          throw new RuntimeException("Inheritable attributes unreachable: Page objects MUST be inserted into their document's Pages collection before being used.");

        return null;
      }
    }
  }
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}