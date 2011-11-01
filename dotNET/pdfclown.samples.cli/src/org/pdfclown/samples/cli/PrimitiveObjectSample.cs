using org.pdfclown.documents;
using org.pdfclown.files;
using org.pdfclown.objects;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to perform advanced editing over a PDF document
    structure accessing primitive objects.</summary>
    <remarks>In this case, it adds an 'open action' to the document so that it opens
    on page 2 magnified just enough to fit the height of the page within the window.
    By the way, note that since version 0.0.7 go-to actions are natively supported
    by PDF Clown (you don't need to work at the low level shown here!).</remarks>
  */
  public class PrimitiveObjectSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);

      // 2. Modifying the document...
      Document document = file.Document;
      {
        // Create the action dictionary!
        PdfDictionary action = new PdfDictionary();
        // Define the action type (in this case: go-to)!
        action[new PdfName("S")] = new PdfName("GoTo");
        // Defining the action destination...
        {
          // Create the destination array!
          PdfArray destination = new PdfArray();
          // Define the 2nd page as the destination target!
          destination.Add(document.Pages[1].BaseObject);
          // Define the location of the document window on the page (fit vertically)!
          destination.Add(new PdfName("FitV"));
          // Define window left-edge horizontal coordinate!
          destination.Add(new PdfInteger(-32768));
          // Associate the destination to the action!
          action[new PdfName("D")] = destination;
        }
        // Associate the action to the document!
        document.BaseDataObject[new PdfName("OpenAction")] = file.Register(action);  // Adds the action to the file, returning its reference.
      }

      // 3. Serialize the PDF file!
      Serialize(file, true, "Primitive objects", "manipulating a document at primitive object level");

      return true;
    }
  }
}