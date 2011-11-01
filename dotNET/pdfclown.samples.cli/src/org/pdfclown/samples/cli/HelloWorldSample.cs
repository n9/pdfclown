using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.files;

using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample is a minimalist introduction to the use of PDF Clown.</summary>
  */
  public class HelloWorldSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Instantiate a new PDF file!
      /* NOTE: a File object is the low-level (syntactic) representation of a PDF file. */
      File file = new File();

      // 2. Get its corresponding document!
      /* NOTE: a Document object is the high-level (semantic) representation of a PDF file. */
      Document document = file.Document;

      // 3. Insert the contents into the document!
      Populate(document);

      // 4. Serialize the PDF file!
      Serialize(file, false, "Hello world", "a simple 'hello world'");

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

      // 3. Inserting contents...
      // Set the font to use!
      composer.SetFont(
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
        NOTE: PrimitiveComposer's ShowText() method is the most basic way
        to add text to a page -- see BlockComposer for more advanced uses
        (horizontal and vertical alignment, hyphenation, etc.).
      */
      composer.ShowText(
        "Hello World!",
        new PointF(32,48)
        );

      // 4. Flush the contents into the page!
      composer.Flush();
    }
  }
}