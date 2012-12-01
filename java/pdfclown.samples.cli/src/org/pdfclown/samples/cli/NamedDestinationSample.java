package org.pdfclown.samples.cli;

import java.awt.geom.Point2D;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.NamedDestinations;
import org.pdfclown.documents.Pages;
import org.pdfclown.documents.interaction.navigation.document.Destination;
import org.pdfclown.documents.interaction.navigation.document.LocalDestination;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfString;

/**
  This sample demonstrates <b>how to manipulate the named destinations</b> within a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.8
  @version 0.1.2, 11/30/12
*/
public class NamedDestinationSample
  extends Sample
{
  @Override
  public void run(
    )
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
    Pages pages = document.getPages();

    // 2. Inserting page destinations...
    NamedDestinations destinations = document.getNames().getDestinations();
    destinations.put(new PdfString("d31e1142"), new LocalDestination(pages.get(0)));
    if(pages.size() > 1)
    {
      destinations.put(new PdfString("N84afaba6"), new LocalDestination(pages.get(1), Destination.ModeEnum.FitHorizontal, 0, null));
      destinations.put(new PdfString("d38e1142"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("M38e1142"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("d3A8e1142"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("z38e1142"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("f38e1142"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("e38e1142"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("B84afaba6"), new LocalDestination(pages.get(1)));
      destinations.put(new PdfString("Z38e1142"), new LocalDestination(pages.get(1)));

      if(pages.size() > 2)
      {destinations.put(new PdfString("1845505298"), new LocalDestination(pages.get(2), Destination.ModeEnum.XYZ, new Point2D.Double(50, Double.NaN), null));}
    }

    // 3. Serialize the PDF file!
    serialize(file, "Named destinations", "manipulating named destinations", "named destinations, creation");
  }
}