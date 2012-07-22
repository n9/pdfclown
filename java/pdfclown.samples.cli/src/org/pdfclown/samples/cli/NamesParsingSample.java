package org.pdfclown.samples.cli;

import java.util.Map;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.NamedDestinations;
import org.pdfclown.documents.Names;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfReference;

/**
  This sample demonstrates <b>how to inspect the object names</b> within a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.2, 02/04/12
*/
public class NamesParsingSample extends Sample
{
  @Override
  public boolean run()
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

    // 2. Named objects extraction.
    Names names = document.getNames();
    if(names == null)
    {System.out.println("\nNo names dictionary.");}
    else
    {
      System.out.println("\nNames dictionary found (" + names.getContainer().getReference() + ")");

      NamedDestinations namedDestinations = names.getDestinations();
      if(namedDestinations == null)
      {System.out.println("\nNo named destinations.");}
      else
      {
        System.out.println("\nNamed destinations found (" + namedDestinations.getContainer().getReference() + ")");

        // Parsing the named destinations...
        for(Map.Entry<String,Destination> namedDestination : namedDestinations.entrySet())
        {
          String key = namedDestination.getKey();
          Destination value = namedDestination.getValue();

          System.out.println("  Destination '" + key + "' (" + value.getContainer().getReference() + ")");

          System.out.print("    Target Page: number = ");
          Object pageRef = value.getPage();
          if(pageRef instanceof Integer) // NOTE: numeric page refs are typical of remote destinations.
          {System.out.println(((Integer)pageRef) + 1);}
          else // NOTE: explicit page refs are typical of local destinations.
          {
            Page page = (Page)pageRef;
            System.out.println((page.getIndex() + 1) + "; ID = " + ((PdfReference)page.getBaseObject()).getId());
          }
        }

        System.out.println("Named destinations count = " + namedDestinations.size());
      }
    }

    return true;
  }
}
