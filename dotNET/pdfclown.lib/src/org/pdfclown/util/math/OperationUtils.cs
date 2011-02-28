/*
  Copyright 2009-2010 Stefano Chizzolini. http://www.pdfclown.org

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

using System;

namespace org.pdfclown.util
{
  /**
    <summary>Specialized math operations.</summary>
  */
  public sealed class OperationUtils
  {
    /**
      <summary>Big-endian comparison.</summary>
    */
    public static int Compare(
      byte[] data1,
      byte[] data2
      )
    {
      for(
        int index = 0,
          length = data1.Length;
        index < length;
        index++
        )
      {
        switch((int)Math.Sign((data1[index] & 0xff)-(data2[index] & 0xff)))
        {
          case -1:
            return -1;
          case 1:
            return 1;
        }
      }
      return 0;
    }

    /**
      <summary>Big-endian increment.</summary>
    */
    public static void Increment(
      byte[] data
      )
    {Increment(data, data.Length-1);}

    /**
      <summary>Big-endian increment.</summary>
    */
    public static void Increment(
      byte[] data,
      int position
      )
    {
      if((data[position] & 0xff) == 255)
      {
        data[position] = 0;
        Increment(data, position-1);
      }
      else
      {data[position]++;}
    }
  }
}