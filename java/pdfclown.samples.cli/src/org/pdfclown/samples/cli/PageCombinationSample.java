package org.pdfclown.samples.cli;

import java.awt.geom.Dimension2D;
import java.awt.geom.Point2D;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.PageFormat;
import org.pdfclown.documents.PageFormat.OrientationEnum;
import org.pdfclown.documents.PageFormat.SizeEnum;
import org.pdfclown.documents.Pages;
import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.xObjects.XObject;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to combine multiple pages into single bigger pages</b> (for example
  two A4 modules into one A3 module) using form XObjects [PDF:1.6:4.9].
  <p>Form XObjects are a convenient way to represent contents multiple times on multiple pages as
  templates.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.2
  @version 0.1.2, 01/02/12
*/
public class PageCombinationSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    // 1. Instantiate the source PDF file!
    File sourceFile;
    {
      String filePath = promptPdfFileChoice("Please select a PDF file to use as source");
      try
      {sourceFile = new File(filePath);}
      catch(Exception e)
      {throw new RuntimeException(filePath + " file access error.",e);}
    }

    // 2. Instantiate the target PDF file!
    File file = new File();

    // 3. Source page combination into target file.
    Document document = file.getDocument();
    Pages pages = document.getPages();
    PrimitiveComposer composer = null;
    int pageIndex = -1;
    Dimension2D targetPageSize = PageFormat.getSize(SizeEnum.A4);
    for(Page sourcePage : sourceFile.getDocument().getPages())
    {
      pageIndex++;
      int pageMod = pageIndex % 2;

      // Convert the source page into a form inside the target document!
      XObject form = sourcePage.toXObject(document);

      if(pageMod == 0)
      {
        if(composer != null)
        {composer.flush();}

        // Add a page to the target document!
        Page page = new Page(
          document,
          PageFormat.getSize(SizeEnum.A3, OrientationEnum.Landscape)
          ); // Instantiates the page inside the document context.
        pages.add(page); // Puts the page in the pages collection.
        // Create a composer for the target content stream!
        composer = new PrimitiveComposer(page);
      }

      // Add the form to the target page!
      composer.showXObject(
        form,
        new Point2D.Double(targetPageSize.getWidth() * pageMod, 0),
        targetPageSize,
        AlignmentXEnum.Left,
        AlignmentYEnum.Top,
        0
        );
    }
    composer.flush();

    // 4. Serialize the PDF file!
    serialize(file, false, "Page combination", "combining multiple pages into single bigger ones");

    return true;
  }
}