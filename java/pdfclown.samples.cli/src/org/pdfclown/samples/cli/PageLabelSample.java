package org.pdfclown.samples.cli;

import java.util.Map;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.PageLabels;
import org.pdfclown.documents.interaction.navigation.page.PageLabel;
import org.pdfclown.documents.interaction.navigation.page.PageLabel.NumberStyleEnum;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to define, read and modify page labels</b>.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.2, 02/15/12
*/
public class PageLabelSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    String outputFilePath;
    {
      // 1. Opening the PDF file...
      File file;
      {
        String filePath = promptFileChoice("Please select a PDF file");
        try
        {file = new File(filePath);}
        catch(Exception e)
        {throw new RuntimeException(filePath + " file access error.",e);}
      }
      Document document = file.getDocument();

      int pageCount = document.getPages().size();
      PageLabels pageLabels = document.getPageLabels();
      if(pageLabels != null)
      {pageLabels.clear();}
      else
      {document.setPageLabels(pageLabels = new PageLabels(document));}

      /*
        NOTE: This sample applies labels to arbitrary page ranges: no sensible connection with their
        actual content has therefore to be expected.
      */
      pageLabels.put(0, new PageLabel(document, "Introduction ", NumberStyleEnum.UCaseRomanNumber, 5));
      if(pageCount > 3)
      {pageLabels.put(3, new PageLabel(document, NumberStyleEnum.UCaseLetter));}
      if(pageCount > 6)
      {pageLabels.put(6, new PageLabel(document, "Contents ", NumberStyleEnum.ArabicNumber, 0));}

      // 3. Serialize the PDF file!
      outputFilePath = serialize(file, "Page labelling", "labelling a document's pages");
    }

    {
      File file;
      try
      {file = new File(outputFilePath);}
      catch(Exception e)
      {throw new RuntimeException(outputFilePath + " file access error.",e);}

      for(Map.Entry<Integer,PageLabel> entry : file.getDocument().getPageLabels().entrySet())
      {
        System.out.println("Page label " + entry.getValue().getBaseObject());
        System.out.println("    Initial page: " + (entry.getKey() + 1));
        System.out.println("    Prefix: " + (entry.getValue().getPrefix()));
        System.out.println("    Number style: " + (entry.getValue().getNumberStyle()));
        System.out.println("    Number base: " + (entry.getValue().getNumberBase()));
      }
    }

    return true;
  }
}