using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.composition;
using entities = org.pdfclown.documents.contents.entities;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.fileSpecs;
using org.pdfclown.documents.interaction.actions;
using annotations = org.pdfclown.documents.interaction.annotations;
using org.pdfclown.documents.interaction.navigation.document;
using files = org.pdfclown.files;

using System;
using System.Drawing;
using System.IO;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to apply links to a PDF document.</summary>
  */
  public class LinkCreationSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. Creating the document...
      files::File file = new files::File();
      Document document = file.Document;

      // 2. Applying links...
      BuildLinks(document);

      // 3. Serialize the PDF file!
      Serialize(file, true, "Link annotations", "applying link annotations");

      return true;
    }

    private void BuildLinks(
      Document document
      )
    {
      Pages pages = document.Pages;
      Page page = new Page(document);
      pages.Add(page);

      StandardType1Font font = new StandardType1Font(
        document,
        StandardType1Font.FamilyEnum.Courier,
        true,
        false
        );

      PrimitiveComposer composer = new PrimitiveComposer(page);
      BlockComposer blockComposer = new BlockComposer(composer);

      /*
        2.1. Goto-URI link.
      */
      {
        blockComposer.Begin(new RectangleF(30,100,200,50),AlignmentXEnum.Left,AlignmentYEnum.Middle);
        composer.SetFont(font,12);
        blockComposer.ShowText("Go-to-URI link");
        composer.SetFont(font,8);
        blockComposer.ShowText("\nIt allows you to navigate to a network resource.");
        composer.SetFont(font,5);
        blockComposer.ShowText("\n\nClick on the box to go to the project's SourceForge.net repository.");
        blockComposer.End();

        try
        {
          /*
            NOTE: This statement instructs the PDF viewer to navigate to the given URI when the link is clicked.
          */
          annotations::Link link = new annotations::Link(
            page,
            new Rectangle(240,100,100,50),
            new GoToURI(
              document,
              new Uri("http://www.sourceforge.net/projects/clown")
              )
            );
          link.Border = new annotations::Border(
            document,
            3,
            annotations::Border.StyleEnum.Beveled,
            null
            );
        }
        catch(Exception exception)
        {throw new Exception("",exception);}
      }

      /*
        2.2. Embedded-goto link.
      */
      {
        string filePath = PromptPdfFileChoice("Please select a PDF file to attach");

        /*
          NOTE: These statements instruct PDF Clown to attach a PDF file to the current document.
          This is necessary in order to test the embedded-goto functionality,
          as you can see in the following link creation (see below).
        */
        int fileAttachmentPageIndex = page.Index;
        string fileAttachmentName = "attachedSamplePDF";
        string fileName = System.IO.Path.GetFileName(filePath);
        annotations::FileAttachment attachment = new annotations::FileAttachment(
          page,
          new Rectangle(0, -20, 10, 10),
          new FileSpec(
            EmbeddedFile.Get(
              document,
              filePath
              ),
            fileName
            )
          );
        attachment.Name = fileAttachmentName;
        attachment.Text = "File attachment annotation";
        attachment.IconType = annotations::FileAttachment.IconTypeEnum.PaperClip;

        blockComposer.Begin(new RectangleF(30,170,200,50),AlignmentXEnum.Left,AlignmentYEnum.Middle);
        composer.SetFont(font,12);
        blockComposer.ShowText("Go-to-embedded link");
        composer.SetFont(font,8);
        blockComposer.ShowText("\nIt allows you to navigate to a destination within an embedded PDF file.");
        composer.SetFont(font,5);
        blockComposer.ShowText("\n\nClick on the button to go to the 2nd page of the attached PDF file (" + fileName + ").");
        blockComposer.End();

        /*
          NOTE: This statement instructs the PDF viewer to navigate to the page 2 of a PDF file
          attached inside the current document as described by the FileAttachment annotation on page 1 of the current document.
        */
        annotations::Link link = new annotations::Link(
          page,
          new Rectangle(240,170,100,50),
          new GoToEmbedded(
            document,
            new GoToEmbedded.TargetObject(
              document,
              fileAttachmentPageIndex, // Page of the current document containing the file attachment annotation of the target document.
              fileAttachmentName, // Name of the file attachment annotation corresponding to the target document.
              null // No sub-target.
              ), // Target represents the document to go to.
            new RemoteDestination(
              document,
              1, // Show the page 2 of the target document.
              Destination.ModeEnum.Fit, // Show the target document page entirely on the screen.
              null // No view parameters.
              ) // The destination must be within the target document.
            )
          );
        link.Border = new annotations::Border(
          document,
          1,
          annotations::Border.StyleEnum.Dashed,
          new LineDash(new double[]{8,5,2,5})
          );
      }

      /*
        2.3. Textual link.
      */
      {
        blockComposer.Begin(new RectangleF(30,240,200,50),AlignmentXEnum.Left,AlignmentYEnum.Middle);
        composer.SetFont(font,12);
        blockComposer.ShowText("Textual link");
        composer.SetFont(font,8);
        blockComposer.ShowText("\nIt allows you to expose any kind of link (including the above-mentioned types) as text.");
        composer.SetFont(font,5);
        blockComposer.ShowText("\n\nClick on the text links to go either to the project's SourceForge.net repository or to the project's home page.");
        blockComposer.End();

        try
        {
          composer.BeginLocalState();
          composer.SetFont(font,10);
          composer.SetFillColor(new DeviceRGBColor(0,0,1));
          composer.ShowText(
            "PDF Clown Project's repository at SourceForge.net",
            new PointF(240,265),
            AlignmentXEnum.Left,
            AlignmentYEnum.Middle,
            0,
            new GoToURI(
              document,
              new Uri("http://www.sourceforge.net/projects/clown")
              )
            );
          composer.ShowText(
            "PDF Clown Project's home page",
            new PointF(240,285),
            AlignmentXEnum.Left,
            AlignmentYEnum.Bottom,
            -90,
            new GoToURI(
              document,
              new Uri("http://www.pdfclown.org")
              )
            );
          composer.End();
        }
        catch
        {}
      }

      composer.Flush();
    }
  }
}