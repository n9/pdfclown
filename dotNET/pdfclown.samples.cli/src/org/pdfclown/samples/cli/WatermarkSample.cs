using org.pdfclown.documents;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.composition;
using fonts = org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.xObjects;
using files = org.pdfclown.files;
using org.pdfclown.tools;

using System;
using System.Drawing;
using System.IO;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to insert a watermark text on an existing PDF document.</summary>
    <remarks>
      <para>This implementation uses a Form XObject [PDF:1.6:4.9] to conveniently achieve a consistent
      page background. Form XObjects provide context independence encapsulating their contents (and
      resources) in a single stream: such an approach allows content reuse.</para>
      <para>The watermark is seamlessly inserted under each page content using the PageStamper
      class.</para>
    </remarks>
  */
  public class WatermarkSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Open the PDF file!
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      files::File file = new files::File(filePath);
      Document document = file.Document;

      // 2. Create a watermark!
      FormXObject watermark = CreateWatermark(document);

      // 3. Apply the watermark to the pages of the document!
      ApplyWatermark(watermark);

      // 4. Serialize the PDF file!
      Serialize(file, true, "Watermark", "how to place some content behind existing pages");

      return true;
    }

    private void ApplyWatermark(
      FormXObject watermark
      )
    {
      // 1. Instantiate the stamper!
      /* NOTE: The PageStamper is optimized for dealing with pages. */
      PageStamper stamper = new PageStamper();

      // 2. Inserting the watermark into each page of the document...
      foreach(Page page in watermark.Document.Pages)
      {
        // 2.1. Associate the page to the stamper!
        stamper.Page = page;

        // 2.2. Stamping the watermark on the background...
        // Get the background 'layer' of the page!
        PrimitiveComposer background = stamper.Background;
        // Show the watermark into the page background!
        background.ShowXObject(watermark);

        // 2.3. End the stamping!
        stamper.Flush();
      }
    }

    private FormXObject CreateWatermark(
      Document document
      )
    {
      // 1. Create a new external form object to represent the watermark!
      FormXObject watermark = new FormXObject(document);
      // Size.
      SizeF size = document.GetSize();
      watermark.Size = size;

      // 2. Inserting the contents of the watermark...
      // 2.1. Create a content composer for the watermark!
      PrimitiveComposer composer = new PrimitiveComposer(watermark);
      // 2.2. Inserting the contents...
      // Set the font to use!
      composer.SetFont(
        new fonts::StandardType1Font(
          document,
          fonts::StandardType1Font.FamilyEnum.Times,
          true,
          false
          ),
        120
        );
      // Set the color to fill the text characters!
      composer.SetFillColor(
        new DeviceRGBColor(115f/255, 164f/255, 232f/255)
        );
      // Show the text!
      composer.ShowText(
        "PDFClown", // Text to show.
        new PointF(size.Width/2f,size.Height/2f), // Anchor location: page center.
        AlignmentXEnum.Center, // Horizontal placement (relative to the anchor): center.
        AlignmentYEnum.Middle, // Vertical placement (relative to the anchor): middle.
        50 // Rotation: 50-degree-counterclockwise.
        );
      // 2.3. Flush the contents into the watermark!
      composer.Flush();

      return watermark;
    }
  }
}