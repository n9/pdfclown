/*
  Copyright 2008-2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.interaction.forms.styles;

import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.colorSpaces.DeviceRGBColor;
import org.pdfclown.documents.contents.composition.AlignmentXEnum;
import org.pdfclown.documents.contents.composition.AlignmentYEnum;
import org.pdfclown.documents.contents.composition.BlockComposer;
import org.pdfclown.documents.contents.composition.PrimitiveComposer;
import org.pdfclown.documents.contents.fonts.StandardType1Font;
import org.pdfclown.documents.contents.xObjects.FormXObject;
import org.pdfclown.documents.interaction.annotations.Appearance;
import org.pdfclown.documents.interaction.annotations.AppearanceStates;
import org.pdfclown.documents.interaction.annotations.DualWidget;
import org.pdfclown.documents.interaction.annotations.Widget;
import org.pdfclown.documents.interaction.forms.CheckBox;
import org.pdfclown.documents.interaction.forms.ChoiceItem;
import org.pdfclown.documents.interaction.forms.ComboBox;
import org.pdfclown.documents.interaction.forms.Field;
import org.pdfclown.documents.interaction.forms.ListBox;
import org.pdfclown.documents.interaction.forms.PushButton;
import org.pdfclown.documents.interaction.forms.RadioButton;
import org.pdfclown.documents.interaction.forms.TextField;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfReal;
import org.pdfclown.objects.PdfString;
import org.pdfclown.util.math.geom.Dimension;

import java.awt.geom.Dimension2D;
import java.awt.geom.Point2D;
import java.awt.geom.Rectangle2D;

/**
  Default field appearance style.

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.0
*/
public final class DefaultStyle
  extends FieldStyle
{
  // <dynamic>
  // <constructors>
  public DefaultStyle(
    )
  {setBackColor(new DeviceRGBColor(.9f,.9f,.9f));}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public void apply(
    Field field
    )
  {
    if(field instanceof PushButton)
    {apply((PushButton)field);}
    else if(field instanceof CheckBox)
    {apply((CheckBox)field);}
    else if(field instanceof TextField)
    {apply((TextField)field);}
    else if(field instanceof ComboBox)
    {apply((ComboBox)field);}
    else if(field instanceof ListBox)
    {apply((ListBox)field);}
    else if(field instanceof RadioButton)
    {apply((RadioButton)field);}
  }

  private void apply(
    CheckBox field
    )
  {
    Document document = field.getDocument();
    for(Widget widget : field.getWidgets())
    {
      {
        PdfDictionary widgetDataObject = widget.getBaseDataObject();
        widgetDataObject.put(
          PdfName.DA,
          new PdfString("/ZaDb 0 Tf 0 0 0 rg")
          );
        widgetDataObject.put(
          PdfName.MK,
          new PdfDictionary(
            new PdfName[]
            {
              PdfName.BG,
              PdfName.BC,
              PdfName.CA
            },
            new PdfDirectObject[]
            {
              new PdfArray(new PdfDirectObject[]{new PdfReal(0.9412),new PdfReal(0.9412),new PdfReal(0.9412)}),
              new PdfArray(new PdfDirectObject[]{new PdfInteger(0),new PdfInteger(0),new PdfInteger(0)}),
              new PdfString("4")
            }
            )
          );
        widgetDataObject.put(
          PdfName.BS,
          new PdfDictionary(
            new PdfName[]
            {
              PdfName.W,
              PdfName.S
            },
            new PdfDirectObject[]
            {
              new PdfReal(0.8),
              PdfName.S
            }
            )
          );
        widgetDataObject.put(
          PdfName.H,
          PdfName.P
          );
      }

      Appearance appearance = widget.getAppearance();
      if(appearance == null)
      {widget.setAppearance(appearance = new Appearance(document));}

      AppearanceStates normalAppearance = appearance.getNormal();
      FormXObject onState = new FormXObject(document);
      normalAppearance.put(PdfName.Yes,onState);

//TODO:verify!!!
//   appearance.getRollover().put(PdfName.Yes,onState);
//   appearance.getDown().put(PdfName.Yes,onState);
//   appearance.getRollover().put(PdfName.Off,offState);
//   appearance.getDown().put(PdfName.Off,offState);

      Rectangle2D widgetBox = widget.getBox();
      Dimension2D size = new Dimension(widgetBox.getWidth(),widgetBox.getHeight());
      Rectangle2D frame = new Rectangle2D.Double(0,0,size.getWidth(),size.getHeight());
      {
        onState.setSize(size);

        PrimitiveComposer composer = new PrimitiveComposer(onState);

        composer.beginLocalState();
        composer.setFillColor(getBackColor());
        composer.setStrokeColor(getForeColor());
        composer.drawRectangle(frame);
        composer.fillStroke();
        composer.end();

        BlockComposer blockComposer = new BlockComposer(composer);
        blockComposer.begin(frame,AlignmentXEnum.Center,AlignmentYEnum.Middle);
        composer.setFillColor(getForeColor());
        composer.setFont(
          new StandardType1Font(
            document,
            StandardType1Font.FamilyEnum.ZapfDingbats,
            true,
            false
            ),
          (float)(size.getHeight() * 0.8)
          );
        blockComposer.showText(new String(new char[]{getCheckSymbol()}));
        blockComposer.end();

        composer.flush();
      }

      FormXObject offState = new FormXObject(document);
      normalAppearance.put(PdfName.Off,offState);
      {
        offState.setSize(size);

        PrimitiveComposer composer = new PrimitiveComposer(offState);

        composer.beginLocalState();
        composer.setFillColor(getBackColor());
        composer.setStrokeColor(getForeColor());
        composer.drawRectangle(frame);
        composer.fillStroke();
        composer.end();

        composer.flush();
      }
    }
  }

  private void apply(
    RadioButton field
    )
  {
    Document document = field.getDocument();
    for(Widget widget : field.getWidgets())
    {
      {
        PdfDictionary widgetDataObject = widget.getBaseDataObject();
        widgetDataObject.put(
          PdfName.DA,
          new PdfString("/ZaDb 0 Tf 0 0 0 rg")
          );
        widgetDataObject.put(
          PdfName.MK,
          new PdfDictionary(
            new PdfName[]
            {
              PdfName.BG,
              PdfName.BC,
              PdfName.CA
            },
            new PdfDirectObject[]
            {
              new PdfArray(new PdfDirectObject[]{new PdfReal(0.9412),new PdfReal(0.9412),new PdfReal(0.9412)}),
              new PdfArray(new PdfDirectObject[]{new PdfInteger(0),new PdfInteger(0),new PdfInteger(0)}),
              new PdfString("l")
            }
            )
          );
        widgetDataObject.put(
          PdfName.BS,
          new PdfDictionary(
            new PdfName[]
            {
              PdfName.W,
              PdfName.S
            },
            new PdfDirectObject[]
            {
              new PdfReal(0.8),
              PdfName.S
            }
            )
          );
        widgetDataObject.put(
          PdfName.H,
          PdfName.P
          );
      }

      Appearance appearance = widget.getAppearance();
      if(appearance == null)
      {widget.setAppearance(appearance = new Appearance(document));}

      AppearanceStates normalAppearance = appearance.getNormal();
      FormXObject onState = normalAppearance.get(new PdfName(((DualWidget)widget).getWidgetName()));

//TODO:verify!!!
//   appearance.getRollover().put(new PdfName(...),onState);
//   appearance.getDown().put(new PdfName(...),onState);
//   appearance.getRollover().put(PdfName.Off,offState);
//   appearance.getDown().put(PdfName.Off,offState);

      Rectangle2D widgetBox = widget.getBox();
      Dimension2D size = new Dimension(widgetBox.getWidth(),widgetBox.getHeight());
      Rectangle2D frame = new Rectangle2D.Double(1,1,size.getWidth()-2,size.getHeight()-2);
      {
        onState.setSize(size);

        PrimitiveComposer composer = new PrimitiveComposer(onState);

        composer.beginLocalState();
        composer.setFillColor(getBackColor());
        composer.setStrokeColor(getForeColor());
        composer.drawEllipse(frame);
        composer.fillStroke();
        composer.end();

        BlockComposer blockComposer = new BlockComposer(composer);
        blockComposer.begin(frame,AlignmentXEnum.Center,AlignmentYEnum.Middle);
        composer.setFillColor(getForeColor());
        composer.setFont(
          new StandardType1Font(
            document,
            StandardType1Font.FamilyEnum.ZapfDingbats,
            true,
            false
            ),
          (float)(size.getHeight() * 0.8)
          );
        blockComposer.showText(new String(new char[]{getRadioSymbol()}));
        blockComposer.end();

        composer.flush();
      }

      FormXObject offState = new FormXObject(document);
      normalAppearance.put(PdfName.Off,offState);
      {
        offState.setSize(size);

        PrimitiveComposer composer = new PrimitiveComposer(offState);

        composer.beginLocalState();
        composer.setFillColor(getBackColor());
        composer.setStrokeColor(getForeColor());
        composer.drawEllipse(frame);
        composer.fillStroke();
        composer.end();

        composer.flush();
      }
    }
  }

  private void apply(
    PushButton field
    )
  {
    Document document = field.getDocument();
    Widget widget = field.getWidgets().get(0);

    Appearance appearance = widget.getAppearance();
    if(appearance == null)
    {widget.setAppearance(appearance = new Appearance(document));}

    FormXObject normalAppearanceState = new FormXObject(document);
    {
      Rectangle2D widgetBox = widget.getBox();
      Dimension2D size = new Dimension(widgetBox.getWidth(),widgetBox.getHeight());
      normalAppearanceState.setSize(size);
      PrimitiveComposer composer = new PrimitiveComposer(normalAppearanceState);

      composer.beginLocalState();
      float lineWidth = 1;
      composer.setLineWidth(lineWidth);
      composer.setFillColor(getBackColor());
      composer.setStrokeColor(getForeColor());
      Rectangle2D frame = new Rectangle2D.Double(lineWidth/2,lineWidth/2,size.getWidth()-lineWidth,size.getHeight()-lineWidth);
      composer.drawRectangle(frame,5);
      composer.fillStroke();
      composer.end();

      String caption = (String)field.getValue();
      if(caption != null)
      {
        BlockComposer blockComposer = new BlockComposer(composer);
        blockComposer.begin(frame,AlignmentXEnum.Center,AlignmentYEnum.Middle);
        composer.setFillColor(getForeColor());
        composer.setFont(
          new StandardType1Font(
            document,
            StandardType1Font.FamilyEnum.Helvetica,
            true,
            false
            ),
          (float)(size.getHeight() * 0.5)
          );
        blockComposer.showText(caption);
        blockComposer.end();
      }

      composer.flush();
    }
    appearance.getNormal().put(null,normalAppearanceState);
  }

  private void apply(
    TextField field
    )
  {
    Document document = field.getDocument();
    Widget widget = field.getWidgets().get(0);

    Appearance appearance = widget.getAppearance();
    if(appearance == null)
    {widget.setAppearance(appearance = new Appearance(document));}

    widget.getBaseDataObject().put(
      PdfName.DA,
      new PdfString("/Helv " + getFontSize() + " Tf 0 0 0 rg")
      );

    FormXObject normalAppearanceState = new FormXObject(document);
    {
      Rectangle2D widgetBox = widget.getBox();
      Dimension2D size = new Dimension(widgetBox.getWidth(),widgetBox.getHeight());
      normalAppearanceState.setSize(size);
      PrimitiveComposer composer = new PrimitiveComposer(normalAppearanceState);

      composer.beginLocalState();
      float lineWidth = 1;
      composer.setLineWidth(lineWidth);
      composer.setFillColor(getBackColor());
      composer.setStrokeColor(getForeColor());
      Rectangle2D frame = new Rectangle2D.Double(lineWidth/2,lineWidth/2,size.getWidth()-lineWidth,size.getHeight()-lineWidth);
      composer.drawRectangle(frame,5);
      composer.fillStroke();
      composer.end();

      composer.beginMarkedContent(PdfName.Tx);
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Helvetica,
          false,
          false
          ),
        getFontSize()
        );
      composer.showText(
        (String)field.getValue(),
        new Point2D.Double(0,size.getHeight()/2),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        0
        );
      composer.end();

      composer.flush();
    }
    appearance.getNormal().put(null,normalAppearanceState);
  }

  private void apply(
    ComboBox field
    )
  {
    Document document = field.getDocument();
    Widget widget = field.getWidgets().get(0);

    Appearance appearance = widget.getAppearance();
    if(appearance == null)
    {widget.setAppearance(appearance = new Appearance(document));}

    widget.getBaseDataObject().put(
      PdfName.DA,
      new PdfString("/Helv " + getFontSize() + " Tf 0 0 0 rg")
      );

    FormXObject normalAppearanceState = new FormXObject(document);
    {
      Rectangle2D widgetBox = widget.getBox();
      Dimension2D size = new Dimension(widgetBox.getWidth(),widgetBox.getHeight());
      normalAppearanceState.setSize(size);
      PrimitiveComposer composer = new PrimitiveComposer(normalAppearanceState);

      composer.beginLocalState();
      float lineWidth = 1;
      composer.setLineWidth(lineWidth);
      composer.setFillColor(getBackColor());
      composer.setStrokeColor(getForeColor());
      Rectangle2D frame = new Rectangle2D.Double(lineWidth/2,lineWidth/2,size.getWidth()-lineWidth,size.getHeight()-lineWidth);
      composer.drawRectangle(frame,5);
      composer.fillStroke();
      composer.end();

      composer.beginMarkedContent(PdfName.Tx);
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Helvetica,
          false,
          false
          ),
        getFontSize()
        );
      composer.showText(
        (String)field.getValue(),
        new Point2D.Double(0,size.getHeight()/2),
        AlignmentXEnum.Left,
        AlignmentYEnum.Middle,
        0
        );
      composer.end();

      composer.flush();
    }
    appearance.getNormal().put(null,normalAppearanceState);
  }

  private void apply(
    ListBox field
    )
  {
    Document document = field.getDocument();
    Widget widget = field.getWidgets().get(0);

    Appearance appearance = widget.getAppearance();
    if(appearance == null)
    {widget.setAppearance(appearance = new Appearance(document));}

    {
      PdfDictionary widgetDataObject = widget.getBaseDataObject();
      widgetDataObject.put(
        PdfName.DA,
        new PdfString("/Helv " + getFontSize() + " Tf 0 0 0 rg")
        );
      widgetDataObject.put(
        PdfName.MK,
        new PdfDictionary(
          new PdfName[]
          {
            PdfName.BG,
            PdfName.BC
          },
          new PdfDirectObject[]
          {
            new PdfArray(new PdfDirectObject[]{new PdfReal(.9),new PdfReal(.9),new PdfReal(.9)}),
            new PdfArray(new PdfDirectObject[]{new PdfInteger(0),new PdfInteger(0),new PdfInteger(0)})
          }
          )
        );
    }

    FormXObject normalAppearanceState = new FormXObject(document);
    {
      Rectangle2D widgetBox = widget.getBox();
      Dimension2D size = new Dimension(widgetBox.getWidth(),widgetBox.getHeight());
      normalAppearanceState.setSize(size);
      PrimitiveComposer composer = new PrimitiveComposer(normalAppearanceState);

      composer.beginLocalState();
      float lineWidth = 1;
      composer.setLineWidth(lineWidth);
      composer.setFillColor(getBackColor());
      composer.setStrokeColor(getForeColor());
      Rectangle2D frame = new Rectangle2D.Double(lineWidth/2,lineWidth/2,size.getWidth()-lineWidth,size.getHeight()-lineWidth);
      composer.drawRectangle(frame,5);
      composer.fillStroke();
      composer.end();

      composer.beginLocalState();
      composer.drawRectangle(frame,5);
      composer.clip(); // Ensures that the visible content is clipped within the rounded frame.

      composer.beginMarkedContent(PdfName.Tx);
      composer.setFont(
        new StandardType1Font(
          document,
          StandardType1Font.FamilyEnum.Helvetica,
          false,
          false
          ),
        getFontSize()
        );
      double y = 3;
      for(ChoiceItem item : field.getItems())
      {
        composer.showText(
          item.getText(),
          new Point2D.Double(0,y)
          );
        y += getFontSize() * 1.175;
        if(y > size.getHeight())
          break;
      }
      composer.end();
      composer.end();

      composer.flush();
    }
    appearance.getNormal().put(null,normalAppearanceState);
  }
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}