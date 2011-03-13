/*
  Copyright 2009-2011 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.documents.contents.objects;
using org.pdfclown.objects;
using org.pdfclown.tokens;
using org.pdfclown.util;

using System;
using System.Collections.Generic;
using System.Globalization;
using io = System.IO;
using System.Text;

namespace org.pdfclown.documents.contents.fonts
{
  /**
    <summary>Content stream parser [PDF:1.6:3.7.1].</summary>
  */
  sealed class CMapParser
  {
/*
TODO:IMPL this parser evaluates a subset of the lexical domain of the token parser (clown.tokens.Parser): it should be better to derive both parsers from a common parsing engine in order to avoid unwieldy duplications.
*/
    #region static
    #region fields
    private static readonly string BeginBaseFontCharOperator = "beginbfchar";
    private static readonly string BeginBaseFontRangeOperator = "beginbfrange";
    private static readonly string BeginCIDCharOperator = "begincidchar";
    private static readonly string BeginCIDRangeOperator = "begincidrange";

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
    private bytes::IInputStream stream;
    private Object token;
    private TokenTypeEnum tokenType;
    #endregion

    #region constructors
    public CMapParser(
      io::Stream stream
      ) : this(new bytes::Buffer(stream))
    {}

    public CMapParser(
      bytes::IInputStream stream
      )
    {this.stream = stream;}
    #endregion

    #region interface
    #region public
    public long Length
    {get{return stream.Length;}}

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
      /*
        TODO: It'd be interesting to evaluate an alternative regular-expression-based
        implementation...
      */
      int c = 0;

      // Skip white-space characters [PDF:1.6:3.1.1].
      do
      {
        c = stream.ReadByte();
        if(c == -1)
        {return false;}
      } while(IsWhitespace(c)); // Keep goin' till there's a white-space character...

      StringBuilder buffer = null;
      token = null;
      // Which character is it?
      switch(c)
      {
        case '/': // Name.
          tokenType = TokenTypeEnum.Name;

          buffer = new StringBuilder();
          while(true)
          {
            c = stream.ReadByte();
            if(c == -1)
              throw new FileFormatException("Unexpected EOF (malformed name object).",stream.Position);
            if(IsDelimiter(c) || IsWhitespace(c))
              break;
            // Is it an hexadecimal code [PDF:1.6:3.2.4]?
            if(c == '#')
            {c = (GetHex(stream.ReadByte()) << 4) + GetHex(stream.ReadByte());}

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
              throw new FileFormatException("Unexpected EOF (malformed number object).",stream.Position);
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
        case '%': // Comment.
          tokenType = TokenTypeEnum.Comment;
          // Skipping comment content...
          do
          {
            c = stream.ReadByte();
            if(c == -1)
              break;
          } while(!IsEOL(c));
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
              case "false":
              case "true": // Boolean.
                tokenType = TokenTypeEnum.Boolean;
                token =  bool.Parse((string)token);
                break;
              case "null": // Null.
                tokenType = TokenTypeEnum.Null;
                token = null;
                break;
            }
            break;
          case TokenTypeEnum.Comment:
          case TokenTypeEnum.Name:
            token = buffer.ToString();
            break;
          case TokenTypeEnum.Literal:
            token = buffer.ToString();
            break;
          case TokenTypeEnum.Hex:
            token = ConvertUtils.HexToByteArray(buffer.ToString());
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
      <summary>Parses the character-code-to-unicode mapping [PDF:1.6:5.9.1].</summary>
    */
    public IDictionary<ByteArray,int> Parse(
      )
    {
      stream.Position = 0;
      IDictionary<ByteArray,int> codes = new Dictionary<ByteArray,int>();
      {
        int itemCount = 0;
        try
        {
          while(MoveNext())
          {
            switch(tokenType)
            {
              case TokenTypeEnum.Keyword:
              {
                string operator_ = (String)token;
                if(operator_.Equals(BeginBaseFontCharOperator)
                  || operator_.Equals(BeginCIDCharOperator))
                {
                  /*
                    NOTE: The first element on each line is the input code of the template font;
                    the second element is the code or name of the character.
                  */
                  for(
                    int itemIndex = 0;
                    itemIndex < itemCount;
                    itemIndex++
                    )
                  {
                    MoveNext();
                    ByteArray inputCode = new ByteArray((byte[])token);
                    MoveNext();
                    codes[inputCode] = ParseUnicode();
                  }
                }
                else if(operator_.Equals(BeginBaseFontRangeOperator)
                  || operator_.Equals(BeginCIDRangeOperator))
                {
                  /*
                    NOTE: The first and second elements in each line are the beginning and
                    ending valid input codes for the template font; the third element is
                    the beginning character code for the range.
                  */
                  for(
                    int itemIndex = 0;
                    itemIndex < itemCount;
                    itemIndex++
                    )
                  {
                    // 1. Beginning input code.
                    MoveNext();
                    byte[] beginInputCode = (byte[])token;
                    // 2. Ending input code.
                    MoveNext();
                    byte[] endInputCode = (byte[])token;
                    // 3. Character codes.
                    MoveNext();
                    switch(tokenType)
                    {
                      case TokenTypeEnum.ArrayBegin:
                      {
                        byte[] inputCode = beginInputCode;
                        while(MoveNext()
                          && tokenType != TokenTypeEnum.ArrayEnd)
                        {
                          codes[new ByteArray(inputCode)] = ParseUnicode();
                          OperationUtils.Increment(inputCode);
                        }
                        break;
                      }
                      default:
                      {
                        byte[] inputCode = beginInputCode;
                        int charCode = ParseUnicode();
                        int endCharCode = charCode + (ConvertUtils.ByteArrayToInt(endInputCode) - ConvertUtils.ByteArrayToInt(beginInputCode));
                        while(true)
                        {
                          codes[new ByteArray(inputCode)] = charCode;
                          if(charCode == endCharCode)
                            break;

                          OperationUtils.Increment(inputCode);
                          charCode++;
                        }
                        break;
                      }
                    }
                  }
                }
                break;
              }
              case TokenTypeEnum.Integer:
              {
                itemCount = (int)token;
                break;
              }
            }
          }
        }
        catch(FileFormatException fileFormatException)
        {throw new Exception("Failed character map parsing.", fileFormatException);}
      }
      return codes;
    }

    public long Position
    {get{return stream.Position;}}

    public void Seek(
      long position
      )
    {
      if(position < 0)
        throw new ArgumentException("Lower than acceptable.","position");
      if(position > stream.Length)
        throw new ArgumentException("Higher than acceptable.","position");

      stream.Seek(position);
    }

    public void Skip(
      long offset
      )
    {
      long position = stream.Position + offset;
      if(position < 0)
        throw new ArgumentException("Lower than acceptable.","offset");
      if(position > stream.Length)
        throw new ArgumentException("Higher than acceptable.","offset");

      stream.Seek(position);
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
      <summary>Gets the stream to be parsed.</summary>
    */
    public bytes::IInputStream Stream
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
    #endregion

    #region private
    /**
      <summary>Converts the current token into its Unicode value.</summary>
    */
    private int ParseUnicode(
      )
    {
      switch(tokenType)
      {
        case TokenTypeEnum.Hex: // Character code in hexadecimal format.
          return ConvertUtils.ByteArrayToInt((byte[])token);
        case TokenTypeEnum.Integer: // Character code in plain format.
          return (int)token;
        case TokenTypeEnum.Name: // Character name.
          return GlyphMapping.NameToCode((string)token);
        default:
          throw new Exception(
            "Hex string, integer or name expected instead of " + tokenType
            );
      }
    }
    #endregion
    #endregion
    #endregion
  }
}