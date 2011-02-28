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
using org.pdfclown.files;
using org.pdfclown.objects;

using system = System;

namespace org.pdfclown.documents.interaction.actions
{
  /**
    <summary>Action to be performed by the viewer application [PDF:1.6:8.5].</summary>
  */
  [PDF(VersionEnum.PDF11)]
  public class Action
    : PdfNamedObjectWrapper<PdfDictionary>
  {
    #region static
    #region interface
    #region public
    /**
      <summary>Wraps an action reference into an action object.</summary>
      <param name="reference">Reference to an action object.</param>
      <returns>Action object associated to the reference.</returns>
    */
    public static Action Wrap(
      PdfReference reference
      )
    {return Wrap(reference, null);}

    /**
      <summary>Wraps an action base object into an action object.</summary>
      <param name="baseObject">Action base object.</param>
      <param name="container">Action base object container.</param>
      <returns>Action object associated to the base object.</returns>
    */
    public static Action Wrap(
      PdfDirectObject baseObject,
      PdfIndirectObject container
      )
    {return Wrap(baseObject, container, null);}

    /**
      <summary>Wraps an action base object into an action object.</summary>
      <param name="baseObject">Action base object.</param>
      <param name="container">Action base object container.</param>
      <param name="name">Action name.</param>
      <returns>Action object associated to the base object.</returns>
    */
    public static Action Wrap(
      PdfDirectObject baseObject,
      PdfIndirectObject container,
      PdfString name
      )
    {
      if(baseObject == null)
        return null;

      PdfDictionary dataObject = (PdfDictionary)File.Resolve(baseObject);
      PdfName actionType = (PdfName)dataObject[PdfName.S];
      if(actionType == null
        || (dataObject.ContainsKey(PdfName.Type)
            && !dataObject[PdfName.Type].Equals(PdfName.Action)))
        return null;

      if(actionType.Equals(PdfName.GoTo))
        return new GoToLocal(baseObject,container);
      else if(actionType.Equals(PdfName.GoToR))
        return new GoToRemote(baseObject,container);
      else if(actionType.Equals(PdfName.GoToE))
        return new GoToEmbedded(baseObject,container);
      else if(actionType.Equals(PdfName.Launch))
        return new Launch(baseObject,container);
      else if(actionType.Equals(PdfName.Thread))
        return new GoToThread(baseObject,container);
      else if(actionType.Equals(PdfName.URI))
        return new GoToURI(baseObject,container);
      else if(actionType.Equals(PdfName.Sound))
        return new PlaySound(baseObject,container);
      else if(actionType.Equals(PdfName.Movie))
        return new PlayMovie(baseObject,container);
      else if(actionType.Equals(PdfName.Hide))
        return new ToggleVisibility(baseObject,container);
      else if(actionType.Equals(PdfName.Named))
      {
        PdfName actionName = (PdfName)dataObject[PdfName.N];
        if(actionName.Equals(PdfName.NextPage))
          return new GoToNextPage(baseObject,container);
        else if(actionName.Equals(PdfName.PrevPage))
          return new GoToPreviousPage(baseObject,container);
        else if(actionName.Equals(PdfName.FirstPage))
          return new GoToFirstPage(baseObject,container);
        else if(actionName.Equals(PdfName.LastPage))
          return new GoToLastPage(baseObject,container);
        else // Custom named action.
          return new NamedAction(baseObject,container);
      }
      else if(actionType.Equals(PdfName.SubmitForm))
        return new SubmitForm(baseObject,container);
      else if(actionType.Equals(PdfName.ResetForm))
        return new ResetForm(baseObject,container);
      else if(actionType.Equals(PdfName.ImportData))
        return new ImportData(baseObject,container);
      else if(actionType.Equals(PdfName.JavaScript))
        return new JavaScript(baseObject,container,name);
      else if(actionType.Equals(PdfName.SetOCGState))
        return new SetOcgState(baseObject,container);
      else if(actionType.Equals(PdfName.Rendition))
        return new Rendition(baseObject,container);
      else if(actionType.Equals(PdfName.Trans))
        return new DoTransition(baseObject,container);
      else if(actionType.Equals(PdfName.GoTo3DView))
        return new GoTo3dView(baseObject,container);
      else // Custom action.
        return new Action(baseObject,container,name);
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    /**
      <summary>Creates a new action within the given document context.</summary>
    */
    protected Action(
      Document context,
      PdfName actionType
      ) : base(
        context.File,
        new PdfDictionary(
          new PdfName[]
          {
            PdfName.Type,
            PdfName.S
          },
          new PdfDirectObject[]
          {
            PdfName.Action,
            actionType
          }
          )
        )
    {}

    protected Action(
      PdfDirectObject baseObject,
      PdfIndirectObject container,
      PdfString name
      ) : base(baseObject, container, name)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets the actions to be performed after the current one.</summary>
    */
    [PDF(VersionEnum.PDF12)]
    public ChainedActions Actions
    {
      get
      {
        PdfDirectObject nextObject = BaseDataObject[PdfName.Next];
        return nextObject == null ? null : new ChainedActions(nextObject, Container, this);
      }
      set
      {BaseDataObject[PdfName.Next] = value.BaseObject;}
    }

    public override object Clone(
      Document context
      )
    {throw new system::NotImplementedException();}
    #endregion
    #endregion
    #endregion
  }
}