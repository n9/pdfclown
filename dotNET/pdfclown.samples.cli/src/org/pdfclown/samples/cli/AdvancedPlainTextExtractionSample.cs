using org.pdfclown.documents;
using org.pdfclown.files;
using org.pdfclown.tools;

using System;
using System.Collections.Generic;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to retrieve text content from a PDF document.</summary>
    <remarks>If you need further information (such as text style and position),
    please see <see cref="AdvancedTextExtractionSample"/>.</remarks>
  */
  public class AdvancedPlainTextExtractionSample
    : Sample
  {
    public override bool Run(
      )
    {
      string filePath = PromptPdfFileChoice("Please select a PDF file");

      // 1. Open the PDF file!
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Plain text extraction from the document pages.
      TextExtractor extractor = new TextExtractor();
      foreach(Page page in document.Pages)
      {
          if(!PromptNextPage(page, false))
          return false;

        // Extract plain text from the current page!
        Console.WriteLine(extractor.ExtractPlain(page));
      }

      return true;
    }
  }
}