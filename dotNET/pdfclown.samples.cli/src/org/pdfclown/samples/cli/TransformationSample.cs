using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using entities = org.pdfclown.documents.contents.entities;
using org.pdfclown.documents.contents.fonts;
using files = org.pdfclown.files;

using System.Drawing;
using System.IO;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to spacially manipulate an image object within a PDF page.</summary>
  */
  public class TransformationSample
    : Sample
  {
    private static readonly float Margin = 36;

    public override bool Run(
      )
    {
      // 1. PDF file instantiation.
      files::File file = new files::File();
      Document document = file.Document;

      // 2. Content creation.
      Populate(document);

      // 3. Serialize the PDF file!
      Serialize(file, false, "Transformation", "graphics object transformation");

      return true;
    }

    /**
      <summary>Populates a PDF file with contents.</summary>
    */
    private void Populate(
      Document document
      )
    {
      Page page = new Page(document);
      document.Pages.Add(page);
      SizeF pageSize = page.Size;

      PrimitiveComposer composer = new PrimitiveComposer(page);
      {
        BlockComposer blockComposer = new BlockComposer(composer);
        blockComposer.Hyphenation = true;
        blockComposer.Begin(
          new RectangleF(
            Margin,
            Margin,
            (float)pageSize.Width - Margin * 2,
            (float)pageSize.Height - Margin * 2
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
        composer.SetFont(bodyFont,32);
        blockComposer.ShowText("Transformation sample"); blockComposer.ShowBreak();
        composer.SetFont(bodyFont,16);
        blockComposer.ShowText("Showing the GNU logo placed on the page center, rotated by 25 degrees clockwise.");
        blockComposer.End();
      }
      // Showing the 'GNU' image...
      {
        // Instantiate a jpeg image object!
        entities::Image image = entities::Image.Get(
          InputPath + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "gnu.jpg"
          ); // Abstract image (entity).
        // Show the image!
        composer.ShowXObject(
          image.ToXObject(document),
          new PointF(
            (float)pageSize.Width / 2,
            (float)pageSize.Height / 2
            ),
          new SizeF(0,0),
          AlignmentXEnum.Center,
          AlignmentYEnum.Middle,
          -25
          );
      }
      composer.Flush();
    }
  }
}