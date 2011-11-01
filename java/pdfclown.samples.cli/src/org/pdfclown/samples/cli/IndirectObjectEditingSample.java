package org.pdfclown.samples.cli;

import org.pdfclown.bytes.IBuffer;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfDataObject;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfStream;

/**
  This sample demonstrates <b>how to modify existing PDF indirect objects</b>.
  <p>As a practical case, we suppose to have to modify existing unicode font mappings.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 11/01/11
*/
public class IndirectObjectEditingSample
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

    // 2. Iterating through the indirect objects to discover existing unicode mapping (ToUnicode) streams to edit...
    /*
      NOTE: For the sake of simplicity, I assume that all font objects
      tipically reside in distinct indirect objects.
    */
    for(PdfIndirectObject indirectObject : file.getIndirectObjects())
    {
      /*
        NOTE: In order to get the unicode mapping stream we have to get
        its corresponding font object, which is a dictionary.
      */
      PdfDataObject dataObject = indirectObject.getDataObject();
      if(!(dataObject instanceof PdfDictionary) // Data object is NOT a dictionary.
        || !PdfName.Font.equals(((PdfDictionary)dataObject).get(PdfName.Type))) // Dictionary is NOT a font object.
        continue;

      // Get the indirect reference to the ToUnicode stream associated to the font object!
      PdfReference toUnicodeReference = (PdfReference)((PdfDictionary)dataObject).get(PdfName.ToUnicode);
      if(toUnicodeReference == null) // No ToUnicode stream.
        continue;

      /*
        NOTE: You can either:
          1) modify the existing data object contained within the indirect object;
        or
          2) create a new data object to replace the existing data object contained within the indirect object.
        For the sake of completeness, in this sample we apply both these approaches.

        NOTE: Data objects may be any object inheriting from the PdfDataObject class
        (for example: PdfName, PdfInteger, PdfTextString, PdfArray, PdfStream, PdfDictionary and so on...).
        Unicode mapping streams are... (you guess!) PdfStream objects.
      */
      if(toUnicodeReference.getObjectNumber() % 2 == 0) // NOTE: Arbitrary condition to force the use of both the approaches.
      {
        /*
          Approach 1: Modifying the existing data object.
        */
        // Get the existing data object from the corresponding indirect object!
        PdfStream toUnicodeStream = (PdfStream)toUnicodeReference.getDataObject();

        // Editing the data object...
        IBuffer streamBody = toUnicodeStream.getBody();
        streamBody.setLength(0); // Erases the stream content to prepare it for new content insertion.
        streamBody.append("... modified ..."); // Adds arbitrary contents (NOTE: this would NOT be done in a real ToUnicode stream! We are just testing the editing functionality...).
      }
      else
      {
        /*
          Approach 2: Creating a new data object.
        */
        // Create a new data object!
        PdfStream toUnicodeStream = new PdfStream();
        // Associate the new data object to the existing indirect object, replacing the old one!
        toUnicodeReference.setDataObject(toUnicodeStream);

        // Editing the data object...
        IBuffer streamBody = toUnicodeStream.getBody();
        streamBody.append("... created ..."); // Adds arbitrary contents (NOTE: this would NOT be done in a real ToUnicode stream! We are just testing the editing functionality...).
      }
    }

    // 3. Serialize the PDF file!
    serialize(file);

    return true;
  }
}