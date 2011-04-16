package org.pdfclown.samples.cli;

import java.awt.Dimension;
import java.awt.geom.Dimension2D;
import java.awt.geom.Point2D;
import java.awt.geom.Rectangle2D;
import java.util.EnumSet;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.Page;
import org.pdfclown.documents.contents.LineCapEnum;
import org.pdfclown.documents.contents.LineJoinEnum;
import org.pdfclown.documents.contents.colorSpaces.DeviceRGBColor;
import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.BlockComposer;
import org.pdfclown.documents.contents.composition.Length.UnitModeEnum;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.entities.Image;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.files.File;
import org.pdfclown.util.math.geom.Quad;

/**
  This sample demonstrates some of the <b>graphics operations</b> available through the PrimitiveComposer
  and BlockComposer classes to compose a PDF document.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 04/16/11
*/
public class GraphicsSample
  extends Sample
{
  private static final DeviceRGBColor SampleColor = new DeviceRGBColor(1,0,0);
  private static final DeviceRGBColor BackColor = new DeviceRGBColor(210/256,232/256,245/256);

  @Override
  public boolean run(
    )
  {
    // 1. Instantiate a new PDF file!
    File file = new File();
    Document document = file.getDocument();

    // 2. Insert the contents into the document!
    buildCurvesPage(document);
    buildMiscellaneousPage(document);
    buildSimpleTextPage(document);
    buildTextBlockPage(document);
    buildTextBlockPage2(document);

    // (boilerplate metadata insertion -- ignore it)
    buildAccessories(document,"Composition elements","applying the composition elements");

    // 3. Serialize the PDF file!
    serialize(file,false);

    return true;
  }

  private void buildCurvesPage(
    Document document
    )
  {
    // 1. Add the page to the document!
    Page page = new Page(document); // Instantiates the page inside the document context.
    document.getPages().add(page); // Puts the page in the pages collection.

    Dimension2D pageSize = page.getSize();

    // 2. Create a content composer for the page!
    PrimitiveComposer composer = new PrimitiveComposer(page);

    // 3. Drawing the page contents...
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );
    }
    catch(Exception e)
    {throw new RuntimeException(e);}

    {
      BlockComposer blockComposer = new BlockComposer(composer);
      blockComposer.begin(new Rectangle2D.Double(30,0,pageSize.getWidth()-60,50),AlignmentXEnum.Center,AlignmentYEnum.Middle);
      blockComposer.showText("Curves");
      blockComposer.end();
    }

    // 3.1. Arcs.
    {
      float y = 100;
      for(
        int rowIndex = 0;
        rowIndex < 4;
        rowIndex++
        )
      {
        int angleStep = 45;
        int startAngle = 0;
        int endAngle = angleStep;
        float x = 100;
        float diameterX;
        float diameterY;
        switch(rowIndex)
        {
          case 0: default:
            diameterX = 40;
            diameterY = 40;
            break;
          case 1:
            diameterX = 40;
            diameterY = 20;
            break;
          case 2:
            diameterX = 20;
            diameterY = 40;
            break;
          case 3:
            diameterX = 40;
            diameterY = 40;
            break;
        }
        for(
          int index = 0,
            length = 360/angleStep;
          index < length;
          index++
          )
        {
          Rectangle2D arcFrame = new Rectangle2D.Double(x,y,diameterX,diameterY);

          // Drawing the arc frame...
          composer.beginLocalState();
          composer.setLineWidth(.25f);
          composer.setLineDash(3,5,5);
          composer.drawRectangle(arcFrame);
          composer.stroke();
          composer.end();

          // Draw the arc!
          composer.drawArc(arcFrame,startAngle,endAngle);
          composer.stroke();

          endAngle += angleStep;
          switch(rowIndex)
          {
            case 3:
              startAngle += angleStep;
              break;
          }

          x += 50;
        }

        y += diameterY + 10;
      }
    }

    // 3.2. Circle.
    {
      Rectangle2D arcFrame = new Rectangle2D.Double(
        100,
        300,
        100,
        100
        );

      // Drawing the circle frame...
      composer.beginLocalState();
      composer.setLineWidth(.25f);
      composer.setLineDash(3,5,5);
      composer.drawRectangle(arcFrame);
      composer.stroke();
      composer.end();

      // Drawing the circle...
      composer.setFillColor(new DeviceRGBColor(1,0,0));
      composer.drawEllipse(arcFrame);
      composer.fillStroke();
    }

    // 3.3. Horizontal ellipse.
    {
      Rectangle2D arcFrame = new Rectangle2D.Double(
        210,
        300,
        100,
        50
        );

      // Drawing the ellipse frame...
      composer.beginLocalState();
      composer.setLineWidth(.25f);
      composer.setLineDash(3,5,5);
      composer.drawRectangle(arcFrame);
      composer.stroke();
      composer.end();

      // Drawing the ellipse...
      composer.setFillColor(new DeviceRGBColor(0,1,0));
      composer.drawEllipse(arcFrame);
      composer.fillStroke();
    }

    // 3.4. Vertical ellipse.
    {
      Rectangle2D arcFrame = new Rectangle2D.Double(
        320,
        300,
        50,
        100
        );

      // Drawing the ellipse frame...
      composer.beginLocalState();
      composer.setLineWidth(.25f);
      composer.setLineDash(3,5,5);
      composer.drawRectangle(arcFrame);
      composer.stroke();
      composer.end();

      // Drawing the ellipse...
      composer.setFillColor(new DeviceRGBColor(0,0,1));
      composer.drawEllipse(arcFrame);
      composer.fillStroke();
    }

    // 3.5. Spirals.
    {
      float y = 500;
      float spiralWidth = 100;
      composer.setLineWidth(.5f);
      for(
        int rowIndex = 0;
        rowIndex < 3;
        rowIndex++
        )
      {
        float x = 150;
        float branchWidth = .5f;
        float branchRatio = 1;
        for(
          int spiralIndex = 0;
          spiralIndex < 4;
          spiralIndex++
          )
        {
          float spiralTurnsCount;
          switch(rowIndex)
          {
            case 0: default:
              spiralTurnsCount = spiralWidth/(branchWidth*8);
              break;
            case 1:
              spiralTurnsCount = spiralWidth/(branchWidth*8*(spiralIndex*1.15f+1));
              break;
          }
          switch(rowIndex)
          {
            case 2:
              composer.setLineDash(0,10,5);
              composer.setLineCap(LineCapEnum.Round);
              break;
            default:
              break;
          }

          composer.drawSpiral(
            new Point2D.Double(x,y),
            0,
            360*spiralTurnsCount,
            branchWidth,
            branchRatio
            );
          composer.stroke();

          x += spiralWidth + 10;

          switch(rowIndex)
          {
            case 0: default:
              branchWidth += 1;
              break;
            case 1:
              branchRatio += .035;
              break;
          }
          switch(rowIndex)
          {
            case 2:
              composer.setLineWidth(composer.getState().getLineWidth() + .5f);
              break;
          }
        }

        y += spiralWidth + 10;
      }
    }

    // 4. Flush the contents into the page!
    composer.flush();
  }

  private void buildMiscellaneousPage(
    Document document
    )
  {
    // 1. Add the page to the document!
    Page page = new Page(document); // Instantiates the page inside the document context.
    document.getPages().add(page); // Puts the page in the pages collection.

    Dimension2D pageSize = page.getSize();

    // 2. Create a content composer for the page!
    PrimitiveComposer composer = new PrimitiveComposer(page);

    // 3. Drawing the page contents...
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );
    }
    catch(Exception e)
    {throw new RuntimeException(e);}

    {
      BlockComposer blockComposer = new BlockComposer(composer);
      blockComposer.begin(new Rectangle2D.Double(30,0,pageSize.getWidth()-60,50),AlignmentXEnum.Center,AlignmentYEnum.Middle);
      blockComposer.showText("Miscellaneous");
      blockComposer.end();
    }

    composer.beginLocalState();
    composer.setLineJoin(LineJoinEnum.Round);
    composer.setLineCap(LineCapEnum.Round);

    // 3.1. Polygon.
    composer.drawPolygon(
      new Point2D[]
      {
        new Point2D.Double(100,200),
        new Point2D.Double(150,150),
        new Point2D.Double(200,150),
        new Point2D.Double(250,200)
      }
      );

    // 3.2. Polyline.
    composer.drawPolyline(
      new Point2D[]
      {
        new Point2D.Double(300,200),
        new Point2D.Double(350,150),
        new Point2D.Double(400,150),
        new Point2D.Double(450,200)
      }
      );

    composer.stroke();

    // 3.3. Rectangle (both squared and rounded).
    int x = 50;
    int radius = 0;
    while(x < 500)
    {
      if(x > 300)
      {
        composer.setLineDash(3,5,5);
      }

      composer.setFillColor(new DeviceRGBColor(1,x/500,x/500));
      composer.drawRectangle(
          new Rectangle2D.Double(x,250,150,100),
          radius // NOTE: radius parameter determines the rounded angle size.
          );
      composer.fillStroke();

      x += 175;
      radius += 10;
    }
    composer.end(); // End local state.

    composer.beginLocalState();
    composer.setFont(
      composer.getState().getFont(),
      12
      );

    // 3.4. Line cap parameter.
    int y = 400;
    for(LineCapEnum lineCap
      : EnumSet.allOf(LineCapEnum.class))
    {
      composer.showText(
        lineCap + ":",
        new Point2D.Double(50,y),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        0
        );
      composer.setLineWidth(12);
      composer.setLineCap(lineCap);
      composer.drawLine(
        new Point2D.Double(120,y),
        new Point2D.Double(220,y)
        );
      composer.stroke();

      composer.beginLocalState();
      composer.setLineWidth(1);
      composer.setStrokeColor(DeviceRGBColor.White);
      composer.setLineCap(LineCapEnum.Butt);
      composer.drawLine(
        new Point2D.Double(120,y),
        new Point2D.Double(220,y)
        );
      composer.stroke();
      composer.end(); // End local state.

      y += 30;
    }

    // 3.5. Line join parameter.
    y += 50;
    for(LineJoinEnum lineJoin
      : EnumSet.allOf(LineJoinEnum.class))
    {
      composer.showText(
        lineJoin + ":",
        new Point2D.Double(50,y),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        0
        );
      composer.setLineWidth(12);
      composer.setLineJoin(lineJoin);
      Point2D.Double[] points = new Point2D.Double[]
        {
          new Point2D.Double(120,y+25),
          new Point2D.Double(150,y-25),
          new Point2D.Double(180,y+25)
        };
      composer.drawPolyline(points);
      composer.stroke();

      composer.beginLocalState();
      composer.setLineWidth(1);
      composer.setStrokeColor(DeviceRGBColor.White);
      composer.setLineCap(LineCapEnum.Butt);
      composer.drawPolyline(points);
      composer.stroke();
      composer.end(); // End local state.

      y += 50;
    }
    composer.end(); // End local state.

    // 3.6. Clipping.
    /*
      NOTE: Clipping should be conveniently enclosed within a local state
      in order to easily resume the unaltered drawing area after the operation completes.
    */
    composer.beginLocalState();
    composer.drawPolygon(
      new Point2D[]
      {
        new Point2D.Double(220,410),
        new Point2D.Double(300,490),
        new Point2D.Double(450,360),
        new Point2D.Double(430,520),
        new Point2D.Double(590,565),
        new Point2D.Double(420,595),
        new Point2D.Double(460,730),
        new Point2D.Double(380,650),
        new Point2D.Double(330,765),
        new Point2D.Double(310,640),
        new Point2D.Double(220,710),
        new Point2D.Double(275,570),
        new Point2D.Double(170,500),
        new Point2D.Double(275,510)
      }
      );
    composer.clip();
    // Showing a clown image...
    // Instantiate a jpeg image object!
    Image image = Image.get(getInputPath() + java.io.File.separator + "images" + java.io.File.separator + "Clown.jpg"); // Abstract image (entity).
    // Show the image!
    composer.showXObject(
      image.toXObject(document),
      new Point2D.Double(
        170,
        320
        ),
      new Dimension(450,0)
      );
    composer.end(); // End local state.

    // 4. Flush the contents into the page!
    composer.flush();
  }

  private void buildSimpleTextPage(
    Document document
    )
  {
    // 1. Add the page to the document!
    Page page = new Page(document); // Instantiates the page inside the document context.
    document.getPages().add(page); // Puts the page in the pages collection.

    Dimension2D pageSize = page.getSize();

    // 2. Create a content composer for the page!
    PrimitiveComposer composer = new PrimitiveComposer(page);
    // 3. Inserting contents...
    // Set the font to use!
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );
    }
    catch(Exception e)
    {}

    EnumSet<AlignmentXEnum> xAlignments = EnumSet.allOf(AlignmentXEnum.class);
    EnumSet<AlignmentYEnum> yAlignments = EnumSet.allOf(AlignmentYEnum.class);
    int step = (int)(pageSize.getHeight()) / ((xAlignments.size()-1) * yAlignments.size()+1);

    BlockComposer blockComposer = new BlockComposer(composer);
    Rectangle2D frame = new Rectangle2D.Double(
      30,
      0,
      pageSize.getWidth()-60,
      step/2
      );
    blockComposer.begin(frame,AlignmentXEnum.Center,AlignmentYEnum.Middle);
    blockComposer.showText(
      "Simple text alignment"
      );
    blockComposer.end();

    frame = new Rectangle2D.Double(
      30,
      pageSize.getHeight()-step/2,
      pageSize.getWidth()-60,
      step/2 -10
      );
    blockComposer.begin(frame,AlignmentXEnum.Left,AlignmentYEnum.Bottom);
    composer.setFont(composer.getState().getFont(),10);
    blockComposer.showText(
      "NOTE: showText(...) methods return the actual bounding box of the text shown.\n"
        + "NOTE: The rotation parameter can be freely defined as a floating point value."
      );
    blockComposer.end();

    composer.setFont(composer.getState().getFont(),12);
    int x = 30;
    int y = step;
    int alignmentIndex = 0;
    for(AlignmentXEnum alignmentX
      : EnumSet.allOf(AlignmentXEnum.class))
    {
      /*
        NOTE: As text shown through PrimitiveComposer has no bounding box constraining its extension,
        applying the justified alignment has no effect (it degrades to center alignment);
        in order to get such an effect, use BlockComposer instead.
      */
      if(alignmentX.equals(AlignmentXEnum.Justify))
        continue;

      for(AlignmentYEnum alignmentY
        : EnumSet.allOf(AlignmentYEnum.class))
      {
        if(alignmentIndex % 2 == 0)
        {
          composer.beginLocalState();
          composer.setFillColor(BackColor);
          composer.drawRectangle(
            new Rectangle2D.Double(
              0,
              y-step/2,
              pageSize.getWidth(),
              step
              )
            );
          composer.fill();
          composer.end();
        }

        composer.showText(
          alignmentX + " " + alignmentY + ":",
          new Point2D.Double(x,y),
          AlignmentXEnum.Left,
          AlignmentYEnum.Middle,
          0
          );

        y+=step;
        alignmentIndex++;
      }
    }

    float rotationStep = 0;
    float rotation = 0;
    for(
      int columnIndex = 0;
      columnIndex < 2;
      columnIndex++
      )
    {
      switch(columnIndex)
      {
        case 0:
          x = 200;
          rotationStep = 0;
          break;
        case 1:
          x = (int)pageSize.getWidth() / 2 + 100;
          rotationStep = 360 / ((xAlignments.size()-1) * yAlignments.size()-1);
          break;
      }
      y = step;
      rotation = 0;
      for(AlignmentXEnum alignmentX
        : EnumSet.allOf(AlignmentXEnum.class))
      {
        /*
          NOTE: As text shown through PrimitiveComposer has no bounding box constraining its extension,
          applying the justified alignment has no effect (it degrades to center alignment);
          in order to get such an effect, use BlockComposer instead.
        */
        if(alignmentX.equals(AlignmentXEnum.Justify))
          continue;

        for(AlignmentYEnum alignmentY
          : EnumSet.allOf(AlignmentYEnum.class))
        {
          float startArcAngle = 0;
          switch(alignmentX)
          {
            case Left:
              // OK -- NOOP.
              break;
            case Right:
            case Center:
              startArcAngle = 180;
              break;
          }

          composer.drawArc(
            new Rectangle2D.Double(
              x-10,
              y-10,
              20,
              20
              ),
            startArcAngle,
            startArcAngle+rotation
            );

          drawText(
            composer,
            "PDF Clown",
            new Point2D.Double(x,y),
            alignmentX,
            alignmentY,
            rotation
            );
          y+=step;
          rotation+=rotationStep;
        }
      }
    }

    // 4. Flush the contents into the page!
    composer.flush();
  }

  private void buildTextBlockPage(
    Document document
    )
  {
    // 1. Add the page to the document!
    Page page = new Page(document); // Instantiates the page inside the document context.
    document.getPages().add(page); // Puts the page in the pages collection.

    Dimension2D pageSize = page.getSize();

    // 2. Create a content composer for the page!
    PrimitiveComposer composer = new PrimitiveComposer(page);

    // 3. Drawing the page contents...
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );
    }
    catch(Exception e)
    {throw new RuntimeException(e);}

    EnumSet<AlignmentXEnum> xAlignments = EnumSet.allOf(AlignmentXEnum.class);
    EnumSet<AlignmentYEnum> yAlignments = EnumSet.allOf(AlignmentYEnum.class);
    int step = (int)(pageSize.getHeight()) / (xAlignments.size() * yAlignments.size()+1);

    BlockComposer blockComposer = new BlockComposer(composer);
    {
      blockComposer.begin(
        new Rectangle2D.Double(
          30,
          0,
          pageSize.getWidth()-60,
          step*.8
          ),
        AlignmentXEnum.Center,
        AlignmentYEnum.Middle
        );
      blockComposer.showText(
        "Text block alignment"
        );
      blockComposer.end();
    }

    // Drawing the text blocks...
    int x = 30;
    int y = (int)(step*1.2);
    for(AlignmentXEnum alignmentX
      : EnumSet.allOf(AlignmentXEnum.class))
    {
      for(AlignmentYEnum alignmentY
        : EnumSet.allOf(AlignmentYEnum.class))
      {
        composer.setFont(
          composer.getState().getFont(),
          12
          );
        composer.showText(
          alignmentX + " " + alignmentY + ":",
          new Point2D.Double(x,y),
          AlignmentXEnum.Left,
          AlignmentYEnum.Middle,
          0
          );

        composer.setFont(
          composer.getState().getFont(),
          8
          );
        for(
          int index = 0;
          index < 2;
          index++
          )
        {
          int frameX;
          switch(index)
          {
            case 0:
              frameX = 150;
              blockComposer.setHyphenation(false);
              break;
            case 1:
              frameX = 360;
              blockComposer.setHyphenation(true);
              break;
            default:
              throw new RuntimeException();
          }

          Rectangle2D frame = new Rectangle2D.Double(
            frameX,
            y-step*.4,
            200,
            step*.8
            );
          blockComposer.begin(frame,alignmentX,alignmentY);
          blockComposer.showText(
            "Demonstrating how to constrain text inside a page area using PDF Clown. See the other available code samples (such as TypesettingSample) to discover more functionality details."
            );
          blockComposer.end();

          composer.beginLocalState();
          composer.setLineWidth(.2f);
          composer.setLineDash(5,5,5);
          composer.drawRectangle(frame);
          composer.stroke();
          composer.end();
        }

        y+=step;
      }
    }

    // 4. Flush the contents into the page!
    composer.flush();
  }

  private void buildTextBlockPage2(
    Document document
    )
  {
    // 1. Add the page to the document!
    Page page = new Page(document); // Instantiates the page inside the document context.
    document.getPages().add(page); // Puts the page in the pages collection.

    Dimension2D pageSize = page.getSize();

    // 2. Create a content composer for the page!
    PrimitiveComposer composer = new PrimitiveComposer(page);

    // 3. Drawing the page contents...
    try
    {
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );
    }
    catch(Exception e)
    {throw new RuntimeException(e);}

    int stepCount = 5;
    int step = (int)(pageSize.getHeight()) / (stepCount + 1);
    BlockComposer blockComposer = new BlockComposer(composer);
    {
      blockComposer.begin(
        new Rectangle2D.Double(
          30,
          0,
          pageSize.getWidth()-60,
          step*.8
          ),
        AlignmentXEnum.Center,
        AlignmentYEnum.Middle
        );
      blockComposer.showText(
        "Text block line space"
        );
      blockComposer.end();
    }

    // Drawing the text blocks...
    int x = 30;
    int y = (int)(step*1.1);
    blockComposer.getLineSpace().setUnitMode(UnitModeEnum.Relative);
    for(int index = 0; index < stepCount; index++)
    {
      float relativeLineSpace = 0.5f * index;
      blockComposer.getLineSpace().setValue(relativeLineSpace);

      composer.setFont(
        composer.getState().getFont(),
        10
        );
      composer.showText(
        relativeLineSpace + ":",
        new Point2D.Double(x,y),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        0
        );

      composer.setFont(
        composer.getState().getFont(),
        9
        );
      Rectangle2D frame = new Rectangle2D.Double(
        150,
        y-step*.4,
        350,
        step*.9
        );
      blockComposer.begin(frame,AlignmentXEnum.Left,AlignmentYEnum.Top);
      blockComposer.showText(
        "Demonstrating how to set the block line space. Line space can be expressed either as an absolute value (in user-space units) or as a relative one (as a floating-point ratio); in the latter case the base value is represented by the current font's line height (so that, for example, 2 means \"a line space that's double the line height\")."
        );
      blockComposer.end();

      composer.beginLocalState();
      composer.setLineWidth(0.2f);
      composer.setLineDash(5,5,5);
      composer.drawRectangle(frame);
      composer.stroke();
      composer.end();

      y+=step;
    }

    // 4. Flush the contents into the page!
    composer.flush();
  }

  private void drawCross(
    PrimitiveComposer composer,
    Point2D center
    )
  {
    composer.drawLine(
      new Point2D.Double(center.getX()-10,center.getY()),
      new Point2D.Double(center.getX()+10,center.getY())
      );
    composer.drawLine(
      new Point2D.Double(center.getX(),center.getY()-10),
      new Point2D.Double(center.getX(),center.getY()+10)
      );
    composer.stroke();
  }

  private void drawFrame(
    PrimitiveComposer composer,
    Point2D[] frameVertices
    )
  {
    composer.beginLocalState();
    composer.setLineWidth(.2f);
    composer.setLineDash(5,5,5);
    composer.drawPolygon(frameVertices);
    composer.stroke();
    composer.end();
  }

  private void drawText(
    PrimitiveComposer composer,
    String value,
    Point2D location,
    AlignmentXEnum alignmentX,
    AlignmentYEnum alignmentY,
    float rotation
    )
  {
    // Show the anchor point!
    drawCross(composer,location);

    composer.beginLocalState();
    composer.setFillColor(SampleColor);
    // Show the text onto the page!
    Quad textFrame = composer.showText(
      value,
      location,
      alignmentX,
      alignmentY,
      rotation
      );
    composer.end();

    // Draw the frame binding the shown text!
    drawFrame(
      composer,
      textFrame.getPoints()
      );

    composer.beginLocalState();
    composer.setFont(composer.getState().getFont(),8);
    // Draw the rotation degrees!
    composer.showText(
      "(" + ((int)rotation) + " degrees)",
      new Point2D.Double(
        location.getX()+70,
        location.getY()
        ),
      AlignmentXEnum.Left,
      AlignmentYEnum.Middle,
      0
      );
    composer.end();
  }
}