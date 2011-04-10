/*
  Copyright 2008-2011 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.documents.fileSpecs;

import java.util.ArrayList;
import java.util.Collection;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.objects.PdfTextString;
import org.pdfclown.util.NotImplementedException;

/**
  Embedded files referenced by another one (dependencies) [PDF:1.6:3.10.3].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 04/10/11
*/
@PDF(VersionEnum.PDF13)
public final class RelatedFiles
  extends PdfObjectWrapper<PdfArray>
  implements Map<String,EmbeddedFile>
{
  // <class>
  // <classes>
  private class Entry
    implements Map.Entry<String,EmbeddedFile>
  {
    // <class>
    // <dynamic>
    // <fields>
    private final String key;
    private final EmbeddedFile value;
    // </fields>

    // <constructors>
    private Entry(
      String key,
      EmbeddedFile value
      )
    {
      this.key = key;
      this.value = value;
    }
    // </constructors>

    // <interface>
    // <public>
    // <Map.Entry>
    @Override
    public String getKey(
      )
    {return key;}

    @Override
    public EmbeddedFile getValue(
      )
    {return value;}

    @Override
    public EmbeddedFile setValue(
      EmbeddedFile value
      )
    {throw new UnsupportedOperationException();}
    // </Map.Entry>
    // </public>
    // </interface>
    // </dynamic>
    // </class>
  }
  // </classes>

  // <dynamic>
  // <constructors>
  public RelatedFiles(
    Document context
    )
  {
    super(
      context.getFile(),
      new PdfArray()
      );
  }

  RelatedFiles(
    PdfDirectObject baseObject
    )
  {super(baseObject);}
  // </constructors>

  // <interface>
  // <public>
  @Override
  public RelatedFiles clone(
    Document context
    )
  {throw new NotImplementedException();}

  // <Map>
  @Override
  public void clear(
    )
  {getBaseDataObject().clear();}

  @Override
  public boolean containsKey(
    Object key
    )
  {
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 0,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      if(((PdfTextString)itemPairs.get(index)).getValue().equals(key))
        return true;
    }

    return false;
  }

  @Override
  public boolean containsValue(
    Object value
    )
  {
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 1,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      if(itemPairs.get(index).equals(value))
        return true;
    }

    return false;
  }

  @Override
  public Set<Map.Entry<String,EmbeddedFile>> entrySet(
    )
  {
    HashSet<Map.Entry<String,EmbeddedFile>> entrySet = new HashSet<Map.Entry<String,EmbeddedFile>>();
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 0,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      entrySet.add(
        new Entry(
          ((PdfTextString)itemPairs.get(index)).getValue(),
          new EmbeddedFile(itemPairs.get(index+1))
          )
        );
    }

    return entrySet;
  }

  @Override
  public boolean equals(
    Object object
    )
  {throw new NotImplementedException();}

  @Override
  public EmbeddedFile get(
    Object key
    )
  {
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 0,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      if(((PdfTextString)itemPairs.get(index)).getValue().equals(key))
        return new EmbeddedFile(itemPairs.get(index+1));
    }

    return null;
  }

  @Override
  public int hashCode(
    )
  {throw new NotImplementedException();}

  @Override
  public boolean isEmpty(
    )
  {return (getBaseDataObject().size() == 0);}

  @Override
  public Set<String> keySet(
    )
  {
    HashSet<String> keySet = new HashSet<String>();
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 0,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      keySet.add(
        ((PdfTextString)itemPairs.get(index)).getValue()
        );
    }

    return keySet;
  }

  @Override
  public EmbeddedFile put(
    String key,
    EmbeddedFile value
    )
  {
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 0,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      // Already existing entry?
      if(((PdfTextString)itemPairs.get(index)).getValue().equals(key))
      {
        EmbeddedFile oldEmbeddedFile = new EmbeddedFile(itemPairs.get(index+1));

        itemPairs.set(index+1,value.getBaseObject());

        return oldEmbeddedFile;
      }
    }

    // New entry.
    itemPairs.add(new PdfTextString(key));
    itemPairs.add(value.getBaseObject());

    return null;
  }

  @Override
  public void putAll(
    Map<? extends String,? extends EmbeddedFile> entries
    )
  {throw new NotImplementedException();}

  @Override
  public EmbeddedFile remove(
    Object key
    )
  {
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 0,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      if(((PdfTextString)itemPairs.get(index)).getValue().equals(key))
      {
        itemPairs.remove(index); // Key removed.

        return new EmbeddedFile(itemPairs.remove(index)); // Value removed.
      }
    }

    return null;
  }

  @Override
  public int size(
    )
  {return getBaseDataObject().size();}

  @Override
  public Collection<EmbeddedFile> values(
    )
  {
    List<EmbeddedFile> values = new ArrayList<EmbeddedFile>();
    PdfArray itemPairs = getBaseDataObject();
    for(
      int index = 1,
        length = itemPairs.size();
      index < length;
      index += 2
      )
    {
      values.add(
        new EmbeddedFile(itemPairs.get(index))
        );
    }

    return values;
  }
  // </Map>
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}