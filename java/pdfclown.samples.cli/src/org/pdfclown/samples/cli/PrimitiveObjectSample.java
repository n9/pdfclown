package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;

/**
  This sample demonstrates <b>how to perform advanced editing over a PDF document
  structure accessing primitive objects</b>.
  <p>In this case, it adds an 'open action' to the document so that it opens
  on page 2 magnified just enough to fit the height of the page within the window.
  By the way, note that since version 0.0.7 go-to actions are natively supported
  by PDF Clown (you don't need to work at the low level shown here!).</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.1, 11/01/11
*/
public class PrimitiveObjectSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    // 1. Opening the PDF file...
    File file;
    {
      String filePath = promptPdfFileChoice("Please select a PDF file");
      try
      {file = new File(filePath);}
      catch(Exception e)
      {throw new RuntimeException(filePath + " file access error.",e);}
    }

    // 2. Modifying the document...
    Document document = file.getDocument();
    {
      // Create the action dictionary!
      PdfDictionary action = new PdfDictionary();
      // Define the action type (in this case: go-to)!
      action.put(new PdfName("S"),new PdfName("GoTo"));
      // Defining the action destination...
      {
        // Create the destination array!
        PdfArray destination = new PdfArray();
        // Define the 2nd page as the destination target!
        destination.add(document.getPages().get(1).getBaseObject());
        // Define the location of the document window on the page (fit vertically)!
        destination.add(new PdfName("FitV"));
        // Define the window's left-edge horizontal coordinate!
        destination.add(new PdfInteger(-32768));
        // Associate the destination to the action!
        action.put(new PdfName("D"),destination);
      }
      // Associate the action to the document!
      document.getBaseDataObject().put(
        new PdfName("OpenAction"),
        file.register(action) // Adds the action to the file, returning its reference.
        );
    }

    // 3. Serialize the PDF file!
    serialize(file, true, "Primitive objects", "manipulating a document at primitive object level");

    return true;
  }
}