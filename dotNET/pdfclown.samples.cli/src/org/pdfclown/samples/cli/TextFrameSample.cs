using org.pdfclown.documents;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.composition;
using fonts = org.pdfclown.documents.contents.fonts;
using org.pdfclown.files;

using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to obtain the actual area occupied by text shown in a PDF page.</summary>
  */
  public class TextFrameSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Instantiate a new PDF file!
      File file = new File();
      Document document = file.Document;

      // 2. Insert the contents into the document!
      Populate(document);

      // 3. Serialize the PDF file!
      Serialize(file, false, "Text frame", "getting the actual bounding box of text shown");

      return true;
    }

    /**
      <summary>Populates a PDF file with contents.</summary>
    */
    private void Populate(
      Document document
      )
    {
      // 1. Add the page to the document!
      Page page = new Page(document); // Instantiates the page inside the document context.
      document.Pages.Add(page); // Puts the page in the pages collection.

      // 2. Create a content composer for the page!
      PrimitiveComposer composer = new PrimitiveComposer(page);

      BlockComposer blockComposer = new BlockComposer(composer);
      blockComposer.Begin(new RectangleF(300, 400, 200, 100), AlignmentXEnum.Left, AlignmentYEnum.Middle);
      composer.SetFont(
        new fonts::StandardType1Font(
          document,
          fonts::StandardType1Font.FamilyEnum.Times,
          false,
          true
          ),
        12
        );
      composer.SetFillColor(new DeviceRGBColor(115d/255, 164d/255, 232d/255));
      blockComposer.ShowText("PrimitiveComposer.ShowText(...) methods return the actual bounding box of the text shown, allowing to precisely determine its location on the page.");
      blockComposer.End();

      composer.SetStrokeColor(new DeviceRGBColor(115d/255, 164d/255, 232d/255));

      // 3. Inserting contents...
      // Set the font to use!
      composer.SetFont(
        new fonts::StandardType1Font(
          document,
          fonts::StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        72
        );
      composer.DrawPolygon(
        composer.ShowText(
          "Text frame",
          new PointF(150, 360),
          AlignmentXEnum.Left,
          AlignmentYEnum.Middle,
          45
          ).Points
        );
      composer.Stroke();

      composer.SetFont(
        fonts::Font.Get(
          document,
          InputPath + System.IO.Path.DirectorySeparatorChar + "fonts" + System.IO.Path.DirectorySeparatorChar + "Ruritania-Outline.ttf"
          ),
        102
        );
      composer.DrawPolygon(
        composer.ShowText(
          "Text frame",
          new PointF(300, 600),
          AlignmentXEnum.Center,
          AlignmentYEnum.Middle,
          -25
          ).Points
        );
      composer.Stroke();

      // 4. Flush the contents into the page!
      composer.Flush();
    }
  }
}