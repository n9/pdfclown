/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

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

using bytes = org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.contents.composition;
using fonts = org.pdfclown.documents.contents.fonts;
using org.pdfclown.documents.contents.objects;
using org.pdfclown.documents.contents.tokens;
using org.pdfclown.documents.contents.xObjects;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Drawing;

namespace org.pdfclown.documents.interaction.forms
{
  /**
    <summary>Text field [PDF:1.6:8.6.3].</summary>
  */
  [PDF(VersionEnum.PDF12)]
  public sealed class TextField
    : Field
  {
    #region dynamic
    #region constructors
    /**
      <summary>Creates a new text field within the given document context.</summary>
    */
    public TextField(
      string name,
      Widget widget,
      string value
      ) : base(
        PdfName.Tx,
        name,
        widget
        )
    {Value = value;}

    public TextField(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Gets/Sets whether the field can contain multiple lines of text.</summary>
    */
    public bool IsMultiline
    {
      get
      {return ((Flags & FlagsEnum.Multiline) == FlagsEnum.Multiline);}
      set
      {
        FlagsEnum flags = Flags;
        if(value)
        {flags |= FlagsEnum.Multiline;}
        else
        {flags ^= FlagsEnum.Multiline;}
        Flags = flags;
      }
    }

    /**
      <summary>Gets/Sets whether the field is intended for entering a secure password.</summary>
    */
    public bool IsPassword
    {
      get
      {return ((Flags & FlagsEnum.Password) == FlagsEnum.Password);}
      set
      {
        FlagsEnum flags = Flags;
        if(value)
        {flags |= FlagsEnum.Password;}
        else
        {flags ^= FlagsEnum.Password;}
        Flags = flags;
      }
    }

    /**
      <summary>Gets/Sets the justification to be used in displaying this field's text.</summary>
    */
    public JustificationEnum Justification
    {
      get
      {return JustificationEnumExtension.Get((PdfInteger)BaseDataObject[PdfName.Q]);}
      set
      {BaseDataObject[PdfName.Q] = value.GetCode();}
    }

    /**
      <summary>Gets/Sets the maximum length of the field's text, in characters.</summary>
    */
    public int MaxLength
    {
      get
      {
        PdfInteger maxLengthObject = (PdfInteger)File.Resolve(
          GetInheritableAttribute(PdfName.MaxLen)
          );
        if(maxLengthObject == null)
          return Int32.MaxValue;

        return maxLengthObject.RawValue;
      }
      set
      {throw new NotImplementedException();}
    }

    /**
      <summary>Gets/Sets whether text entered in the field is spell-checked.</summary>
    */
    public bool SpellChecked
    {
      get
      {return !((Flags & FlagsEnum.DoNotSpellCheck) == FlagsEnum.DoNotSpellCheck);}
      set
      {
        FlagsEnum flags = Flags;
        if(value)
        {flags ^= FlagsEnum.DoNotSpellCheck;}
        else
        {flags |= FlagsEnum.DoNotSpellCheck;}
        Flags = flags;
      }
    }

    public override object Value
    {
      get
      {return base.Value;}
      set
      {
        BaseDataObject[PdfName.V] = new PdfTextString((string)value);
        RefreshAppearance();
      }
    }
    #endregion

    #region private
    private void RefreshAppearance(
      )
    {
      Widget widget = Widgets[0];
      Appearance appearance = widget.Appearance;
      if(appearance == null)
      {widget.Appearance = appearance = new Appearance(Document);}

      FormXObject normalAppearance = appearance.Normal[null];
      if(normalAppearance == null)
      {appearance.Normal[null] = normalAppearance = new FormXObject(Document, widget.Box.Size);}

      PdfName fontName = null;
      double fontSize = 0;
      {
        PdfString defaultAppearanceState = DefaultAppearanceState;
        if(defaultAppearanceState == null)
        {
          // Retrieving the font to define the default appearance...
          fonts::Font defaultFont = null;
          PdfName defaultFontName = null;
          {
            Resources normalAppearanceResources = normalAppearance.Resources;
            if(normalAppearanceResources == null)
            {Document.Form.Resources = normalAppearanceResources = new Resources(Document);}

            FontResources normalAppearanceFontResources = normalAppearanceResources.Fonts;
            if(normalAppearanceFontResources == null)
            {normalAppearanceResources.Fonts = normalAppearanceFontResources = new FontResources(Document);}

            foreach(KeyValuePair<PdfName,fonts::Font> entry in normalAppearanceFontResources)
            {
              if(!entry.Value.Symbolic)
              {
                defaultFont = entry.Value;
                defaultFontName = entry.Key;
                break;
              }
            }
            if(defaultFontName == null)
            {
              Resources formResources = Document.Form.Resources;
              if(formResources == null)
              {Document.Form.Resources = formResources = new Resources(Document);}

              FontResources formFontResources = formResources.Fonts;
              if(formFontResources == null)
              {formResources.Fonts = formFontResources = new FontResources(Document);}

              foreach(KeyValuePair<PdfName,fonts::Font> entry in formFontResources)
              {
                if(!entry.Value.Symbolic)
                {
                  defaultFont = entry.Value;
                  defaultFontName = entry.Key;
                  break;
                }
              }
              if(defaultFontName == null)
              {
                //TODO:manage name collision!
                formFontResources[
                  defaultFontName = new PdfName("default")
                  ] = defaultFont = new fonts::StandardType1Font(
                    Document,
                    fonts::StandardType1Font.FamilyEnum.Helvetica,
                    false,
                    false
                    );
              }
              normalAppearanceFontResources[defaultFontName] = defaultFont;
            }
          }
          bytes::Buffer buffer = new bytes::Buffer();
          new SetFont(defaultFontName, IsMultiline ? 10 : 0).WriteTo(buffer, Document);
          widget.BaseDataObject[PdfName.DA] = defaultAppearanceState = new PdfString(buffer.ToByteArray());
        }

        // Retrieving the font to use...
        ContentParser parser = new ContentParser(defaultAppearanceState.ToByteArray());
        foreach(ContentObject content in parser.ParseContentObjects())
        {
          if(content is SetFont)
          {
            SetFont setFontOperation = (SetFont)content;
            fontName = setFontOperation.Name;
            fontSize = setFontOperation.Size;
            break;
          }
        }

        {
          Resources normalAppearanceResources = normalAppearance.Resources;
          if(normalAppearanceResources == null)
          {Document.Form.Resources = normalAppearanceResources = new Resources(Document);}

          FontResources normalAppearanceFontResources = normalAppearanceResources.Fonts;
          if(normalAppearanceFontResources == null)
          {normalAppearanceResources.Fonts = normalAppearanceFontResources = new FontResources(Document);}

          normalAppearanceFontResources[fontName] = Document.Form.Resources.Fonts[fontName];
        }
      }

      // Refreshing the field appearance...
      /*
       * TODO: resources MUST be resolved both through the apperance stream resource dictionary and
       * from the DR-entry acroform resource dictionary
       */
      PrimitiveComposer baseComposer = new PrimitiveComposer(normalAppearance);
      BlockComposer composer = new BlockComposer(baseComposer);
      ContentScanner currentLevel = composer.Scanner;
      bool textShown = false;
      while(currentLevel != null)
      {
        if(!currentLevel.MoveNext())
        {
          currentLevel = currentLevel.ParentLevel;
          continue;
        }

        ContentObject content = currentLevel.Current;
        if(content is MarkedContent)
        {
          MarkedContent markedContent = (MarkedContent)content;
          if(PdfName.Tx.Equals(((BeginMarkedContent)markedContent.Header).Tag))
          {
            // Remove old text representation!
            markedContent.Objects.Clear();
            // Add new text representation!
            baseComposer.Scanner = currentLevel.ChildLevel; // Ensures the composer places new contents within the marked content block.
            ShowText(composer, fontName, fontSize);
            textShown = true;
          }
        }
        else if(content is Text)
        {currentLevel.Remove();}
        else if(currentLevel.ChildLevel != null)
        {currentLevel = currentLevel.ChildLevel;}
      }
      if(!textShown)
      {
        baseComposer.BeginMarkedContent(PdfName.Tx);
        ShowText(composer, fontName, fontSize);
        baseComposer.End();
      }
      baseComposer.Flush();
    }

    private void ShowText(
      BlockComposer composer,
      PdfName fontName,
      double fontSize
      )
    {
      PrimitiveComposer baseComposer = composer.BaseComposer;
      ContentScanner scanner = baseComposer.Scanner;
      RectangleF textBox = scanner.ContentContext.Box;
      textBox.X += 2;
      textBox.Y += 2;
      textBox.Width -= 4;
      textBox.Height -= 4;
      composer.Begin(
        textBox,
        Justification.ToAlignmentX(),
        IsMultiline ? AlignmentYEnum.Top : AlignmentYEnum.Middle
        );
      if(scanner.State.Font == null)
      {
        /*
          NOTE: A zero value for size means that the font is to be auto-sized: its size is computed as
          a function of the height of the annotation rectangle.
        */
        if(fontSize == 0)
        {fontSize = textBox.Height * 0.9;}
        baseComposer.SetFont(fontName, fontSize);
      }
      composer.ShowText((string)Value);
      composer.End();
    }
    #endregion
    #endregion
    #endregion
  }
}