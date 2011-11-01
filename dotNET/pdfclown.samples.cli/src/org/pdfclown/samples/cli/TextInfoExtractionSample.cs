using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;
using org.pdfclown.tools;

using System;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to retrieve text content along with its graphic attributes
    (font, font size, text color, text rendering mode, text bounding box...) from a PDF document;
    it also generates a document version decorated by text bounding boxes.</summary>
  */
  public class TextInfoExtractionSample
    : Sample
  {
    private DeviceRGBColor[] textCharBoxColors = new DeviceRGBColor[]
      {
        new DeviceRGBColor(200 / 255, 100 / 255, 100 / 255),
        new DeviceRGBColor(100 / 255, 200 / 255, 100 / 255),
        new DeviceRGBColor(100 / 255, 100 / 255, 200 / 255)
      };
    private DeviceRGBColor textStringBoxColor = DeviceRGBColor.Black;

    public override bool Run(
      )
    {
      // 1. Opening the PDF file...
      string filePath = PromptPdfFileChoice("Please select a PDF file");
      File file = new File(filePath);
      Document document = file.Document;

      PageStamper stamper = new PageStamper(); // NOTE: Page stamper is used to draw contents on existing pages.

      // 2. Iterating through the document pages...
      foreach(Page page in document.Pages)
      {
        Console.WriteLine("\nScanning page " + (page.Index+1) + "...\n");

        stamper.Page = page;

        Extract(
          new ContentScanner(page), // Wraps the page contents into a scanner.
          stamper.Foreground
          );

        stamper.Flush();
      }

      // 3. Decorated version serialization.
      Serialize(file,false);

      return true;
    }

    /**
      <summary>Scans a content level looking for text.</summary>
    */
    /*
      NOTE: Page contents are represented by a sequence of content objects,
      possibly nested into multiple levels.
    */
    private void Extract(
      ContentScanner level,
      PrimitiveComposer composer
      )
    {
      if(level == null)
        return;

      while(level.MoveNext())
      {
        ContentObject content = level.Current;
        if(content is Text)
        {
          ContentScanner.TextWrapper text = (ContentScanner.TextWrapper)level.CurrentWrapper;
          int colorIndex = 0;
          foreach(ContentScanner.TextStringWrapper textString in text.TextStrings)
          {
            RectangleF textStringBox = textString.Box.Value;
            Console.WriteLine(
              "Text ["
                + "x:" + Math.Round(textStringBox.X) + ","
                + "y:" + Math.Round(textStringBox.Y) + ","
                + "w:" + Math.Round(textStringBox.Width) + ","
                + "h:" + Math.Round(textStringBox.Height)
                + "] [font size:" + Math.Round(textString.Style.FontSize) + "]: " + textString.Text
                );

            // Drawing text character bounding boxes...
            colorIndex = (colorIndex + 1) % textCharBoxColors.Length;
            composer.SetStrokeColor(textCharBoxColors[colorIndex]);
            foreach(TextChar textChar in textString.TextChars)
            {
              /*
                NOTE: You can get further text information
                (font, font size, text color, text rendering mode)
                through textChar.style.
              */
              composer.DrawRectangle(textChar.Box);
              composer.Stroke();
            }

            // Drawing text string bounding box...
            composer.BeginLocalState();
            composer.SetLineDash(0, 5);
            composer.SetStrokeColor(textStringBoxColor);
            composer.DrawRectangle(textString.Box.Value);
            composer.Stroke();
            composer.End();
          }
        }
        else if(content is ContainerObject)
        {
          // Scan the inner level!
          Extract(level.ChildLevel, composer);
        }
      }
    }
  }
}