using org.pdfclown.documents;
using org.pdfclown.documents.interaction.navigation.document;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to inspect the object names within a PDF document.</summary>
  */
  public class NamesParsingSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;

      // 2. Named objects extraction.
      Names names = document.Names;
      if(names == null)
      {Console.WriteLine("No names dictionary.");}
      else
      {
        Console.WriteLine("Names dictionary found (" + names.Container.Reference + ")");

        NamedDestinations namedDestinations = names.Destinations;
        if(namedDestinations == null)
        {Console.WriteLine("No named destinations.");}
        else
        {
          Console.WriteLine("Named destinations found (" + namedDestinations.Container.Reference + ")");

          // Parsing the named destinations...
          foreach(KeyValuePair<PdfString,Destination> namedDestination in namedDestinations)
          {
            PdfString key = namedDestination.Key;
            Destination value = namedDestination.Value;

            Console.WriteLine("Destination '" + key.Value + "' (" + value.Container.Reference + ")");

            Console.Write("    Target Page: number = ");
            object pageRef = value.PageRef;
            if(pageRef is Int32) // NOTE: numeric page refs are typical of remote destinations.
            {Console.WriteLine(((int)pageRef) + 1);}
            else // NOTE: explicit page refs are typical of local destinations.
            {
              Page page = (Page)pageRef;
              Console.WriteLine((page.Index + 1) + "; ID = " + ((PdfReference)page.BaseObject).Id);
            }
          }

          Console.WriteLine("Named destinations count = " + namedDestinations.Count);
        }
      }

      return true;
    }
  }
}