package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.NamedDestinations;
import org.pdfclown.documents.Names;
import org.pdfclown.documents.Pages;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.documents.interaction.navigation.document.LocalDestination;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfString;

/**
  This sample demonstrates <b>how to manipulate the named destinations</b> within a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.0
*/
public class NamedDestinationSample
  extends Sample
{
  @Override
  public boolean run(
    )
  {
    // 1. Opening the PDF file...
    File file;
    {
      String filePath = promptPdfFileChoice("Please select a PDF file");

      try
      {file = new File(filePath);}
      catch(Exception e)
      {throw new RuntimeException(filePath + " file access error.",e);}
    }
    Document document = file.getDocument();
    Pages pages = document.getPages();

    // 2. Inserting page destinations...
    Names names = document.getNames(); if(names == null){document.setNames(names = new Names(document));}
    NamedDestinations destinations = names.getDestinations(); if(destinations == null){names.setDestinations(destinations = new NamedDestinations(document));}
    destinations.put(new PdfString("First page"), new LocalDestination(pages.get(0)));
    if(pages.size() > 1)
    {
      destinations.put(new PdfString("Second page"), new LocalDestination(pages.get(1), Destination.ModeEnum.FitHorizontal, new Float[]{0f}));

      if(pages.size() > 2)
      {destinations.put(new PdfString("Third page"), new LocalDestination(pages.get(2), Destination.ModeEnum.XYZ, new Float[]{50f,null,null}));}
    }

    // (boilerplate metadata insertion -- ignore it)
    buildAccessories(document,"Named destinations","manipulating named destinations");

    // 3. Serialize the PDF file!
    serialize(file);
    
    return true;
  }
}