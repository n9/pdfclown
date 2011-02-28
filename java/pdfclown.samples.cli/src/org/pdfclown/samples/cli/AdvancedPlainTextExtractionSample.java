package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.files.File;
import org.pdfclown.tools.TextExtractor;

/**
  This sample demonstrates <b>how to retrieve text content</b> from a PDF document.
  If you need further information (such as text style and position), please
  see {@link AdvancedTextExtractionSample}.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.0
*/
public class AdvancedPlainTextExtractionSample
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

    // 2. Plain text extraction from the document pages.
    TextExtractor extractor = new TextExtractor();
    for(Page page : document.getPages())
    {
      if(!promptNextPage(page, false))
        return false;

      // Extract plain text from the current page!
      System.out.println(extractor.extractPlain(page));
    }

    return true;
  }
}
