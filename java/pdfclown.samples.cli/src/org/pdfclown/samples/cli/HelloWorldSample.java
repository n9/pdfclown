package org.pdfclown.samples.cli;

import java.awt.geom.Point2D;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.files.File;

/**
  This sample is a <b>minimalist introduction to the use of PDF Clown</b>.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.2, 11/30/12
*/
public class HelloWorldSample
  extends Sample
{
  @Override
  public void run(
    )
  {
    // 1. Instantiate a new PDF file!
    /* NOTE: a File object is the low-level (syntactic) representation of a PDF file. */
    File file = new File();

    // 2. Get its corresponding document!
    /* NOTE: a Document object is the high-level (semantic) representation of a PDF file. */
    Document document = file.getDocument();

    // 3. Insert the contents into the document!
    populate(document);

    // 4. Serialize the PDF file!
    serialize(file, "Hello world", "a simple 'hello world'", "Hello world");
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

    // 3. Inserting contents...
    // Set the font to use!
    composer.setFont(
      new StandardType1Font(
        document,
        StandardType1Font.FamilyEnum.Courier,
        true,
        false
        ),
      32
      );
    // Show the text onto the page!
    /*
      NOTE: PrimitiveComposer's showText() method is the most basic way to add text to a page -- see
      BlockComposer for more advanced uses (horizontal and vertical alignment, hyphenation, etc.).
    */
    composer.showText(
      "Hello World!",
      new Point2D.Double(32,48)
      );

    // 4. Flush the contents into the page!
    composer.flush();
  }
}