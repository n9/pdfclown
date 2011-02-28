package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.NamedDestinations;
import org.pdfclown.documents.Names;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfString;

import java.util.Map;

/**
  This sample demonstrates <b>how to inspect the object names</b> within a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.0
*/
public class NamesParsingSample extends Sample
{
  @Override
  public boolean run()
  {
    String filePath = promptPdfFileChoice("Please select a PDF file");

    // 1. Open the PDF file!
    File file;
    try
    {file = new File(filePath);}
    catch(Exception e)
    {throw new RuntimeException(filePath + " file access error.",e);}

    Document document = file.getDocument();

    // 2. Named objects extraction.
    Names names = document.getNames();
    if(names == null)
    {System.out.println("No names dictionary.");}
    else
    {
      System.out.println("Names dictionary found (" + names.getContainer().getReference() + ")");

      NamedDestinations namedDestinations = names.getDestinations();
      if(namedDestinations == null)
      {System.out.println("No named destinations.");}
      else
      {
        System.out.println("Named destinations found (" + namedDestinations.getContainer().getReference() + ")");

        // Parsing the named destinations...
        for(Map.Entry<PdfString,Destination> namedDestination : namedDestinations.entrySet())
        {
          PdfString key = namedDestination.getKey();
          Destination value = namedDestination.getValue();

          System.out.println("Destination '" + key.getValue() + "' (" + value.getContainer().getReference() + ")");

          System.out.print("    Target Page: number = ");
          Object pageRef = value.getPageRef();
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
