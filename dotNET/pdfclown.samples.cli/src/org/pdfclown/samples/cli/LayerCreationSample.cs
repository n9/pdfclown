using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.layers;
using org.pdfclown.files;

using System;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to define layers to control content visibility.</summary>
  */
  public class LayerCreationSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. PDF file instantiation.
      File file = new File();
      Document document = file.Document;

      // 2. Content creation.
      Populate(document);

      // 3. PDF file serialization.
      Serialize(file, false, "Layer", "inserting layers");

      return true;
    }

    /**
      <summary>Populates a PDF file with contents.</summary>
    */
    private void Populate(
      Document document
      )
    {
      // Initialize a new page!
      Page page = new Page(document);
      document.Pages.Add(page);

      // Initialize the primitive composer (within the new page context)!
      PrimitiveComposer composer = new PrimitiveComposer(page);
      composer.SetFont(new StandardType1Font(document, StandardType1Font.FamilyEnum.Helvetica, true, false), 12);

      // Initialize the block composer (wrapping the primitive one)!
      BlockComposer blockComposer = new BlockComposer(composer);

      // Initialize the document layer configuration!
      LayerDefinition layerDefinition = new LayerDefinition(document); // Creates the document layer configuration.
      document.Layer = layerDefinition; // Activates the document layer configuration.
      document.PageMode = Document.PageModeEnum.Layers; // Shows the layers tab on document opening.

      // Get the root layers collection!
      Layers rootLayers = layerDefinition.Layers;

      // 1. Nested layers.
      {
        Layer nestedLayer = new Layer(document, "Nested layers");
        rootLayers.Add(nestedLayer);
        Layers nestedSubLayers = nestedLayer.Layers;

        Layer nestedLayer1 = new Layer(document, "Nested layer 1");
        nestedSubLayers.Add(nestedLayer1);

        Layer nestedLayer2 = new Layer(document, "Nested layer 2");
        nestedSubLayers.Add(nestedLayer2);
        nestedLayer2.Locked = true;

        /*
          NOTE: Text in this section is shown using PrimitiveComposer.
        */
        composer.BeginLayer(nestedLayer);
        composer.ShowText(nestedLayer.Title, new PointF(50, 50));
        composer.End();

        composer.BeginLayer(nestedLayer1);
        composer.ShowText(nestedLayer1.Title, new PointF(50, 75));
        composer.End();

        composer.BeginLayer(nestedLayer2);
        composer.ShowText(nestedLayer2.Title, new PointF(50, 100));
        composer.End();
      }

      // 2. Simple group (labeled group of non-nested, inclusive-state layers).
      {
        Layers simpleGroup = new Layers(document, "Simple group");
        rootLayers.Add(simpleGroup);

        Layer layer1 = new Layer(document, "Grouped layer 1");
        simpleGroup.Add(layer1);

        Layer layer2 = new Layer(document, "Grouped layer 2");
        simpleGroup.Add(layer2);

        /*
          NOTE: Text in this section is shown using BlockComposer along with PrimitiveComposer
          to demonstrate their flexible cooperation.
        */
        blockComposer.Begin(new RectangleF(50, 125, 200, 50), AlignmentXEnum.Left, AlignmentYEnum.Middle);

        composer.BeginLayer(layer1);
        blockComposer.ShowText(layer1.Title);
        composer.End();

        blockComposer.ShowBreak(new SizeF(0, 15));

        composer.BeginLayer(layer2);
        blockComposer.ShowText(layer2.Title);
        composer.End();

        blockComposer.End();
      }

      // 3. Radio group (labeled group of non-nested, exclusive-state layers).
      {
        Layers radioGroup = new Layers(document, "Radio group");
        rootLayers.Add(radioGroup);

        Layer radio1 = new Layer(document, "Radiogrouped layer 1");
        radioGroup.Add(radio1);
        radio1.ViewState = Layer.StateEnum.On;

        Layer radio2 = new Layer(document, "Radiogrouped layer 2");
        radioGroup.Add(radio2);
        radio2.ViewState = Layer.StateEnum.Off;

        Layer radio3 = new Layer(document, "Radiogrouped layer 3");
        radioGroup.Add(radio3);
        radio3.ViewState = Layer.StateEnum.Off;

        // Register this option group in the layer configuration!
        LayerGroup options = new LayerGroup(document);
        options.Add(radio1);
        options.Add(radio2);
        options.Add(radio3);
        layerDefinition.OptionGroups.Add(options);

        /*
          NOTE: Text in this section is shown using BlockComposer along with PrimitiveComposer
          to demonstrate their flexible cooperation.
        */
        blockComposer.Begin(new RectangleF(50, 185, 200, 75), AlignmentXEnum.Left, AlignmentYEnum.Middle);

        composer.BeginLayer(radio1);
        blockComposer.ShowText(radio1.Title);
        composer.End();

        blockComposer.ShowBreak(new SizeF(0, 15));

        composer.BeginLayer(radio2);
        blockComposer.ShowText(radio2.Title);
        composer.End();

        blockComposer.ShowBreak(new SizeF(0, 15));

        composer.BeginLayer(radio3);
        blockComposer.ShowText(radio3.Title);
        composer.End();

        blockComposer.End();
      }
      composer.Flush();
    }
  }
}