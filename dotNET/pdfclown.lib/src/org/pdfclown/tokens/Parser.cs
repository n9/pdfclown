/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)
    * Haakan Aakerberg (bugfix contributor):
      - [FIX:0.0.4:1]
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

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace org.pdfclown.tokens
{
  /**
    <summary>Token parser.</summary>
  */
  public sealed class Parser
    : IDisposable
  {
    #region types
    public struct Reference
    {
      #region fields
      public readonly int ObjectNumber;
      public readonly int GenerationNumber;
      #endregion

      #region constructors
      internal Reference(
        int objectNumber,
        int generationNumber
        )
      {
        this.ObjectNumber = objectNumber;
        this.GenerationNumber = generationNumber;
      }
      #endregion
    }
    #endregion

    #region static
    #region fields
    private static readonly NumberFormatInfo StandardNumberFormatInfo = NumberFormatInfo.InvariantInfo;
    #endregion

    #region interface
    #region private
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
    {
      return c == Symbol.OpenRoundBracket
        || c == Symbol.CloseRoundBracket
        || c == Symbol.OpenAngleBracket
        || c == Symbol.CloseAngleBracket
        || c == Symbol.OpenSquareBracket
        || c == Symbol.CloseSquareBracket
        || c == Symbol.Slash
        || c == Symbol.Percent;
    }

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
    private files.File file;
    private IInputStream stream;
    private object token;
    private TokenTypeEnum tokenType;

    private bool multipleTokenParsing;
    #endregion

    #region constructors
    internal Parser(
      IInputStream stream,
      files.File file
      )
    {
      this.stream = stream;
      this.file = file;
    }
    #endregion

    #region interface
    #region public
    public override int GetHashCode(
      )
    {return stream.GetHashCode();}

    /**
      <summary>Gets a token after moving to the given offset.</summary>
      <param name="offset">Number of tokens to skip before reaching the intended one.</param>
      <seealso cref="Token"/>
    */
    public object GetToken(
      int offset
      )
    {MoveNext(offset); return Token;}

    public long Length
    {
      get
      {return stream.Length;}
    }

    /**
      <summary>Moves the pointer to the next token.</summary>
      <param name="offset">Number of tokens to skip before reaching the intended one.</param>
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
      <summary>Parses the next token [PDF:1.6:3.1].</summary>
      <remarks>To properly parse the current token, the pointer MUST be just before its starting
      (leading whitespaces are ignored). When this method terminates, the pointer IS
      at the last byte of the current token.</remarks>
      <returns>Whether a new token was found.</returns>
    */
    public bool MoveNext(
      )
    {
      /*
        NOTE: It'd be interesting to evaluate an alternative regular-expression-based
        implementation...
      */
      StringBuilder buffer = null;
      token = null;
      int c = 0;

      // Skip white-space characters [PDF:1.6:3.1.1].
      do
      {
        c = stream.ReadByte();
        if(c == -1)
          return false;
      } while(IsWhitespace(c)); // Keep goin' till there's a white-space character...

      // Which character is it?
      switch(c)
      {
        case Symbol.Slash: // Name.
          tokenType = TokenTypeEnum.Name;

          buffer = new StringBuilder();
          while(true)
          {
            c = stream.ReadByte();
            if(c == -1)
              throw new FileFormatException("Unexpected EOF (malformed name object).",stream.Position);
            if(IsDelimiter(c) || IsWhitespace(c))
              break;

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
            case '-':
            case '+': // Signum.
              tokenType = TokenTypeEnum.Integer; // By default (it may be real).
              break;
            default: // Digit.
              if(multipleTokenParsing) // Plain number (multiple token parsing -- see indirect reference search).
              {
                tokenType = TokenTypeEnum.Integer; // By default (it may be real).
              }
              else // Maybe an indirect reference (postfix notation [PDF:1.6:3.2.9]).
              {
                /*
                  NOTE: We need to identify this pattern:
                  ref :=  { int int 'R' }
                */
                // Enable multiple token parsing!
                // NOTE: This state MUST be disabled before returning.
                multipleTokenParsing = true;

                // 1. Object number.
                // Try the possible object number!
                stream.Skip(-1); MoveNext();
                // Isn't it a valid object number?
                if(tokenType != TokenTypeEnum.Integer)
                {
                  // Disable multiple token parsing!
                  multipleTokenParsing = false;
                  return true;
                }
                // Assign object number!
                int objectNumber = (int)token;
                // Backup the recovery position!
                long oldOffset = stream.Position;

                // 2. Generation number.
                // Try the possible generation number!
                MoveNext();
                // Isn't it a valid generation number?
                if(tokenType != TokenTypeEnum.Integer)
                {
                  // Rollback!
                  stream.Seek(oldOffset);
                  token = objectNumber; tokenType = TokenTypeEnum.Integer;
                  // Disable multiple token parsing!
                  multipleTokenParsing = false;
                  return true;
                }
                // Assign generation number!
                int generationNumber = (int)token;

                // 3. Reference keyword.
                // Try the possible reference keyword!
                MoveNext();
                // Isn't it a valid reference keyword?
                if(tokenType != TokenTypeEnum.Reference)
                {
                  // Rollback!
                  stream.Seek(oldOffset);
                  token = objectNumber; tokenType = TokenTypeEnum.Integer;
                  // Disable multiple token parsing!
                  multipleTokenParsing = false;
                  return true;
                }
                token = new Reference(objectNumber,generationNumber);
                // Disable multiple token parsing!
                multipleTokenParsing = false;
                return true;
              }
              break;
          }

          // Building the number...
          buffer = new StringBuilder();
          do
          {
            buffer.Append((char)c);
            c = stream.ReadByte();
            if(c == -1)
              throw new FileFormatException("Unexpected EOF (malformed number object).",stream.Position);
            if(c == '.')
              tokenType = TokenTypeEnum.Real;
            else if(c < '0' || c > '9')
              break;
          } while(true);

          stream.Skip(-1); // Recover the first byte after the current token.
          break;
        case Symbol.OpenSquareBracket: // Array (begin).
          tokenType = TokenTypeEnum.ArrayBegin;
          break;
        case Symbol.CloseSquareBracket: // Array (end).
          tokenType = TokenTypeEnum.ArrayEnd;
          break;
        case Symbol.OpenAngleBracket: // Dictionary (begin) | Hexadecimal string.
          c = stream.ReadByte();
          if(c == -1)
            throw new FileFormatException("Unexpected EOF (isolated opening angle-bracket character).",stream.Position);
          // Is it a dictionary (2nd angle bracket [PDF:1.6:3.2.6])?
          if(c == Symbol.OpenAngleBracket)
          {
            tokenType = TokenTypeEnum.DictionaryBegin;
            break;
          }

          // Hexadecimal string (single angle bracket [PDF:1.6:3.2.3]).
          tokenType = TokenTypeEnum.Hex;

          // [FIX:0.0.4:4] It skipped after the first hexadecimal character, missing it.
          buffer = new StringBuilder();
          while(c != Symbol.CloseAngleBracket) // NOT string end.
          {
            buffer.Append((char)c);

            c = stream.ReadByte();
            if(c == -1)
              throw new FileFormatException("Unexpected EOF (malformed hex string).",stream.Position);
          }
          break;
        case Symbol.CloseAngleBracket: // Dictionary (end).
          c = stream.ReadByte();
          if(c != Symbol.CloseAngleBracket)
            throw new FileFormatException("Malformed dictionary.",stream.Position);

          tokenType = TokenTypeEnum.DictionaryEnd;
          break;
        case Symbol.OpenRoundBracket: // Literal string.
          tokenType = TokenTypeEnum.Literal;

          buffer = new StringBuilder();
          int level = 0;
          while(true)
          {
            c = stream.ReadByte();
            if(c == -1)
              break;
            if(c == Symbol.OpenRoundBracket)
              level++;
            else if(c == Symbol.CloseRoundBracket)
              level--;
            else if(c == '\\')
            {
              bool lineBreak = false;
              c = stream.ReadByte();
              switch(c)
              {
                case 'n':
                  c = Symbol.LineFeed;
                  break;
                case 'r':
                  c = Symbol.CarriageReturn;
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
                case Symbol.OpenRoundBracket:
                case Symbol.CloseRoundBracket:
                case '\\':
                  break;
                case Symbol.CarriageReturn:
                  lineBreak = true;
                  c = stream.ReadByte();
                  if(c != Symbol.LineFeed)
                    stream.Skip(-1);
                  break;
                case Symbol.LineFeed:
                  lineBreak = true;
                  break;
                default:
                {
                  // Is it outside the octal encoding?
                  if(c < '0' || c > '7')
                    break;

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
            else if(c == Symbol.CarriageReturn)
            {
              c = stream.ReadByte();
              if(c == -1)
                break;
              if(c != Symbol.LineFeed)
              {c = Symbol.LineFeed; stream.Skip(-1);}
            }
            if(level == -1)
              break;

            buffer.Append((char)c);
          }
          if(c == -1)
            throw new FileFormatException("Malformed literal string.",stream.Position);

          break;
        case Symbol.CapitalR: // Indirect reference.
          tokenType = TokenTypeEnum.Reference;
          break;
        case Symbol.Percent: // Comment [PDF:1.6:3.1.2].
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
        default: // Keyword object.
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
          Current token initialization.
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
            if(((string)token).StartsWith(Keyword.DatePrefix)) // Date.
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
      <summary>Parses the current PDF object [PDF:1.6:3.2].</summary>
    */
    public PdfDataObject ParsePdfObject(
      )
    {
      /*
        NOTE: Object parsing is intrinsically a sequential operation tied to the stream pointer.
        Calls bound towards other classes are potentially disruptive for the predictability of
        the position of the stream pointer, so we are forced to carefully keep track of our
        current position in order to recover its proper state after any outbound call.
      */
      do
      {
        // Which token type?
        switch(tokenType)
        {
          case TokenTypeEnum.Integer:
            return new PdfInteger((int)token);
          case TokenTypeEnum.Name:
            return new PdfName((string)token,true);
          case TokenTypeEnum.Reference:
            /*
              NOTE: Curiously, PDF references are the only primitive objects that require
              a file reference. That's because they deal with indirect objects, which are strongly
              coupled with the current state of the file: so, PDF references are the fundamental
              bridge between the token layer and the file layer.
          */
            return new PdfReference(
              (Reference)token,
              file
              );
          case TokenTypeEnum.Literal:
            return new PdfTextString(
              Encoding.Encode((string)token)
              );
          case TokenTypeEnum.DictionaryBegin:
            PdfDictionary dictionary = new PdfDictionary();
            while(true)
            {
              // Key.
              MoveNext(); if(tokenType == TokenTypeEnum.DictionaryEnd) break;
              PdfName key = (PdfName)ParsePdfObject();
              // Value.
              MoveNext();
              PdfDirectObject value = (PdfDirectObject)ParsePdfObject();
              // Add the current entry to the dictionary!
              dictionary[key] = value;
            }

            int oldOffset = (int)stream.Position;
            MoveNext();
            // Is this dictionary the header of a stream object [PDF:1.6:3.2.7]?
            if((tokenType == TokenTypeEnum.Keyword)
              && token.Equals(Keyword.BeginStream))
            {
              // Keep track of current position!
              long position = stream.Position;

              // Get the stream length!
              /*
                NOTE: Indirect reference resolution is an outbound call (stream pointer hazard!),
                so we need to recover our current position after it returns.
              */
              int length = ((PdfInteger)files.File.Resolve(dictionary[PdfName.Length])).RawValue;

              // Move to the stream data beginning!
              stream.Seek(position); SkipEOL();

              // Copy the stream data to the instance!
              byte[] data = new byte[length];
              stream.Read(data);

              MoveNext(); // Postcondition (last token should be 'endstream' keyword).

            Object streamType = dictionary[PdfName.Type];
            if(PdfName.ObjStm.Equals(streamType)) // Object stream [PDF:1.6:3.4.6].
              return new ObjectStream(
                dictionary,
                new bytes.Buffer(data),
                file
                );
            else if(PdfName.XRef.Equals(streamType)) // Cross-reference stream [PDF:1.6:3.4.7].
              return new XRefStream(
                dictionary,
                new bytes.Buffer(data),
                file
                );
            else // Generic stream.
              return new PdfStream(
                dictionary,
                new bytes.Buffer(data)
                );
            }
            else // Stand-alone dictionary.
            {
              stream.Seek(oldOffset); // Restores postcondition (last token should be the dictionary end).

              return dictionary;
            }
          case TokenTypeEnum.ArrayBegin:
            PdfArray array = new PdfArray();
            while(true)
            {
              // Value.
              MoveNext(); if(tokenType == TokenTypeEnum.ArrayEnd) break;
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
            return new PdfTextString(
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

    /**
      <summary>Parses a PDF object after moving to the given token offset.</summary>
      <param name="offset">Number of tokens to skip before reaching the intended one.</param>
      <seealso cref="ParsePdfObject()"/>
    */
    public PdfDataObject ParsePdfObject(
      int offset
      )
    {MoveNext(offset); return ParsePdfObject();}

    /**
      <summary>Retrieves the PDF version of the file [PDF:1.6:3.4.1].</summary>
    */
    public string RetrieveVersion(
      )
    {
      stream.Seek(0);
      string header = stream.ReadString(10);
      if(!header.StartsWith(Keyword.BOF))
        throw new FileFormatException("PDF header not found.",stream.Position);

      return header.Substring(Keyword.BOF.Length,3);
    }

    /**
      <summary>Retrieves the starting position of the last xref-table section.</summary>
    */
    public long RetrieveXRefOffset(
      )
    {return RetrieveXRefOffset(stream.Length);}

    /**
      <summary>Retrieves the starting position of an xref-table section [PDF:1.6:3.4.4].</summary>
      <param name="offset">Position of the EOF marker related to the section intended to be parsed.</param>
    */
    public long RetrieveXRefOffset(
      long offset
      )
    {
      const int chunkSize = 1024; // [PDF:1.6:H.3.18].

      // Move back before 'startxref' keyword!
      long position = offset - chunkSize;
      if (position < 0)
      {position = 0;} // [FIX:0.0.4:1] It failed to deal with less-than-1024-byte-long PDF files.
      stream.Seek(position);

      // Get 'startxref' keyword position!
      int index = stream.ReadString(chunkSize).LastIndexOf(Keyword.StartXRef);
      if(index < 0)
        throw new FileFormatException("'" + Keyword.StartXRef + "' keyword not found.", stream.Position);

      // Go past the startxref keyword!
      stream.Seek(position + index); MoveNext();

      // Go to the xref offset!
      MoveNext();
      if(tokenType != TokenTypeEnum.Integer)
        throw new FileFormatException("'" + Keyword.StartXRef + "' value invalid.", stream.Position);

      return (int)token;
    }

    public long Position
    {
      get
      {return stream.Position;}
    }

    /**
      <summary>Moves the pointer to the given absolute byte position.</summary>
    */
    public void Seek(
      long offset
      )
    {stream.Seek(offset);}

    /**
      <summary>Moves the pointer to the given relative byte position.</summary>
    */
    public void Skip(
      long offset
      )
    {stream.Skip(offset);}

    /**
      <summary>Moves the pointer before the next non-EOL character after the current position.</summary>
      <returns>Whether the stream can be further read.</returns>
    */
    public bool SkipEOL(
      )
    {
      int c;
      do
      {
        c = stream.ReadByte();
        if(c == -1)
          return false;
      } while(IsEOL(c)); // Keeps going till there's an EOL character.
      stream.Skip(-1); // Moves back to the first non-EOL character position.
      return true;
    }

    /**
      <summary>Moves the pointer before the next non-whitespace character after the current position.</summary>
      <returns>Whether the stream can be further read.</returns>
    */
    public bool SkipWhitespace(
      )
    {
      int c;
      do
      {
        c = stream.ReadByte();
        if(c == -1)
          return false;
      } while(IsWhitespace(c)); // Keeps going till there's a whitespace character.
      stream.Skip(-1); // Moves back to the first non-whitespace character position.
      return true;
    }

    public IInputStream Stream
    {
      get
      {return stream;}
    }

    /**
      <summary>Gets the currently-parsed token.</summary>
    */
    public object Token
    {
      get
      {return token;}
    }

    /**
      <summary>Gets the currently-parsed token type.</summary>
    */
    public TokenTypeEnum TokenType
    {
      get
      {return tokenType;}
    }

    #region IDisposable
    public void Dispose(
      )
    {
      if(stream != null)
      {
        stream.Dispose();
        stream = null;
      }

      GC.SuppressFinalize(this);
    }
    #endregion
    #endregion
    #endregion
    #endregion
  }
}