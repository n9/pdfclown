/*
  Copyright 2009-2010 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.objects;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace org.pdfclown.tools
{
  /**
    <summary>Tool for extracting text from <see cref="IContentContext">content contexts</see>.</summary>
  */
  public sealed class TextExtractor
  {
    #region types
    /**
      <summary>Text-to-area matching mode.</summary>
    */
    public enum AreaModeEnum
    {
      /**
        <summary>Text string must be contained by the area.</summary>
      */
      Containment,
      /**
        <summary>Text string must intersect the area.</summary>
      */
      Intersection
    }

    /**
      <summary>Text string.</summary>
      <remarks>This is typically used to assemble multiple raw text strings
      laying on the same line.</remarks>
    */
    private class TextString
      : ITextString
    {
      #region dynamic
      #region fields
      private List<TextChar> textChars = new List<TextChar>();
      #endregion

      #region interface
      #region public
      public RectangleF? Box
      {
        get
        {
          RectangleF? box = null;
          foreach(TextChar textChar in textChars)
          {
            if(!box.HasValue)
            {box = (RectangleF?)textChar.Box;}
            else
            {box = RectangleF.Union(box.Value,textChar.Box);}
          }
          return box;
        }
      }

      public string Text
      {
        get
        {
          StringBuilder textBuilder = new StringBuilder();
          foreach(TextChar textChar in textChars)
          {textBuilder.Append(textChar);}
          return textBuilder.ToString();
        }
      }

      public List<TextChar> TextChars
      {get{return textChars;}}
      #endregion
      #endregion
      #endregion
    }

    /**
      Text string position comparer.
    */
    private class TextStringPositionComparer<T>
      : IComparer<T>
      where T : ITextString
    {
      #region static
      /**
        <summary>Gets whether the given boxes lay on the same text line.</summary>
      */
      public static bool IsOnTheSameLine(
        RectangleF box1,
        RectangleF box2
        )
      {
        /*
          NOTE: In order to consider the two boxes being on the same line,
          we apply a simple rule of thumb: at least 25% of a box's height MUST
          lay on the horizontal projection of the other one.
        */
        double minHeight = Math.Min(box1.Height, box2.Height);
        double yThreshold = minHeight * .75;
        return ((box1.Y > box2.Y - yThreshold
            && box1.Y < box2.Bottom + yThreshold - minHeight)
          || (box2.Y > box1.Y - yThreshold
            && box2.Y < box1.Bottom + yThreshold - minHeight));
      }
      #endregion

      #region dynamic
      #region IComparer
      public int Compare(
        T textString1,
        T textString2
        )
      {
        RectangleF box1 = textString1.Box.Value;
        RectangleF box2 = textString2.Box.Value;
        if(IsOnTheSameLine(box1,box2))
        {
          if(box1.X < box2.X)
            return -1;
          else if(box1.X > box2.X)
            return 1;
          else
            return 0;
        }
        else if(box1.Y < box2.Y)
          return -1;
        else
          return 1;
      }
      #endregion
      #endregion
    }
    #endregion

    #region static
    #region fields
    public static readonly RectangleF DefaultArea = new RectangleF(0,0,0,0);
    #endregion
    #endregion

    #region dynamic
    #region fields
    private AreaModeEnum areaMode = AreaModeEnum.Containment;
    private List<RectangleF> areas;
    private float areaTolerance = 0;
    private bool sorted;
    #endregion

    #region constructors
    public TextExtractor(
      ) : this(true)
    {}

    public TextExtractor(
      bool sorted
      ) : this(null,sorted)
    {}

    public TextExtractor(
      IList<RectangleF> areas,
      bool sorted
      )
    {
      Areas = areas;
      Sorted = sorted;
    }
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the text-to-area matching mode.</summary>
    */
    public AreaModeEnum AreaMode
    {
      get{return areaMode;}
      set{areaMode = value;}
    }

    /**
      <summary>Gets the graphic areas whose text has to be extracted.</summary>
    */
    public IList<RectangleF> Areas
    {
      get
      {return areas;}
      set
      {areas = (value == null ? new List<RectangleF>() : new List<RectangleF>(value));}
    }

    /**
      <summary>Gets the admitted outer area (in points) for containment matching purposes.</summary>
      <remarks>This measure is useful to ensure that text whose boxes overlap with the area bounds
      is not excluded from the match.</remarks>
    */
    public float AreaTolerance
    {
      get
      {return areaTolerance;}
      set
      {areaTolerance = value;}
    }

    /**
      <summary>Extracts text strings from the given content context.</summary>
      <param name="contentContext">Source content context.</param>
    */
    public IDictionary<RectangleF?,IList<ITextString>> Extract(
      IContentContext contentContext
      )
    {
      IDictionary<RectangleF?,IList<ITextString>> extractedTextStrings;
      {
        List<ITextString> textStrings = new List<ITextString>();
        {
          // 1. Extract the source text strings!
          List<ContentScanner.TextStringWrapper> rawTextStrings = new List<ContentScanner.TextStringWrapper>();
          Extract(
            new ContentScanner(contentContext),
            rawTextStrings
            );

          // 2. Sort the target text strings!
          if(sorted)
          {Sort(rawTextStrings,textStrings);}
          else
          {
            foreach(ContentScanner.TextStringWrapper rawTextString in rawTextStrings)
            {textStrings.Add(rawTextString);}
          }
        }

        // 3. Filter the target text strings!
        if(areas.Count == 0)
        {
          extractedTextStrings = new Dictionary<RectangleF?,IList<ITextString>>();
          extractedTextStrings[DefaultArea] = textStrings;
        }
        else
        {extractedTextStrings = Filter(textStrings,areas.ToArray());}
      }
      return extractedTextStrings;
    }

    /**
      <summary>Extracts text strings from the given contents.</summary>
      <param name="contents">Source contents.</param>
    */
    public IDictionary<RectangleF?,IList<ITextString>> Extract(
      Contents contents
      )
    {return Extract(contents.ContentContext);}

    /**
      <summary>Extracts plain text from the given content context.</summary>
      <param name="contentContext">Source content context.</param>
    */
    public string ExtractPlain(
      IContentContext contentContext
      )
    {
      StringBuilder textBuilder = new StringBuilder();
      foreach(List<ITextString> textStrings in Extract(contentContext).Values)
      {
        if(textBuilder.Length > 0)
        {textBuilder.Append('\n');} // Separates text belonging to different areas.

        foreach(ITextString textString in textStrings)
        {textBuilder.Append(textString.Text).Append('\n');}
      }
      return textBuilder.ToString();
    }

    /**
      <summary>Extracts plain text from the given contents.</summary>
      <param name="contents">Source contents.</param>
    */
    public string ExtractPlain(
      Contents contents
      )
    {return ExtractPlain(contents.ContentContext);}

    /**
      <summary>Gets the text strings matching the given area.</summary>
      <param name="textStrings">Text strings to filter, grouped by source area.</param>
      <param name="area">Graphic area which text strings have to be matched to.</param>
    */
    public IList<ITextString> Filter(
      IDictionary<RectangleF?,IList<ITextString>> textStrings,
      RectangleF area
      )
    {return Filter(textStrings,new RectangleF[]{area})[area];}

    /**
      <summary>Gets the text strings matching the given areas.</summary>
      <param name="textStrings">Text strings to filter, grouped by source area.</param>
      <param name="areas">Graphic areas which text strings have to be matched to.</param>
    */
    public IDictionary<RectangleF?,IList<ITextString>> Filter(
      IDictionary<RectangleF?,IList<ITextString>> textStrings,
      params RectangleF[] areas
      )
    {
      IDictionary<RectangleF?,IList<ITextString>> filteredTextStrings = null;
      foreach(IList<ITextString> areaTextStrings in textStrings.Values)
      {
        IDictionary<RectangleF?,IList<ITextString>> filteredAreasTextStrings = Filter(areaTextStrings,areas);
        if(filteredTextStrings == null)
        {filteredTextStrings = filteredAreasTextStrings;}
        else
        {
          foreach(KeyValuePair<RectangleF?,IList<ITextString>> filteredAreaTextStringsEntry in filteredAreasTextStrings)
          {
            IList<ITextString> filteredTextStringsList = filteredTextStrings[filteredAreaTextStringsEntry.Key];
            foreach(ITextString filteredAreaTextString in filteredAreaTextStringsEntry.Value)
            {filteredTextStringsList.Add(filteredAreaTextString);}
          }
        }
      }
      return filteredTextStrings;
    }

    /**
      <summary>Gets the text strings matching the given area.</summary>
      <param name="textStrings">Text strings to filter.</param>
      <param name="area">Graphic area which text strings have to be matched to.</param>
    */
    public IList<ITextString> Filter(
      IList<ITextString> textStrings,
      RectangleF area
      )
    {return Filter(textStrings,new RectangleF[]{area})[area];}

    /**
      <summary>Gets the text strings matching the given areas.</summary>
      <param name="textStrings">Text strings to filter.</param>
      <param name="areas">Graphic areas which text strings have to be matched to.</param>
    */
    public IDictionary<RectangleF?,IList<ITextString>> Filter(
      IList<ITextString> textStrings,
      params RectangleF[] areas
      )
    {
      IDictionary<RectangleF?,IList<ITextString>> filteredAreasTextStrings = new Dictionary<RectangleF?,IList<ITextString>>();
      foreach(RectangleF area in areas)
      {
        IList<ITextString> filteredAreaTextStrings = new List<ITextString>();
        filteredAreasTextStrings[area] = filteredAreaTextStrings;
        RectangleF toleratedArea = (areaTolerance != 0
          ? new RectangleF(
            area.X - areaTolerance,
            area.Y - areaTolerance,
            area.Width + areaTolerance * 2,
            area.Height + areaTolerance * 2
            )
          : area);
        foreach(ITextString textString in textStrings)
        {
          RectangleF? textStringBox = textString.Box;
          if(toleratedArea.IntersectsWith(textStringBox.Value))
          {
            TextString filteredTextString = new TextString();
            List<TextChar> filteredTextStringChars = filteredTextString.TextChars;
            foreach(TextChar textChar in textString.TextChars)
            {
              RectangleF textCharBox = textChar.Box;
              if((areaMode == AreaModeEnum.Containment && toleratedArea.Contains(textCharBox))
                || (areaMode == AreaModeEnum.Intersection && toleratedArea.IntersectsWith(textCharBox)))
              {filteredTextStringChars.Add(textChar);}
            }
            filteredAreaTextStrings.Add(filteredTextString);
          }
        }
      }
      return filteredAreasTextStrings;
    }

    /**
      <summary>Gets whether the text strings have to be sorted.</summary>
    */
    public bool Sorted
    {
      get
      {return sorted;}
      set
      {sorted = value;}
    }
    #endregion

    #region private
    /**
      <summary>Scans a content level looking for text.</summary>
    */
    private void Extract(
      ContentScanner level,
      IList<ContentScanner.TextStringWrapper> extractedTextStrings
      )
    {
      while(level.MoveNext())
      {
        ContentObject content = level.Current;
        if(content is Text)
        {
          // Collect the text strings!
          foreach(ContentScanner.TextStringWrapper textString in ((ContentScanner.TextWrapper)level.CurrentWrapper).TextStrings)
          {extractedTextStrings.Add(textString);}
        }
        else if(content is ContainerObject)
        {
          // Scan the inner level!
          Extract(
            level.ChildLevel,
            extractedTextStrings
            );
        }
      }
    }

    /**
      <summary>Sorts the extracted text strings.</summary>
      <remarks>Sorting implies text position ordering, integration and aggregation.</remarks>
      <param name="rawTextStrings">Source (lower-level) text strings.</param>
      <param name="textStrings">Target (higher-level) text strings.</param>
    */
    private void Sort(
      List<ContentScanner.TextStringWrapper> rawTextStrings,
      List<ITextString> textStrings
      )
    {
      // Sorting the source text strings...
      {
        TextStringPositionComparer<ContentScanner.TextStringWrapper> positionComparator = new TextStringPositionComparer<ContentScanner.TextStringWrapper>();
        rawTextStrings.Sort(positionComparator);
      }

      // Aggregating and integrating the source text strings into the target ones...
      TextString textString = null;
      TextChar previousTextChar = null;
      foreach(ContentScanner.TextStringWrapper rawTextString in rawTextStrings)
      {
        /*
          NOTE: Contents on the same line are grouped together within the same text string.
        */
        // Add a new text string in case of new line!
        if(textString == null
          || (textString.TextChars.Count > 0
            && !TextStringPositionComparer<ITextString>.IsOnTheSameLine(
              textString.Box.Value,
              rawTextString.Box.Value)))
        {
          textStrings.Add(textString = new TextString());
          previousTextChar = null;
        }

        TextStyle textStyle = rawTextString.Style;
        float spaceWidth = 0;
        try
        {spaceWidth = textStyle.Font.GetWidth(' ', textStyle.FontSize);}
        catch
        { /* NOOP. */ }
        if(spaceWidth == 0)
        {spaceWidth = textStyle.FontSize * .25f;} // NOTE: as a rule of thumb, space width is estimated according to the font size.
        foreach(TextChar textChar in rawTextString.TextChars)
        {
          if(previousTextChar != null)
          {
            /*
              NOTE: PDF files may have text contents omitting space characters,
              so they must be inferred and synthesized, marking them as virtual
              in order to allow the user to distinguish between original contents
              and augmented ones.
            */
            float characterSpace = textChar.Box.X - previousTextChar.Box.Right;
            if(characterSpace >= spaceWidth)
            {
              // Add synthesized space character!
              textString.TextChars.Add(
                new TextChar(
                  ' ',
                  new RectangleF(
                    previousTextChar.Box.Right,
                    textChar.Box.Y,
                    characterSpace,
                    textChar.Box.Height
                    ),
                  textStyle,
                  true
                  )
                );
            }
          }
          textString.TextChars.Add(previousTextChar = textChar);
        }
      }
    }
    #endregion
    #endregion
    #endregion
  }
}
