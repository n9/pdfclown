/*
  Copyright 2007-2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.contents;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.bytes.IBuffer;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.contents.objects.ContentObject;
import org.pdfclown.documents.contents.tokens.ContentParser;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDataObject;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfIndirectObject;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfReference;
import org.pdfclown.objects.PdfStream;
import org.pdfclown.util.NotImplementedException;

import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;

/**
  <b>Content stream</b> [PDF:1.6:3.7.1].
  <p>During its loading, this content stream is parsed and its instructions
  are exposed as a list; in case of modifications, it's user responsability
  to call the {@link #flush()} method in order to serialize back the instructions
  into this content stream.</p> 

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.4
  @version 0.1.1, 03/17/11
*/
@PDF(VersionEnum.PDF10)
public final class Contents
  extends PdfObjectWrapper<PdfDataObject>
  implements List<ContentObject>
{
  // <class>
  // <dynamic>
  // <fields>
  private List<ContentObject> items;

  private IContentContext contentContext;
  // </fields>

  // <constructors>
  /**
    For internal use only.
  */
  public Contents(
    PdfDirectObject baseObject,
    PdfIndirectObject container,
    IContentContext contentContext
    )
  {
    super(baseObject, container);
    this.contentContext = contentContext;
    load();
  }
  // </constructors>

  // <interface>
  // <public>
  @Override
  public Contents clone(
    Document context
    )
  {throw new NotImplementedException();}

  /**
    Serializes the contents into the content stream.
  */
  public void flush(
    )
  {
    PdfStream stream;
    PdfDataObject baseDataObject = getBaseDataObject();
    // Are contents just a single stream object?
    if(baseDataObject instanceof PdfStream) // Single stream.
    {stream = (PdfStream)baseDataObject;}
    else // Array of streams.
    {
      PdfArray streams = (PdfArray)baseDataObject;
      // No stream available?
      if(streams.size() == 0) // No stream.
      {
        // Add first stream!
        stream = new PdfStream();
        streams.add( // Inserts the new stream into the content stream.
          getFile().register(stream) // Inserts the new stream into the file.
          );
      }
      else // Streams exist.
      {
        // Eliminating exceeding streams...
        /*
          NOTE: Applications that consume or produce PDF files are not required to preserve
          the existing structure of the Contents array [PDF:1.6:3.6.2].
        */
        while(streams.size() > 1)
        {
          getFile().unregister( // Removes the exceeding stream from the file.
            (PdfReference)streams.remove(1) // Removes the exceeding stream from the content stream.
            );
        }

        PdfReference streamReference = (PdfReference)streams.get(0);
        File.update(streamReference); // Updates the existing stream into the file.
        stream = (PdfStream)streamReference.getDataObject();
      }
    }

    // Get the stream buffer!
    IBuffer buffer = stream.getBody();
    // Delete old contents from the stream buffer!
    buffer.setLength(0);
    // Serializing the new contents into the stream buffer...
    for(ContentObject item : items)
    {item.writeTo(buffer);}

    // Update the content stream container!
    update();
  }

  public IContentContext getContentContext(
    )
  {return contentContext;}

  // <List>
  @Override
  public void add(
    int index,
    ContentObject content
    )
  {items.add(index,content);}

  @Override
  public boolean addAll(
    int index,
    Collection<? extends ContentObject> contents
    )
  {return items.addAll(index,contents);}

  @Override
  public ContentObject get(
    int index
    )
  {return items.get(index);}

  @Override
  public int indexOf(
    Object content
    )
  {return items.indexOf(content);}

  @Override
  public int lastIndexOf(
    Object content
    )
  {return items.lastIndexOf(content);}

  @Override
  public ListIterator<ContentObject> listIterator(
    )
  {return items.listIterator();}

  @Override
  public ListIterator<ContentObject> listIterator(
    int index
    )
  {return items.listIterator(index);}

  @Override
  public ContentObject remove(
    int index
    )
  {return items.remove(index);}

  @Override
  public ContentObject set(
    int index,
    ContentObject content
    )
  {return items.set(index,content);}

  @Override
  public List<ContentObject> subList(
    int fromIndex,
    int toIndex
    )
  {return items.subList(fromIndex,toIndex);}

  // <Collection>
  @Override
  public boolean add(
    ContentObject content
    )
  {return items.add(content);}

  @Override
  public boolean addAll(
    Collection<? extends ContentObject> contents
    )
  {return items.addAll(contents);}

  @Override
  public void clear(
    )
  {items.clear();}

  @Override
  public boolean contains(
    Object content
    )
  {return items.contains(content);}

  @Override
  public boolean containsAll(
    Collection<?> contents
    )
  {return items.containsAll(contents);}

  @Override
  public boolean equals(
    Object object
    )
  {throw new NotImplementedException();}

  @Override
  public int hashCode(
    )
  {throw new NotImplementedException();}

  @Override
  public boolean isEmpty(
    )
  {return items.isEmpty();}

  @Override
  public boolean remove(
    Object content
    )
  {return items.remove(content);}

  @Override
  public boolean removeAll(
    Collection<?> contents
    )
  {return items.removeAll(contents);}

  @Override
  public boolean retainAll(
    Collection<?> contents
    )
  {return items.retainAll(contents);}

  @Override
  public int size(
    )
  {return items.size();}

  @Override
  public Object[] toArray(
    )
  {return items.toArray();}

  @Override
  public <T> T[] toArray(
    T[] contents
    )
  {return items.toArray(contents);}

  // <Iterable>
  @Override
  public Iterator<ContentObject> iterator(
    )
  {return items.iterator();}
  // </Iterable>
  // </Collection>
  // </List>
  // </public>

  // <private>
  private void load(
    )
  {
    final ContentParser parser = new ContentParser(getBaseDataObject());
    try
    {items = parser.parseContentObjects();}
    catch(Exception e)
    {throw new RuntimeException(e);}
  }
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}