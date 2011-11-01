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
using fonts = org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.documents.contents.composition
{
  /**
    <summary>
      <para>Content block composer.</para>
      <para>It provides content positioning functionalities for page typesetting.</para>
    </summary>
  */
  /*
    TODO: Manage all the graphics parameters (especially
    those text-related, like horizontal scaling etc.) using ContentScanner -- see PDF:1.6:5.2-3!!!
  */
  public sealed class BlockComposer
  {
    #region types
    private sealed class ContentPlaceholder
      : Operation
    {
      public List<ContentObject> objects = new List<ContentObject>();

      public ContentPlaceholder(
        ) : base(null)
      {}

      public List<ContentObject> Objects
      {get{return objects;}}

      public override void WriteTo(
        IOutputStream stream,
        Document context
        )
      {
        foreach(ContentObject obj in objects)
        {obj.WriteTo(stream, context);}
      }
    }

    private sealed class Row
    {
      /**
        <summary>Row's objects.</summary>
      */
      public List<RowObject> Objects = new List<RowObject>();
      /**
        <summary>Number of space characters.</summary>
      */
      public int SpaceCount = 0;
      /**
        <summary>Row's graphics objects container.</summary>
      */
      public ContentPlaceholder Container;

      public double Height;
      /**
        <summary>Vertical location relative to the block frame.</summary>
      */
      public double Y;
      public double Width;

      internal Row(
        ContentPlaceholder container,
        double y
        )
      {
        this.Container = container;
        this.Y = y;
      }
    }

    private sealed class RowObject
    {
      /**
        <summary>Row object's graphics objects container.</summary>
      */
      public ContainerObject Container;

      public double Height;
      public double Width;

      public int SpaceCount;

      internal RowObject(
        ContainerObject container,
        double height,
        double width,
        int spaceCount
        )
      {
        this.Container = container;
        this.Height = height;
        this.Width = width;
        this.SpaceCount = spaceCount;
      }
    }
    #endregion

    #region dynamic
    /*
      NOTE: In order to provide fine-grained alignment,
      there are 2 postproduction state levels:
        1- row level (see EndRow());
        2- block level (see End()).

      NOTE: Graphics instructions' layout follows this scheme (XS-BNF syntax):
        block = { beginLocalState translation parameters rows endLocalState }
        beginLocalState { "q\r" }
        translation = { "1 0 0 1 " number ' ' number "cm\r" }
        parameters = { ... } // Graphics state parameters.
        rows = { row* }
        row = { object* }
        object = { parameters beginLocalState translation content endLocalState }
        content = { ... } // Text, image (and so on) showing operators.
        endLocalState = { "Q\r" }
      NOTE: all the graphics state parameters within a block are block-level ones,
      i.e. they can't be represented inside row's or row object's local state, in order to
      facilitate parameter reuse within the same block.
    */
    #region fields
    private readonly PrimitiveComposer baseComposer;
    private readonly ContentScanner scanner;

    private AlignmentXEnum alignmentX;
    private AlignmentYEnum alignmentY;
    private bool hyphenation;
    private Length lineSpace = new Length(0, Length.UnitModeEnum.Relative);

    /** <summary>Available area where to render the block contents inside.</summary> */
    private RectangleF frame;
    /** <summary>Actual area occupied by the block contents.</summary> */
    private RectangleF boundBox;

    private Row currentRow;
    private bool rowEnded;

    private LocalGraphicsState container;
    #endregion

    #region constructors
    public BlockComposer(
      PrimitiveComposer baseComposer
      )
    {
      this.baseComposer = baseComposer;
      this.scanner = baseComposer.Scanner;
    }
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the base composer.</summary>
    */
    public PrimitiveComposer BaseComposer
    {get{return baseComposer;}}

    /**
      <summary>Begins a content block.</summary>
    */
    public void Begin(
      RectangleF frame,
      AlignmentXEnum alignmentX,
      AlignmentYEnum alignmentY
      )
    {
      this.frame = frame;
      this.alignmentX = alignmentX;
      this.alignmentY = alignmentY;

      // Open the block local state!
      /*
        NOTE: This device allows a fine-grained control over the block representation.
        It MUST be coupled with a closing statement on block end.
      */
      container = baseComposer.BeginLocalState();

      boundBox = new RectangleF(
        frame.X,
        frame.Y,
        frame.Width,
        0
        );

      BeginRow();
    }

    /**
      <summary>Gets the actual area occupied by the block contents.</summary>
    */
    public RectangleF BoundBox
    {get{return boundBox;}}

    /**
      <summary>Ends the content block.</summary>
    */
    public void End(
      )
    {
      // End last row!
      EndRow(true);

      // Block translation.
      container.Objects.Insert(
        0,
        new ModifyCTM(
          1, 0, 0, 1,
          boundBox.X, // Horizontal translation.
          -boundBox.Y // Vertical translation.
          )
        );

      // Close the block local state!
      baseComposer.End();
    }

    /**
      <summary>Gets the available area where to render the block contents inside.</summary>
    */
    public RectangleF Frame
    {get{return frame;}}

    /**
      <summary>Gets/Sets whether the hyphenation algorithm has to be applied.</summary>
    */
    public bool Hyphenation
    {
      get{return hyphenation;}
      set{hyphenation = value;}
    }

    /**
      <summary>Gets/Sets the text interline spacing.</summary>
    */
    public Length LineSpace
    {
      get{return lineSpace;}
      set{lineSpace = value;}
    }

    /**
      <summary>Gets the content scanner.</summary>
    */
    public ContentScanner Scanner
    {get{return scanner;}}

    /**
      <summary>Ends current paragraph.</summary>
    */
    public void ShowBreak(
      )
    {
      EndRow(true);
      BeginRow();
    }

    /**
      <summary>Ends current paragraph, specifying the offset of the next one.</summary>
      <remarks>This functionality allows higher-level features such as paragraph indentation
      and margin.</remarks>
      <param name="offset">Relative location of the next paragraph.</param>
    */
    public void ShowBreak(
      SizeF offset
      )
    {
      ShowBreak();

      currentRow.Y += offset.Height;
      currentRow.Width = offset.Width;
    }

    /**
      <summary>Ends current paragraph, specifying the alignment of the next one.</summary>
      <remarks>This functionality allows higher-level features such as paragraph indentation and margin.</remarks>
      <param name="alignmentX">Horizontal alignment.</param>
    */
    public void ShowBreak(
      AlignmentXEnum alignmentX
      )
    {
      ShowBreak();

      this.alignmentX = alignmentX;
    }

    /**
      <summary>Ends current paragraph, specifying the offset and alignment of the next one.</summary>
      <remarks>This functionality allows higher-level features such as paragraph indentation and margin.</remarks>
      <param name="offset">Relative location of the next paragraph.</param>
      <param name="alignmentX">Horizontal alignment.</param>
    */
    public void ShowBreak(
      SizeF offset,
      AlignmentXEnum alignmentX
      )
    {
      ShowBreak(offset);

      this.alignmentX = alignmentX;
    }

    /**
      <summary>Shows text.</summary>
      <returns>Last shown character index.</returns>
    */
    public int ShowText(
      string text
      )
    {
      if(currentRow == null
        || text == null)
        return 0;

      ContentScanner.GraphicsState state = baseComposer.State;
      fonts::Font font = state.Font;
      double fontSize = state.FontSize;
      double lineHeight = font.GetLineHeight(fontSize);

      TextFitter textFitter = new TextFitter(
        text,
        0,
        font,
        fontSize,
        hyphenation
        );
      int textLength = text.Length;
      int index = 0;

      while(true)
      {
        // Beginning of current row?
        if(currentRow.Width == 0)
        {
          // Removing leading space...
          while(true)
          {
            // Did we reach the text end?
            if(index == textLength)
              goto endTextShowing; // NOTE: I know GOTO is evil, yet in this case it's a much cleaner solution.

            if(text[index] != ' ')
              break;

            index++;
          }
        }

        // Text height: exceeds current row's height?
        if(lineHeight > currentRow.Height)
        {
          // Text height: exceeds block's remaining vertical space?
          if(lineHeight > frame.Height - currentRow.Y) // Text exceeds.
          {
            // Terminate the current row!
            EndRow(false);
            goto endTextShowing; // NOTE: I know GOTO is evil, yet in this case it's a much cleaner solution.
          }
          else // Text doesn't exceed.
          {
            // Adapt current row's height!
            currentRow.Height = lineHeight;
          }
        }

        // Does the text fit?
        if(textFitter.Fit(
          index,
          frame.Width - currentRow.Width // Remaining row width.
          ))
        {
          // Get the fitting text!
          string textChunk = textFitter.FittedText;
          double textChunkWidth = textFitter.FittedWidth;
          PointF textChunkLocation = new PointF(
            (float)currentRow.Width,
            (float)currentRow.Y
            );

          // Render the fitting text:
          // - open the row object's local state!
          /*
            NOTE: This device allows a fine-grained control over the row object's representation.
            It MUST be coupled with a closing statement on row object's end.
          */
          RowObject obj = new RowObject(
            baseComposer.BeginLocalState(),
            lineHeight,
            textChunkWidth,
            CountOccurrence(' ',textChunk)
            );
          currentRow.Objects.Add(obj);
          currentRow.SpaceCount += obj.SpaceCount;
          // - show the text chunk!
          baseComposer.ShowText(
            textChunk,
            textChunkLocation
            );
          // - close the row object's local state!
          baseComposer.End();

          // Update ancillary parameters:
          // - update row width!
          currentRow.Width += textChunkWidth;
          // - update cursor position!
          index = textFitter.EndIndex;
        }

        // Evaluating trailing text...
        while(true)
        {
          // Did we reach the text end?
          if(index == textLength)
            goto endTextShowing; // NOTE: I know GOTO is evil, yet in this case it's a much cleaner solution.

          switch(text[index])
          {
            case '\r':
              break;
            case '\n':
              // New paragraph!
              index++;
              ShowBreak();
              goto endTrailParsing; // NOTE: I know GOTO is evil, yet in this case it's a much cleaner solution.
            default:
              // New row (within the same paragraph)!
              EndRow(false); BeginRow();
              goto endTrailParsing; // NOTE: I know GOTO is evil, yet in this case it's a much cleaner solution.
          }

          index++;
        } endTrailParsing:;
      } endTextShowing:;

      return index;
    }
    #endregion

    #region private
    /**
      Begins a content row.
    */
    private void BeginRow(
      )
    {
      rowEnded = false;

      double rowY = boundBox.Height;
      if(rowY > 0)
      {
        ContentScanner.GraphicsState state = baseComposer.State;
        rowY += lineSpace.GetValue(state.Font.GetLineHeight(state.FontSize));
      }
      currentRow = new Row(
        (ContentPlaceholder)baseComposer.Add(new ContentPlaceholder()),
        rowY
        );
    }

    private int CountOccurrence(
      char value,
      string text
      )
    {
      int count = 0;
      int fromIndex = 0;
      do
      {
        int foundIndex = text.IndexOf(value,fromIndex);
        if(foundIndex == -1)
          return count;

        count++;

        fromIndex = foundIndex + 1;
      }
      while(true);
    }

    /**
      <summary>Ends the content row.</summary>
      <param name="broken">Indicates whether this is the end of a paragraph.</param>
    */
    private void EndRow(
      bool broken
      )
    {
      if(rowEnded)
        return;

      rowEnded = true;

      double[] objectXOffsets = new double[currentRow.Objects.Count]; // Horizontal object displacements.
      double wordSpace = 0; // Exceeding space among words.
      double rowXOffset = 0; // Horizontal row offset.

      List<RowObject> objects = currentRow.Objects;

      // Horizontal alignment.
      AlignmentXEnum alignmentX = this.alignmentX;
      switch(alignmentX)
      {
        case AlignmentXEnum.Left:
          break;
        case AlignmentXEnum.Right:
          rowXOffset = frame.Width - currentRow.Width;
          break;
        case AlignmentXEnum.Center:
          rowXOffset = (frame.Width - currentRow.Width) / 2;
          break;
        case AlignmentXEnum.Justify:
          // Are there NO spaces?
          if(currentRow.SpaceCount == 0
            || broken) // NO spaces.
          {
            /* NOTE: This situation equals a simple left alignment. */
            alignmentX = AlignmentXEnum.Left;
          }
          else // Spaces exist.
          {
            // Calculate the exceeding spacing among the words!
            wordSpace = (frame.Width - currentRow.Width) / currentRow.SpaceCount;

            // Define the horizontal offsets for justified alignment.
            for(
              int index = 1,
                count = objects.Count;
              index < count;
              index++
              )
            {
              /*
                NOTE: The offset represents the horizontal justification gap inserted
                at the left side of each object.
              */
              objectXOffsets[index] = objectXOffsets[index - 1] + objects[index - 1].SpaceCount * wordSpace;
            }
          }
          break;
      }

      SetWordSpace wordSpaceOperation = new SetWordSpace(wordSpace);

      // Vertical alignment and translation.
      for(
        int index = objects.Count - 1;
        index >= 0;
        index--
        )
      {
        RowObject obj = objects[index];

        // Vertical alignment.
        double objectYOffset = 0;
        //TODO:IMPL image support!!!
//         switch(obj.Type)
//         {
//           case RowObjectTypeEnum.Text:
            objectYOffset = -(currentRow.Height - obj.Height); // Linebase-anchored vertical alignment.
//             break;
//           case RowObjectTypeEnum.Image:
//             objectYOffset = -(currentRow.Height - obj.Height) / 2; // Centered vertical alignment.
//             break;
//         }

        IList<ContentObject> containedGraphics = obj.Container.Objects;
        // Word spacing.
        containedGraphics.Insert(0,wordSpaceOperation);
        // Translation.
        containedGraphics.Insert(
          0,
          new ModifyCTM(
            1, 0, 0, 1,
            objectXOffsets[index] + rowXOffset, // Horizontal alignment.
            objectYOffset // Vertical alignment.
            )
          );
      }

      // Update the actual block height!
      boundBox.Height = (float)(currentRow.Y + currentRow.Height);

      // Update the actual block vertical location!
      double xOffset;
      switch(alignmentY)
      {
        case AlignmentYEnum.Bottom:
          xOffset = frame.Height - boundBox.Height;
          break;
        case AlignmentYEnum.Middle:
          xOffset = (frame.Height - boundBox.Height) / 2;
          break;
        case AlignmentYEnum.Top:
        default:
          xOffset = 0;
          break;
      }
      boundBox.Y = (float)(frame.Y + xOffset);

      // Discard the current row!
      currentRow = null;
    }
    #endregion
    #endregion
    #endregion
  }
}