package org.pdfclown.samples.cli;

import java.awt.Dimension;
import java.awt.geom.Dimension2D;
import java.awt.geom.Point2D;
import java.awt.geom.Rectangle2D;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.BlockComposer;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.entities.Image;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to spacially manipulate an image object</b> within a PDF page.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 11/01/11
*/
public class TransformationSample
  extends Sample
{
  private static final float Margin = 36;

  @Override
  public boolean run(
    )
  {
    // 1. PDF file instantiation.
    File file = new File();
    Document document = file.getDocument();

    // 2. Content creation.
    populate(document);

    // 3. Serialize the PDF file!
    serialize(file, false, "Transformation", "graphics object transformation");

    return true;
  }

  /**
    Populates a PDF file with contents.
  */
  private void populate(
    Document document
    )
  {
    Page page = new Page(document);
    document.getPages().add(page);
    Dimension2D pageSize = page.getSize();

    PrimitiveComposer composer = new PrimitiveComposer(page);
    {
      BlockComposer blockComposer = new BlockComposer(composer);
      blockComposer.setHyphenation(true);
      blockComposer.begin(
        new Rectangle2D.Double(
          Margin,
          Margin,
          (float)pageSize.getWidth() - Margin * 2,
          (float)pageSize.getHeight() - Margin * 2
          ),
        AlignmentXEnum.Justify,
        AlignmentYEnum.Top
        );
      StandardType1Font bodyFont = new StandardType1Font(
        document,
        StandardType1Font.FamilyEnum.Courier,
        true,
        false
        );
      composer.setFont(bodyFont,32);
      blockComposer.showText("Transformation sample"); blockComposer.showBreak();
      composer.setFont(bodyFont,16);
      blockComposer.showText("Showing the GNU logo placed on the page center, rotated by 25 degrees clockwise.");
      blockComposer.end();
    }
    // Showing the 'GNU' image...
    {
      // Instantiate a jpeg image object!
      Image image = Image.get(
        getInputPath() + java.io.File.separator + "images" + java.io.File.separator + "gnu.jpg"
        ); // Abstract image (entity).
      // Show the image!
      composer.showXObject(
        image.toXObject(document),
        new Point2D.Double(
          (float)pageSize.getWidth() / 2,
          (float)pageSize.getHeight() / 2
          ),
        new Dimension(0,0),
        AlignmentXEnum.Center,
        AlignmentYEnum.Middle,
        -25
        );
    }
    composer.flush();
  }
}