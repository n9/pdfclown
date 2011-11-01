using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.files;

using System;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample generates a series of PDF pages from the default page formats available,
    varying both in size and orientation.</summary>
  */
  public class PageFormatSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. PDF file instantiation.
      File file = new File();
      Document document = file.Document;

      // 2. Populate the document!
      Populate(document);

      // 3. Serialize the PDF file!
      Serialize(file, false, "Page Format", "page formats");

      return true;
    }

    private void Populate(
      Document document
      )
    {
      StandardType1Font bodyFont = new StandardType1Font(
        document,
        StandardType1Font.FamilyEnum.Courier,
        true,
        false
        );

      Pages pages = document.Pages;
      foreach(
        PageFormat.SizeEnum pageFormat
          in (PageFormat.SizeEnum[])Enum.GetValues(typeof(PageFormat.SizeEnum))
        )
      {
        foreach(
          PageFormat.OrientationEnum pageOrientation
            in (PageFormat.OrientationEnum[])Enum.GetValues(typeof(PageFormat.OrientationEnum))
          )
        {
          // Add a page to the document!
          Page page = new Page(document); // Instantiates the page inside the document context.
          pages.Add(page); // Puts the page in the pages collection.

          // Set the page size!
          page.Size = PageFormat.GetSize(
            pageFormat,
            pageOrientation
            );

          // Drawing the text label on the page...
          SizeF pageSize = page.Size;
          PrimitiveComposer composer = new PrimitiveComposer(page);
          composer.SetFont(bodyFont,32);
          composer.ShowText(
            pageFormat + " (" + pageOrientation + ")", // Text.
            new PointF(
              pageSize.Width / 2,
              pageSize.Height / 2
              ), // Location: page center.
            AlignmentXEnum.Center, // Place the text on horizontal center of the location.
            AlignmentYEnum.Middle, // Place the text on vertical middle of the location.
            45 // Rotate the text 45 degrees counterclockwise.
            );
          composer.Flush();
        }
      }
    }
  }
}