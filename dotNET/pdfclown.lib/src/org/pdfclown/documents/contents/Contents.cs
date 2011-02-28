/*
  Copyright 2007-2010 Stefano Chizzolini. http://www.pdfclown.org

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
using org.pdfclown.documents.contents.tokens;
using org.pdfclown.files;
using org.pdfclown.objects;

using System;
using System.Collections;
using System.Collections.Generic;

namespace org.pdfclown.documents.contents
{
  /**
    <summary>Content stream [PDF:1.6:3.7.1].</summary>
    <remarks>During its loading, this content stream is parsed and its instructions
    are exposed as a list; in case of modifications, it's user responsability
    to call the <see cref="Flush()"/> method in order to serialize back the instructions
    into this content stream.</remarks>
  */
  [PDF(VersionEnum.PDF10)]
  public sealed class Contents
    : PdfObjectWrapper<PdfDataObject>,
      IList<ContentObject>
  {
    #region dynamic
    #region fields
    private List<ContentObject> items;

    private IContentContext contentContext;
    #endregion

    #region constructors
    internal Contents(
      PdfDirectObject baseObject,
      PdfIndirectObject container,
      IContentContext contentContext
      ) : base(baseObject, container)
    {
      this.contentContext = contentContext;
      Load();
    }
    #endregion

    #region interface
    #region public
    public override object Clone(
      Document context
      )
    {throw new NotImplementedException();}

    /**
      <summary>Serializes the contents into the content stream.</summary>
    */
    public void Flush(
      )
    {
      PdfStream stream;
      PdfDataObject baseDataObject = BaseDataObject;
      // Are contents just a single stream object?
      if(baseDataObject is PdfStream) // Single stream.
      {stream = (PdfStream)baseDataObject;}
      else // Array of streams.
      {
        PdfArray streams = (PdfArray)baseDataObject;
        // No stream available?
        if(streams.Count == 0) // No stream.
        {
          // Add first stream!
          stream = new PdfStream();
          streams.Add( // Inserts the new stream into the content stream.
            File.Register(stream) // Inserts the new stream into the file.
            );
        }
        else // Streams exist.
        {
          // Eliminating exceeding streams...
          /*
            NOTE: Applications that consume or produce PDF files are not required to preserve
            the existing structure of the Contents array [PDF:1.6:3.6.2].
          */
          while(streams.Count > 1)
          {
            File.Unregister((PdfReference)streams[1]); // Removes the exceeding stream from the file.
            streams.RemoveAt(1); // Removes the exceeding stream from the content stream.
          }

          PdfReference streamReference = (PdfReference)streams[0];
          File.Update(streamReference); // Updates the existing stream into the file.
          stream = (PdfStream)streamReference.DataObject;
        }
      }

      // Get the stream buffer!
      bytes::IBuffer buffer = stream.Body;
      // Delete old contents from the stream buffer!
      buffer.SetLength(0);
      // Serializing the new contents into the stream buffer...
      foreach(ContentObject item in items)
      {item.WriteTo(buffer);}

      // Update the content stream container!
      Update();
    }

    public IContentContext ContentContext
    {get{return contentContext;}}

    #region IList
    public int IndexOf(
      ContentObject obj
      )
    {return items.IndexOf(obj);}

    public void Insert(
      int index,
      ContentObject obj
      )
    {items.Insert(index,obj);}

    public void RemoveAt(
      int index
      )
    {items.RemoveAt(index);}

    public ContentObject this[
      int index
      ]
    {
      get{return items[index];}
      set{items[index] = value;}
    }

    #region ICollection
    public void Add(
      ContentObject obj
      )
    {items.Add(obj);}

    public void Clear(
      )
    {items.Clear();}

    public bool Contains(
      ContentObject obj
      )
    {return items.Contains(obj);}

    public void CopyTo(
      ContentObject[] objs,
      int index
      )
    {items.CopyTo(objs,index);}

    public int Count
    {get{return items.Count;}}

    public bool IsReadOnly
    {get{return false;}}

    public bool Remove(
      ContentObject obj
      )
    {return items.Remove(obj);}

    #region IEnumerable<ContentObject>
    public IEnumerator<ContentObject> GetEnumerator(
      )
    {return items.GetEnumerator();}

    #region IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {return ((IEnumerable<ContentObject>)this).GetEnumerator();}
    #endregion
    #endregion
    #endregion
    #endregion
    #endregion

    #region private
    private void Load(
      )
    {
      Parser parser = new Parser(BaseDataObject);
      try
      {items = parser.ParseContentObjects();}
      catch(Exception e)
      {throw e;}
    }
    #endregion
    #endregion
    #endregion
  }
}