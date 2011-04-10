/*
  Copyright 2007-2011 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using colors = org.pdfclown.documents.contents.colorSpaces;
using fonts = org.pdfclown.documents.contents.fonts;
using objects = org.pdfclown.documents.contents.objects;
using org.pdfclown.documents.contents.xObjects;
using actions = org.pdfclown.documents.interaction.actions;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.documents.contents.composition
{
  /**
    <summary>
      <para>Content stream primitive composer.</para>
      <para>It provides the basic (primitive) operations described by the PDF specification
      for graphics content composition.</para>
    </summary>
    <remarks>This class leverages the object-oriented content stream modelling infrastructure,
    which encompasses 1st-level content stream objects (operations),
    2nd-level content stream objects (graphics objects) and full graphics state support.</remarks>
  */
  public sealed class PrimitiveComposer
  {
    #region dynamic
    #region fields
    private ContentScanner scanner;
    #endregion

    #region constructors
    public PrimitiveComposer(
      ContentScanner scanner
      )
    {Scanner = scanner;}

    public PrimitiveComposer(
      IContentContext context
      ) : this(
        new ContentScanner(context.Contents)
        )
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Adds a content object.</summary>
      <returns>The added content object.</returns>
    */
    public objects::ContentObject Add(
      objects::ContentObject obj
      )
    {
      scanner.Insert(obj);
      scanner.MoveNext();

      return obj;
    }

    /**
      <summary>Applies a transformation to the coordinate system from user space to device space
      [PDF:1.6:4.3.3].</summary>
      <remarks>The transformation is applied to the current transformation matrix (CTM) by
      concatenation, i.e. it doesn't replace it.</remarks>
      <param name="a">Item 0,0 of the matrix.</param>
      <param name="b">Item 0,1 of the matrix.</param>
      <param name="c">Item 1,0 of the matrix.</param>
      <param name="d">Item 1,1 of the matrix.</param>
      <param name="e">Item 2,0 of the matrix.</param>
      <param name="f">Item 2,1 of the matrix.</param>
    */
    public void ApplyMatrix(
      double a,
      double b,
      double c,
      double d,
      double e,
      double f
      )
    {Add(new objects::ModifyCTM(a,b,c,d,e,f));}

    /**
      <summary>Adds a composite object beginning it.</summary>
      <returns>The added composite object.</returns>
    */
    public objects.CompositeObject Begin(
      objects.CompositeObject obj
      )
    {
      // Insert the new object at the current level!
      scanner.Insert(obj);
      // The new object's children level is the new current level!
      scanner = scanner.ChildLevel;

      return obj;
    }

    /**
      <summary>Begins a new nested graphics state context [PDF:1.6:4.3.1].</summary>
      <returns>The added local graphics state object.</returns>
    */
    public objects::LocalGraphicsState BeginLocalState(
      )
    {return (objects::LocalGraphicsState)Begin(new objects::LocalGraphicsState());}

    /**
      <summary>Begins a new marked-content sequence [PDF:1.6:10.5].</summary>
      <returns>The added marked-content sequence.</returns>
    */
    public objects::MarkedContent BeginMarkedContent(
      PdfName tag
      )
    {
      return (objects::MarkedContent)Begin(
        new objects::MarkedContent(
          new objects::BeginMarkedContent(tag)
          )
        );
    }

    /**
      <summary>Modifies the current clipping path by intersecting it with the current path [PDF:1.6:4.4.1].</summary>
      <remarks>It can be validly called only just before painting the current path.</remarks>
    */
    public void Clip(
      )
    {
      Add(objects::ModifyClipPath.NonZero);
      Add(objects::PaintPath.EndPathNoOp);
    }

    /**
      <summary>Closes the current subpath by appending a straight line segment
      from the current point to the starting point of the subpath [PDF:1.6:4.4.1].</summary>
    */
    public void ClosePath(
      )
    {Add(objects::CloseSubpath.Value);}

    /**
      <summary>Draws a circular arc.</summary>
      <param name="location">Arc location.</param>
      <param name="startAngle">Starting angle.</param>
      <param name="endAngle">Ending angle.</param>
    */
    public void DrawArc(
      RectangleF location,
      float startAngle,
      float endAngle
      )
    {DrawArc(location,startAngle,endAngle,0,1);}

    /**
      <summary>Draws an arc.</summary>
      <param name="location">Arc location.</param>
      <param name="startAngle">Starting angle.</param>
      <param name="endAngle">Ending angle.</param>
      <param name="branchWidth">Distance between the spiral branches. '0' value degrades to a circular arc.</param>
      <param name="branchRatio">Linear coefficient applied to the branch width. '1' value degrades to a constant branch width.</param>
    */
    public void DrawArc(
      RectangleF location,
      float startAngle,
      float endAngle,
      float branchWidth,
      float branchRatio
      )
    {DrawArc(location,startAngle,endAngle,branchWidth,branchRatio,true);}

    /**
      <summary>Draws a cubic Bezier curve from the current point [PDF:1.6:4.4.1].</summary>
      <param name="endPoint">Ending point.</param>
      <param name="startControl">Starting control point.</param>
      <param name="endControl">Ending control point.</param>
    */
    public void DrawCurve(
      PointF endPoint,
      PointF startControl,
      PointF endControl
      )
    {
      float contextHeight = scanner.ContentContext.Box.Height;
      Add(
        new objects::DrawCurve(
          endPoint.X,
          contextHeight - endPoint.Y,
          startControl.X,
          contextHeight - startControl.Y,
          endControl.X,
          contextHeight - endControl.Y
          )
        );
    }

    /**
      <summary>Draws a cubic Bezier curve [PDF:1.6:4.4.1].</summary>
      <param name="startPoint">Starting point.</param>
      <param name="endPoint">Ending point.</param>
      <param name="startControl">Starting control point.</param>
      <param name="endControl">Ending control point.</param>
    */
    public void DrawCurve(
      PointF startPoint,
      PointF endPoint,
      PointF startControl,
      PointF endControl
      )
    {
      BeginSubpath(startPoint);
      DrawCurve(endPoint,startControl,endControl);
    }

    /**
      <summary>Draws an ellipse.</summary>
      <param name="location">Ellipse location.</param>
    */
    public void DrawEllipse(
      RectangleF location
      )
    {DrawArc(location,0,360);}

    /**
      <summary>Draws a line from the current point [PDF:1.6:4.4.1].</summary>
      <param name="endPoint">Ending point.</param>
    */
    public void DrawLine(
      PointF endPoint
      )
    {
      Add(
        new objects::DrawLine(
          endPoint.X,
          scanner.ContentContext.Box.Height - endPoint.Y
          )
        );
    }

    /**
      <summary>Draws a line [PDF:1.6:4.4.1].</summary>
      <param name="startPoint">Starting point.</param>
      <param name="endPoint">Ending point.</param>
    */
    public void DrawLine(
      PointF startPoint,
      PointF endPoint
      )
    {
      BeginSubpath(startPoint);
      DrawLine(endPoint);
    }

    /**
      <summary>Draws a polygon.</summary>
      <remarks>A polygon is the same as a multiple line except that it's a closed path.</remarks>
      <param name="points">Points.</param>
    */
    public void DrawPolygon(
      PointF[] points
      )
    {
      DrawPolyline(points);
      ClosePath();
    }

    /**
      <summary>Draws a multiple line.</summary>
      <param name="points">Points.</param>
    */
    public void DrawPolyline(
      PointF[] points
      )
    {
      BeginSubpath(points[0]);
      for(
        int index = 1,
          length = points.Length;
        index < length;
        index++
        )
      {DrawLine(points[index]);}
    }

    /**
      <summary>Draws a rectangle [PDF:1.6:4.4.1].</summary>
      <param name="location">Rectangle location.</param>
    */
    public void DrawRectangle(
      RectangleF location
      )
    {DrawRectangle(location,0);}

    /**
      <summary>Draws a rounded rectangle.</summary>
      <param name="location">Rectangle location.</param>
      <param name="radius">Vertex radius, '0' value degrades to squared vertices.</param>
    */
    public void DrawRectangle(
      RectangleF location,
      float radius
      )
    {
      if(radius == 0)
      {
        Add(
          new objects::DrawRectangle(
            location.X,
            scanner.ContentContext.Box.Height - location.Y - location.Height,
            location.Width,
            location.Height
            )
          );
      }
      else
      {
        double endRadians = Math.PI * 2;
        double quadrantRadians = Math.PI / 2;
        double radians = 0;
        while(radians < endRadians)
        {
          double radians2 = radians + quadrantRadians;
          int sin2 = (int)Math.Sin(radians2);
          int cos2 = (int)Math.Cos(radians2);
          float x1 = 0, x2 = 0, y1 = 0, y2 = 0;
          float xArc = 0, yArc = 0;
          if(cos2 == 0)
          {
            if(sin2 == 1)
            {
              x1 = x2 = location.X + location.Width;
              y1 = location.Y + location.Height - radius;
              y2 = location.Y + radius;

              xArc =- radius * 2;
              yArc =- radius;

              BeginSubpath(new PointF((float)x1,(float)y1));
            }
            else
            {
              x1 = x2 = location.X;
              y1 = location.Y + radius;
              y2 = location.Y + location.Height - radius;

              yArc =- radius;
            }
          }
          else if(cos2 == 1)
          {
            x1 = location.X + radius;
            x2 = location.X + location.Width - radius;
            y1 = y2 = location.Y + location.Height;

            xArc =- radius;
            yArc =- radius * 2;
          }
          else if(cos2 == -1)
          {
            x1 = location.X + location.Width - radius;
            x2 = location.X + radius;
            y1 = y2 = location.Y;

            xArc =- radius;
          }
          DrawLine(
            new PointF((float)x2,(float)y2)
            );
          DrawArc(
            new RectangleF((float)(x2+xArc), (float)(y2+yArc), (float)(radius*2), (float)(radius*2)),
            (float)((180 / Math.PI) * radians),
            (float)((180 / Math.PI) * radians2),
            0,
            1,
            false
            );

          radians = radians2;
        }
      }
    }

    /**
      <summary>Draws a spiral.</summary>
      <param name="center">Spiral center.</param>
      <param name="startAngle">Starting angle.</param>
      <param name="endAngle">Ending angle.</param>
      <param name="branchWidth">Distance between the spiral branches.</param>
      <param name="branchRatio">Linear coefficient applied to the branch width.</param>
    */
    public void DrawSpiral(
      PointF center,
      float startAngle,
      float endAngle,
      float branchWidth,
      float branchRatio
      )
    {
      DrawArc(
        new RectangleF(center.X, center.Y, 0.0001f, 0.0001f),
        startAngle,
        endAngle,
        branchWidth,
        branchRatio
        );
    }

    /**
      <summary>Ends the current (innermostly-nested) composite object.</summary>
    */
    public void End(
      )
    {
      scanner = scanner.ParentLevel;
      scanner.MoveNext();
    }

    /**
      <summary>Fills the path using the current color [PDF:1.6:4.4.2].</summary>
    */
    public void Fill(
      )
    {Add(objects::PaintPath.Fill);}

    /**
      <summary>Fills and then strokes the path using the current colors [PDF:1.6:4.4.2].</summary>
    */
    public void FillStroke(
      )
    {Add(objects::PaintPath.FillStroke);}

    /**
      <summary>Serializes the contents into the content stream.</summary>
    */
    public void Flush(
      )
    {scanner.Contents.Flush();}

    /**
      <summary>Gets/Sets the content stream scanner.</summary>
    */
    public ContentScanner Scanner
    {
      get{return scanner;}
      set{scanner = value;}
    }

    /**
      <summary>Gets the current graphics state [PDF:1.6:4.3].</summary>
    */
    public ContentScanner.GraphicsState State
    {get{return scanner.State;}}

    /**
      <summary>Applies a rotation to the coordinate system from user space to device space
      [PDF:1.6:4.2.2].</summary>
      <param name="angle">Rotational counterclockwise angle.</param>
    */
    public void Rotate(
      float angle
      )
    {
      double rad = angle * Math.PI / 180;
      double cos = Math.Cos(rad);
      double sin = Math.Sin(rad);
      ApplyMatrix(cos, sin, -sin, cos, 0, 0);
    }

    /**
      <summary>Applies a rotation to the coordinate system from user space to device space [PDF:1.6:4.2.2].</summary>
      <param name="angle">Rotational counterclockwise angle.</param>
      <param name="origin">Rotational pivot point; it becomes the new coordinates origin.</param>
    */
    public void Rotate(
      float angle,
      PointF origin
      )
    {
      // Center to the new origin!
      Translate(
        origin.X,
        scanner.ContentContext.Box.Height - origin.Y
        );
      // Rotate on the new origin!
      Rotate(angle);
      // Restore the standard vertical coordinates system!
      Translate(
        0,
        -scanner.ContentContext.Box.Height
        );
    }

    /**
      <summary>Applies a scaling to the coordinate system from user space to device space
      [PDF:1.6:4.2.2].</summary>
      <param name="ratioX">Horizontal scaling ratio.</param>
      <param name="ratioY">Vertical scaling ratio.</param>
    */
    public void Scale(
      float ratioX,
      float ratioY
      )
    {ApplyMatrix(ratioX, 0, 0, ratioY, 0, 0);}

    /**
      <summary>Sets the character spacing parameter [PDF:1.6:5.2.1].</summary>
    */
    public void SetCharSpace(
      float value
      )
    {Add(new objects::SetCharSpace(value));}

    /**
      <summary>Sets the nonstroking color value [PDF:1.6:4.5.7].</summary>
    */
    public void SetFillColor(
      colors::Color value
      )
    {
      if(!scanner.State.FillColorSpace.Equals(value.ColorSpace))
      {
        // Set filling color space!
        Add(
          new objects::SetFillColorSpace(
            GetColorSpaceName(
              value.ColorSpace
              )
            )
          );
      }

      Add(new objects::SetFillColor(value));
    }

    /**
      <summary>Sets the font [PDF:1.6:5.2].</summary>
      <param name="name">Resource identifier of the font.</param>
      <param name="size">Scaling factor (points).</param>
    */
    public void SetFont(
      PdfName name,
      float size
      )
    {
      // Doesn't the font exist in the context resources?
      if(!scanner.ContentContext.Resources.Fonts.ContainsKey(name))
        throw new ArgumentException("No font resource associated to the given argument.","name");

      Add(new objects::SetFont(name,size));
    }

    /**
      <summary>Sets the font [PDF:1.6:5.2].</summary>
      <remarks>The <paramref cref="value"/> is checked for presence in the
      current resource dictionary: if it isn't available, it's automatically added.
      If you need to avoid such a behavior, use <see cref="SetFont(PdfName,float)"/>.</remarks>
      <param name="value">Font.</param>
      <param name="size">Scaling factor (points).</param>
    */
    public void SetFont(
      fonts::Font value,
      float size
      )
    {SetFont(GetFontName(value),size);}

    /**
      <summary>Sets the text horizontal scaling [PDF:1.6:5.2.3].</summary>
    */
    public void SetTextScale(
      float value
      )
    {Add(new objects::SetTextScale(value));}

    /**
      <summary>Sets the text leading [PDF:1.6:5.2.4].</summary>
    */
    public void SetTextLead(
      float value
      )
    {Add(new objects::SetTextLead(value));}

    /**
      <summary>Sets the line cap style [PDF:1.6:4.3.2].</summary>
    */
    public void SetLineCap(
      LineCapEnum value
      )
    {Add(new objects::SetLineCap(value));}

    /**
      <summary>Sets the line dash pattern [PDF:1.6:4.3.2].</summary>
      <param name="phase">Distance into the dash pattern at which to start the dash.</param>
      <param name="unitsOn">Length of evenly alternating dashes and gaps.</param>
    */
    public void SetLineDash(
      int phase,
      int unitsOn
      )
    {SetLineDash(phase,unitsOn,unitsOn);}

    /**
      <summary>Sets the line dash pattern [PDF:1.6:4.3.2].</summary>
      <param name="phase">Distance into the dash pattern at which to start the dash.</param>
      <param name="unitsOn">Length of dashes.</param>
      <param name="unitsOff">Length of gaps.</param>
    */
    public void SetLineDash(
      int phase,
      int unitsOn,
      int unitsOff
      )
    {Add(new objects::SetLineDash(phase,unitsOn,unitsOff));}

    /**
      <summary>Sets the line join style [PDF:1.6:4.3.2].</summary>
    */
    public void SetLineJoin(
      LineJoinEnum value
      )
    {Add(new objects::SetLineJoin(value));}

    /**
      <summary>Sets the line width [PDF:1.6:4.3.2].</summary>
    */
    public void SetLineWidth(
      float value
      )
    {Add(new objects::SetLineWidth(value));}

    /**
      <summary>Sets the transformation of the coordinate system from user space
      to device space [PDF:1.6:4.3.3].</summary>
      <param name="a">Item 0,0 of the matrix.</param>
      <param name="b">Item 0,1 of the matrix.</param>
      <param name="c">Item 1,0 of the matrix.</param>
      <param name="d">Item 1,1 of the matrix.</param>
      <param name="e">Item 2,0 of the matrix.</param>
      <param name="f">Item 2,1 of the matrix.</param>
    */
    public void SetMatrix(
      float a,
      float b,
      float c,
      float d,
      float e,
      float f
      )
    {
      // Reset the CTM!
      Add(objects::ModifyCTM.GetResetCTM(scanner.State));
      // Apply the transformation!
      Add(new objects::ModifyCTM(a,b,c,d,e,f));
    }

    /**
      <summary>Sets the miter limit [PDF:1.6:4.3.2].</summary>
    */
    public void SetMiterLimit(
      float value
      )
    {Add(new objects::SetMiterLimit(value));}

    /**
      <summary>Sets the stroking color value [PDF:1.6:4.5.7].</summary>
    */
    public void SetStrokeColor(
      colors::Color value
      )
    {
      if(!scanner.State.StrokeColorSpace.Equals(value.ColorSpace))
      {
        // Set stroking color space!
        Add(
          new objects::SetStrokeColorSpace(
            GetColorSpaceName(
              value.ColorSpace
              )
            )
          );
      }

      Add(new objects::SetStrokeColor(value));
    }

    /**
      <summary>Sets the text rendering mode [PDF:1.6:5.2.5].</summary>
    */
    public void SetTextRenderMode(
      TextRenderModeEnum value
      )
    {Add(new objects::SetTextRenderMode(value));}

    /**
      <summary>Sets the text rise [PDF:1.6:5.2.6].</summary>
    */
    public void SetTextRise(
      float value
      )
    {Add(new objects::SetTextRise(value));}

    /**
      <summary>Sets the word spacing [PDF:1.6:5.2.2].</summary>
    */
    public void SetWordSpace(
      float value
      )
    {Add(new objects::SetWordSpace(value));}

    /**
      <summary>Shows the specified text on the page at the current location
      [PDF:1.6:5.3.2].</summary>
      <param name="value">Text to show.</param>
      <returns>Bounding box vertices in default user space units.</returns>
    */
    public PointF[] ShowText(
      string value
      )
    {
      return ShowText(
        value,
        new PointF(0,0)
        );
    }

    /**
      <summary>Shows the link associated to the specified text on the page at the current location.</summary>
      <param name="value">Text to show.</param>
      <param name="action">Action to apply when the link is activated.</param>
      <returns>Link.</returns>
    */
    public Link ShowText(
      string value,
      actions::Action action
      )
    {
      return ShowText(
        value,
        new PointF(0,0),
        action
        );
    }

    /**
      <summary>Shows the specified text on the page at the specified location
      [PDF:1.6:5.3.2].</summary>
      <param name="value">Text to show.</param>
      <param name="location">Position at which showing the text.</param>
      <returns>Bounding box vertices in default user space units.</returns>
    */
    public PointF[] ShowText(
      string value,
      PointF location
      )
    {
      return ShowText(
        value,
        location,
        AlignmentXEnum.Left,
        AlignmentYEnum.Top,
        0
        );
    }

    /**
      <summary>Shows the link associated to the specified text on the page at the specified location.</summary>
      <param name="value">Text to show.</param>
      <param name="location">Position at which showing the text.</param>
      <param name="action">Action to apply when the link is activated.</param>
      <returns>Link.</returns>
    */
    public Link ShowText(
      string value,
      PointF location,
      actions::Action action
      )
    {
      return ShowText(
        value,
        location,
        AlignmentXEnum.Left,
        AlignmentYEnum.Top,
        0,
        action
        );
    }

    /**
      <summary>Shows the specified text on the page at the specified location
      [PDF:1.6:5.3.2].</summary>
      <param name="value">Text to show.</param>
      <param name="location">Anchor position at which showing the text.</param>
      <param name="alignmentX">Horizontal alignment.</param>
      <param name="alignmentY">Vertical alignment.</param>
      <param name="rotation">Rotational counterclockwise angle.</param>
      <returns>Bounding box vertices in default user space units.</returns>
    */
    public PointF[] ShowText(
      string value,
      PointF location,
      AlignmentXEnum alignmentX,
      AlignmentYEnum alignmentY,
      float rotation
      )
    {
      ContentScanner.GraphicsState state = scanner.State;
      fonts::Font font = state.Font;
      float fontSize = state.FontSize;
      float x = location.X;
      float y = location.Y;
      float width = font.GetKernedWidth(value,fontSize);
      float height = font.GetLineHeight(fontSize);
      float descent = font.GetDescent(fontSize);

      PointF[] frame = new PointF[4];

      if(alignmentX == AlignmentXEnum.Left
        && alignmentY == AlignmentYEnum.Top)
      {
        BeginText();
        try
        {
          if(rotation == 0)
          {
            TranslateText(
              x,
              scanner.ContentContext.Box.Height - y - font.GetAscent(fontSize)
              );
          }
          else
          {
            double rad = rotation * Math.PI / 180.0;
            double cos = Math.Cos(rad);
            double sin = Math.Sin(rad);

            SetTextMatrix(
              cos,
              sin,
              -sin,
              cos,
              x,
              scanner.ContentContext.Box.Height - y - font.GetAscent(fontSize)
              );
          }

          state = scanner.State;
          frame[0] = state.TextToDeviceSpace(new PointF(0,descent), true);
          frame[1] = state.TextToDeviceSpace(new PointF(width,descent), true);
          frame[2] = state.TextToDeviceSpace(new PointF(width,height+descent), true);
          frame[3] = state.TextToDeviceSpace(new PointF(0,height+descent), true);

          // Add the text!
          Add(new objects::ShowSimpleText(font.Encode(value)));
        }
        catch(Exception e)
        {throw new Exception("Failed to show text.", e);}
        finally
        {End(); /* Ends the text object. */}
      }
      else
      {
        BeginLocalState();
        try
        {
          // Coordinates transformation.
          double cos, sin;
          if(rotation == 0)
          {
            cos = 1;
            sin = 0;
          }
          else
          {
            double rad = rotation * Math.PI / 180.0;
            cos = Math.Cos(rad);
            sin = Math.Sin(rad);
          }
          // Apply the transformation!
          ApplyMatrix(
            cos,
            sin,
            -sin,
            cos,
            x,
            scanner.ContentContext.Box.Height - y
            );

          // Begin the text object!
          BeginText();
          try
          {
            // Text coordinates adjustment.
            switch(alignmentX)
            {
              case AlignmentXEnum.Left:
                x = 0;
                break;
              case AlignmentXEnum.Right:
                x = -width;
                break;
              case AlignmentXEnum.Center:
              case AlignmentXEnum.Justify:
                x = -width / 2;
                break;
            }
            switch(alignmentY)
            {
              case AlignmentYEnum.Top:
                y = -font.GetAscent(fontSize);
                break;
              case AlignmentYEnum.Bottom:
                y = height - font.GetAscent(fontSize);
                break;
              case AlignmentYEnum.Middle:
                y = height / 2 - font.GetAscent(fontSize);
                break;
            }
            // Apply the text coordinates adjustment!
            TranslateText(x,y);

            state = scanner.State;
            frame[0] = state.TextToDeviceSpace(new PointF(0,descent), true);
            frame[1] = state.TextToDeviceSpace(new PointF(width,descent), true);
            frame[2] = state.TextToDeviceSpace(new PointF(width,height+descent), true);
            frame[3] = state.TextToDeviceSpace(new PointF(0,height+descent), true);

            // Add the text!
            Add(new objects::ShowSimpleText(font.Encode(value)));
          }
          catch (Exception e)
          {throw new Exception("Failed to show text.", e);}
          finally
          {End(); /* Ends the text object. */}
        }
        catch(Exception e)
        {throw new Exception("Failed to show text.", e);}
        finally
        {End(); /* Ends the local state. */}
      }

      return frame;
    }

    /**
      <summary>Shows the link associated to the specified text on the page at the specified location.</summary>
      <param name="value">Text to show.</param>
      <param name="location">Anchor position at which showing the text.</param>
      <param name="alignmentX">Horizontal alignment.</param>
      <param name="alignmentY">Vertical alignment.</param>
      <param name="rotation">Rotational counterclockwise angle.</param>
      <param name="action">Action to apply when the link is activated.</param>
      <returns>Link.</returns>
    */
    public Link ShowText(
      string value,
      PointF location,
      AlignmentXEnum alignmentX,
      AlignmentYEnum alignmentY,
      float rotation,
      actions::Action action
      )
    {
      PointF[] textFramePoints = ShowText(
        value,
        location,
        alignmentX,
        alignmentY,
        rotation
        );

      IContentContext contentContext = scanner.ContentContext;
      if(!(contentContext is Page))
        throw new Exception("Link can be shown only on page contexts.");

      Page page = (Page)contentContext;
      RectangleF linkBox = new RectangleF(textFramePoints[0].X,textFramePoints[0].Y,0,0);
      for(
        int index = 1,
          length = textFramePoints.Length;
        index < length;
        index++
        )
      {
        PointF textFramePoint = textFramePoints[index];
        if(textFramePoint.X < linkBox.X)
        {
          linkBox.Width = linkBox.Right - textFramePoint.X;
          linkBox.X = textFramePoint.X;
        }
        else if(textFramePoint.X > linkBox.Right)
        {linkBox.Width = textFramePoint.X - linkBox.X;}
        if(textFramePoint.Y < linkBox.Y)
        {
          linkBox.Height = linkBox.Bottom - textFramePoint.Y;
          linkBox.Y = textFramePoint.Y;
        }
        else if(textFramePoint.Y > linkBox.Bottom)
        {linkBox.Height = textFramePoint.Y - linkBox.Y;}
      }

      return new Link(
        page,
        linkBox,
        action
        );
    }

    /**
      <summary>Shows the specified external object [PDF:1.6:4.7].</summary>
      <param name="name">Resource identifier of the external object.</param>
    */
    public void ShowXObject(
      PdfName name
      )
    {Add(new objects::PaintXObject(name));}

    /**
      <summary>Shows the specified external object [PDF:1.6:4.7].</summary>
      <remarks>The <paramref cref="value"/> is checked for presence in the
      current resource dictionary: if it isn't available, it's automatically added.
      If you need to avoid such a behavior, use <see cref="ShowXObject(PdfName)"/>.</remarks>
      <param name="value">External object.</param>
    */
    public void ShowXObject(
      XObject value
      )
    {ShowXObject(GetXObjectName(value));}

    /**
      <summary>Shows the specified external object at the specified position
      [PDF:1.6:4.7].</summary>
      <param name="name">Resource identifier of the external object.</param>
      <param name="location">Position at which showing the external object.</param>
    */
    public void ShowXObject(
      PdfName name,
      PointF location
      )
    {
      ShowXObject(
        name,
        location,
        new SizeF(0,0)
        );
    }

    /**
      <summary>Shows the specified external object at the specified position
      [PDF:1.6:4.7].</summary>
      <remarks>The <paramref cref="value"/> is checked for presence in the
      current resource dictionary: if it isn't available, it's automatically added.
      If you need to avoid such a behavior, use <see cref="ShowXObject(PdfName,PointF)"/>.</remarks>
      <param name="value">External object.</param>
      <param name="location">Position at which showing the external object.</param>
    */
    public void ShowXObject(
      XObject value,
      PointF location
      )
    {
      ShowXObject(
        GetXObjectName(value),
        location
        );
    }

    /**
      <summary>Shows the specified external object at the specified position
      [PDF:1.6:4.7].</summary>
      <param name="name">Resource identifier of the external object.</param>
      <param name="location">Position at which showing the external object.</param>
      <param name="size">Size of the external object.</param>
    */
    public void ShowXObject(
      PdfName name,
      PointF location,
      SizeF size
      )
    {
      ShowXObject(
        name,
        location,
        size,
        AlignmentXEnum.Left,
        AlignmentYEnum.Top,
        0
        );
    }

    /**
      <summary>Shows the specified external object at the specified position
      [PDF:1.6:4.7].</summary>
      <remarks>The <paramref cref="value"/> is checked for presence in the
      current resource dictionary: if it isn't available, it's automatically added.
      If you need to avoid such a behavior, use <see cref="ShowXObject(PdfName,PointF,SizeF)"/>.</remarks>
      <param name="value">External object.</param>
      <param name="location">Position at which showing the external object.</param>
      <param name="size">Size of the external object.</param>
    */
    public void ShowXObject(
      XObject value,
      PointF location,
      SizeF size
      )
    {
      ShowXObject(
        GetXObjectName(value),
        location,
        size
        );
    }

    /**
      <summary>Shows the specified external object at the specified position
      [PDF:1.6:4.7].</summary>
      <param name="name">Resource identifier of the external object.</param>
      <param name="location">Position at which showing the external object.</param>
      <param name="size">Size of the external object.</param>
      <param name="alignmentX">Horizontal alignment.</param>
      <param name="alignmentY">Vertical alignment.</param>
      <param name="rotation">Rotational counterclockwise angle.</param>
    */
    public void ShowXObject(
      PdfName name,
      PointF location,
      SizeF size,
      AlignmentXEnum alignmentX,
      AlignmentYEnum alignmentY,
      float rotation
      )
    {
      XObject xObject = scanner.ContentContext.Resources.XObjects[name];

      // Adjusting default dimensions...
      /*
        NOTE: Zero-valued dimensions represent default proportional dimensions.
      */
      SizeF xObjectSize = xObject.Size;
      if(size.Width == 0)
      {
        if(size.Height == 0)
        {
          size.Width = xObjectSize.Width;
          size.Height = xObjectSize.Height;
        }
        else
        {size.Width = size.Height * xObjectSize.Width / xObjectSize.Height;}
      }
      else if(size.Height == 0)
      {size.Height = size.Width * xObjectSize.Height / xObjectSize.Width;}

      // Scaling.
      double[] matrix = xObject.GetMatrix();
      double scaleX, scaleY;
      scaleX = size.Width / (xObjectSize.Width * matrix[0]);
      scaleY = size.Height / (xObjectSize.Height * matrix[3]);

      // Alignment.
      float locationOffsetX, locationOffsetY;
      switch(alignmentX)
      {
        case AlignmentXEnum.Left: locationOffsetX = 0; break;
        case AlignmentXEnum.Right: locationOffsetX = size.Width; break;
        case AlignmentXEnum.Center:
        case AlignmentXEnum.Justify:
        default: locationOffsetX = size.Width / 2; break;
      }
      switch(alignmentY)
      {
        case AlignmentYEnum.Top: locationOffsetY = size.Height; break;
        case AlignmentYEnum.Bottom: locationOffsetY = 0; break;
        case AlignmentYEnum.Middle:
        default: locationOffsetY = size.Height / 2; break;
      }

      BeginLocalState();
      try
      {
        Translate(
          location.X,
          scanner.ContentContext.Box.Height - location.Y
          );
        if(rotation != 0)
        {Rotate(rotation);}
        ApplyMatrix(
          scaleX, 0, 0,
          scaleY,
          -locationOffsetX,
          -locationOffsetY
          );
        ShowXObject(name);
      }
      catch (Exception e)
      {throw new Exception("Failed to show the xobject.",e);}
      finally
      {End(); /* Ends the local state. */}
    }

    /**
      <summary>Shows the specified external object at the specified position
      [PDF:1.6:4.7].</summary>
      <remarks>The <paramref cref="value"/> is checked for presence in the
      current resource dictionary: if it isn't available, it's automatically added.
      If you need to avoid such a behavior, use <see cref="ShowXObject(PdfName,PointF,SizeF,AlignmentXEnum,AlignmentYEnum,double)"/>.</remarks>
      <param name="value">External object.</param>
      <param name="location">Position at which showing the external object.</param>
      <param name="size">Size of the external object.</param>
      <param name="alignmentX">Horizontal alignment.</param>
      <param name="alignmentY">Vertical alignment.</param>
      <param name="rotation">Rotational counterclockwise angle.</param>
    */
    public void ShowXObject(
      XObject value,
      PointF location,
      SizeF size,
      AlignmentXEnum alignmentX,
      AlignmentYEnum alignmentY,
      float rotation
      )
    {
      ShowXObject(
        GetXObjectName(value),
        location,
        size,
        alignmentX,
        alignmentY,
        rotation
        );
    }

    /**
      <summary>Strokes the path using the current color [PDF:1.6:4.4.2].</summary>
    */
    public void Stroke(
      )
    {Add(objects::PaintPath.Stroke);}

    /**
      <summary>Applies a translation to the coordinate system from user space
      to device space [PDF:1.6:4.2.2].</summary>
      <param name="distanceX">Horizontal distance.</param>
      <param name="distanceY">Vertical distance.</param>
    */
    public void Translate(
      float distanceX,
      float distanceY
      )
    {ApplyMatrix(1, 0, 0, 1, distanceX, distanceY);}
    #endregion

    #region private
    /**
      <summary>Begins a subpath [PDF:1.6:4.4.1].</summary>
      <param name="startPoint">Starting point.</param>
    */
    private void BeginSubpath(
      PointF startPoint
      )
    {
      Add(
        new objects::BeginSubpath(
          startPoint.X,
          scanner.ContentContext.Box.Height - startPoint.Y
          )
        );
    }

    /**
      <summary>Begins a text object [PDF:1.6:5.3].</summary>
    */
    private objects::Text BeginText(
      )
    {return (objects::Text)Begin(new objects::Text());}

    //TODO: drawArc MUST seamlessly manage already-begun paths.
    private void DrawArc(
      RectangleF location,
      float startAngle,
      float endAngle,
      float branchWidth,
      float branchRatio,
      bool beginPath
      )
    {
      /*
        NOTE: Strictly speaking, arc drawing is NOT a PDF primitive;
        it leverages the cubic bezier curve operator (thanks to
        G. Adam Stanislav, whose article was greatly inspirational:
        see http://www.whizkidtech.redprince.net/bezier/circle/).
      */

      if(startAngle > endAngle)
      {
        float swap = startAngle;
        startAngle = endAngle;
        endAngle = swap;
      }

      float radiusX = location.Width / 2;
      float radiusY = location.Height / 2;

      PointF center = new PointF(
        (float)(location.X + radiusX),
        (float)(location.Y + radiusY)
        );

      double radians1 = (Math.PI / 180) * startAngle;
      PointF point1 = new PointF(
        (float)(center.X + Math.Cos(radians1) * radiusX),
        (float)(center.Y - Math.Sin(radians1) * radiusY)
        );

      if(beginPath)
      {BeginSubpath(point1);}

      double endRadians = (Math.PI / 180) * endAngle;
      double quadrantRadians = Math.PI / 2;
      double radians2 = Math.Min(
        radians1 + quadrantRadians - radians1 % quadrantRadians,
        endRadians
        );
      double kappa = 0.5522847498;
      while(true)
      {
        double segmentX = radiusX * kappa;
        double segmentY = radiusY * kappa;

        // Endpoint 2.
        PointF point2 = new PointF(
          (float)(center.X + Math.Cos(radians2) * radiusX),
          (float)(center.Y - Math.Sin(radians2) * radiusY)
          );

        // Control point 1.
        double tangentialRadians1 = Math.Atan(
          -(Math.Pow(radiusY,2) * (point1.X-center.X))
            / (Math.Pow(radiusX,2) * (point1.Y-center.Y))
          );
        double segment1 = (
          segmentY * (1 - Math.Abs(Math.Sin(radians1)))
            + segmentX * (1 - Math.Abs(Math.Cos(radians1)))
          ) * (radians2-radians1) / quadrantRadians; // TODO: control segment calculation is still not so accurate as it should -- verify how to improve it!!!
        PointF control1 = new PointF(
          (float)(point1.X + Math.Abs(Math.Cos(tangentialRadians1) * segment1) * Math.Sign(-Math.Sin(radians1))),
          (float)(point1.Y + Math.Abs(Math.Sin(tangentialRadians1) * segment1) * Math.Sign(-Math.Cos(radians1)))
          );

        // Control point 2.
        double tangentialRadians2 = Math.Atan(
          -(Math.Pow(radiusY,2) * (point2.X-center.X))
            / (Math.Pow(radiusX,2) * (point2.Y-center.Y))
          );
        double segment2 = (
          segmentY * (1 - Math.Abs(Math.Sin(radians2)))
            + segmentX * (1 - Math.Abs(Math.Cos(radians2)))
          ) * (radians2-radians1) / quadrantRadians; // TODO: control segment calculation is still not so accurate as it should -- verify how to improve it!!!
        PointF control2 = new PointF(
          (float)(point2.X + Math.Abs(Math.Cos(tangentialRadians2) * segment2) * Math.Sign(Math.Sin(radians2))),
          (float)(point2.Y + Math.Abs(Math.Sin(tangentialRadians2) * segment2) * Math.Sign(Math.Cos(radians2)))
          );

        // Draw the current quadrant curve!
        DrawCurve(
          point2,
          control1,
          control2
          );

        // Last arc quadrant?
        if(radians2 == endRadians)
          break;

        // Preparing the next quadrant iteration...
        point1 = point2;
        radians1 = radians2;
        radians2 += quadrantRadians;
        if(radians2 > endRadians)
        {radians2 = endRadians;}

        double quadrantRatio = (radians2 - radians1) / quadrantRadians;
        radiusX += (float)(branchWidth * quadrantRatio);
        radiusY += (float)(branchWidth * quadrantRatio);

        branchWidth *= branchRatio;
      }
    }

    private PdfName GetFontName(
      fonts::Font value
      )
    {
      // Ensuring that the font exists within the context resources...
      Resources resources = scanner.ContentContext.Resources;
      FontResources fonts = resources.Fonts;
      // No font resources collection?
      if(fonts == null)
      {
        // Create the font resources collection!
        fonts = new FontResources(scanner.Contents.Document);
        resources.Fonts = fonts;
      }
      // Get the key associated to the font!
      PdfName name = fonts.BaseDataObject.GetKey(value.BaseObject);
      // No key found?
      if(name == null)
      {
        // Insert the font within the resources!
        int fontIndex = fonts.Count;
        do
        {name = new PdfName((++fontIndex).ToString());}
        while(fonts.ContainsKey(name));
        fonts[name] = value;
      }

      return name;
    }

    private PdfName GetXObjectName(
      XObject value
      )
    {
      // Ensuring that the external object exists within the context resources...
      Resources resources = scanner.ContentContext.Resources;
      XObjectResources xObjects = resources.XObjects;
      // No external object resources collection?
      if(xObjects == null)
      {
        // Create the external object resources collection!
        xObjects = new XObjectResources(scanner.Contents.Document);
        resources.XObjects = xObjects;
      }
      // Get the key associated to the external object!
      PdfName name = xObjects.BaseDataObject.GetKey(value.BaseObject);
      // No key found?
      if(name == null)
      {
        // Insert the external object within the resources!
        int xObjectIndex = xObjects.Count;
        do
        {name = new PdfName((++xObjectIndex).ToString());}
        while(xObjects.ContainsKey(name));
        xObjects[name] = value;
      }

      return name;
    }

    private PdfName GetColorSpaceName(
      colors::ColorSpace value
      )
    {
      if(value is colors::DeviceGrayColorSpace)
      {return PdfName.DeviceGray;}
      else if(value is colors::DeviceRGBColorSpace)
      {return PdfName.DeviceRGB;}
      else if(value is colors::DeviceCMYKColorSpace)
      {return PdfName.DeviceCMYK;}
      else
        throw new NotImplementedException("colorSpace MUST be converted to its associated name; you need to implement a method in PdfDictionary that, given a PdfDirectObject, returns its associated key.");
    }

    /**
      <summary>Applies a rotation to the coordinate system from text space to user space
      [PDF:1.6:4.2.2].</summary>
      <param name="angle">Rotational counterclockwise angle.</param>
    */
    private void RotateText(
      float angle
      )
    {
      double rad = angle * Math.PI / 180;
      double cos = Math.Cos(rad);
      double sin = Math.Sin(rad);

      SetTextMatrix(cos, sin, -sin, cos, 0, 0);
    }

    /**
      <summary>Applies a scaling to the coordinate system from text space to user space
      [PDF:1.6:4.2.2].</summary>
      <param name="ratioX">Horizontal scaling ratio.</param>
      <param name="ratioY">Vertical scaling ratio.</param>
    */
    private void ScaleText(
      float ratioX,
      float ratioY
      )
    {SetTextMatrix(ratioX, 0, 0, ratioY, 0, 0);}

    /**
      <summary>Sets the transformation of the coordinate system from text space to user space
      [PDF:1.6:5.3.1].</summary>
      <remarks>The transformation replaces the current text matrix.</remarks>
      <param name="a">Item 0,0 of the matrix.</param>
      <param name="b">Item 0,1 of the matrix.</param>
      <param name="c">Item 1,0 of the matrix.</param>
      <param name="d">Item 1,1 of the matrix.</param>
      <param name="e">Item 2,0 of the matrix.</param>
      <param name="f">Item 2,1 of the matrix.</param>
    */
    private void SetTextMatrix(
      double a,
      double b,
      double c,
      double d,
      double e,
      double f
      )
    {Add(new objects::SetTextMatrix(a,b,c,d,e,f));}

    /**
      <summary>Applies a translation to the coordinate system from text space
      to user space [PDF:1.6:4.2.2].</summary>
      <param name="distanceX">Horizontal distance.</param>
      <param name="distanceY">Vertical distance.</param>
    */
    private void TranslateText(
      float distanceX,
      float distanceY
      )
    {SetTextMatrix(1, 0, 0, 1, distanceX, distanceY);}

    /**
      <summary>Applies a translation to the coordinate system from text space to user space,
      relative to the start of the current line [PDF:1.6:5.3.1].</summary>
      <param name="offsetX">Horizontal offset.</param>
      <param name="offsetY">Vertical offset.</param>
    */
    private void TranslateTextRelative(
      float offsetX,
      float offsetY
      )
    {
      Add(
        new objects::TranslateTextRelative(
          offsetX,
          -offsetY
          )
        );
    }

    /**
      <summary>Applies a translation to the coordinate system from text space to user space,
      moving to the start of the next line [PDF:1.6:5.3.1].</summary>
    */
    private void TranslateTextToNextLine(
      )
    {Add(objects::TranslateTextToNextLine.Value);}
    #endregion
    #endregion
    #endregion
  }
}