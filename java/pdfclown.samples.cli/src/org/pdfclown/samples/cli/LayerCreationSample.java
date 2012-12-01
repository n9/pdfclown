package org.pdfclown.samples.cli;

import java.awt.Dimension;
import java.awt.Point;
import java.awt.Rectangle;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Document.PageModeEnum;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.composition.BlockComposer;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.composition.XAlignmentEnum;
import org.pdfclown.documents.contents.composition.YAlignmentEnum;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.documents.contents.layers.Layer;
import org.pdfclown.documents.contents.layers.LayerDefinition;
import org.pdfclown.documents.contents.layers.LayerGroup;
import org.pdfclown.documents.contents.layers.Layers;
import org.pdfclown.files.File;

/**
  This sample demonstrates how to define layers to control content visibility.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.2, 09/24/12
*/
public class LayerCreationSample
  extends Sample
{
  @Override
  public void run(
    )
  {
    // 1. PDF file instantiation.
    File file = new File();
    Document document = file.getDocument();

    // 2. Content creation.
    populate(document);

    // 3. PDF file serialization.
    serialize(file, "Layer", "inserting layers", "layers, optional content");
  }

  private void populate(
    Document document
    )
  {
    // Initialize a new page!
    Page page = new Page(document);
    document.getPages().add(page);

    // Initialize the primitive composer (within the new page context)!
    PrimitiveComposer composer = new PrimitiveComposer(page);
    composer.setFont(new StandardType1Font(document, StandardType1Font.FamilyEnum.Helvetica, true, false), 12);

    // Initialize the block composer (wrapping the primitive one)!
    BlockComposer blockComposer = new BlockComposer(composer);

    // Initialize the document layer configuration!
    LayerDefinition layerDefinition = document.getLayer();
    document.setPageMode(PageModeEnum.Layers); // Shows the layers tab on document opening.

    // Get the root layers collection!
    Layers rootLayers = layerDefinition.getLayers();

    // 1. Nested layers.
    {
      Layer nestedLayer = new Layer(document, "Nested layers");
      rootLayers.add(nestedLayer);
      Layers nestedSubLayers = nestedLayer.getLayers();

      Layer nestedLayer1 = new Layer(document, "Nested layer 1");
      nestedSubLayers.add(nestedLayer1);

      Layer nestedLayer2 = new Layer(document, "Nested layer 2");
      nestedSubLayers.add(nestedLayer2);
      nestedLayer2.setLocked(true);

      /*
        NOTE: Text in this section is shown using PrimitiveComposer.
      */
      composer.beginLayer(nestedLayer);
      composer.showText(nestedLayer.getTitle(), new Point(50, 50));
      composer.end();

      composer.beginLayer(nestedLayer1);
      composer.showText(nestedLayer1.getTitle(), new Point(50, 75));
      composer.end();

      composer.beginLayer(nestedLayer2);
      composer.showText(nestedLayer2.getTitle(), new Point(50, 100));
      composer.end();
    }

    // 2. Simple group (labeled group of non-nested, inclusive-state layers).
    {
      Layers simpleGroup = new Layers(document, "Simple group");
      rootLayers.add(simpleGroup);

      Layer layer1 = new Layer(document, "Grouped layer 1");
      simpleGroup.add(layer1);

      Layer layer2 = new Layer(document, "Grouped layer 2");
      simpleGroup.add(layer2);

      /*
        NOTE: Text in this section is shown using BlockComposer along with PrimitiveComposer
        to demonstrate their flexible cooperation.
      */
      blockComposer.begin(new Rectangle(50, 125, 200, 50), XAlignmentEnum.Left, YAlignmentEnum.Middle);

      composer.beginLayer(layer1);
      blockComposer.showText(layer1.getTitle());
      composer.end();

      blockComposer.showBreak(new Dimension(0, 15));

      composer.beginLayer(layer2);
      blockComposer.showText(layer2.getTitle());
      composer.end();

      blockComposer.end();
    }

    // 3. Radio group (labeled group of non-nested, exclusive-state layers).
    {
      Layers radioGroup = new Layers(document, "Radio group");
      rootLayers.add(radioGroup);

      Layer radio1 = new Layer(document, "Radiogrouped layer 1");
      radioGroup.add(radio1);
      radio1.setVisible(true);

      Layer radio2 = new Layer(document, "Radiogrouped layer 2");
      radioGroup.add(radio2);
      radio2.setVisible(false);

      Layer radio3 = new Layer(document, "Radiogrouped layer 3");
      radioGroup.add(radio3);
      radio3.setVisible(false);

      // Register this option group in the layer configuration!
      LayerGroup options = new LayerGroup(document);
      options.add(radio1);
      options.add(radio2);
      options.add(radio3);
      layerDefinition.getOptionGroups().add(options);

      /*
        NOTE: Text in this section is shown using BlockComposer along with PrimitiveComposer
        to demonstrate their flexible cooperation.
      */
      blockComposer.begin(new Rectangle(50, 185, 200, 75), XAlignmentEnum.Left, YAlignmentEnum.Middle);

      composer.beginLayer(radio1);
      blockComposer.showText(radio1.getTitle());
      composer.end();

      blockComposer.showBreak(new Dimension(0, 15));

      composer.beginLayer(radio2);
      blockComposer.showText(radio2.getTitle());
      composer.end();

      blockComposer.showBreak(new Dimension(0, 15));

      composer.beginLayer(radio3);
      blockComposer.showText(radio3.getTitle());
      composer.end();

      blockComposer.end();
    }

    // 4. Print-only layer.
    {
      Layer printOnlyLayer = new Layer(document, "Print-only layer");
      printOnlyLayer.setVisible(false);
      printOnlyLayer.setPrintable(true);
      printOnlyLayer.setLocked(true);
      rootLayers.add(printOnlyLayer);

      composer.beginLayer(printOnlyLayer);
      composer.showText(printOnlyLayer.getTitle(), new Point(50, 270));
      composer.end();
    }
    composer.flush();
  }
}
