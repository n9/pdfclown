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

using org.pdfclown.documents;
using org.pdfclown.files;
using org.pdfclown.objects;
using org.pdfclown.util;

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
      <param name="name">Destination name.</param>
      <returns>Destination object associated to the base object.</returns>
    */
    public static Destination Wrap(
      PdfDirectObject baseObject,
      PdfString name
      )
    {
      if(baseObject == null)
        return null;

      PdfArray dataObject = (PdfArray)File.Resolve(baseObject);
      PdfDirectObject pageObject = dataObject[0];
      if(pageObject is PdfReference)
        return new LocalDestination(baseObject, name);
      else if(pageObject is PdfInteger)
        return new RemoteDestination(baseObject, name);
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
      <param name="viewParams">View parameters. Their actual composition depends on the <code>mode</code>
        value (see ModeEnum for more info).</param>
    */
    protected Destination(
      Document context,
      PdfDirectObject pageObject,
      ModeEnum mode,
      double?[] viewParams
      ) : base(context, new PdfArray())
    {
      PdfArray baseDataObject = BaseDataObject;
      {
        baseDataObject.Add(pageObject);
        baseDataObject.Add(mode.GetName());
        switch(mode)
        {
          case ModeEnum.Fit:
            break;
          case ModeEnum.FitBoundingBox:
            break;
          case ModeEnum.FitBoundingBoxHorizontal:
            baseDataObject.Add(PdfReal.Get(viewParams[0]));
            break;
          case ModeEnum.FitBoundingBoxVertical:
            baseDataObject.Add(PdfReal.Get(viewParams[0]));
            break;
          case ModeEnum.FitHorizontal:
            baseDataObject.Add(PdfReal.Get(viewParams[0]));
            break;
          case ModeEnum.FitRectangle:
            baseDataObject.Add(PdfReal.Get(viewParams[0]));
            baseDataObject.Add(PdfReal.Get(viewParams[1]));
            baseDataObject.Add(PdfReal.Get(viewParams[2]));
            baseDataObject.Add(PdfReal.Get(viewParams[3]));
            break;
          case ModeEnum.FitVertical:
            baseDataObject.Add(PdfReal.Get(viewParams[0]));
            break;
          case ModeEnum.XYZ:
            baseDataObject.Add(PdfReal.Get(viewParams[0]));
            baseDataObject.Add(PdfReal.Get(viewParams[1]));
            baseDataObject.Add(PdfReal.Get(viewParams[2]));
            break;
        }
      }
    }

    protected Destination(
      PdfDirectObject baseObject,
      PdfString name
      ) : base(baseObject, name)
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
      {return ModeEnumExtension.Get((PdfName)BaseDataObject[1]).Value;}
    }

    /**
      <summary>Gets the target page reference.</summary>
    */
    public abstract object PageRef
    {
      get;
    }
    #endregion
    #endregion
    #endregion
  }

  internal static class ModeEnumExtension
  {
    private static readonly BiDictionary<Destination.ModeEnum,PdfName> codes;

    static ModeEnumExtension()
    {
      codes = new BiDictionary<Destination.ModeEnum,PdfName>();
      codes[Destination.ModeEnum.Fit] = PdfName.Fit;
      codes[Destination.ModeEnum.FitBoundingBox] = PdfName.FitB;
      codes[Destination.ModeEnum.FitBoundingBoxHorizontal] = PdfName.FitBH;
      codes[Destination.ModeEnum.FitBoundingBoxVertical] = PdfName.FitBV;
      codes[Destination.ModeEnum.FitHorizontal] = PdfName.FitH;
      codes[Destination.ModeEnum.FitRectangle] = PdfName.FitR;
      codes[Destination.ModeEnum.FitVertical] = PdfName.FitV;
      codes[Destination.ModeEnum.XYZ] = PdfName.XYZ;
    }

    public static Destination.ModeEnum? Get(
      PdfName name
      )
    {
      if(name == null)
        return null;

      Destination.ModeEnum? mode = codes.GetKey(name);
      if(!mode.HasValue)
        throw new NotSupportedException("Mode unknown: " + name);

      return mode;
    }

    public static PdfName GetName(
      this Destination.ModeEnum mode
      )
    {return codes[mode];}
  }
}