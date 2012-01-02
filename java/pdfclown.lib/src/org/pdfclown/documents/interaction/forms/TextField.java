/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library"
  (the Program): see the accompanying README files for more info.

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

package org.pdfclown.documents.interaction.forms;

import java.awt.geom.Rectangle2D;
import java.util.EnumSet;
import java.util.Map;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.bytes.Buffer;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.ContentScanner;
import org.pdfclown.documents.contents.FontResources;
import org.pdfclown.documents.contents.Resources;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.BlockComposer;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.fonts.Font;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.documents.contents.objects.ContentObject;
import org.pdfclown.documents.contents.objects.MarkedContent;
import org.pdfclown.documents.contents.objects.SetFont;
import org.pdfclown.documents.contents.objects.Text;
import org.pdfclown.documents.contents.tokens.ContentParser;
import org.pdfclown.documents.contents.xObjects.FormXObject;
import org.pdfclown.documents.interaction.JustificationEnum;
import org.pdfclown.documents.interaction.annotations.Appearance;
import org.pdfclown.documents.interaction.annotations.Widget;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfString;
import org.pdfclown.objects.PdfTextString;
import org.pdfclown.util.NotImplementedException;
import org.pdfclown.util.math.geom.Dimension;

/**
  Text field [PDF:1.6:8.6.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.2, 01/02/12
*/
@PDF(VersionEnum.PDF12)
public final class TextField
  extends Field
{
  // <class>
  // <dynamic>
  // <constructors>
  /**
    Creates a new text field within the given document context.
  */
  public TextField(
    String name,
    Widget widget,
    String value
    )
  {
    super(
      PdfName.Tx,
      name,
      widget
      );

    setValue(value);
  }

  public TextField(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public TextField clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Gets the justification to be used in displaying this field's text.
  */
  public JustificationEnum getJustification(
    )
  {return JustificationEnum.valueOf((PdfInteger)getBaseDataObject().get(PdfName.Q));}

  /**
    Gets the maximum length of this field's text, in characters.
  */
  public int getMaxLength(
    )
  {
    PdfInteger maxLengthObject = (PdfInteger)File.resolve(
      getInheritableAttribute(PdfName.MaxLen)
      );
    if(maxLengthObject == null)
      return Integer.MAX_VALUE;//TODO:verify!!!

    return maxLengthObject.getRawValue();
  }

  /**
    Gets whether the field can contain multiple lines of text.
  */
  public boolean isMultiline(
    )
  {return getFlags().contains(FlagsEnum.Multiline);}

  /**
    Gets whether the field is intended for entering a secure password.
  */
  public boolean isPassword(
    )
  {return getFlags().contains(FlagsEnum.Password);}

  /**
    Gets whether text entered in the field is spell-checked.
  */
  public boolean isSpellChecked(
    )
  {return !getFlags().contains(FlagsEnum.DoNotSpellCheck);}

  /**
    @see #getJustification()
  */
  public void setJustification(
    JustificationEnum value
    )
  {getBaseDataObject().put(PdfName.Q, value.getCode());}

  /**
    @see #getMaxLength()
  */
  public void setMaxLength(
    int value
    )
  {throw new NotImplementedException();}

  /**
    @see #isMultiline()
  */
  public void setMultiline(
    boolean value
    )
  {
    EnumSet<FlagsEnum> flags = getFlags();
    if(value)
    {flags.add(FlagsEnum.Multiline);}
    else
    {flags.remove(FlagsEnum.Multiline);}
    setFlags(flags);
  }

  /**
    @see #isPassword()
  */
  public void setPassword(
    boolean value
    )
  {
    EnumSet<FlagsEnum> flags = getFlags();
    if(value)
    {flags.add(FlagsEnum.Password);}
    else
    {flags.remove(FlagsEnum.Password);}
    setFlags(flags);
  }

  /**
    @see #isSpellChecked()
  */
  public void setSpellChecked(
    boolean value
    )
  {
    EnumSet<FlagsEnum> flags = getFlags();
    if(value)
    {flags.remove(FlagsEnum.DoNotSpellCheck);}
    else
    {flags.add(FlagsEnum.DoNotSpellCheck);}
    setFlags(flags);
  }

  @Override
  public void setValue(
    Object value
    )
  {
    getBaseDataObject().put(PdfName.V,new PdfTextString((String)value));
    refreshAppearance();
  }
  // </public>

  // <private>
  private void refreshAppearance(
    )
  {
    Widget widget = getWidgets().get(0);
    Appearance appearance = widget.getAppearance();
    if(appearance == null)
    {widget.setAppearance(appearance = new Appearance(getDocument()));}

    FormXObject normalAppearance = appearance.getNormal().get(null);
    if(normalAppearance == null)
    {
      appearance.getNormal().put(
        null,
        normalAppearance = new FormXObject(
          getDocument(),
          Dimension.get(widget.getBox())
          )
        );
    }

    PdfName fontName = null;
    double fontSize = 0;
    {
      PdfString defaultAppearanceState = getDefaultAppearanceState();
      if(defaultAppearanceState == null)
      {
        // Retrieving the font to define the default appearance...
        Font defaultFont = null;
        PdfName defaultFontName = null;
        {
          Resources normalAppearanceResources = normalAppearance.getResources();
          if(normalAppearanceResources == null)
          {getDocument().getForm().setResources(normalAppearanceResources = new Resources(getDocument()));}

          FontResources normalAppearanceFontResources = normalAppearanceResources.getFonts();
          if(normalAppearanceFontResources == null)
          {normalAppearanceResources.setFonts(normalAppearanceFontResources = new FontResources(getDocument()));}

          for(Map.Entry<PdfName,Font> entry : normalAppearanceFontResources.entrySet())
          {
            if(!entry.getValue().isSymbolic())
            {
              defaultFont = entry.getValue();
              defaultFontName = entry.getKey();
              break;
            }
          }
          if(defaultFontName == null)
          {
            Resources formResources = getDocument().getForm().getResources();
            if(formResources == null)
            {getDocument().getForm().setResources(formResources = new Resources(getDocument()));}

            FontResources formFontResources = formResources.getFonts();
            if(formFontResources == null)
            {formResources.setFonts(formFontResources = new FontResources(getDocument()));}

            for(Map.Entry<PdfName,Font> entry : formFontResources.entrySet())
            {
              if(!entry.getValue().isSymbolic())
              {
                defaultFont = entry.getValue();
                defaultFontName = entry.getKey();
                break;
              }
            }
            if(defaultFontName == null)
            {
              //TODO:manage name collision!
              formFontResources.put(
                defaultFontName = new PdfName("default"),
                defaultFont = new StandardType1Font(
                  getDocument(),
                  StandardType1Font.FamilyEnum.Helvetica,
                  false,
                  false
                  )
                );
            }
            normalAppearanceFontResources.put(defaultFontName, defaultFont);
          }
        }
        Buffer buffer = new Buffer();
        new SetFont(defaultFontName, isMultiline() ? 10 : 0).writeTo(buffer, getDocument());
        widget.getBaseDataObject().put(PdfName.DA, defaultAppearanceState = new PdfString(buffer.toByteArray()));
      }

      // Retrieving the font to use...
      ContentParser parser = new ContentParser(defaultAppearanceState.toByteArray());
      for(ContentObject content : parser.parseContentObjects())
      {
        if(content instanceof SetFont)
        {
          SetFont setFontOperation = (SetFont)content;
          fontName = setFontOperation.getName();
          fontSize = setFontOperation.getSize();
          break;
        }
      }

      Resources normalAppearanceResources = normalAppearance.getResources();
      if(normalAppearanceResources == null)
      {getDocument().getForm().setResources(normalAppearanceResources = new Resources(getDocument()));}

      FontResources normalAppearanceFontResources = normalAppearanceResources.getFonts();
      if(normalAppearanceFontResources == null)
      {normalAppearanceResources.setFonts(normalAppearanceFontResources = new FontResources(getDocument()));}

      normalAppearanceFontResources.put(fontName, getDocument().getForm().getResources().getFonts().get(fontName));
    }

    // Refreshing the field appearance...
    /*
     * TODO: resources MUST be resolved both through the apperance stream resource dictionary and
     * from the DR-entry acroform resource dictionary
     */
    PrimitiveComposer baseComposer = new PrimitiveComposer(normalAppearance);
    BlockComposer composer = new BlockComposer(baseComposer);
    ContentScanner currentLevel = composer.getScanner();
    boolean textShown = false;
    while(currentLevel != null)
    {
      if(!currentLevel.moveNext())
      {
        currentLevel = currentLevel.getParentLevel();
        continue;
      }

      ContentObject content = currentLevel.getCurrent();
      if(content instanceof MarkedContent)
      {
        MarkedContent markedContent = (MarkedContent)content;
        if(PdfName.Tx.equals(markedContent.getHeader().getTag()))
        {
          // Remove old text representation!
          markedContent.getObjects().clear();
          // Add new text representation!
          baseComposer.setScanner(currentLevel.getChildLevel()); // Ensures the composer places new contents within the marked content block.
          showText(composer, fontName, fontSize);
          textShown = true;
        }
      }
      else if(content instanceof Text)
      {currentLevel.remove();}
      else if(currentLevel.getChildLevel() != null)
      {currentLevel = currentLevel.getChildLevel();}
    }
    if(!textShown)
    {
      baseComposer.beginMarkedContent(PdfName.Tx);
      showText(composer, fontName, fontSize);
      baseComposer.end();
    }
    baseComposer.flush();
  }

  private void showText(
    BlockComposer composer,
    PdfName fontName,
    double fontSize
    )
  {
    PrimitiveComposer baseComposer = composer.getBaseComposer();
    ContentScanner scanner = baseComposer.getScanner();
    Rectangle2D textBox = scanner.getContentContext().getBox();
    textBox.setRect(
      textBox.getX() + 2,
      textBox.getY() + 2,
      textBox.getWidth() - 4,
      textBox.getHeight() - 4
      );
    composer.begin(
      textBox,
      getJustification().toAlignmentX(),
      isMultiline() ? AlignmentYEnum.Top : AlignmentYEnum.Middle
      );
    if(scanner.getState().getFont() == null)
    {
      /*
        NOTE: A zero value for size means that the font is to be auto-sized: its size is computed as
        a function of the height of the annotation rectangle.
      */
      if(fontSize == 0)
      {fontSize = textBox.getHeight() * 0.9;}
      baseComposer.setFont(fontName, fontSize);
    }
    composer.showText((String)getValue());
    composer.end();
  }
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}