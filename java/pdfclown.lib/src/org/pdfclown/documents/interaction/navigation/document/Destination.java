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

package org.pdfclown.documents.interaction.navigation.document;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfNamedObjectWrapper;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfString;
import org.pdfclown.util.NotImplementedException;

/**
  Interaction target [PDF:1.6:8.2.1].
  <p>It represents a particular view of a document, consisting of the following items:</p>
  <ul>
    <li>the page of the document to be displayed;</li>
    <li>the location of the document window on that page;</li>
    <li>the magnification (zoom) factor to use when displaying the page.</li>
  </ul>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 11/01/11
*/
@PDF(VersionEnum.PDF10)
public abstract class Destination
  extends PdfNamedObjectWrapper<PdfArray>
{
  // <class>
  // <classes>
  /**
    Destination mode [PDF:1.6:8.2.1].
  */
  public enum ModeEnum
  {
    /**
      Display the page at the given upper-left position,
      applying the given magnification.

      <p>View parameters:</p>
      <ol>
        <li>left coordinate</li>
        <li>top coordinate</li>
        <li>zoom</li>
      </ol>
    */
    XYZ(PdfName.XYZ),
    /**
      Display the page with its contents magnified just enough to fit
      the entire page within the window both horizontally and vertically.

      <p>No view parameters.</p>
    */
    Fit(PdfName.Fit),
    /**
      Display the page with the vertical coordinate <code>top</code> positioned
      at the top edge of the window and the contents of the page magnified
      just enough to fit the entire width of the page within the window.

      <p>View parameters:</p>
      <ol>
        <li>top coordinate</li>
      </ol>
    */
    FitHorizontal(PdfName.FitH),
    /**
      Display the page with the horizontal coordinate <code>left</code> positioned
      at the left edge of the window and the contents of the page magnified
      just enough to fit the entire height of the page within the window.

      <p>View parameters:</p>
      <ol>
        <li>left coordinate</li>
      </ol>
    */
    FitVertical(PdfName.FitV),
    /**
      Display the page with its contents magnified just enough to fit
      the rectangle specified by the given coordinates entirely
      within the window both horizontally and vertically.

      <p>View parameters:</p>
      <ol>
        <li>left coordinate</li>
        <li>bottom coordinate</li>
        <li>right coordinate</li>
        <li>top coordinate</li>
      </ol>
    */
    FitRectangle(PdfName.FitR),
    /**
      Display the page with its contents magnified just enough to fit
      its bounding box entirely within the window both horizontally and vertically.

      <p>No view parameters.</p>
    */
    FitBoundingBox(PdfName.FitB),
    /**
      Display the page with the vertical coordinate <code>top</code> positioned
      at the top edge of the window and the contents of the page magnified
      just enough to fit the entire width of its bounding box within the window.

      <p>View parameters:</p>
      <ol>
        <li>top coordinate</li>
      </ol>
    */
    FitBoundingBoxHorizontal(PdfName.FitBH),
    /**
      Display the page with the horizontal coordinate <code>left</code> positioned
      at the left edge of the window and the contents of the page magnified
      just enough to fit the entire height of its bounding box within the window.

      <p>View parameters:</p>
      <ol>
        <li>left coordinate</li>
      </ol>
    */
    FitBoundingBoxVertical(PdfName.FitBV);

    public static ModeEnum valueOf(
      PdfName name
      )
    {
      if(name == null)
        return null;

      for(ModeEnum value : values())
      {
        if(value.getName().equals(name))
          return value;
      }
      throw new UnsupportedOperationException("Mode unknown: " + name);
    }

    private PdfName name;

    private ModeEnum(
      PdfName name
      )
    {this.name = name;}

    public PdfName getName(
      )
    {return name;}
  }
  // </classes>

  // <static>
  // <interface>
  // <public>
  /**
    Wraps a destination base object into a destination object.

    @param baseObject Destination base object.
    @param name Destination name.
    @return Destination object associated to the base object.
  */
  public static final Destination wrap(
    PdfDirectObject baseObject,
    PdfString name
    )
  {
    if(baseObject == null)
      return null;

    PdfArray dataObject = (PdfArray)File.resolve(baseObject);
    PdfDirectObject pageObject = dataObject.get(0);
    if(pageObject instanceof PdfReference)
      return new LocalDestination(baseObject, name);
    else if(pageObject instanceof PdfInteger)
      return new RemoteDestination(baseObject, name);
    else
      throw new IllegalArgumentException("'baseObject' parameter doesn't represent a valid destination object.");
  }
  // </public>
  // </interface>
  // </static>

  // <dynamic>
  // <constructors>
  /**
    Creates a new destination within the given document context.

    @param context Document context.
    @param pageObject Page reference. It may be either an actual page reference (PdfReference)
      or a page index (PdfInteger).
    @param mode Destination mode.
    @param viewParams View parameters. Their actual composition depends on the <code>mode</code> value
      (see ModeEnum for more info).
  */
  protected Destination(
    Document context,
    PdfDirectObject pageObject,
    ModeEnum mode,
    Double[] viewParams
    )
  {
    super(context, new PdfArray());
    PdfArray baseDataObject = getBaseDataObject();
    {
      baseDataObject.add(pageObject);
      baseDataObject.add(mode.getName());
      switch(mode)
      {
        case Fit:
          break;
        case FitBoundingBox:
          break;
        case FitBoundingBoxHorizontal:
          baseDataObject.add(PdfReal.get(viewParams[0]));
          break;
        case FitBoundingBoxVertical:
          baseDataObject.add(PdfReal.get(viewParams[0]));
          break;
        case FitHorizontal:
          baseDataObject.add(PdfReal.get(viewParams[0]));
          break;
        case FitRectangle:
          baseDataObject.add(PdfReal.get(viewParams[0]));
          baseDataObject.add(PdfReal.get(viewParams[1]));
          baseDataObject.add(PdfReal.get(viewParams[2]));
          baseDataObject.add(PdfReal.get(viewParams[3]));
          break;
        case FitVertical:
          baseDataObject.add(PdfReal.get(viewParams[0]));
          break;
        case XYZ:
          baseDataObject.add(PdfReal.get(viewParams[0]));
          baseDataObject.add(PdfReal.get(viewParams[1]));
          baseDataObject.add(PdfReal.get(viewParams[2]));
          break;
      }
    }
  }

  protected Destination(
    PdfDirectObject baseObject,
    PdfString name
    )
  {super(baseObject, name);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Destination clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Gets the destination mode.
  */
  public ModeEnum getMode(
    )
  {return ModeEnum.valueOf((PdfName)getBaseDataObject().get(1));}

  /**
    Gets the target page reference.
  */
  public abstract Object getPageRef(
    );
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}