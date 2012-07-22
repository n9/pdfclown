using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.documents.interaction.navigation.page;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to define, read and modify page labels.</summary>
  */
  public class PageLabelSample
    : Sample
  {
    public override bool Run(
      )
    {
      string outputFilePath;
      {
        // 1. Opening the PDF file...
        string filePath = PromptFileChoice("Please select a PDF file");
        File file = new File(filePath);
        Document document = file.Document;

        int pageCount = document.Pages.Count;
        PageLabels pageLabels = document.PageLabels;
        if(pageLabels != null)
        {pageLabels.Clear();}
        else
        {document.PageLabels = pageLabels = new PageLabels(document);}

        /*
          NOTE: This sample applies labels to arbitrary page ranges: no sensible connection with their
          actual content has therefore to be expected.
        */
        pageLabels[0] = new PageLabel(document, "Introduction ", PageLabel.NumberStyleEnum.UCaseRomanNumber, 5);
        if(pageCount > 3)
        {pageLabels[3] = new PageLabel(document, PageLabel.NumberStyleEnum.UCaseLetter);}
        if(pageCount > 6)
        {pageLabels[6] = new PageLabel(document, "Contents ", PageLabel.NumberStyleEnum.ArabicNumber, 0);}

        // 3. Serialize the PDF file!
        outputFilePath = Serialize(file, "Page labelling", "labelling a document's pages");
      }

      {
        File file = new File(outputFilePath);
        foreach(KeyValuePair<int,PageLabel> entry in file.Document.PageLabels)
        {
          Console.WriteLine("Page label " + entry.Value.BaseObject);
          Console.WriteLine("    Initial page: " + (entry.Key + 1));
          Console.WriteLine("    Prefix: " + (entry.Value.Prefix));
          Console.WriteLine("    Number style: " + (entry.Value.NumberStyle));
          Console.WriteLine("    Number base: " + (entry.Value.NumberBase));
        }
      }

      return true;
    }
  }
}