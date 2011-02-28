/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Haakan Aakerberg (bugfix contributor):
      - [FIX:0.0.4:4]

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
using org.pdfclown.documents.contents.objects;
using org.pdfclown.objects;
using org.pdfclown.tokens;

using System;
using System.Collections.Generic;
using System.Globalization;
using io = System.IO;
using System.Text;

namespace org.pdfclown.documents.contents.tokens
{
  /**
    <summary>Content stream parser [PDF:1.6:3.7.1].</summary>
  */
  public sealed class Parser
  {
/*
TODO:IMPL this parser evaluates a subset of the lexical domain of the token parser (clown.tokens.Parser): it should be better to derive both parsers from a common parsing engine in order to avoid unwieldy duplications.
*/
    #region static
    #region fields
    private static readonly NumberFormatInfo StandardNumberFormatInfo = NumberFormatInfo.InvariantInfo;
    #endregion

    #region interface
    #region protected
    private static int GetHex(
      int c
      )
    {
      if(c >= '0' && c <= '9')
        return (c - '0');
      else if(c >= 'A' && c <= 'F')
        return (c - 'A' + 10);
      else if(c >= 'a' && c <= 'f')
        return (c - 'a' + 10);
      else
        return -1;
    }

    /**
      <summary>Evaluate whether a character is a delimiter [PDF:1.6:3.1.1].</summary>
    */
    private static bool IsDelimiter(
      int c
      )
    {return (c == '(' || c == ')' || c == '<' || c == '>' || c == '[' || c == ']' || c == '/' || c == '%');}

    /**
      <summary>Evaluate whether a character is an EOL marker [PDF:1.6:3.1.1].</summary>
    */
    private static bool IsEOL(
      int c
      )
    {return (c == 10 || c == 13);}

    /**
      <summary>Evaluate whether a character is a white-space [PDF:1.6:3.1.1].</summary>
    */
    private static bool IsWhitespace(
      int c
      )
    {return c == 32 || IsEOL(c) || c == 0 || c == 9 || c == 12;}
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region fields
    private readonly PdfDataObject contentStream;

    private long basePosition;
    private bytes::IInputStream stream;
    private int streamIndex = -1;
    private object token;
    private TokenTypeEnum tokenType;
    #endregion

    #region constructors
    internal Parser(
      PdfDataObject contentStream
      )
    {
      this.contentStream = contentStream;

      MoveNextStream();
    }
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the content stream on which parsing is done.</summary>
      <remarks>A content stream may be made up of either a single stream
      or an array of streams.</remarks>
    */
    public PdfDataObject ContentStream
    {get{return contentStream;}}

    public long Length
    {
      get
      {
        if(contentStream is PdfStream) // Single stream.
          return ((PdfStream)contentStream).Body.Length;
        else // Array of streams.
        {
          long length = 0;
          foreach(PdfDirectObject stream in (PdfArray)contentStream)
          {length += ((PdfStream)((PdfReference)stream).DataObject).Body.Length;}
          return length;
        }
      }
    }

    /**
      <param name="offset">Number of tokens to be skipped before reaching the
      intended one.</param>
    */
    public bool MoveNext(
      int offset
      )
    {
      for(
        int index = 0;
        index < offset;
        index++
        )
      {
        if(!MoveNext())
          return false;
      }

      return true;
    }

    /**
      <summary>Parse the next token [PDF:1.6:3.1].</summary>
      <remarks>
        Contract:
        <list type="bullet">
          <li>Preconditions:
            <list type="number">
              <item>To properly parse the current token, the pointer MUST be just before its starting (leading whitespaces are ignored).</item>
            </list>
          </item>
          <item>Postconditions:
            <list type="number">
              <item id="moveNext_contract_post[0]">When this method terminates, the pointer IS at the last byte of the current token.</item>
            </list>
          </item>
          <item>Invariants:
            <list type="number">
              <item>The byte-level position of the pointer IS anytime (during token parsing) at the end of the current token (whereas the 'current token' represents the token-level position of the pointer).</item>
            </list>
          </item>
          <item>Side-effects:
            <list type="number">
              <item>See <see href="#moveNext_contract_post[0]">Postconditions</see>.</item>
            </list>
          </item>
        </list>
      </remarks>
      <returns>Whether a new token was found.</returns>
    */
    public bool MoveNext(
      )
    {
      if(stream == null)
        return false;

      /*
        NOTE: It'd be interesting to evaluate an alternative regular-expression-based
        implementation...
      */
      int c = 0;

      // Skip white-space characters [PDF:1.6:3.1.1].
      while(true)
      {
        c = stream.ReadByte();
        if(c == -1)
        {
          /* NOTE: Current stream has finished. */
          // Move to the next stream!
          MoveNextStream();
          // No more streams?
          if(stream == null)
            return false;
        }
        else if(!IsWhitespace(c)) // Keep goin' till there's a white-space character...
        {break;}
      }

      StringBuilder buffer = null;
      token = null;
      // Which character is it?
      switch(c)
      {
        case '/': // Name.
          tokenType = TokenTypeEnum.Name;

          buffer = new StringBuilder();
          while((c = stream.ReadByte()) != -1)
          {
            if(IsDelimiter(c) || IsWhitespace(c))
              break;
            // Is it an hexadecimal code [PDF:1.6:3.2.4]?
            if(c == '#')
            {
              try
              {c = (GetHex(stream.ReadByte()) << 4) + GetHex(stream.ReadByte());}
              catch
              {throw new FileFormatException("Unexpected EOF (malformed hexadecimal code in name object).",stream.Position);}
            }

            buffer.Append((char)c);
          }
          stream.Skip(-1); // Recover the first byte after the current token.
          break;
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
        case '.':
        case '-':
        case '+': // Number [PDF:1.6:3.2.2] | Indirect reference.
          switch(c)
          {
            case '.': // Decimal point.
              tokenType = TokenTypeEnum.Real;
              break;
            default: // Digit or signum.
              tokenType = TokenTypeEnum.Integer; // By default (it may be real).
              break;
          }

          // Building the number...
          buffer = new StringBuilder();
          do
          {
            buffer.Append((char)c);
            c = stream.ReadByte();
            if(c == -1)
              break;

            if(c == '.')
              tokenType = TokenTypeEnum.Real;
            else if(c < '0' || c > '9')
              break;
          } while(true);

          stream.Skip(-1); // Recover the first byte after the current token.

          break;
        case '[': // Array (begin).
          tokenType = TokenTypeEnum.ArrayBegin;

          break;
        case ']': // Array (end).
          tokenType = TokenTypeEnum.ArrayEnd;

          break;
        case '<': // Dictionary (begin) | Hexadecimal string.
          c = stream.ReadByte();
          if(c == -1)
            throw new FileFormatException("Unexpected EOF (isolated opening angle-bracket character).",stream.Position);
          // Is it a dictionary (2nd angle bracket [PDF:1.6:3.2.6])?
          if(c == '<')
          {
            tokenType = TokenTypeEnum.DictionaryBegin;
            break;
          }

          // Hexadecimal string (single angle bracket [PDF:1.6:3.2.3]).
          tokenType = TokenTypeEnum.Hex;

          // [FIX:0.0.4:4] It skipped after the first hexadecimal character, missing it.
          buffer = new StringBuilder();
          while(c != '>') // NOT string end.
          {
            buffer.Append((char)c);

            c = stream.ReadByte();
            if(c == -1)
              throw new FileFormatException("Unexpected EOF (malformed hex string).",stream.Position);
          }

          break;
        case '>': // Dictionary (end).
          c = stream.ReadByte();
          if(c != '>')
            throw new FileFormatException("Malformed dictionary.",stream.Position);

          tokenType = TokenTypeEnum.DictionaryEnd;

          break;
        case '(': // Literal string.
          tokenType = TokenTypeEnum.Literal;

          buffer = new StringBuilder();
          int level = 0;
          while(true)
          {
            c = stream.ReadByte();
            if(c == -1)
              break;
            if(c == '(')
              level++;
            else if(c == ')')
              level--;
            else if(c == '\\')
            {
              bool lineBreak = false;
              c = stream.ReadByte();
              switch(c)
              {
                case 'n':
                  c = '\n';
                  break;
                case 'r':
                  c = '\r';
                  break;
                case 't':
                  c = '\t';
                  break;
                case 'b':
                  c = '\b';
                  break;
                case 'f':
                  c = '\f';
                  break;
                case '(':
                case ')':
                case '\\':
                  break;
                case '\r':
                  lineBreak = true;
                  c = stream.ReadByte();
                  if(c != '\n')
                    stream.Skip(-1);
                  break;
                case '\n':
                  lineBreak = true;
                  break;
                default:
                {
                  // Is it outside the octal encoding?
                  if(c < '0' || c > '7') break;

                  // Octal [PDF:1.6:3.2.3].
                  int octal = c - '0';
                  c = stream.ReadByte();
                  // Octal end?
                  if(c < '0' || c > '7')
                  {c = octal; stream.Skip(-1); break;}
                  octal = (octal << 3) + c - '0';
                  c = stream.ReadByte();
                  // Octal end?
                  if(c < '0' || c > '7')
                  {c = octal; stream.Skip(-1); break;}
                  octal = (octal << 3) + c - '0';
                  c = octal & 0xff;
                  break;
                }
              }
              if(lineBreak)
                continue;
              if(c == -1)
                break;
            }
            else if(c == '\r')
            {
              c = stream.ReadByte();
              if(c == -1)
                break;
              if(c != '\n')
              {c = '\n'; stream.Skip(-1);}
            }
            if(level == -1)
              break;

            buffer.Append((char)c);
          }
          if(c == -1)
            throw new FileFormatException("Malformed literal string.",stream.Position);

          break;
        case '%': // Comment.
          tokenType = TokenTypeEnum.Comment;

          buffer = new StringBuilder();
          while(true)
          {
            c = stream.ReadByte();
            if(c == -1
              || IsEOL(c))
              break;

            buffer.Append((char)c);
          }

          break;
        default: // Keyword.
          tokenType = TokenTypeEnum.Keyword;

          buffer = new StringBuilder();
          do
          {
            buffer.Append((char)c);
            c = stream.ReadByte();
            if(c == -1)
              break;
          } while(!IsDelimiter(c) && !IsWhitespace(c));
          stream.Skip(-1); // Recover the first byte after the current token.

          break;
      }

      if(buffer != null)
      {
        /*
          Here we prepare the current token state.
        */
        // Wich token type?
        switch(tokenType)
        {
          case TokenTypeEnum.Keyword:
            token = buffer.ToString();
            // Late recognition.
            switch((string)token)
            {
              case Keyword.False:
              case Keyword.True: // Boolean.
                tokenType = TokenTypeEnum.Boolean;
                token =  bool.Parse((string)token);
                break;
              case Keyword.Null: // Null.
                tokenType = TokenTypeEnum.Null;
                token = null;
                break;
            }
            break;
          case TokenTypeEnum.Comment:
          case TokenTypeEnum.Hex:
          case TokenTypeEnum.Name:
            token = buffer.ToString();
            break;
          case TokenTypeEnum.Literal:
            token = buffer.ToString();
            // Late recognition.
            if(((string)token).StartsWith("D:")) // Date.
            {
              tokenType = TokenTypeEnum.Date;
              token = PdfDate.ToDate((string)token);
            }
            break;
          case TokenTypeEnum.Integer:
            token = Int32.Parse(
              buffer.ToString(),
              NumberStyles.Integer,
              StandardNumberFormatInfo
              );
            break;
          case TokenTypeEnum.Real:
            // [FIX:1668410] Parsing of float numbers was buggy (localized default number format).
            token = Single.Parse(
              buffer.ToString(),
              NumberStyles.Float,
              StandardNumberFormatInfo
              );
            break;
        }
      }

      return true;
    }

    /**
      <summary>Parses the next content object [PDF:1.6:4.1], may it be a single operation
      or a graphics object.</summary>
    */
    public ContentObject ParseContentObject(
      )
    {
      Operation operation = ParseOperation();
      if(operation is PaintXObject) // External object.
        return new XObject((PaintXObject)operation);
      else if(operation is PaintShading) // Shading.
        return new Shading((PaintShading)operation);
      else if(operation is BeginSubpath
        || operation is DrawRectangle) // Path.
        return ParsePath(operation);
      else if(operation is BeginText) // Text.
        return new Text(
          ParseContentObjects()
          );
      else if(operation is SaveGraphicsState) // Local graphics state.
        return new LocalGraphicsState(
          ParseContentObjects()
          );
      else if(operation is BeginMarkedContent) // Marked-content sequence.
        return new MarkedContent(
          (BeginMarkedContent)operation,
          ParseContentObjects()
          );
      else if(operation is BeginInlineImage) // Inline image.
        return ParseInlineImage();
      else // Single operation.
        return operation;
    }

    public List<ContentObject> ParseContentObjects(
      )
    {
      List<ContentObject> contentObjects = new List<ContentObject>();
      while(MoveNext())
      {
        ContentObject contentObject = ParseContentObject();
        // Multiple-operation graphics object end?
        if(contentObject is EndText // Text.
          || contentObject is RestoreGraphicsState // Local graphics state.
          || contentObject is EndMarkedContent // End marked-content sequence.
          || contentObject is EndInlineImage) // Inline image.
          return contentObjects;

        contentObjects.Add(contentObject);
      }
      return contentObjects;
    }

    public Operation ParseOperation(
      )
    {
      string operator_ = null;
      List<PdfDirectObject> operands = new List<PdfDirectObject>();
      // Parsing the operation parts...
      while(true)
      {
        // Did we reach the operator keyword?
        if(tokenType == TokenTypeEnum.Keyword)
        {
          operator_ = (string)token;
          break;
        }

        operands.Add(ParsePdfObject()); MoveNext();
      }
      return Operation.Get(operator_,operands);
    }

    /**
      <remarks>
        <para>Require[0]: when this method is invoked, the pointer MUST be at (the end of) the first
        token of the object.</para>
        <para>Ensure[0]: when this method terminates, the pointer IS at (the end of) the last token of the object.</para>
        <para>Invariant[0]: stream data IS kept untouched.</para>
        <para>Side effect[0]: see Ensure[0].</para>
      </remarks>
    */
    public PdfDirectObject ParsePdfObject(
      )
    {
      do
      {
        switch(tokenType)
        {
          case TokenTypeEnum.Integer:
            return new PdfInteger((int)token);
          case TokenTypeEnum.Name:
            return new PdfName((string)token,true);
          case TokenTypeEnum.Literal:
            return new PdfString(
              org.pdfclown.tokens.Encoding.Encode((string)token),
              PdfString.SerializationModeEnum.Literal
              );
          case TokenTypeEnum.DictionaryBegin:
            PdfDictionary dictionary = new PdfDictionary();
            // Populate the dictionary.
            while(true)
            {
              // Key.
              MoveNext();
              if(tokenType == TokenTypeEnum.DictionaryEnd)
                break;
              PdfName key = (PdfName)ParsePdfObject();

              // Value.
              MoveNext();
              PdfDirectObject value = (PdfDirectObject)ParsePdfObject();

              // Add the current entry to the dictionary!
              dictionary[key] = value;
            }
            return dictionary;
          case TokenTypeEnum.ArrayBegin:
            PdfArray array = new PdfArray();
            // Populate the array.
            while(true)
            {
              // Value.
              MoveNext();
              if(tokenType == TokenTypeEnum.ArrayEnd)
                break;

              // Add the current item to the array!
              array.Add((PdfDirectObject)ParsePdfObject());
            }
            return array;
          case TokenTypeEnum.Real:
            return new PdfReal((float)token);
          case TokenTypeEnum.Boolean:
            return PdfBoolean.Get((bool)token);
          case TokenTypeEnum.Date:
            return new PdfDate((DateTime)token);
          case TokenTypeEnum.Hex:
            return new PdfString(
              (string)token,
              PdfString.SerializationModeEnum.Hex
              );
          case TokenTypeEnum.Null:
            return null;
          case TokenTypeEnum.Comment:
            // NOOP: Comments are simply ignored and skipped.
            break;
          default:
            throw new Exception("Unknown type: " + tokenType);
        }
      } while(MoveNext());

      return null;
    }

    public long Position
    {get{return basePosition + stream.Position;}}

    public void Seek(
      long position
      )
    {
      while(true)
      {
        if(position < basePosition) //Before current stream.
        {
          if(!MovePreviousStream())
            throw new ArgumentException("Lower than acceptable.","position");
        }
        else if(position > basePosition + stream.Length) // After current stream.
        {
          if(!MoveNextStream())
            throw new ArgumentException("Higher than acceptable.","position");
        }
        else // At current stream.
        {
          stream.Seek(position - basePosition);
          break;
        }
      }
    }

    public void Skip(
      long offset
      )
    {
      while(true)
      {
        long position = stream.Position + offset;
        if(position < 0) //Before current stream.
        {
          offset += stream.Position;
          if(!MovePreviousStream())
            throw new ArgumentException("Lower than acceptable.","offset");

          stream.Position = stream.Length;
        }
        else if(position > stream.Length) // After current stream.
        {
          offset -= (stream.Length - stream.Position);
          if(!MoveNextStream())
            throw new ArgumentException("Higher than acceptable.","offset");
        }
        else // At current stream.
        {
          stream.Skip(position);
          break;
        }
      }
    }

    /**
      <summary>Move to the last whitespace after the current position in order
      to let read the first non-whitespace.</summary>
    */
    public bool SkipWhitespace(
      )
    {
      int b;
      do
      {
        b = stream.ReadByte();
        if(b == -1)
          return false;
      } while(IsWhitespace(b)); // Keep goin' till there's a white-space character...
      // Recover the last whitespace position!
      stream.Skip(-1); // Recover the last whitespace position.

      return true;
    }

    /**
      <summary>Gets the current stream.</summary>
    */
    public bytes::IInputStream Stream
    {get{return stream;}}

    /**
      <summary>Gets the current stream index.</summary>
    */
    public int StreamIndex
    {get{return streamIndex;}}

    /**
      <summary>Gets the currently-parsed token.</summary>
      <returns>The current token.</returns>
    */
    public object Token
    {get{return token;}}

    /**
      <summary>Gets the currently-parsed token type.</summary>
      <returns>The current token type.</returns>
    */
    public TokenTypeEnum TokenType
    {get{return tokenType;}}
    #endregion

    #region private
    private bool MoveNextStream(
      )
    {
      // Is the content stream just a single stream?
      /*
        NOTE: A content stream may be made up of multiple streams [PDF:1.6:3.6.2].
      */
      if(contentStream is PdfStream) // Single stream.
      {
        if(streamIndex < 1)
        {
          streamIndex++;

          basePosition = (streamIndex == 0
            ? 0
            : basePosition + stream.Length);

          stream = (streamIndex < 1
            ? ((PdfStream)contentStream).Body
            : null);
        }
      }
      else // Multiple streams.
      {
        PdfArray streams = (PdfArray)contentStream;
        if(streamIndex < streams.Count)
        {
          streamIndex++;

          basePosition = (streamIndex == 0
            ? 0
            : basePosition + stream.Length);

          stream = (streamIndex < streams.Count
            ? ((PdfStream)streams.Resolve(streamIndex)).Body
            : null);
        }
      }
      if(stream == null)
        return false;

      stream.Position = 0;
      return true;
    }

    private bool MovePreviousStream(
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
      if(contentStream is PdfStream) // Single stream.
      {
        stream = ((PdfStream)contentStream).Body;
        basePosition = 0;
      }
      else // Array of streams.
      {
        PdfArray streams = (PdfArray)contentStream;

        stream = ((PdfStream)((PdfReference)streams[streamIndex]).DataObject).Body;
        basePosition -= stream.Length;
      }

      return true;
    }

    private InlineImage ParseInlineImage(
      )
    {
      /*
        NOTE: Inline images use a peculiar syntax that's an exception to the usual rule
        that the data in a content stream is interpreted according to the standard PDF syntax
        for objects.
      */
      InlineImageHeader header;
      {
        List<PdfDirectObject> operands = new List<PdfDirectObject>();
        // Parsing the image entries...
        while(MoveNext()
          && tokenType != TokenTypeEnum.Keyword) // Not keyword (i.e. end at image data beginning (ID operator)).
        {operands.Add(ParsePdfObject());}
        header = new InlineImageHeader(operands);
      }

      InlineImageBody body;
      {
        MoveNext();
        bytes::Buffer data = new bytes::Buffer();
        byte c1 = 0, c2 = 0;
        do
        {
          try
          {
            while(true)
            {
              c1 = (byte)stream.ReadByte();
              c2 = (byte)stream.ReadByte();
              if(c1 == 'E' && c2 == 'I')
                break;

              data.Append(c1);
              data.Append(c2);
            } break;
          }
          catch
          {
            /* NOTE: Current stream has finished. */
            // Move to the next stream!
            MoveNextStream();
          }
        } while(stream != null);
        body = new InlineImageBody(data);
      }

      return new InlineImage(
        header,
        body
        );
    }

    private Path ParsePath(
      Operation beginOperation
      )
    {
      /*
        NOTE: Paths do not have an explicit end operation, so we must infer it
        looking for the first non-painting operation.
      */
      IList<ContentObject> operations = new List<ContentObject>();
      {
        operations.Add(beginOperation);
        long position = Position;
        bool closeable = false;
        while(MoveNext())
        {
          Operation operation = ParseOperation();
          // Multiple-operation graphics object closeable?
          if(operation is PaintPath) // Painting operation.
          {closeable = true;}
          else if(closeable) // Past end (first non-painting operation).
          {
            Seek(position); // Rolls back to the last path-related operation.

            break;
          }

          operations.Add(operation);
          position = Position;
        }
      }
      return new Path(operations);
    }
    #endregion
    #endregion
    #endregion
  }
}