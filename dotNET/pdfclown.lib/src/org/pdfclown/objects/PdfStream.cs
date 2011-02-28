/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

using org.pdfclown.bytes;
using org.pdfclown.bytes.filters;
using org.pdfclown.files;
using org.pdfclown.tokens;

using System;
using System.Collections;
using System.Collections.Generic;

namespace org.pdfclown.objects
{
  /**
    <summary>PDF stream object [PDF:1.6:3.2.7].</summary>
  */
  public class PdfStream
    : PdfDataObject
  {
    #region static
    #region fields
    private static readonly byte[] BeginStreamBodyChunk = tokens.Encoding.Encode(Symbol.LineFeed + Keyword.BeginStream + Symbol.LineFeed);
    private static readonly byte[] EndStreamBodyChunk = tokens.Encoding.Encode(Symbol.LineFeed + Keyword.EndStream);
    #endregion
    #endregion

    #region dynamic
    #region fields
    private IBuffer body;
    private PdfDictionary header;
    #endregion

    #region constructors
    public PdfStream(
      ) : this(
        new PdfDictionary(),
        new bytes.Buffer()
        )
    {}

    public PdfStream(
      PdfDictionary header
      ) : this(
        header,
        new bytes.Buffer()
        )
    {}

    public PdfStream(
      IBuffer body
      ) : this(
        new PdfDictionary(),
        body
        )
    {}

    public PdfStream(
      PdfDictionary header,
      IBuffer body
      )
    {
      this.header = header;
      this.body = body;
    }
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets the decoded stream body.</summary>
    */
    public IBuffer Body
    {
      get
      {
        /*
          NOTE: Encoding filters are removed by default because they belong to a lower layer
          (token layer), so that it's appropriate and consistent to transparently keep
          the object layer unaware of such a facility.
        */
        return GetBody(true);
      }
    }

    public override object Clone(
      File context
      )
    {
      PdfStream clone = (PdfStream)MemberwiseClone();
      {
        // Deep cloning...
        clone.header = (PdfDictionary)header.Clone(context);
        clone.body = (IBuffer)body.Clone();
      }
      return clone;
    }

    /**
      <summary>Gets the stream body.</summary>
      <param name="decode">Defines whether the body has to be decoded.</param>
    */
    public IBuffer GetBody(
      bool decode
      )
    {
      if(decode)
      {
        // Get 'Filter' entry!
        /*
          NOTE: It defines possible encodings applied to the stream.
        */
        PdfDirectObject filterObj = header[PdfName.Filter];
        if(filterObj != null) // Stream encoded.
        {
          /*
            NOTE: If the stream is encoded, we must decode it before continuing.
          */
          PdfDataObject filterDataObj = files.File.Resolve(filterObj);
          PdfDataObject decodeParms = header.Resolve(PdfName.DecodeParms);
          if(filterDataObj is PdfName) // PdfName.
          {
            PdfDictionary filterDecodeParms = (PdfDictionary)decodeParms;
            body.Decode(Filter.Get((PdfName)filterDataObj), filterDecodeParms);
          }
          else // MUST be PdfArray.
          {
            IEnumerator<PdfDirectObject> filterObjIterator = ((PdfArray)filterDataObj).GetEnumerator();
            IEnumerator<PdfDirectObject> decodeParmsIterator = (decodeParms != null ? ((PdfArray)decodeParms).GetEnumerator() : null);
            while(filterObjIterator.MoveNext())
            {
              PdfDictionary filterDecodeParms;
              if(decodeParmsIterator == null)
              {filterDecodeParms = null;}
              else
              {
                decodeParmsIterator.MoveNext();
                filterDecodeParms = (PdfDictionary)files.File.Resolve(decodeParmsIterator.Current);
              }
              body.Decode(Filter.Get((PdfName)files.File.Resolve(filterObjIterator.Current)), filterDecodeParms);
            }
          }
          // Update 'Filter' entry!
          header[PdfName.Filter] = null; // The stream is free from encodings.
        }
      }
      return body;
    }

    /**
      <summary>Gets the stream header.</summary>
    */
    public PdfDictionary Header
    {
      get
      {return header;}
    }

    public override void WriteTo(
      IOutputStream stream
      )
    {
      bool unencodedBody;
      byte[] bodyData;
      int bodyLength;

      // 1. Header.
      // Encoding.
      /*
        NOTE: As the contract establishes that a stream instance should be kept
        free from encodings in order to be editable, encoding is NOT applied to
        the actual online stream, but to its serialized representation only.
        That is, as encoding is just a serialization practise, it is excluded from
        alive, instanced streams.
      */
      PdfDirectObject filterObj = header[PdfName.Filter];
      if(filterObj == null) // Unencoded body.
      {
        /*
          NOTE: As online representation is unencoded,
          header entries related to the encoded stream body are temporary
          (instrumental to the current serialization process).
        */
        unencodedBody = true;

        // Set the filter to apply!
        filterObj = PdfName.FlateDecode; // zlib/deflate filter.
        // Get encoded body data applying the filter to the stream!
        bodyData = body.Encode(Filter.Get((PdfName)filterObj), null);
        // Set encoded length!
        bodyLength = bodyData.Length;
        // Update 'Filter' entry!
        header[PdfName.Filter] = filterObj;
      }
      else // Encoded body.
      {
        unencodedBody = false;

        // Get encoded body data!
        bodyData = body.ToByteArray();
        // Set encoded length!
        bodyLength = (int)body.Length;
      }
      // Set encoded length!
      header[PdfName.Length] = new PdfInteger(bodyLength);

      header.WriteTo(stream);

      // Is the body free from encodings?
      if(unencodedBody)
      {
        // Restore actual header entries!
        ((PdfInteger)header[PdfName.Length]).Value = (int)body.Length;
        header[PdfName.Filter] = null;
      }

      // 2. Body.
      stream.Write(BeginStreamBodyChunk);
      stream.Write(bodyData);
      stream.Write(EndStreamBodyChunk);
    }
    #endregion
    #endregion
    #endregion
  }
}