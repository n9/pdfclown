using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to manipulate the named destinations within a PDF document.</summary>
  */
  public class NamedDestinationSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;
      Pages pages = document.Pages;

      // 2. Inserting page destinations...
      Names names = document.Names;
      if(names == null)
      {document.Names = names = new Names(document);}

      NamedDestinations destinations = names.Destinations;
      if(destinations == null)
      {names.Destinations = destinations = new NamedDestinations(document);}

      destinations[new PdfString("First page")] = new LocalDestination(pages[0]);
      if(pages.Count > 1)
      {
        destinations[new PdfString("Second page")] = new LocalDestination(pages[1], Destination.ModeEnum.FitHorizontal, new double?[]{0});

        if(pages.Count > 2)
        {destinations[new PdfString("Third page")] = new LocalDestination(pages[2], Destination.ModeEnum.XYZ, new double?[]{50,null,null});}
      }

      // 3. Serialize the PDF file!
      Serialize(file, true, "Named destinations", "manipulating named destinations");

      return true;
    }
  }
}