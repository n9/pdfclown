package org.pdfclown.samples.cli;

import java.awt.geom.Point2D;
import java.awt.geom.Rectangle2D;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.colorSpaces.DeviceRGBColor;
import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.BlockComposer;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.fonts.Font;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to obtain the actual area occupied by text</b>
  shown in a PDF page.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 04/16/11
*/
public class TextFrameSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    // 1. Instantiate a new PDF file!
    File file = new File();
    Document document = file.getDocument();

    // 2. Insert the contents into the document!
    populate(document);

    // (boilerplate metadata insertion -- ignore it)
    buildAccessories(document,"Text frame","getting the actual bounding box of text shown");

    // 3. Serialize the PDF file!
    serialize(file,false);

    return true;
  }

  /**
    Populates a PDF file with contents.
  */
  private void populate(
    Document document
    )
  {
    // 1. Add the page to the document!
    Page page = new Page(document); // Instantiates the page inside the document context.
    document.getPages().add(page); // Puts the page in the pages collection.

    // 2. Create a content composer for the page!
    PrimitiveComposer composer = new PrimitiveComposer(page);

    BlockComposer blockComposer = new BlockComposer(composer);
    blockComposer.begin(new Rectangle2D.Double(300,400,200,100),AlignmentXEnum.Left,AlignmentYEnum.Middle);
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Times,
          false,
          true
          ),
        12
        );
    }
    catch(Exception e)
    {}
    composer.setFillColor(new DeviceRGBColor(115f/255,164f/255,232f/255));
    blockComposer.showText("showText() methods return the actual bounding box of the shown text, allowing to precisely determine its location on the page.");
    blockComposer.end();

    composer.setStrokeColor(new DeviceRGBColor(115f/255,164f/255,232f/255));

    // 3. Inserting contents...
    // Set the font to use!
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        72
        );
    }
    catch(Exception e)
    {}
    composer.drawPolygon(
      composer.showText(
        "Text frame",
        new Point2D.Double(150,360),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        45
        ).getPoints()
      );
    composer.stroke();

    try
    {
      composer.setFont(
        Font.get(
          document,
          getInputPath() + java.io.File.separator + "fonts" + java.io.File.separator + "Ruritania-Outline.ttf"
          ),
        102
        );
    }
    catch(Exception e)
    {}
    composer.drawPolygon(
      composer.showText(
        "Text frame",
        new Point2D.Double(300,600),
        AlignmentXEnum.Center,
        AlignmentYEnum.Middle,
        -25
        ).getPoints()
      );
    composer.stroke();

    // 4. Flush the contents into the page!
    composer.flush();
  }
}