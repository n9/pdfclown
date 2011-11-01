using org.pdfclown.documents;
using org.pdfclown.documents.contents.composition;
using org.pdfclown.documents.contents.fonts;
using org.pdfclown.files;

using System;
using System.Drawing;

namespace org.pdfclown.samples.cli
{
  /**
    <summary>This sample demonstrates the use of standard Type 1 fonts, which are the 14 built-in fonts
    prescribed by the PDF specification to be shipped along with any conformant PDF viewer.</summary>
    <remarks>In particular, this sample displays the complete glyphset of each standard font,
    iterating through character codes and glyph styles (regular, italic, bold).</remarks>
  */
  public class StandardFontSample
    : Sample
  {
    private static readonly int FontBaseSize = 20;
    private static readonly int Margin = 50;

    public override bool Run(
      )
    {
      // 1. PDF file instantiation.
      File file = new File();
      Document document = file.Document;

      // 2. Content creation.
      Populate(document);

      // 3. Serialize the PDF file!
      Serialize(file, false, "Standard Type 1 fonts", "applying standard Type 1 fonts");

      return true;
    }

    private void Populate(
      Document document
      )
    {
      Page page = new Page(document);
      document.Pages.Add(page);
      SizeF pageSize = page.Size;

      PrimitiveComposer composer = new PrimitiveComposer(page);

      int x = Margin, y = Margin;
      StandardType1Font titleFont = new StandardType1Font(
        document,
        StandardType1Font.FamilyEnum.Times,
        true,
        true
        );
      StandardType1Font font = null;
      // Iterating through the standard Type 1 fonts...
      foreach(StandardType1Font.FamilyEnum fontFamily
        in (StandardType1Font.FamilyEnum[])Enum.GetValues(typeof(StandardType1Font.FamilyEnum)))
      {
        // Iterating through the font styles...
        for(int styleIndex = 0; styleIndex < 4; styleIndex++)
        {
          /*
            NOTE: Symbol and Zapf Dingbats are available just as regular fonts (no italic or bold variant).
          */
          if(styleIndex > 0
            && (fontFamily == StandardType1Font.FamilyEnum.Symbol
              || fontFamily == StandardType1Font.FamilyEnum.ZapfDingbats))
              break;

          bool bold, italic;
          switch(styleIndex)
          {
            case 0: // Regular.
              bold = false;
              italic = false;
              break;
            case 1: // Bold.
              bold = true;
              italic = false;
              break;
            case 2: // Italic.
              bold = false;
              italic = true;
              break;
            case 3: // Bold italic.
              bold = true;
              italic = true;
              break;
            default:
              throw new Exception("styleIndex " + styleIndex + " not supported.");
          }
          // Define the font used to show its character set!
          font = new StandardType1Font(
            document,
            fontFamily,
            bold,
            italic
            );

          if(y > pageSize.Height - Margin)
          {
            composer.Flush();

            page = new Page(document);
            document.Pages.Add(page);
            pageSize = page.Size;
            composer = new PrimitiveComposer(page);
            x = Margin; y = Margin;
          }

          if(styleIndex == 0)
          {
            composer.DrawLine(
              new PointF(x,y),
              new PointF(pageSize.Width - Margin,y)
              );
            composer.Stroke();
            y += 5;
          }

          composer.SetFont(
            titleFont,
            FontBaseSize * (styleIndex == 0 ? 1.5f : 1)
            );
          composer.ShowText(
            fontFamily.ToString() + (bold ? " bold" : "") + (italic ? " italic" : ""),
            new PointF(x,y)
            );

          y += 40;
          // Set the font used to show its character set!
          composer.SetFont(font,FontBaseSize);
          // Iterating through the font characters...
          for(int charCode = 32; charCode < 256; charCode++)
          {
            if(y > pageSize.Height - Margin)
            {
              composer.Flush();

              page = new Page(document);
              document.Pages.Add(page);
              pageSize = page.Size;
              composer = new PrimitiveComposer(page);
              x = Margin; y = Margin;

              composer.SetFont(titleFont,FontBaseSize);
              composer.ShowText(
                fontFamily.ToString() + " (continued)",
                new PointF(pageSize.Width - Margin, y),
                AlignmentXEnum.Right,
                AlignmentYEnum.Top,
                0
                );
              composer.SetFont(font,FontBaseSize);
              y += FontBaseSize * 2;
            }

            try
            {
              // Show the current character (using the current standard Type 1 font)!
              composer.ShowText(
                new String(new char[]{(char)charCode}),
                new PointF(x,y)
                );
              x += FontBaseSize;
              if(x > pageSize.Width - Margin)
              {x = Margin; y += 30;}
            }
            catch
            { /* Ignore */ }
          }

          x = Margin; y += Margin;
        }
      }
      composer.Flush();
    }
  }
}