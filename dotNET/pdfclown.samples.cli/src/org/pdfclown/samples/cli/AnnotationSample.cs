using org.pdfclown.documents;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.entities;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.xObjects;
using org.pdfclown.documents.fileSpecs;
using org.pdfclown.documents.interaction;
using annotations = org.pdfclown.documents.interaction.annotations;
using files = org.pdfclown.files;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates how to insert annotations into a PDF document.</summary>
  */
  public class AnnotationSample
    : Sample
  {
    public override bool Run(
      )
    {
      // 1. PDF file instantiation.
      files::File file = new files::File();
      Document document = file.Document;

      // 2. Content creation.
      Populate(document);

      // 3. Serialize the PDF file!
      Serialize(file, false, "Annotations", "inserting annotations");

      return true;
    }

    private void Populate(
      Document document
      )
    {
      Page page = new Page(document);
      document.Pages.Add(page);

      PrimitiveComposer composer = new PrimitiveComposer(page);
      StandardType1Font font = new StandardType1Font(
        document,
        StandardType1Font.FamilyEnum.Courier,
        true,
        false
        );
      composer.SetFont(font,12);

      // Note.
      composer.ShowText("Note annotation:", new Point(35,35));
      annotations::Note note = new annotations::Note(
        page,
        new Point(50, 50),
        "Note annotation"
        );
      note.IconType = annotations::Note.IconTypeEnum.Help;
      note.ModificationDate = new DateTime();
      note.IsOpen = true;

      // Callout.
      composer.ShowText("Callout note annotation:", new Point(35,85));
      annotations::CalloutNote calloutNote = new annotations::CalloutNote(
        page,
        new Rectangle(50, 100, 200, 24),
        "Callout note annotation"
        );
      calloutNote.Justification = JustificationEnum.Right;
      calloutNote.Line = new annotations::CalloutNote.LineObject(
        page,
        new Point(150,650),
        new Point(100,600),
        new Point(50,100)
        );

      // File attachment.
      composer.ShowText("File attachment annotation:", new Point(35,135));
      annotations::FileAttachment attachment = new annotations::FileAttachment(
        page,
        new Rectangle(50, 150, 12, 12),
        new FileSpec(
          EmbeddedFile.Get(
            document,
            InputPath + Path.DirectorySeparatorChar + "images" + Path.DirectorySeparatorChar + "gnu.jpg"
            ),
          "happyGNU.jpg"
          )
        );
      attachment.Text = "File attachment annotation";
      attachment.IconType = annotations::FileAttachment.IconTypeEnum.PaperClip;

      composer.BeginLocalState();

      // Arrow line.
      composer.ShowText("Line annotation:", new Point(35,185));
      composer.SetFont(font,10);
      composer.ShowText("Arrow:", new Point(50,200));
      annotations::Line line = new annotations::Line(
        page,
        new Point(50, 260),
        new Point(200,210)
        );
      line.FillColor = new DeviceRGBColor(1,0,0);
      line.StartStyle = annotations::Line.LineEndStyleEnum.Circle;
      line.EndStyle = annotations::Line.LineEndStyleEnum.ClosedArrow;
      line.Text = "Arrow line annotation";
      line.CaptionVisible = true;

      // Dimension line.
      composer.ShowText("Dimension:", new Point(300,200));
      line = new annotations::Line(
        page,
        new Point(300,220),
        new Point(500,220)
        );
      line.LeaderLineLength = 20;
      line.LeaderLineExtensionLength = 10;
      line.Text = "Dimension line annotation";
      line.CaptionVisible = true;

      composer.End();

      // Scribble.
      composer.ShowText("Scribble annotation:", new Point(35,285));
      annotations::Scribble scribble = new annotations::Scribble(
        page,
        new RectangleF(50, 300, 100, 30),
        new List<IList<PointF>>(
          new List<PointF>[]
          {
            new List<PointF>(
              new PointF[]
              {
                new PointF(50,300),
                new PointF(70,310),
                new PointF(100,320)
              }
              )
          }
          )
        );
      scribble.Text = "Scribble annotation";

      // Rectangle.
      composer.ShowText("Rectangle annotation:", new Point(35,335));
      annotations::Rectangle rectangle = new annotations::Rectangle(
        page,
        new Rectangle(50, 350, 100, 30)
        );
      rectangle.FillColor = new DeviceRGBColor(1,0,0);
      rectangle.Text = "Rectangle annotation";

      // Ellipse.
      composer.ShowText("Ellipse annotation:", new Point(35,385));
      annotations::Ellipse ellipse = new annotations::Ellipse(
        page,
        new Rectangle(50, 400, 100, 30)
        );
      ellipse.FillColor = new DeviceRGBColor(0,0,1);
      ellipse.Text = "Ellipse annotation";

      // Rubber stamp.
      composer.ShowText("Rubber stamp annotation:", new Point(35,435));
      annotations::RubberStamp rubberStamp = new annotations::RubberStamp(
        page,
        new Rectangle(50, 450, 100, 30),
        annotations::RubberStamp.IconTypeEnum.Approved
        );
      rubberStamp.Text = "Rubber stamp annotation";

      // Caret.
      composer.ShowText("Caret annotation:", new Point(35,485));
      annotations::Caret caret = new annotations::Caret(
        page,
        new Rectangle(50, 500, 100, 30)
        );
      caret.Text = "Caret annotation";
      caret.SymbolType = annotations::Caret.SymbolTypeEnum.NewParagraph;

      composer.Flush();
    }
  }
}