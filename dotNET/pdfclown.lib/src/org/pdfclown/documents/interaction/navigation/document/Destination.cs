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

namespace org.pdfclown.documents.interaction.navigation.document
{
  /**
    <summary>Interaction target [PDF:1.6:8.2.1].</summary>
    <remarks>
      It represents a particular view of a document, consisting of the following items:
      <list type="bullet">
        <item>the page of the document to be displayed;</item>
        <item>the location of the document window on that page;</item>
        <item>the magnification (zoom) factor to use when displaying the page.</item>
      </list>
    </remarks>
  */
  [PDF(VersionEnum.PDF10)]
  public abstract class Destination
    : PdfNamedObjectWrapper<PdfArray>
  {
    #region types
    /**
      <summary>Destination mode [PDF:1.6:8.2.1].</summary>
    */
    public enum ModeEnum
    {
      /**
        <summary>Display the page at the given upper-left position,
        applying the given magnification.</summary>
        <remarks>
          View parameters:
          <list type="number">
            <item>left coordinate</item>
            <item>top coordinate</item>
            <item>zoom</item>
          </list>
        </remarks>
      */
      XYZ,
      /**
        <summary>Display the page with its contents magnified just enough to fit
        the entire page within the window both horizontally and vertically.</summary>
        <remarks>No view parameters.</remarks>
      */
      Fit,
      /**
        <summary>Display the page with the vertical coordinate <code>top</code> positioned
        at the top edge of the window and the contents of the page magnified
        just enough to fit the entire width of the page within the window.</summary>
        <remarks>
          View parameters:
          <list type="number">
            <item>top coordinate</item>
          </list>
        </remarks>
      */
      FitHorizontal,
      /**
        <summary>Display the page with the horizontal coordinate <code>left</code> positioned
        at the left edge of the window and the contents of the page magnified
        just enough to fit the entire height of the page within the window.</summary>
        <remarks>
          View parameters:
          <list type="number">
            <item>left coordinate</item>
          </list>
        </remarks>
      */
      FitVertical,
      /**
        <summary>Display the page with its contents magnified just enough to fit
        the rectangle specified by the given coordinates entirely
        within the window both horizontally and vertically.</summary>
        <remarks>
          View parameters:
          <list type="number">
            <item>left coordinate</item>
            <item>bottom coordinate</item>
            <item>right coordinate</item>
            <item>top coordinate</item>
          </list>
        </remarks>
      */
      FitRectangle,
      /**
        <summary>Display the page with its contents magnified just enough to fit
        its bounding box entirely within the window both horizontally and vertically.</summary>
        <remarks>No view parameters.</remarks>
      */
      FitBoundingBox,
      /**
        <summary>Display the page with the vertical coordinate <code>top</code> positioned
        at the top edge of the window and the contents of the page magnified
        just enough to fit the entire width of its bounding box within the window.</summary>
        <remarks>
          View parameters:
          <list type="number">
            <item>top coordinate</item>
          </list>
        </remarks>
      */
      FitBoundingBoxHorizontal,
      /**
        <summary>Display the page with the horizontal coordinate <code>left</code> positioned
        at the left edge of the window and the contents of the page magnified
        just enough to fit the entire height of its bounding box within the window.</summary>
        <remarks>
          View parameters:
          <list type="number">
            <item>left coordinate</item>
          </list>
        </remarks>
      */
      FitBoundingBoxVertical
    }
    #endregion

    #region static
    #region interface
    #region public
    /**
      <summary>Wraps a destination base object into a destination object.</summary>
      <param name="baseObject">Destination base object.</param>
      <param name="container">Destination base object container.</param>
      <param name="name">Destination name.</param>
      <returns>Destination object associated to the base object.</returns>
    */
    public static Destination Wrap(
      PdfDirectObject baseObject,
      PdfIndirectObject container,
      PdfString name
      )
    {
      /*
        NOTE: This is a factory method for any destination-derived object.
      */
      if(baseObject == null)
        return null;

      PdfArray dataObject = (PdfArray)File.Resolve(baseObject);
      PdfDirectObject pageObject = dataObject[0];
      if(pageObject is PdfReference)
        return new LocalDestination(baseObject,container,name);
      else if(pageObject is PdfInteger)
        return new RemoteDestination(baseObject,container,name);
      else
        throw new ArgumentException("'baseObject' parameter doesn't represent a valid destination object.");
    }
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    /**
      <summary>Creates a new destination within the given document context.</summary>
      <param name="context">Document context.</param>
      <param name="pageObject">Page reference. It may be either an actual page reference (PdfReference)
        or a page index (PdfInteger).</param>
      <param name="mode">Destination mode.</param>
      <param name="viewParams">View parameters. Their actual composition depends on the <code>mode</code> value
        (see ModeEnum for more info).</param>
    */
    protected Destination(
      Document context,
      PdfDirectObject pageObject,
      ModeEnum mode,
      float?[] viewParams
      ) : base(
        context.File,
        new PdfArray()
        )
    {
      PdfArray destinationObject = BaseDataObject;

      destinationObject.Add(pageObject);

      switch(mode)
      {
        case ModeEnum.Fit:
          destinationObject.Add(PdfName.Fit);
          break;
        case ModeEnum.FitBoundingBox:
          destinationObject.Add(PdfName.FitB);
          break;
        case ModeEnum.FitBoundingBoxHorizontal:
          destinationObject.Add(PdfName.FitBH);
          destinationObject.Add(PdfReal.Get(viewParams[0]));
          break;
        case ModeEnum.FitBoundingBoxVertical:
          destinationObject.Add(PdfName.FitBV);
          destinationObject.Add(PdfReal.Get(viewParams[0]));
          break;
        case ModeEnum.FitHorizontal:
          destinationObject.Add(PdfName.FitH);
          destinationObject.Add(PdfReal.Get(viewParams[0]));
          break;
        case ModeEnum.FitRectangle:
          destinationObject.Add(PdfName.FitR);
          destinationObject.Add(PdfReal.Get(viewParams[0]));
          destinationObject.Add(PdfReal.Get(viewParams[1]));
          destinationObject.Add(PdfReal.Get(viewParams[2]));
          destinationObject.Add(PdfReal.Get(viewParams[3]));
          break;
        case ModeEnum.FitVertical:
          destinationObject.Add(PdfName.FitV);
          destinationObject.Add(PdfReal.Get(viewParams[0]));
          break;
        case ModeEnum.XYZ:
          destinationObject.Add(PdfName.XYZ);
          destinationObject.Add(PdfReal.Get(viewParams[0]));
          destinationObject.Add(PdfReal.Get(viewParams[1]));
          destinationObject.Add(PdfReal.Get(viewParams[2]));
          break;
      }
    }

    protected Destination(
      PdfDirectObject baseObject,
      PdfIndirectObject container,
      PdfString name
      ) : base(
        baseObject,
        container,
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
      <summary>Gets the destination mode.</summary>
    */
    public ModeEnum Mode
    {
      get
      {
        PdfName modeObject = (PdfName)BaseDataObject[1];
        if(modeObject == PdfName.FitB)
          return ModeEnum.FitBoundingBox;
        else if(modeObject == PdfName.FitBH)
          return ModeEnum.FitBoundingBoxHorizontal;
        else if(modeObject == PdfName.FitBV)
          return ModeEnum.FitBoundingBoxVertical;
        else if(modeObject == PdfName.FitH)
          return ModeEnum.FitHorizontal;
        else if(modeObject == PdfName.FitR)
          return ModeEnum.FitRectangle;
        else if(modeObject == PdfName.FitV)
          return ModeEnum.FitVertical;
        else if(modeObject == PdfName.XYZ)
          return ModeEnum.XYZ;
        else
          return ModeEnum.Fit;
      }
    }

    /**
      <summary>Gets the target page reference.</summary>
    */
    public abstract object PageRef
    {get;}
    #endregion
    #endregion
    #endregion
  }
}