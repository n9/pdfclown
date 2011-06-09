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

using org.pdfclown.bytes;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using xObjects = org.pdfclown.documents.contents.xObjects;
using org.pdfclown.documents.interaction.navigation.page;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using drawing = System.Drawing;

namespace org.pdfclown.documents
{
  /**
    <summary>Document page [PDF:1.6:3.6.2].</summary>
  */
  [PDF(VersionEnum.PDF10)]
  public class Page
    : PdfObjectWrapper<PdfDictionary>,
      IContentContext
  {
    /*
      NOTE: Inheritable attributes are NOT early-collected, as they are NOT part
      of the explicit representation of a page. They are retrieved everytime
      clients call.
    */
    #region types
    /**
      <summary>Annotations tab order [PDF:1.6:3.6.2].</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public enum TabOrderEnum
    {
      /**
        <summary>Row order.</summary>
      */
      Row,
      /**
        <summary>Column order.</summary>
      */
      Column,
      /**
        <summary>Structure order.</summary>
      */
      Structure
    };
    #endregion

    #region static
    #region fields
    private static readonly Dictionary<TabOrderEnum,PdfName> TabOrderEnumCodes;
    #endregion

    #region constructors
    static Page()
    {
      TabOrderEnumCodes = new Dictionary<TabOrderEnum,PdfName>();
      TabOrderEnumCodes[TabOrderEnum.Row] = PdfName.R;
      TabOrderEnumCodes[TabOrderEnum.Column] = PdfName.C;
      TabOrderEnumCodes[TabOrderEnum.Structure] = PdfName.S;
    }
    #endregion

    #region interface
    #region public
    public static Page Wrap(
      PdfDirectObject baseObject
      )
    {return baseObject == null ? null : new Page(baseObject);}
    #endregion

    #region private
    /**
      <summary>Gets the code corresponding to the given value.</summary>
    */
    private static PdfName ToCode(
      TabOrderEnum value
      )
    {return TabOrderEnumCodes[value];}

    /**
      <summary>Gets the tab order corresponding to the given value.</summary>
    */
    private static TabOrderEnum ToTabOrderEnum(
      PdfName value
      )
    {
      foreach(KeyValuePair<TabOrderEnum,PdfName> tabOrder in TabOrderEnumCodes)
      {
        if(tabOrder.Value.Equals(value))
          return tabOrder.Key;
      }
      return TabOrderEnum.Row;
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    /**
      <summary>Creates a new page within the given document context, using default resources.</summary>
    */
    public Page(
      Document context
      ) : base(
        context,
        new PdfDictionary(
          new PdfName[2]
          {
            PdfName.Type,
            PdfName.Contents
          },
          new PdfDirectObject[2]
          {
            PdfName.Page,
            context.File.Register(
              new PdfStream()
              )
          }
          )
        )
    {}

    /**
      <summary>Creates a new page within the given document context, using custom resources.</summary>
    */
    public Page(
      Document context,
      drawing::Size size,
      Resources resources
      ) : base(
        context,
        new PdfDictionary(
          new PdfName[4]
          {
            PdfName.Type,
            PdfName.MediaBox,
            PdfName.Contents,
            PdfName.Resources
          },
          new PdfDirectObject[4]
          {
            PdfName.Page,
            new Rectangle(0,0,size.Width,size.Height).BaseDataObject,
            context.File.Register(
              new PdfStream()
              ),
            resources.BaseObject
          }
          )
        )
    {}

    private Page(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets the page's behavior in response to trigger events.</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public PageActions Actions
    {
      get
      {
        PdfDirectObject actionsObject = BaseDataObject[PdfName.AA];
        return actionsObject != null ? new PageActions(actionsObject) : null;
      }
      set
      {BaseDataObject[PdfName.AA] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the annotations associated to the page.</summary>
    */
    public PageAnnotations Annotations
    {
      get
      {
        PdfDirectObject annotationsObject = BaseDataObject[PdfName.Annots];
        return annotationsObject != null ? new PageAnnotations(annotationsObject, this) : null;
      }
      set
      {BaseDataObject[PdfName.Annots] = value.BaseObject;}
    }

    public override object Clone(
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

      File contextFile = context.File;
      PdfDictionary clone = new PdfDictionary(BaseDataObject.Count);
      foreach(KeyValuePair<PdfName,PdfDirectObject> entry in BaseDataObject)
      {
        PdfName key = entry.Key;
        // Is the entry unwanted?
        if(key.Equals(PdfName.Parent)
          || key.Equals(PdfName.Annots))
          continue;

        // Insert the clone of the entry into the clone of the page dictionary!
        clone[key] = (PdfDirectObject)entry.Value.Clone(contextFile);
      }
      return new Page(contextFile.IndirectObjects.Add(clone).Reference);
    }

    /**
      <summary>Gets/Sets the page's display duration.</summary>
      <remarks>
        <para>The page's display duration (also called its advance timing)
        is the maximum length of time, in seconds, that the page is displayed
        during presentations before the viewer application automatically advances
        to the next page.</para>
        <para>By default, the viewer does not advance automatically.</para>
      </remarks>
    */
    [PDF(VersionEnum.PDF11)]
    public float Duration
    {
      get
      {
        IPdfNumber durationObject = (IPdfNumber)BaseDataObject[PdfName.Dur];
        return durationObject == null ? 0 : durationObject.RawValue;
      }
      set
      {BaseDataObject[PdfName.Dur] = new PdfReal(value);}
    }

    /**
      <summary>Gets the index of the page.</summary>
    */
    public int Index
    {
      get
      {
        /*
          NOTE: We'll scan sequentially each page-tree level above this page object
          collecting page counts. At each level we'll scan the kids array from the
          lower-indexed item to the ancestor of this page object at that level.
        */
        PdfReference ancestorKidReference = (PdfReference)BaseObject;
        PdfReference parentReference = (PdfReference)BaseDataObject[PdfName.Parent];
        PdfDictionary parent = (PdfDictionary)File.Resolve(parentReference);
        PdfArray kids = (PdfArray)File.Resolve(parent[PdfName.Kids]);
        int index = 0;
        for(
          int i = 0;
          true;
          i++
          )
        {
          PdfReference kidReference = (PdfReference)kids[i];
          // Is the current-level counting complete?
          // NOTE: It's complete when it reaches the ancestor at this level.
          if(kidReference.Equals(ancestorKidReference)) // Ancestor node.
          {
            // Does the current level correspond to the page-tree root node?
            if(!parent.ContainsKey(PdfName.Parent))
            {
              // We reached the top: counting's finished.
              return index;
            }
            // Set the ancestor at the next level!
            ancestorKidReference = parentReference;
            // Move up one level!
            parentReference = (PdfReference)parent[PdfName.Parent];
            parent = (PdfDictionary)File.Resolve(parentReference);
            kids = (PdfArray)File.Resolve(parent[PdfName.Kids]);
            i = -1;
          }
          else // Intermediate node.
          {
            PdfDictionary kid = (PdfDictionary)File.Resolve(kidReference);
            if(kid[PdfName.Type].Equals(PdfName.Page))
              index++;
            else
              index += ((PdfInteger)kid[PdfName.Count]).RawValue;
          }
        }
      }
    }

    /**
      <summary>Gets/Sets the page size.</summary>
    */
    public drawing::SizeF Size
    {
      get
      {return Box.Size;}
      set
      {
        drawing::RectangleF box = Box;
        box.Size = value;
        Box = box;
      }
    }

    /**
      <summary>Gets/Sets the tab order to be used for annotations on the page.</summary>
    */
    [PDF(VersionEnum.PDF15)]
    public TabOrderEnum TabOrder
    {
      get
      {return ToTabOrderEnum((PdfName)BaseDataObject[PdfName.Tabs]);}
      set
      {BaseDataObject[PdfName.Tabs] = ToCode(value);}
    }

    /**
      <summary>Gets the transition effect to be used
      when displaying the page during presentations.</summary>
    */
    [PDF(VersionEnum.PDF11)]
    public Transition Transition
    {
      get
      {
        PdfDirectObject transitionObject = BaseDataObject[PdfName.Trans];
        return transitionObject != null ? new Transition(transitionObject) : null;
      }
      set
      {BaseDataObject[PdfName.Trans] = value.BaseObject;}
    }

    #region IContentContext
    public drawing::RectangleF Box
    {
      get
      {return new Rectangle(GetInheritableAttribute(PdfName.MediaBox)).ToRectangleF();}
      set
      {BaseDataObject[PdfName.MediaBox] = new Rectangle(value).BaseDataObject;}
    }

    public Contents Contents
    {
      get
      {
        PdfDirectObject contentsObject = BaseDataObject[PdfName.Contents];
        if(contentsObject == null)
        {BaseDataObject[PdfName.Contents] = (contentsObject = File.Register(new PdfStream()));}
        return new Contents(contentsObject, this);
      }
    }

    public void Render(
      drawing::Graphics context,
      drawing::SizeF size
      )
    {
      ContentScanner scanner = new ContentScanner(Contents);
      scanner.Render(context, size);
    }

    public Resources Resources
    {
      get
      {return Resources.Wrap(GetInheritableAttribute(PdfName.Resources));}
    }

    public RotationEnum Rotation
    {
      get
      {
        PdfInteger rotationObject = (PdfInteger)GetInheritableAttribute(PdfName.Rotate);
        return (rotationObject == null
          ? RotationEnum.Downward
          : RotationEnumExtension.Get(rotationObject));
      }
      set
      {BaseDataObject[PdfName.Rotate] = new PdfInteger((int)value);}
    }

    #region IContentEntity
    public ContentObject ToInlineObject(
      PrimitiveComposer composer
      )
    {throw new NotImplementedException();}

    public xObjects::XObject ToXObject(
      Document context
      )
    {
      File contextFile = context.File;

      xObjects::FormXObject form = new xObjects::FormXObject(context);
      PdfStream formStream = form.BaseDataObject;

      // Header.
      {
        PdfDictionary formHeader = formStream.Header;
        // Bounding box.
        formHeader[PdfName.BBox] = (PdfDirectObject)GetInheritableAttribute(PdfName.MediaBox).Clone(contextFile);
        // Resources.
        {
          PdfDirectObject resourcesObject = GetInheritableAttribute(PdfName.Resources);
          // Same document?
          /* NOTE: Try to reuse the resource dictionary whenever possible. */
          formHeader[PdfName.Resources] = (context.Equals(Document) ?
            resourcesObject
            : (PdfDirectObject)resourcesObject.Clone(contextFile));
        }
      }

      // Body (contents).
      {
        IBuffer formBody = formStream.Body;
        PdfDataObject contentsDataObject = BaseDataObject.Resolve(PdfName.Contents);
        if(contentsDataObject is PdfStream)
        {formBody.Append(((PdfStream)contentsDataObject).Body);}
        else
        {
          foreach(PdfDirectObject contentStreamObject in (PdfArray)contentsDataObject)
          {formBody.Append(((PdfStream)File.Resolve(contentStreamObject)).Body);}
        }
      }

      return form;
    }
    #endregion
    #endregion
    #endregion

    #region private
    private PdfDirectObject GetInheritableAttribute(
      PdfName key
      )
    {
      /*
        NOTE: It moves upward until it finds the inherited attribute.
      */
      PdfDictionary dictionary = BaseDataObject;
      while(true)
      {
        PdfDirectObject entry = dictionary[key];
        if(entry != null)
          return entry;

        dictionary = (PdfDictionary)File.Resolve(
          dictionary[PdfName.Parent]
          );
        if(dictionary == null)
        {
          // Isn't the page attached to the page tree?
          /* NOTE: This condition is illegal. */
          if(BaseDataObject[PdfName.Parent] == null)
            throw new Exception("Inheritable attributes unreachable: Page objects MUST be inserted into their document's Pages collection before being used.");

          return null;
        }
      }
    }
    #endregion
    #endregion
    #endregion
  }
}