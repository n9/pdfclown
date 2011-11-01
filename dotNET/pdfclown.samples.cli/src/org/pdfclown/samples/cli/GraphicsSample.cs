using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.colorSpaces;
using org.pdfclown.documents.contents.composition;
using entities = org.pdfclown.documents.contents.entities;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.files;
using org.pdfclown.util.math.geom;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates some of the graphics operations available
    through the PrimitiveComposer and BlockComposer classes to compose a PDF document.</summary>
  */
  public class GraphicsSample
    : Sample
  {
    private static readonly DeviceRGBColor SampleColor = new DeviceRGBColor(1,0,0);
    private static readonly DeviceRGBColor BackColor = new DeviceRGBColor(210/256f,232/256f,245/256f);

    public override bool Run(
      )
    {
      // 1. Instantiate a new PDF file!
      File file = new File();
      Document document = file.Document;

      // 2. Insert the contents into the document!
      BuildCurvesPage(document);
      BuildMiscellaneousPage(document);
      BuildSimpleTextPage(document);
      BuildTextBlockPage(document);
      BuildTextBlockPage2(document);

      // 3. Serialize the PDF file!
      Serialize(file, false, "Composition elements", "applying the composition elements");

      return true;
    }

    private void BuildCurvesPage(
      Document document
      )
    {
      // 1. Add the page to the document!
      Page page = new Page(document); // Instantiates the page inside the document context.
      document.Pages.Add(page); // Puts the page in the pages collection.

      SizeF pageSize = page.Size;

      // 2. Create a content composer for the page!
      PrimitiveComposer composer = new PrimitiveComposer(page);

      // 3. Drawing the page contents...
      composer.SetFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );

      {
        BlockComposer blockComposer = new BlockComposer(composer);
        blockComposer.Begin(new RectangleF(30,0,pageSize.Width-60,50),AlignmentXEnum.Center,AlignmentYEnum.Middle);
        blockComposer.ShowText("Curves");
        blockComposer.End();
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
            RectangleF arcFrame = new RectangleF((float)x,(float)y,(float)diameterX,(float)diameterY);

            // Drawing the arc frame...
            composer.BeginLocalState();
            composer.SetLineWidth(0.25f);
            composer.SetLineDash(3,5,5);
            composer.DrawRectangle(arcFrame);
            composer.Stroke();
            composer.End();

            // Draw the arc!
            composer.DrawArc(arcFrame,startAngle,endAngle);
            composer.Stroke();

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
        RectangleF arcFrame = new RectangleF(
          100,
          300,
          100,
          100
          );

        // Drawing the circle frame...
        composer.BeginLocalState();
        composer.SetLineWidth(0.25f);
        composer.SetLineDash(3,5,5);
        composer.DrawRectangle(arcFrame);
        composer.Stroke();
        composer.End();

        // Drawing the circle...
        composer.SetFillColor(new DeviceRGBColor(1,0,0));
        composer.DrawEllipse(arcFrame);
        composer.FillStroke();
      }

      // 3.3. Horizontal ellipse.
      {
        RectangleF arcFrame = new RectangleF(
          210,
          300,
          100,
          50
          );

        // Drawing the ellipse frame...
        composer.BeginLocalState();
        composer.SetLineWidth(0.25f);
        composer.SetLineDash(3,5,5);
        composer.DrawRectangle(arcFrame);
        composer.Stroke();
        composer.End();

        // Drawing the ellipse...
        composer.SetFillColor(new DeviceRGBColor(0,1,0));
        composer.DrawEllipse(arcFrame);
        composer.FillStroke();
      }

      // 3.4. Vertical ellipse.
      {
        RectangleF arcFrame = new RectangleF(
          320,
          300,
          50,
          100
          );

        // Drawing the ellipse frame...
        composer.BeginLocalState();
        composer.SetLineWidth(0.25f);
        composer.SetLineDash(3,5,5);
        composer.DrawRectangle(arcFrame);
        composer.Stroke();
        composer.End();

        // Drawing the ellipse...
        composer.SetFillColor(new DeviceRGBColor(0,0,1));
        composer.DrawEllipse(arcFrame);
        composer.FillStroke();
      }

      // 3.5. Spirals.
      {
        float y = 500;
        float spiralWidth = 100;
        composer.SetLineWidth(.5f);
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
                spiralTurnsCount = (float)(spiralWidth/(branchWidth*8*(spiralIndex*1.15+1)));
                break;
            }
            switch(rowIndex)
            {
              case 2:
                composer.SetLineDash(0,10,5);
                composer.SetLineCap(LineCapEnum.Round);
                break;
              default:
                break;
            }

            composer.DrawSpiral(
              new PointF((float)x,(float)y),
              0,
              360*spiralTurnsCount,
              branchWidth,
              branchRatio
              );
            composer.Stroke();

            x += spiralWidth + 10;

            switch(rowIndex)
            {
              case 0: default:
                branchWidth += 1;
                break;
              case 1:
                branchRatio += .035f;
                break;
            }
            switch(rowIndex)
            {
              case 2:
                composer.SetLineWidth(composer.State.LineWidth+.5f);
                break;
            }
          }

          y += spiralWidth + 10;
        }
      }

      // 4. Flush the contents into the page!
      composer.Flush();
    }

    private void BuildMiscellaneousPage(
      Document document
      )
    {
      // 1. Add the page to the document!
      Page page = new Page(document); // Instantiates the page inside the document context.
      document.Pages.Add(page); // Puts the page in the pages collection.

      SizeF pageSize = page.Size;

      // 2. Create a content composer for the page!
      PrimitiveComposer composer = new PrimitiveComposer(page);

      // 3. Drawing the page contents...
      composer.SetFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );

      {
        BlockComposer blockComposer = new BlockComposer(composer);
        blockComposer.Begin(new RectangleF(30,0,pageSize.Width-60,50),AlignmentXEnum.Center,AlignmentYEnum.Middle);
        blockComposer.ShowText("Miscellaneous");
        blockComposer.End();
      }

      composer.BeginLocalState();
      composer.SetLineJoin(LineJoinEnum.Round);
      composer.SetLineCap(LineCapEnum.Round);

      // 3.1. Polygon.
      composer.DrawPolygon(
        new PointF[]
        {
          new PointF(100,200),
          new PointF(150,150),
          new PointF(200,150),
          new PointF(250,200)
        }
        );

      // 3.2. Polyline.
      composer.DrawPolyline(
        new PointF[]
        {
          new PointF(300,200),
          new PointF(350,150),
          new PointF(400,150),
          new PointF(450,200)
        }
        );

      composer.Stroke();

      // 3.3. Rectangle (both squared and rounded).
      int x = 50;
      int radius = 0;
      while(x < 500)
      {
        if(x > 300)
        {
          composer.SetLineDash(3,5,5);
        }

        composer.SetFillColor(new DeviceRGBColor(1,x / 500f, x / 500f));
        composer.DrawRectangle(
            new RectangleF(x,250,150,100),
            radius // NOTE: radius parameter determines the rounded angle size.
            );
        composer.FillStroke();

        x += 175;
        radius += 10;
      }
      composer.End(); // End local state.

      composer.BeginLocalState();
      composer.SetFont(
        composer.State.Font,
        12
        );

      // 3.4. Line cap parameter.
      int y = 400;
      foreach(LineCapEnum lineCap
        in (LineCapEnum[])Enum.GetValues(typeof(LineCapEnum)))
      {
        composer.ShowText(
          lineCap + ":",
          new PointF(50,y),
          AlignmentXEnum.Left,
          AlignmentYEnum.Middle,
          0
          );
        composer.SetLineWidth(12);
        composer.SetLineCap(lineCap);
        composer.DrawLine(
          new PointF(120,y),
          new PointF(220,y)
          );
        composer.Stroke();

        composer.BeginLocalState();
        composer.SetLineWidth(1);
        composer.SetStrokeColor(DeviceRGBColor.White);
        composer.SetLineCap(LineCapEnum.Butt);
        composer.DrawLine(
          new PointF(120,y),
          new PointF(220,y)
          );
        composer.Stroke();
        composer.End(); // End local state.

        y += 30;
      }

      // 3.5. Line join parameter.
      y += 50;
      foreach(LineJoinEnum lineJoin
        in (LineJoinEnum[])Enum.GetValues(typeof(LineJoinEnum)))
      {
        composer.ShowText(
          lineJoin + ":",
          new PointF(50,y),
          AlignmentXEnum.Left,
          AlignmentYEnum.Middle,
          0
          );
        composer.SetLineWidth(12);
        composer.SetLineJoin(lineJoin);
        PointF[] points = new PointF[]
          {
            new PointF(120,y+25),
            new PointF(150,y-25),
            new PointF(180,y+25)
          };
        composer.DrawPolyline(points);
        composer.Stroke();

        composer.BeginLocalState();
        composer.SetLineWidth(1);
        composer.SetStrokeColor(DeviceRGBColor.White);
        composer.SetLineCap(LineCapEnum.Butt);
        composer.DrawPolyline(points);
        composer.Stroke();
        composer.End(); // End local state.

        y += 50;
      }
      composer.End(); // End local state.

      // 3.6. Clipping.
      /*
        NOTE: Clipping should be conveniently enclosed within a local state
        in order to easily resume the unaltered drawing area after the operation completes.
      */
      composer.BeginLocalState();
      composer.DrawPolygon(
        new PointF[]
        {
          new PointF(220,410),
          new PointF(300,490),
          new PointF(450,360),
          new PointF(430,520),
          new PointF(590,565),
          new PointF(420,595),
          new PointF(460,730),
          new PointF(380,650),
          new PointF(330,765),
          new PointF(310,640),
          new PointF(220,710),
          new PointF(275,570),
          new PointF(170,500),
          new PointF(275,510)
        }
        );
      composer.Clip();
      // Showing a clown image...
      // Instantiate a jpeg image object!
      entities::Image image = entities::Image.Get(InputPath + System.IO.Path.DirectorySeparatorChar + "images" + System.IO.Path.DirectorySeparatorChar + "Clown.jpg"); // Abstract image (entity).
      // Show the image!
      composer.ShowXObject(
        image.ToXObject(document),
        new PointF(
          170,
          320
          ),
        new Size(450,0)
        );
      composer.End(); // End local state.

      // 4. Flush the contents into the page!
      composer.Flush();
    }

    private void BuildSimpleTextPage(
      Document document
      )
    {
      // 1. Add the page to the document!
      Page page = new Page(document); // Instantiates the page inside the document context.
      document.Pages.Add(page); // Puts the page in the pages collection.

      SizeF pageSize = page.Size;

      // 2. Create a content composer for the page!
      PrimitiveComposer composer = new PrimitiveComposer(page);
      // 3. Inserting contents...
      // Set the font to use!
      composer.SetFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );

      AlignmentXEnum[] xAlignments = (AlignmentXEnum[])Enum.GetValues(typeof(AlignmentXEnum));
      AlignmentYEnum[] yAlignments = (AlignmentYEnum[])Enum.GetValues(typeof(AlignmentYEnum));
      int step = (int)(pageSize.Height) / ((xAlignments.Length-1) * yAlignments.Length+1);

      BlockComposer blockComposer = new BlockComposer(composer);
      RectangleF frame = new RectangleF(
        30,
        0,
        pageSize.Width-60,
        step/2
        );
      blockComposer.Begin(frame,AlignmentXEnum.Center,AlignmentYEnum.Middle);
      blockComposer.ShowText(
        "Simple text alignment"
        );
      blockComposer.End();

      frame = new RectangleF(
        30,
        pageSize.Height-step/2,
        pageSize.Width-60,
        step/2 -10
        );
      blockComposer.Begin(frame,AlignmentXEnum.Left,AlignmentYEnum.Bottom);
      composer.SetFont(composer.State.Font,10);
      blockComposer.ShowText(
        "NOTE: showText(...) methods return the actual bounding box of the text shown.\n"
          + "NOTE: The rotation parameter can be freely defined as a floating point value."
        );
      blockComposer.End();

      composer.SetFont(composer.State.Font,12);
      int x = 30;
      int y = step;
      int alignmentIndex = 0;
      foreach(AlignmentXEnum alignmentX
        in (AlignmentXEnum[])Enum.GetValues(typeof(AlignmentXEnum)))
      {
        /*
          NOTE: As text shown through PrimitiveComposer has no bounding box constraining its extension,
          applying the justified alignment has no effect (it degrades to center alignment);
          in order to get such an effect, use BlockComposer instead.
        */
        if(alignmentX.Equals(AlignmentXEnum.Justify))
          continue;

        foreach(AlignmentYEnum alignmentY
          in (AlignmentYEnum[])Enum.GetValues(typeof(AlignmentYEnum)))
        {
          if(alignmentIndex % 2 == 0)
          {
            composer.BeginLocalState();
            composer.SetFillColor(BackColor);
            composer.DrawRectangle(
              new RectangleF(
                0,
                y-step/2,
                pageSize.Width,
                step
                )
              );
            composer.Fill();
            composer.End();
          }

          composer.ShowText(
            alignmentX + " " + alignmentY + ":",
            new PointF(x,y),
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
            x = (int)pageSize.Width / 2 + 100;
            rotationStep = 360 / ((xAlignments.Length-1) * yAlignments.Length-1);
            break;
        }
        y = step;
        rotation = 0;
        foreach(AlignmentXEnum alignmentX
          in (AlignmentXEnum[])Enum.GetValues(typeof(AlignmentXEnum)))
        {
          /*
            NOTE: As text shown through PrimitiveComposer has no bounding box constraining its extension,
            applying the justified alignment has no effect (it degrades to center alignment);
            in order to get such an effect, use BlockComposer instead.
          */
          if(alignmentX.Equals(AlignmentXEnum.Justify))
            continue;

          foreach(AlignmentYEnum alignmentY
            in (AlignmentYEnum[])Enum.GetValues(typeof(AlignmentYEnum)))
          {
            float startArcAngle = 0;
            switch(alignmentX)
            {
              case AlignmentXEnum.Left:
                // OK -- NOOP.
                break;
              case AlignmentXEnum.Right:
              case AlignmentXEnum.Center:
                startArcAngle = 180;
                break;
            }

            composer.DrawArc(
              new RectangleF(
                x-10,
                y-10,
                20,
                20
                ),
              startArcAngle,
              startArcAngle+rotation
              );

            DrawText(
              composer,
              "PDF Clown",
              new PointF(x,y),
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
      composer.Flush();
    }

    private void BuildTextBlockPage(
      Document document
      )
    {
      // 1. Add the page to the document!
      Page page = new Page(document); // Instantiates the page inside the document context.
      document.Pages.Add(page); // Puts the page in the pages collection.

      SizeF pageSize = page.Size;

      // 2. Create a content composer for the page!
      PrimitiveComposer composer = new PrimitiveComposer(page);

      // 3. Drawing the page contents...
      composer.SetFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );

      AlignmentXEnum[] xAlignments = (AlignmentXEnum[])Enum.GetValues(typeof(AlignmentXEnum));
      AlignmentYEnum[] yAlignments = (AlignmentYEnum[])Enum.GetValues(typeof(AlignmentYEnum));
      int step = (int)(pageSize.Height) / (xAlignments.Length * yAlignments.Length+1);

      BlockComposer blockComposer = new BlockComposer(composer);
      {
        blockComposer.Begin(
          new RectangleF(
            30,
            0,
            pageSize.Width-60,
            step*.8f
            ),
          AlignmentXEnum.Center,
          AlignmentYEnum.Middle
          );
        blockComposer.ShowText(
          "Text block alignment"
          );
        blockComposer.End();
      }

      // Drawing the text blocks...
      int x = 30;
      int y = (int)(step*1.2);
      foreach(AlignmentXEnum alignmentX
        in (AlignmentXEnum[])Enum.GetValues(typeof(AlignmentXEnum)))
      {
        foreach(AlignmentYEnum alignmentY
          in (AlignmentYEnum[])Enum.GetValues(typeof(AlignmentYEnum)))
        {
          composer.SetFont(
            composer.State.Font,
            12
            );
          composer.ShowText(
            alignmentX + " " + alignmentY + ":",
            new PointF(x,y),
            AlignmentXEnum.Left,
            AlignmentYEnum.Middle,
            0
            );

          composer.SetFont(
            composer.State.Font,
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
                blockComposer.Hyphenation = false;
                break;
              case 1:
                frameX = 360;
                blockComposer.Hyphenation = true;
                break;
              default:
                throw new Exception();
            }

            RectangleF frame = new RectangleF(
              frameX,
              y-step*.4f,
              200,
              step*.8f
              );
            blockComposer.Begin(frame,alignmentX,alignmentY);
            blockComposer.ShowText(
              "Demonstrating how to constrain text inside a page area using PDF Clown. See the other available code samples (such as TypesettingSample) to discover more functionality details."
              );
            blockComposer.End();

            composer.BeginLocalState();
            composer.SetLineWidth(0.2f);
            composer.SetLineDash(5,5,5);
            composer.DrawRectangle(frame);
            composer.Stroke();
            composer.End();
          }

          y+=step;
        }
      }

      // 4. Flush the contents into the page!
      composer.Flush();
    }

    private void BuildTextBlockPage2(
      Document document
      )
    {
      // 1. Add the page to the document!
      Page page = new Page(document); // Instantiates the page inside the document context.
      document.Pages.Add(page); // Puts the page in the pages collection.

      SizeF pageSize = page.Size;

      // 2. Create a content composer for the page!
      PrimitiveComposer composer = new PrimitiveComposer(page);

      // 3. Drawing the page contents...
      composer.SetFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Courier,
          true,
          false
          ),
        32
        );

      int stepCount = 5;
      int step = (int)(pageSize.Height) / (stepCount + 1);
      BlockComposer blockComposer = new BlockComposer(composer);
      {
        blockComposer.Begin(
          new RectangleF(
            30,
            0,
            pageSize.Width-60,
            step*.8f
            ),
          AlignmentXEnum.Center,
          AlignmentYEnum.Middle
          );
        blockComposer.ShowText(
          "Text block line space"
          );
        blockComposer.End();
      }

      // Drawing the text blocks...
      int x = 30;
      int y = (int)(step*1.1);
      blockComposer.LineSpace.UnitMode = Length.UnitModeEnum.Relative;
      for(int index = 0; index < stepCount; index++)
      {
        float relativeLineSpace = 0.5f * index;
        blockComposer.LineSpace.Value = relativeLineSpace;

        composer.SetFont(
          composer.State.Font,
          10
          );
        composer.ShowText(
          relativeLineSpace + ":",
          new PointF(x,y),
          AlignmentXEnum.Left,
          AlignmentYEnum.Middle,
          0
          );

        composer.SetFont(
          composer.State.Font,
          9
          );
        RectangleF frame = new RectangleF(
          150,
          y-step*.4f,
          350,
          step*.9f
          );
        blockComposer.Begin(frame,AlignmentXEnum.Left,AlignmentYEnum.Top);
        blockComposer.ShowText(
          "Demonstrating how to set the block line space. Line space can be expressed either as an absolute value (in user-space units) or as a relative one (floating-point ratio); in the latter case the base value is represented by the current font's line height (so that, for example, 2 means \"a line space that's twice as the line height\")."
          );
        blockComposer.End();

        composer.BeginLocalState();
        composer.SetLineWidth(0.2f);
        composer.SetLineDash(5,5,5);
        composer.DrawRectangle(frame);
        composer.Stroke();
        composer.End();

        y+=step;
      }

      // 4. Flush the contents into the page!
      composer.Flush();
    }

    private void DrawCross(
      PrimitiveComposer composer,
      PointF center
      )
    {
      composer.DrawLine(
        new PointF(center.X-10,center.Y),
        new PointF(center.X+10,center.Y)
        );
      composer.DrawLine(
        new PointF(center.X,center.Y-10),
        new PointF(center.X,center.Y+10)
        );
      composer.Stroke();
    }

    private void DrawFrame(
      PrimitiveComposer composer,
      PointF[] frameVertices
      )
    {
      composer.BeginLocalState();
      composer.SetLineWidth(0.2f);
      composer.SetLineDash(5,5,5);
      composer.DrawPolygon(frameVertices);
      composer.Stroke();
      composer.End();
    }

    private void DrawText(
      PrimitiveComposer composer,
      string value,
      PointF location,
      AlignmentXEnum alignmentX,
      AlignmentYEnum alignmentY,
      float rotation
      )
    {
      // Show the anchor point!
      DrawCross(composer,location);

      composer.BeginLocalState();
      composer.SetFillColor(SampleColor);
      // Show the text onto the page!
      Quad textFrame = composer.ShowText(
        value,
        location,
        alignmentX,
        alignmentY,
        rotation
        );
      composer.End();

      // Draw the frame binding the shown text!
      DrawFrame(
        composer,
        textFrame.Points
        );

      composer.BeginLocalState();
      composer.SetFont(composer.State.Font,8);
      // Draw the rotation degrees!
      composer.ShowText(
        "(" + ((int)rotation) + " degrees)",
        new PointF(
          location.X+70,
          location.Y
          ),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        0
        );
      composer.End();
    }
  }
}