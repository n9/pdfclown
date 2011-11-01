using bytes = org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to modify existing PDF indirect objects.</summary>
    <remarks>As a practical case, we suppose to have to modify existing unicode font mappings.</remarks>
  */
  public class IndirectObjectEditingSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);

      // 2. Iterating through the indirect objects to discover existing unicode mapping (ToUnicode) streams to edit...
      /*
        NOTE: For the sake of simplicity, I assume that all font objects
        tipically reside in distinct indirect objects.
      */
      foreach(PdfIndirectObject indirectObject in file.IndirectObjects)
      {
        /*
          NOTE: In order to get the unicode mapping stream we have to get
          its corresponding font object, which is a dictionary.
        */
        PdfDataObject dataObject = indirectObject.DataObject;
        if(!(dataObject is PdfDictionary) // Data object is NOT a dictionary.
          || !PdfName.Font.Equals(((PdfDictionary)dataObject)[PdfName.Type])) // Dictionary is NOT a font object.
          continue;

        // Get the indirect reference to the ToUnicode stream associated to the font object!
        PdfReference toUnicodeReference = (PdfReference)((PdfDictionary)dataObject)[PdfName.ToUnicode];
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
        if(toUnicodeReference.ObjectNumber % 2 == 0) // NOTE: Arbitrary condition to force the use of both the approaches.
        {
          /*
            Approach 1: Modifying the existing data object.
          */
          // Get the existing data object from the corresponding indirect object!
          PdfStream toUnicodeStream = (PdfStream)toUnicodeReference.DataObject;

          // Editing the data object...
          bytes::IBuffer streamBody = toUnicodeStream.Body;
          streamBody.SetLength(0); // Erases the stream content to prepare it for new content insertion.
          streamBody.Append("... modified ..."); // Adds arbitrary contents (NOTE: this would NOT be done in a real ToUnicode stream! We are just testing the editing functionality...).
        }
        else
        {
          /*
            Approach 2: Creating a new data object.
          */
          // Create a new data object!
          PdfStream toUnicodeStream = new PdfStream();
          // Associate the new data object to the existing indirect object, replacing the old one!
          toUnicodeReference.DataObject = toUnicodeStream;

          // Editing the data object...
          bytes::IBuffer streamBody = toUnicodeStream.Body;
          streamBody.Append("... created ..."); // Adds arbitrary contents (NOTE: this would NOT be done in a real ToUnicode stream! We are just testing the editing functionality...).
        }
      }

      // 3. Serialize the PDF file!
      Serialize(file);

      return true;
    }
  }
}