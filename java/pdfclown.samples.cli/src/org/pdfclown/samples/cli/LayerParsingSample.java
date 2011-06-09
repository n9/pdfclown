package org.pdfclown.samples.cli;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.layers.ILayerNode;
import org.pdfclown.documents.contents.layers.LayerDefinition;
import org.pdfclown.documents.contents.layers.Layers;
import org.pdfclown.files.File;

/**
  This sample demonstrates how to parse existing layers.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.1
*/
public class LayerParsingSample
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

    // 2. Get the layer definition!
    LayerDefinition layerDefinition = document.getLayer();
    if(layerDefinition == null)
    {System.out.println("\nNo layer definition available.");}
    else
    {
      System.out.println("\nIterating through the layers...\n");

      // 3. Parse the layer hierarchy!
      parse(layerDefinition.getLayers(),0);
    }

    return true;
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