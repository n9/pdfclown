/*
  Copyright 2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.contents.tokens;

import java.io.EOFException;
import java.io.IOException;
import java.nio.ByteOrder;
import java.util.ArrayList;
import java.util.List;

import org.pdfclown.bytes.Buffer;
import org.pdfclown.bytes.IInputStream;
import org.pdfclown.documents.contents.objects.BeginInlineImage;
import org.pdfclown.documents.contents.objects.BeginMarkedContent;
import org.pdfclown.documents.contents.objects.BeginSubpath;
import org.pdfclown.documents.contents.objects.BeginText;
import org.pdfclown.documents.contents.objects.ContentObject;
import org.pdfclown.documents.contents.objects.DrawRectangle;
import org.pdfclown.documents.contents.objects.EndInlineImage;
import org.pdfclown.documents.contents.objects.EndMarkedContent;
import org.pdfclown.documents.contents.objects.EndText;
import org.pdfclown.documents.contents.objects.InlineImage;
import org.pdfclown.documents.contents.objects.InlineImageBody;
import org.pdfclown.documents.contents.objects.InlineImageHeader;
import org.pdfclown.documents.contents.objects.LocalGraphicsState;
import org.pdfclown.documents.contents.objects.MarkedContent;
import org.pdfclown.documents.contents.objects.Operation;
import org.pdfclown.documents.contents.objects.PaintPath;
import org.pdfclown.documents.contents.objects.PaintShading;
import org.pdfclown.documents.contents.objects.PaintXObject;
import org.pdfclown.documents.contents.objects.Path;
import org.pdfclown.documents.contents.objects.RestoreGraphicsState;
import org.pdfclown.documents.contents.objects.SaveGraphicsState;
import org.pdfclown.documents.contents.objects.Shading;
import org.pdfclown.documents.contents.objects.Text;
import org.pdfclown.documents.contents.objects.XObject;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDataObject;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfStream;
import org.pdfclown.objects.PdfString;
import org.pdfclown.tokens.BaseParser;
import org.pdfclown.tokens.Encoding;
import org.pdfclown.tokens.FileFormatException;
import org.pdfclown.util.NotImplementedException;

/**
  Content stream parser [PDF:1.6:3.7.1].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.1.1
  @version 0.1.1, 03/17/11
*/
public final class ContentParser
  extends BaseParser
{
  // <class>
  // <classes>
  public static class ContentStream
    implements IInputStream
  {
    private final PdfDataObject baseDataObject;

    private long basePosition;
    private IInputStream stream;
    private int streamIndex = -1;

    public ContentStream(
      PdfDataObject baseDataObject
      )
    {
      this.baseDataObject = baseDataObject;
      moveNextStream();
    }

    public PdfDataObject getBaseDataObject(
      )
    {return baseDataObject;}

    @Override
    public long getLength(
      )
    {
      if(baseDataObject instanceof PdfStream) // Single stream.
        return ((PdfStream)baseDataObject).getBody().getLength();
      else // Array of streams.
      {
        int length = 0;
        for(PdfDirectObject stream : (PdfArray)baseDataObject)
        {length += ((PdfStream)((PdfReference)stream).getDataObject()).getBody().getLength();}
        return length;
      }
    }

    @Override
    public void close(
      ) throws IOException
    {/* NOOP */}

    @Override
    public ByteOrder getByteOrder(
      )
    {return stream.getByteOrder();}

    @Override
    public long getPosition(
      )
    {return basePosition + stream.getPosition();}

    @Override
    public void read(
      byte[] data
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public void read(
      byte[] data,
      int offset,
      int length
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public byte readByte(
      ) throws EOFException
    {
      while(true)
      {
        try
        {return stream.readByte();}
        catch(Exception e)
        {
          if(stream == null || e instanceof EOFException)
          {
            if(!moveNextStream())
              throw new EOFException();
          }
          else
            throw new RuntimeException(e);
        }
      }
    }

    @Override
    public int readInt(
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public int readInt(
      int length
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public String readLine(
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public short readShort(
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public String readString(
      int length
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public int readUnsignedByte(
      ) throws EOFException
    {
      while(true)
      {
        try
        {return stream.readUnsignedByte();}
        catch(Exception e)
        {
          if(stream == null || e instanceof EOFException)
          {
            if(!moveNextStream())
              throw new EOFException();
          }
          else
            throw new RuntimeException(e);
        }
      }
    }

    @Override
    public int readUnsignedShort(
      ) throws EOFException
    {throw new NotImplementedException();}

    @Override
    public void seek(
      long position
      )
    {
      while(true)
      {
        if(position < basePosition) //Before current stream.
        {
          if(!movePreviousStream())
            throw new IllegalArgumentException("The 'position' argument is lower than acceptable.");
        }
        else if(position > basePosition + stream.getLength()) // After current stream.
        {
          if(!moveNextStream())
            throw new IllegalArgumentException("The 'position' argument is higher than acceptable.");
        }
        else // At current stream.
        {
          stream.seek(position - basePosition);
          break;
        }
      }
    }

    @Override
    public void setByteOrder(
      ByteOrder value
      )
    {throw new UnsupportedOperationException();}

    @Override
    public void setPosition(
      long value
      )
    {seek(value);}

    @Override
    public void skip(
      long offset
      )
    {
      while(true)
      {
        long position = stream.getPosition() + offset;
        if(position < 0) //Before current stream.
        {
          offset += stream.getPosition();
          if(!movePreviousStream())
            throw new IllegalArgumentException("The 'offset' argument is lower than acceptable.");

          stream.setPosition(stream.getLength());
        }
        else if(position > stream.getLength()) // After current stream.
        {
          offset -= (stream.getLength() - stream.getPosition());
          if(!moveNextStream())
            throw new IllegalArgumentException("The 'offset' argument is higher than acceptable.");
        }
        else // At current stream.
        {
          stream.seek(position);
          break;
        }
      }
    }

    @Override
    public byte[] toByteArray(
      )
    {throw new NotImplementedException();}

    private boolean moveNextStream(
      )
    {
      // Is the content stream just a single stream?
      /*
        NOTE: A content stream may be made up of multiple streams [PDF:1.6:3.6.2].
      */
      if(baseDataObject instanceof PdfStream) // Single stream.
      {
        if(streamIndex < 1)
        {
          streamIndex++;

          basePosition = (streamIndex == 0
            ? 0
            : basePosition + stream.getLength());

          stream = (streamIndex < 1
            ? ((PdfStream)baseDataObject).getBody()
            : null);
        }
      }
      else // Multiple streams.
      {
        PdfArray streams = (PdfArray)baseDataObject;
        if(streamIndex < streams.size())
        {
          streamIndex++;

          basePosition = (streamIndex == 0
            ? 0
            : basePosition + stream.getLength());

          stream = (streamIndex < streams.size()
            ? ((PdfStream)streams.resolve(streamIndex)).getBody()
            : null);
        }
      }
      if(stream == null)
        return false;

      stream.setPosition(0);
      return true;
    }

    private boolean movePreviousStream(
      )
    {
      if(streamIndex == 0)
      {
        streamIndex--;
        stream = null;
      }
      if(streamIndex == -1)
        return false;

      streamIndex--;
      /* NOTE: A content stream may be made up of multiple streams [PDF:1.6:3.6.2]. */
      // Is the content stream just a single stream?
      if(baseDataObject instanceof PdfStream) // Single stream.
      {
        stream = ((PdfStream)baseDataObject).getBody();
        basePosition = 0;
      }
      else // Array of streams.
      {
        PdfArray streams = (PdfArray)baseDataObject;

        stream = ((PdfStream)((PdfReference)streams.get(streamIndex)).getDataObject()).getBody();
        basePosition -= stream.getLength();
      }

      return true;
    }
  }
  // <classes>

  // <dynamic>
  // <constructors>
  public ContentParser(
    PdfDataObject contentStreamObject
    )
  {super(new ContentStream(contentStreamObject));}
  // </constructors>

  // <interface>
  // <public>
  /**
    Parses the next content object [PDF:1.6:4.1].
  */
  public ContentObject parseContentObject(
    ) throws FileFormatException
  {
    final Operation operation = parseOperation();
    if(operation instanceof PaintXObject) // External object.
      return new XObject((PaintXObject)operation);
    else if(operation instanceof PaintShading) // Shading.
      return new Shading((PaintShading)operation);
    else if(operation instanceof BeginSubpath
      || operation instanceof DrawRectangle) // Path.
      return parsePath(operation);
    else if(operation instanceof BeginText) // Text.
      return new Text(
        parseContentObjects()
        );
    else if(operation instanceof SaveGraphicsState) // Local graphics state.
      return new LocalGraphicsState(
        parseContentObjects()
        );
    else if(operation instanceof BeginMarkedContent) // Marked-content sequence.
      return new MarkedContent(
        (BeginMarkedContent)operation,
        parseContentObjects()
        );
    else if(operation instanceof BeginInlineImage) // Inline image.
      return parseInlineImage();
    else // Single operation.
      return operation;
  }

  /**
    Parses the next content objects.
  */
  public List<ContentObject> parseContentObjects(
    ) throws FileFormatException
  {
    final List<ContentObject> contentObjects = new ArrayList<ContentObject>();
    while(moveNext())
    {
      ContentObject contentObject = parseContentObject();
      // Multiple-operation graphics object end?
      if(contentObject instanceof EndText // Text.
        || contentObject instanceof RestoreGraphicsState // Local graphics state.
        || contentObject instanceof EndMarkedContent // End marked-content sequence.
        || contentObject instanceof EndInlineImage) // Inline image.
        return contentObjects;

      contentObjects.add(contentObject);
    }
    return contentObjects;
  }

  /**
    Parses the next operation.
  */
  public Operation parseOperation(
    ) throws FileFormatException
  {
    String operator = null;
    final List<PdfDirectObject> operands = new ArrayList<PdfDirectObject>();
    // Parsing the operation parts...
    while(true)
    {
      // Did we reach the operator keyword?
      if(getTokenType() == TokenTypeEnum.Keyword)
      {
        operator = (String)getToken();
        break;
      }

      operands.add(parsePdfObject()); moveNext();
    }
    return Operation.get(operator,operands);
  }

  @Override
  public PdfDirectObject parsePdfObject(
    ) throws FileFormatException
  {
    switch(getTokenType())
    {
      case Literal:
        if(getToken() instanceof String)
          return new PdfString(
            Encoding.encode((String)getToken()),
            PdfString.SerializationModeEnum.Literal
            );
        break;
      case Hex:
        return new PdfString(
          (String)getToken(),
          PdfString.SerializationModeEnum.Hex
          );
    }
    return (PdfDirectObject)super.parsePdfObject();
  }
  // </public>

  // <private>
  private InlineImage parseInlineImage(
    ) throws FileFormatException
  {
    InlineImageHeader header;
    {
      final List<PdfDirectObject> operands = new ArrayList<PdfDirectObject>();
      // Parsing the image entries...
      while(moveNext()
        && getTokenType() != TokenTypeEnum.Keyword) // Ends at image body beginning (ID operator).
      {operands.add(parsePdfObject());}
      header = new InlineImageHeader(operands);
    }

    InlineImageBody body;
    {
      IInputStream stream = getStream();
      moveNext();
      Buffer data = new Buffer();
      try
      {
        while(true)
        {
          byte c1 = stream.readByte();
          byte c2 = stream.readByte();
          if(c1 == 'E' && c2 == 'I')
            break;

          data.append(c1);
          data.append(c2);
        }
      }
      catch(EOFException e)
      {throw new FileFormatException("Unexpected EOF on inline-image body parsing", e, -1);}
      body = new InlineImageBody(data);
    }

    return new InlineImage(header, body);
  }

  private Path parsePath(
    Operation beginOperation
    ) throws FileFormatException
  {
    /*
      NOTE: Paths do not have an explicit end operation, so we must infer it
      looking for the first non-painting operation.
    */
    final List<ContentObject> operations = new ArrayList<ContentObject>();
    {
      operations.add(beginOperation);
      long position = getPosition();
      boolean closeable = false;
      while(moveNext())
      {
        Operation operation = parseOperation();
        // Multiple-operation graphics object closeable?
        if(operation instanceof PaintPath) // Painting operation.
        {closeable = true;}
        else if(closeable) // Past end (first non-painting operation).
        {
          seek(position); // Rolls back to the last path-related operation.
          break;
        }

        operations.add(operation);
        position = getPosition();
      }
    }
    return new Path(operations);
  }
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}