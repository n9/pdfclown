/*
  Copyright 2008-2010 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.objects;

using system = System;
using System.Drawing;

namespace org.pdfclown.documents.interaction.annotations
{
  /**
    <summary>Link annotation [PDF:1.6:8.4.5].</summary>
    <remarks>It represents either a hypertext link to a destination elsewhere in the document
    or an action to be performed.</remarks>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class Link
    : Annotation,
      ILink
  {
    #region dynamic
    #region constructors
    public Link(
      Page page,
      RectangleF box,
      Destination destination
      ) : this(page,box)
    {Destination = destination;}

    public Link(
      Page page,
      RectangleF box,
      Action action
      ) : this(page,box)
    {
      AnnotationActions actions = new AnnotationActions(this);
      actions.OnActivate = action;
      Actions = actions;
    }

    public Link(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      ) : base(baseObject,container)
    {}

    private Link(
      Page page,
      RectangleF box
      ) : base(
        page.Document,
        PdfName.Link,
        box,
        page
        )
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new system::NotImplementedException();}

    #region ILink
    public override Action Action
    {
      get
      {return base.Action;}
      set
      {
        /*
          NOTE: This entry is not permitted in link annotations if a 'Dest' entry is present.
        */
        if(BaseDataObject.ContainsKey(PdfName.Dest)
          && value != null)
        {BaseDataObject.Remove(PdfName.Dest);}

        base.Action = value;
      }
    }

    /**
      <summary>Gets/Sets the destination to jump to.</summary>
    */
    public Destination Destination
    {
      get
      {
        /*
          NOTE: 'Dest' entry may be undefined.
        */
        PdfDirectObject destinationObject = BaseDataObject[PdfName.Dest];
        if(destinationObject == null)
          return null;

        return Document.ResolveName<Destination>(
          destinationObject,
          Container
          );
      }
      set
      {
        if(value == null)
        {BaseDataObject.Remove(PdfName.Dest);}
        else
        {
          /*
            NOTE: This entry is not permitted in link annotations if an 'A' entry is present.
          */
          if(BaseDataObject.ContainsKey(PdfName.A))
          {BaseDataObject.Remove(PdfName.A);}

          BaseDataObject[PdfName.Dest] = value.NamedBaseObject;
        }
      }
    }

    public PdfObjectWrapper Target
    {
      get
      {
        if(BaseDataObject.ContainsKey(PdfName.Dest))
          return Destination;
        else if(BaseDataObject.ContainsKey(PdfName.A))
          return Action;
        else
          return null;
      }
      set
      {
        if(value is Destination)
        {Destination = (Destination)value;}
        else if(value is Action)
        {Action = (Action)value;}
        else
          throw new system::ArgumentException("It MUST be either a Destination or an Action.");
      }
    }
    #endregion
    #endregion
    #endregion
    #endregion
  }
}