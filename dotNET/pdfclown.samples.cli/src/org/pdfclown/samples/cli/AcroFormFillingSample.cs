using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.entities;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.xObjects;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.documents.interaction.forms;
using org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to fill AcroForm fields of a PDF document.</summary>
  */
  public class AcroFormFillingSample
    : Sample
  {
    public override bool Run(
      )
    {
      string filePath = PromptPdfFileChoice("Please select a PDF file");

      // 1. Open the PDF file!
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Get the acroform!
      Form form = document.Form;
      if(form == null)
      {Console.WriteLine("\nNo acroform available (AcroForm dictionary not found).");}
      else
      {
        Console.WriteLine("\nPlease insert a value for each field listed below (or type 'quit' to end this sample).\n");

        // 3. Filling the acroform fields...
        foreach(Field field in form.Fields.Values)
        {
          Console.WriteLine("* " + field.GetType().Name + " '" + field.FullName + "' (" + field.BaseObject + "): ");
          Console.WriteLine("    Current Value:" + field.Value);
          string newValue = PromptChoice("    New Value:");
          if(newValue != null && newValue.Equals("quit"))
            break;

          field.Value = newValue;
        }
      }

      // 4. Serialize the PDF file!
      Serialize(file,false);

      return true;
    }
  }
}