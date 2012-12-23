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

package org.pdfclown.documents;

import java.util.Collection;
import java.util.Iterator;
import java.util.List;
import java.util.ListIterator;
import java.util.NoSuchElementException;

import org.pdfclown.PDF;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.interaction.annotations.Annotation;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfObjectWrapper;
import org.pdfclown.util.NotImplementedException;

/**
  Page annotations [PDF:1.6:3.6.2].

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @since 0.0.7
  @version 0.1.1, 04/10/11
*/
@PDF(VersionEnum.PDF10)
public final class PageAnnotations
  extends PdfObjectWrapper<PdfArray>
  implements List<Annotation>
{
  // <class>
  // <dynamic>
  // <fields>
  private final Page page;
  // </fields>

  // <constructors>
  PageAnnotations(
    PdfDirectObject baseObject,
    Page page
    )
  {
    super(baseObject);

    this.page = page;
  }
  // </constructors>

  // <interface>
  // <public>
  @Override
  public PageAnnotations clone(
    Document context
    )
  {throw new NotImplementedException();} //TODO: deal with page reference.

  /**
    Gets the page associated to these annotations.
  */
  public Page getPage(
    )
  {return page;}

  // <List>
  @Override
  public void add(
    int index,
    Annotation value
    )
  {getBaseDataObject().add(index,value.getBaseObject());}

  @Override
  public boolean addAll(
    int index,
    Collection<? extends Annotation> values
    )
  {
    PdfArray items = getBaseDataObject();
    for(Annotation value : values)
    {items.add(index++,value.getBaseObject());}

    return true;
  }

  @Override
  public Annotation get(
    int index
    )
  {return Annotation.wrap(getBaseDataObject().get(index));}

  @Override
  public int indexOf(
    Object value
    )
  {
    if(!(value instanceof Annotation))
      return -1;

    return getBaseDataObject().indexOf(((Annotation)value).getBaseObject());
  }

  @Override
  public int lastIndexOf(
    Object value
    )
  {
    /*
      NOTE: Annotations are expected not to be duplicate.
    */
    return indexOf(value);
  }

  @Override
  public ListIterator<Annotation> listIterator(
    )
  {throw new NotImplementedException();}

  @Override
  public ListIterator<Annotation> listIterator(
    int index
    )
  {throw new NotImplementedException();}

  @Override
  public Annotation remove(
    int index
    )
  {
    PdfDirectObject annotationObject = getBaseDataObject().remove(index);
    return Annotation.wrap(annotationObject);
  }

  @Override
  public Annotation set(
    int index,
    Annotation value
    )
  {return Annotation.wrap(getBaseDataObject().set(index,value.getBaseObject()));}

  @Override
  public List<Annotation> subList(
    int fromIndex,
    int toIndex
    )
  {throw new NotImplementedException();}

  // <Collection>
  @Override
  public boolean add(
    Annotation value
    )
  {
    // Assign the annotation to the page!
    value.getBaseDataObject().put(PdfName.P,page.getBaseObject());

    return getBaseDataObject().add(value.getBaseObject());
  }

  @Override
  public boolean addAll(
    Collection<? extends Annotation> values
    )
  {
    for(Annotation value : values)
    {add(value);}

    return true;
  }

  @Override
  public void clear(
    )
  {getBaseDataObject().clear();}

  @Override
  public boolean contains(
    Object value
    )
  {
    if(!(value instanceof Annotation))
      return false;

    return getBaseDataObject().contains(((Annotation)value).getBaseObject());
  }

  @Override
  public boolean containsAll(
    Collection<?> values
    )
  {throw new NotImplementedException();}

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
  {return getBaseDataObject().isEmpty();}

  @Override
  public boolean remove(
    Object value
    )
  {return getBaseDataObject().remove(((Annotation)value).getBaseObject());}

  @Override
  public boolean removeAll(
    Collection<?> values
    )
  {throw new NotImplementedException();}

  @Override
  public boolean retainAll(
    Collection<?> values
    )
  {throw new NotImplementedException();}

  @Override
  public int size(
    )
  {return getBaseDataObject().size();}

  @Override
  public Object[] toArray(
    )
  {return toArray(new Annotation[0]);}

  @Override
  @SuppressWarnings("unchecked")
  public <T> T[] toArray(
    T[] values
    )
  {
    PdfArray annotationObjects = getBaseDataObject();
    if(values.length < annotationObjects.size())
    {values = (T[])new Object[annotationObjects.size()];}

    for(
      int index = 0,
        length = annotationObjects.size();
      index < length;
      index++
      )
    {values[index] = (T)Annotation.wrap(annotationObjects.get(index));}
    return values;
  }

  // <Iterable>
  @Override
  public Iterator<Annotation> iterator(
    )
  {
    return new Iterator<Annotation>()
    {
      /** Index of the next item. */
      private int index = 0;
      /** Collection size. */
      private final int size = size();

      @Override
      public boolean hasNext(
        )
      {return (index < size);}

      @Override
      public Annotation next(
        )
      {
        if(!hasNext())
          throw new NoSuchElementException();

        return get(index++);
      }

      @Override
      public void remove(
        )
      {throw new UnsupportedOperationException();}
    };
  }
  // </Iterable>
  // </Collection>
  // </List>
  // </public>
  // </interface>
  // </dynamic>
  // </class>
}