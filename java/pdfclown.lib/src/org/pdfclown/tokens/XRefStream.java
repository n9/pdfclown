/*
  Copyright 2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.tokens;

import java.io.EOFException;
import java.nio.ByteOrder;
import java.util.Collection;
import java.util.Iterator;
import java.util.Map;
import java.util.Set;
import java.util.SortedMap;
import java.util.TreeMap;

import org.pdfclown.bytes.Buffer;
import org.pdfclown.bytes.IBuffer;
import org.pdfclown.bytes.IOutputStream;
import org.pdfclown.files.File;
import org.pdfclown.objects.PdfArray;
import org.pdfclown.objects.PdfDictionary;
import org.pdfclown.objects.PdfDirectObject;
import org.pdfclown.objects.PdfInteger;
import org.pdfclown.objects.PdfName;
import org.pdfclown.objects.PdfStream;
import org.pdfclown.util.ConvertUtils;

/**
	Cross-reference stream containing cross-reference information [PDF:1.6:3.4.7].
	<p>It is alternative to the classic cross-reference table.</p>
 
  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
*/
public final class XRefStream
	extends PdfStream
	implements Map<Integer,XRefEntry>
{
  // <class>
  // <static>
  // <fields>
	private static final int FreeEntryType = 0;
	private static final int InUseEntryType = 1;
	private static final int InUseCompressedEntryType = 2;

	private static final double ByteBaseLog = Math.log(256);
	
	private static final int EntryField0Size = 1; 
	private static final int EntryField2Size = getFieldSize(XRefEntry.GenerationUnreusable);
  // </fields>

  // <interface>
  // <private>
	/**
		Gets the number of bytes needed to store the specified value.
		
		@param maxValue Maximum storable value.
	*/
	private static int getFieldSize(
		int maxValue
		)
	{return (int)Math.ceil(Math.log(maxValue)/ByteBaseLog);}
	
	/**
		Converts the specified value into a customly-sized big-endian byte array.

		@param value Value to convert.
		@param length Byte array's length.
	 */
	private static byte[] numberToByteArray(
		int value,
		int length
		)
	{return ConvertUtils.numberToByteArray(value, length, ByteOrder.BIG_ENDIAN);}
  // </private>
  // </interface>
  // </static>

  // <dynamic>
  // <fields>
	private SortedMap<Integer,XRefEntry> entries;
	private File file;
  // </fields>

  // <constructors>
	public XRefStream(
		File file
		)
	{
		this(
			new PdfDictionary(
				new PdfName[]
          {PdfName.Type},
				new PdfDirectObject[]
          {PdfName.XRef}
				),
			new Buffer(),
			file
			);
		PdfDictionary header = getHeader();
		for(Entry<PdfName,PdfDirectObject> entry : file.getTrailer().entrySet())
		{
			PdfName key = entry.getKey();
			if(key.equals(PdfName.Root)
				|| key.equals(PdfName.Info)
				|| key.equals(PdfName.ID))
			{header.put(key,entry.getValue());}
		}
	}

	public XRefStream(
    PdfDictionary header,
    IBuffer body,
    File file
		)
	{
		super(header, body);
		
		this.file = file;
	}
  // </constructors>

  // <interface>
  // <public>
	/**
		Gets the byte offset from the beginning of the file
		to the beginning of the previous cross-reference stream.
		
		@return <code>-1</code> in case no linked stream exists.
	*/
	public int getLinkedStreamOffset(
		)
	{
		PdfInteger linkedStreamOffsetObject = (PdfInteger)getHeader().get(PdfName.Prev);
		return (linkedStreamOffsetObject != null ? linkedStreamOffsetObject.getValue() : -1);
	}

	@Override
	public void writeTo(
		IOutputStream stream
		)
	{
		if(entries != null)
		{flush(stream);}
		
		super.writeTo(stream);
	}

	// <Map>
	@Override
	public void clear(
		)
	{
		if(entries == null)
		{entries = new TreeMap<Integer,XRefEntry>();}
		else
		{entries.clear();}
	}

	@Override
	public boolean containsKey(
		Object key
		)
	{return getEntries().containsKey(key);}

	@Override
	public boolean containsValue(
		Object value
		)
	{return getEntries().containsValue(value);}

	@Override
	public Set<java.util.Map.Entry<Integer,XRefEntry>> entrySet(
		)
	{return getEntries().entrySet();}

	@Override
	public XRefEntry get(
		Object key
		)
	{return getEntries().get(key);}

	@Override
	public boolean isEmpty(
		)
	{return getEntries().isEmpty();}

	@Override
	public Set<Integer> keySet(
		)
	{return getEntries().keySet();}

	@Override
	public XRefEntry put(
		Integer key,
		XRefEntry value
		)
	{return getEntries().put(key,value);}

	@Override
	public void putAll(
		Map<? extends Integer,? extends XRefEntry> entries
		)
	{getEntries().putAll(entries);}

	@Override
	public XRefEntry remove(
		Object key
		)
	{return getEntries().remove(key);}

	@Override
	public int size(
		)
	{return getEntries().size();}

	@Override
	public Collection<XRefEntry> values(
		)
	{return getEntries().values();}
	// </Map>
  // </public>

  // <private>
	/**
		Serializes the xref stream entries into the stream body.
	*/
	private void flush(
		IOutputStream stream
		)
	{
		// 1. Body.
		final PdfArray indexArray = new PdfArray();
		final int[] entryFieldSizes = new int[]
	    {
				EntryField0Size,
				getFieldSize((int)stream.getLength()), // NOTE: We assume this xref stream is the last indirect object.
				EntryField2Size
			};
		{
	    // Get the stream buffer!
	    final IBuffer body = getBody();
	
	    // Delete the old entries!
	    body.setLength(0);
	
	    // Serializing the entries into the stream buffer...
	    int prevObjectNumber = -2; // Previous-entry object number.
			for(XRefEntry entry : entries.values())
			{
				int entryNumber = entry.getNumber();
	      if(entryNumber - prevObjectNumber != 1) // Current subsection terminated.
	      {
	      	if(!indexArray.isEmpty())
	      	{indexArray.add(new PdfInteger(prevObjectNumber-((PdfInteger)indexArray.get(indexArray.size()-1)).getValue()+1));} // Number of entries in the previous subsection.
	      	indexArray.add(new PdfInteger(entryNumber)); // First object number in the next subsection.
	      }
	      prevObjectNumber = entryNumber;
				
				switch(entry.getUsage())
				{
					case Free:
						body.append((byte)FreeEntryType);
						body.append(numberToByteArray(entry.getOffset(),entryFieldSizes[1]));
						body.append(numberToByteArray(entry.getGeneration(),entryFieldSizes[2]));
						break;
					case InUse:
						body.append((byte)InUseEntryType);
						body.append(numberToByteArray(entry.getOffset(),entryFieldSizes[1]));
						body.append(numberToByteArray(entry.getGeneration(),entryFieldSizes[2]));
						break;
					case InUseCompressed:
						body.append((byte)InUseCompressedEntryType);
						body.append(numberToByteArray(entry.getStreamNumber(),entryFieldSizes[1]));
						body.append(numberToByteArray(entry.getOffset(),entryFieldSizes[2]));
						break;
					default:
						throw new UnsupportedOperationException();
				}
			}
	  	indexArray.add(new PdfInteger(prevObjectNumber-((PdfInteger)indexArray.get(indexArray.size()-1)).getValue()+1)); // Number of entries in the previous subsection.
		}
		
		// 2. Header.
		{
			final PdfDictionary header = getHeader();
			header.put(
				PdfName.Index,
				indexArray
				);
			header.put(
				PdfName.Size,
				new PdfInteger(file.getIndirectObjects().size()+1)
				);
			header.put(
				PdfName.W,
				new PdfArray(
					new PdfInteger(entryFieldSizes[0]),
					new PdfInteger(entryFieldSizes[1]),
					new PdfInteger(entryFieldSizes[2])
					)
				);
		}
	}

	private SortedMap<Integer,XRefEntry> getEntries(
		)
	{
		if(entries == null)
		{
			entries = new TreeMap<Integer,XRefEntry>();

			final IBuffer body = getBody();
			if(body.getLength() > 0)
			{
				final PdfDictionary header = getHeader();
				final int size = ((PdfInteger)header.get(PdfName.Size)).getValue();
				final int[] entryFieldSizes;
				{
					final PdfArray entryFieldSizesObject = (PdfArray)header.get(PdfName.W);
					entryFieldSizes = new int[entryFieldSizesObject.size()];
					for(int index = 0, length = entryFieldSizes.length; index < length; index++)
					{entryFieldSizes[index] = ((PdfInteger)entryFieldSizesObject.get(index)).getValue();}
				}
				
				final PdfArray subsectionBounds;
				if(header.containsKey(PdfName.Index))
				{subsectionBounds = (PdfArray)header.get(PdfName.Index);}
				else
				{
					subsectionBounds = new PdfArray();
					subsectionBounds.add(new PdfInteger(0));
					subsectionBounds.add(new PdfInteger(size));
				}
				
				body.setByteOrder(ByteOrder.BIG_ENDIAN);
				body.seek(0);
					
				final Iterator<PdfDirectObject> subsectionBoundIterator = subsectionBounds.iterator();
				while(subsectionBoundIterator.hasNext())
				{
			    try
			    {
			    	final int start = ((PdfInteger)subsectionBoundIterator.next()).getValue();
			    	final int count = ((PdfInteger)subsectionBoundIterator.next()).getValue();
						for(
							int entryIndex = start,
								length = start + count;
							entryIndex < length;
							entryIndex++
							)
						{
							final int entryFieldType = (entryFieldSizes[0] == 0 ? 1 : body.readInt(entryFieldSizes[0]));
						  switch(entryFieldType)
						  {
							  case FreeEntryType:
							  {
							  	final int nextFreeObjectNumber = body.readInt(entryFieldSizes[1]);
							  	final int generation = body.readInt(entryFieldSizes[2]);
									entries.put(
										entryIndex,
										new XRefEntry(
											entryIndex,
											generation,
											nextFreeObjectNumber,
											XRefEntry.UsageEnum.Free
											)
										);
									break;
							  }
							  case InUseEntryType:
							  {
							  	final int offset = body.readInt(entryFieldSizes[1]);
							  	final int generation = body.readInt(entryFieldSizes[2]);
									entries.put(
										entryIndex,
										new XRefEntry(
											entryIndex,
											generation,
											offset,
											XRefEntry.UsageEnum.InUse
											)
										);
									break;
							  }
							  case InUseCompressedEntryType:
							  {
							  	final int streamNumber = body.readInt(entryFieldSizes[1]);
							  	final int innerNumber = body.readInt(entryFieldSizes[2]);
									entries.put(
										entryIndex,
										new XRefEntry(
											entryIndex,
											innerNumber,
											streamNumber
											)
										);
									break;
							  }
			          default:
			            throw new RuntimeException("Unknown xref entry type '" + entryFieldType + "'.");
						  }
						}
			    }
			    catch(EOFException e)
			    {throw new RuntimeException("Unexpected EOF (malformed cross-reference stream object).",e);}
				}
			}
		}
		return entries;
	}
  // </private>
  // </interface>
  // </dynamic>
  // </class>
}
