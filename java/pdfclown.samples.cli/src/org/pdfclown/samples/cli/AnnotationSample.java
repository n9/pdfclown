package org.pdfclown.samples.cli;

import java.awt.Color;
import java.awt.Point;
import java.awt.Rectangle;
import java.awt.geom.Point2D;
import java.util.Arrays;
import java.util.Date;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.colorSpaces.DeviceRGBColor;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.documents.files.EmbeddedFile;
import org.pdfclown.documents.files.FileSpecification;
import org.pdfclown.documents.interaction.JustificationEnum;
import org.pdfclown.documents.interaction.annotations.CalloutNote;
import org.pdfclown.documents.interaction.annotations.Caret;
import org.pdfclown.documents.interaction.annotations.Ellipse;
import org.pdfclown.documents.interaction.annotations.FileAttachment;
import org.pdfclown.documents.interaction.annotations.Line;
import org.pdfclown.documents.interaction.annotations.Note;
import org.pdfclown.documents.interaction.annotations.RubberStamp;
import org.pdfclown.documents.interaction.annotations.Scribble;
import org.pdfclown.files.File;

/**
  This sample demonstrates <b>how to insert annotations</b> into a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.2, 11/30/12
*/
public class AnnotationSample
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

    // 3. Serialize the PDF file!
    serialize(file, "Annotations", "inserting annotations", "annotations, creation, attachment, note, callout");
  }

  @SuppressWarnings("unchecked")
  private void populate(
    Document document
    )
  {
    Page page = new Page(document);
    document.getPages().add(page);

    PrimitiveComposer composer = new PrimitiveComposer(page);
    StandardType1Font font = new StandardType1Font(
      document,
      StandardType1Font.FamilyEnum.Courier,
      true,
      false
      );
    composer.setFont(font,12);

    // Note.
    composer.showText("Note annotation:", new Point(35,35));
    Note note = new Note(
      page,
      new Point(50, 50),
      "Note annotation"
      );
    note.setIconType(Note.IconTypeEnum.Help);
    note.setModificationDate(new Date());
    note.setOpen(true);

    // Callout.
    composer.showText("Callout note annotation:", new Point(35,85));
    CalloutNote calloutNote = new CalloutNote(
      page,
      new Rectangle(50, 100, 200, 24),
      "Callout note annotation"
      );
    calloutNote.setJustification(JustificationEnum.Right);
    calloutNote.setLine(
      new CalloutNote.LineObject(
        page,
        new Point(150,650),
        new Point(100,600),
        new Point(50,100)
        )
      );

    // File attachment.
    composer.showText("File attachment annotation:", new Point(35,135));
    FileAttachment attachment = new FileAttachment(
      page,
      new Rectangle(50, 150, 12, 12),
      "File attachment annotation",
      FileSpecification.get(
        EmbeddedFile.get(
          document,
          getResourcePath("images" + java.io.File.separator + "gnu.jpg")
          ),
        "happyGNU.jpg"
        )
      );
    attachment.setIconType(FileAttachment.IconTypeEnum.PaperClip);

    composer.beginLocalState();

    // Arrow line.
    composer.showText("Line annotation:", new Point(35,185));
    composer.setFont(font,10);
    composer.showText("Arrow:", new Point(50,200));
    Line line = new Line(
      page,
      new Point(50, 260),
      new Point(200,210),
      "Arrow line annotation"
      );
    line.setFillColor(DeviceRGBColor.get(Color.RED));
    line.setStartStyle(Line.LineEndStyleEnum.Circle);
    line.setEndStyle(Line.LineEndStyleEnum.ClosedArrow);
    line.setCaptionVisible(true);

    // Dimension line.
    composer.showText("Dimension:", new Point(300,200));
    line = new Line(
      page,
      new Point(300,220),
      new Point(500,220),
      "Dimension line annotation"
      );
    line.setLeaderLineLength(20);
    line.setLeaderLineExtensionLength(10);
    line.setCaptionVisible(true);

    composer.end();

    // Scribble.
    composer.showText("Scribble annotation:", new Point(35,285));
    @SuppressWarnings("unused")
    Scribble scribble = new Scribble(
      page,
      new Rectangle(50, 300, 100, 30),
      "Scribble annotation",
      Arrays.asList(
        Arrays.asList(
          (Point2D)new Point(50,300),
          (Point2D)new Point(70,310),
          (Point2D)new Point(100,320)
          )
        )
      );

    // Rectangle.
    composer.showText("Rectangle annotation:", new Point(35,335));
    org.pdfclown.documents.interaction.annotations.Rectangle rectangle = new org.pdfclown.documents.interaction.annotations.Rectangle(
      page,
      new Rectangle(50, 350, 100, 30),
      "Rectangle annotation"
      );
    rectangle.setFillColor(DeviceRGBColor.get(Color.RED));

    // Ellipse.
    composer.showText("Ellipse annotation:", new Point(35,385));
    Ellipse ellipse = new Ellipse(
      page,
      new Rectangle(50, 400, 100, 30),
      "Ellipse annotation"
      );
    ellipse.setFillColor(DeviceRGBColor.get(Color.BLUE));

    // Rubber stamp.
    composer.showText("Rubber stamp annotation:", new Point(35,435));
    @SuppressWarnings("unused")
    RubberStamp rubberStamp = new RubberStamp(
      page,
      new Rectangle(50, 450, 100, 30),
      "Rubber stamp annotation",
      RubberStamp.IconTypeEnum.Approved
      );

    // Caret.
    composer.showText("Caret annotation:", new Point(35,485));
    Caret caret = new Caret(
      page,
      new Rectangle(50, 500, 100, 30),
      "Caret annotation"
      );
    caret.setSymbolType(Caret.SymbolTypeEnum.NewParagraph);

    composer.flush();
  }
}