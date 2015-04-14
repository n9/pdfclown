package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.layers.ILayerNode;
import org.pdfclown.documents.contents.layers.LayerDefinition;
import org.pdfclown.documents.contents.layers.Layers;
import org.pdfclown.files.File;
import org.pdfclown.util.io.IOUtils;

/**
  This sample demonstrates how to parse existing layers.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.2.1, 04/08/15
*/
public class LayerParsingSample
  extends Sample
{
  @Override
  public void run(
    )
  {
    File file = null;
    try
    {
      // 1. Opening the PDF file...
      {
        String filePath = promptFileChoice("Please select a PDF file");
        try
        {file = new File(filePath);}
        catch(Exception e)
        {throw new RuntimeException(filePath + " file access error.",e);}
      }
      Document document = file.getDocument();

      // 2. Get the layer definition!
      LayerDefinition layerDefinition = document.getLayer();
      if(!layerDefinition.exists())
      {System.out.println("\nNo layer definition available.");}
      else
      {
        System.out.println("\nIterating through the layers...\n");

        // 3. Parse the layer hierarchy!
        parse(layerDefinition.getLayers(),0);
      }
    }
    finally
    {
      // 4. Closing the PDF file...
      IOUtils.closeQuietly(file);
    }
  }

  private void parse(
    Layers layers,
    int level
    )
  {
    String indentation = getIndentation(level);
    for(ILayerNode layer : layers)
    {
      System.out.println(indentation + layer.getTitle());
      parse(layer.getLayers(), level+1);
    }
  }
}