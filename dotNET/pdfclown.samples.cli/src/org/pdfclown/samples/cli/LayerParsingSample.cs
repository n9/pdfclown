using org.pdfclown.documents;
using org.pdfclown.documents.contents.layers;
using org.pdfclown.files;

using System;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to parse existing layers.</summary>
  */
  public class LayerParsingSample
    : Sample
  {
    public override void Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptFileChoice("Please select a PDF file");
      using(File file = new File(filePath))
      {
        Document document = file.Document;
  
        // 2. Get the layer definition!
        LayerDefinition layerDefinition = document.Layer;
        if(!layerDefinition.Exists())
        {Console.WriteLine("\nNo layer definition available.");}
        else
        {
          Console.WriteLine("\nIterating through the layers...\n");
  
          // 3. Parse the layer hierarchy!
          Parse(layerDefinition.Layers,0);
        }
      }
    }

    private void Parse(
      Layers layers,
      int level
      )
    {
      string indentation = GetIndentation(level);
      foreach(ILayerNode layer in layers)
      {
        Console.WriteLine(indentation + layer.Title);
        Parse(layer.Layers, level+1);
      }
    }
  }
}