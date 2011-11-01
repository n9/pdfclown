package org.pdfclown.samples.cli;

import java.io.EOFException;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.pdfclown.bytes.IBuffer;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDataObject;
import org.pdfclown.objects.PdfDate;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfSimpleObject;
import org.pdfclown.objects.PdfStream;
import org.pdfclown.objects.PdfString;

/**
  This sample demonstrates <b>how to visit the structure of a PDF file</b>.
  <p>Note this is just a simple exercise for low-level data matching; if you need more advanced
  functionalities (e.g. text content extraction, text highlighting...), see the related samples
  instead (e.g. {@link AdvancedTextExtractionSample}, {@link TextHighlightSample}...).</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.1, 11/01/11
*/
public class DataSearchSample
  extends Sample
{

  @Override
  public boolean run()
  {
    // 1. Opening the PDF file...
    File file;
    {
      String filePath = promptPdfFileChoice("Please select a PDF file");
      try
      {file = new File(filePath);}
      catch(Exception e)
      {throw new RuntimeException(filePath + " file access error.",e);}
    }

    // 2. Define the text pattern to look for!
    String textRegEx = promptChoice("Please enter the pattern to look for: ");
    Pattern pattern = Pattern.compile(textRegEx, Pattern.CASE_INSENSITIVE);

    // 3. Search.
    /*
      NOTE: In order to navigate through the file data, we can either (a) descend from the document
      root or (b) iterate through the indirect objects collection.
    */
    // 3.a. Searching from the document root object.
    System.out.println("We can alternatively search starting from the document root object or iterating through the file's indirect objects collection.");
    System.out.println("\nApproach 1: Search starting from the document root object...");
    search(pattern, file.getDocument().getBaseObject(), "", new HashSet<PdfReference>());

    // 3.b. Searching through the file's indirect objects.
    System.out.println("\nApproach 2: Search iterating through the file's indirect objects collection...");
    Set<PdfReference> visitedReferences = new HashSet<PdfReference>();
    for(PdfIndirectObject object : file.getIndirectObjects())
    {search(pattern, object.getReference(), "", visitedReferences);}

    return true;
  }

  private void search(
    Pattern pattern,
    PdfDataObject object,
    String path,
    Set<PdfReference> visitedReferences
    )
  {
    if(object instanceof PdfReference)
    {
      if(visitedReferences.contains(object))
        return;

      path += "[" + ((PdfReference)object).getIndirectReference() + "]";
      visitedReferences.add((PdfReference)object);
      object = File.resolve(object);
    }

    Matcher matcher = null;
    if(object instanceof PdfName
      || (object instanceof PdfString && !(object instanceof PdfDate)))
    {matcher = pattern.matcher((String)((PdfSimpleObject<?>)object).getValue());}
    else if(object instanceof PdfDictionary)
    {
      for(Map.Entry<PdfName,PdfDirectObject> entry : ((PdfDictionary)object).entrySet())
      {search(pattern, entry.getValue(), path + "/" + entry.getKey(), visitedReferences);}
    }
    else if(object instanceof PdfArray)
    {
      int index = 0;
      for(PdfDirectObject item : (PdfArray)object)
      {search(pattern, item, path + "/" + index++, visitedReferences);}
    }
    else if(object instanceof PdfStream)
    {
      PdfStream stream = (PdfStream)object;
      if(PdfName.XObject.equals(stream.getHeader().get(PdfName.Type))
        && PdfName.Form.equals(stream.getHeader().get(PdfName.Subtype)))
      {
        IBuffer body = ((PdfStream)object).getBody();
        try
        {
          body.seek(0);
          String content = body.readString((int)body.getLength());
          matcher = pattern.matcher(content);
        }
        catch(EOFException e)
        {/* NOOP */}
      }
    }
    if(matcher != null
      && matcher.find())
    {System.out.println(path + ":" + matcher.group());}
  }
}
