package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.interaction.forms.Field;
import org.pdfclown.documents.interaction.forms.Form;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to fill AcroForm fields</b> of a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.1, 07/05/11
*/
public class AcroFormFillingSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    String filePath = promptPdfFileChoice("Please select a PDF file");

    // 1. Open the PDF file!
    File file;
    try
    {file = new File(filePath);}
    catch(Exception e)
    {throw new RuntimeException(filePath + " file access error.",e);}

    Document document = file.getDocument();

    // 2. Get the acroform!
    Form form = document.getForm();
    if(form == null)
    {System.out.println("\nNo acroform available (AcroForm dictionary not found).");}
    else
    {
      System.out.println("\nPlease insert a value for each field listed below (or type 'quit' to end this sample).\n");

      // 3. Filling the acroform fields...
      for(Field field : form.getFields().values())
      {
        System.out.println("* " + field.getClass().getSimpleName() + " '" + field.getFullName() + "' (" + field.getBaseObject() + "): ");
        System.out.println("    Current Value:" + field.getValue());
        String newValue = promptChoice("    New Value:");
        if(newValue != null && newValue.equals("quit"))
          break;

        field.setValue(newValue);
      }
    }

    // 4. Serialize the PDF file!
    serialize(file,false);

    return true;
  }
}
