/*
  Copyright 2010 Stefano Chizzolini. http://www.pdfclown.org

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

namespace org.pdfclown.util.math
{
  /**
    <summary>An interval of comparable objects.</summary>
  */
  public sealed class Interval<T>
    where T : IComparable<T>
  {
    /**
      <summary>Containment mode.</summary>
    */
    public enum ContainModeEnum
    {
      Inclusive,
      Exclusive
    }

    private ContainModeEnum containMode;
    private T low = default(T);
    private T high = default(T);

    public Interval(
      T low,
      T high
      ) : this(low, high, ContainModeEnum.Inclusive)
    {}

    public Interval(
      T low,
      T high,
      ContainModeEnum containMode
      )
    {
      this.low = low;
      this.high = high;
      this.containMode = containMode;
    }

    /**
      <summary>Gets the way a value must be compared for containment against this interval's endpoints.</summary>
    */
    public ContainModeEnum ContainMode
    {
      get
      {return containMode;}
      set
      {containMode = value;}
    }

    /**
      <summary>Gets whether the specified value is contained within this interval.</summary>
      <param name="value">Value to check for containment.</param>
    */
    public bool Contains(
      T value
      )
    {
      int lowCompare = low.CompareTo(value);
      int highCompare = high.CompareTo(value);
      return (lowCompare < 0
          || (lowCompare == 0 && containMode == ContainModeEnum.Inclusive))
        && (highCompare > 0
          || (highCompare == 0 && containMode == ContainModeEnum.Inclusive));
    }

    /**
      <summary>Gets the higher interval endpoint.</summary>
    */
    public T High
    {
      get
      {return high;}
      set
      {high = value;}
    }

    /**
      <summary>Gets the lower interval endpoint.</summary>
    */
    public T Low
    {
      get
      {return low;}
      set
      {low = value;}
    }
  }
}