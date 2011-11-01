package org.pdfclown.samples.cli;

import java.awt.geom.Dimension2D;
import java.awt.geom.Point2D;
import java.util.EnumSet;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.PageFormat;
import org.pdfclown.documents.Pages;
import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.files.File;

/**
  This sample generates a series of PDF pages from the <b>default page formats available</b>,
  <i>varying both in size and orientation</i>.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 11/01/11
*/
public class PageFormatSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    // 1. PDF file instantiation.
    File file = new File();
    Document document = file.getDocument();

    // 2. Populate the document!
    populate(document);

    // 3. Serialize the PDF file!
    serialize(file, false, "Page Format", "page formats");

    return true;
  }

  private void populate(
    Document document
    )
  {
    StandardType1Font bodyFont = new StandardType1Font(
      document,
      StandardType1Font.FamilyEnum.Courier,
      true,
      false
      );

    Pages pages = document.getPages();
    for(PageFormat.SizeEnum pageFormat
      : EnumSet.allOf(PageFormat.SizeEnum.class))
    {
      for(PageFormat.OrientationEnum pageOrientation
        : EnumSet.allOf(PageFormat.OrientationEnum.class))
      {
        // Add a page to the document!
        Page page = new Page(document); // Instantiates the page inside the document context.
        pages.add(page); // Puts the page in the pages collection.

        // Set the page size!
        page.setSize(
          PageFormat.getSize(
            pageFormat,
            pageOrientation
            )
          );

        // Drawing the text label on the page...
        Dimension2D pageSize = page.getSize();
        PrimitiveComposer composer = new PrimitiveComposer(page);
        composer.setFont(bodyFont,32);
        composer.showText(
          pageFormat + " (" + pageOrientation + ")",  // Text.
          new Point2D.Double(
            pageSize.getWidth() / 2,
            pageSize.getHeight() / 2
            ), // Location: page center.
          AlignmentXEnum.Center, // Place the text on horizontal center of the location.
          AlignmentYEnum.Middle, // Place the text on vertical middle of the location.
          45 // Rotate the text 45 degrees counterclockwise.
          );
        composer.flush();
      }
    }
  }
}