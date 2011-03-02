/*
  Copyright 2006-2010 Stefano Chizzolini. http://www.pdfclown.org

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

package org.pdfclown.objects;

import java.beans.BeanInfo;
import java.beans.IntrospectionException;
import java.beans.Introspector;
import java.beans.PropertyDescriptor;
import java.lang.reflect.AnnotatedElement;
import java.lang.reflect.Member;
import java.util.Collection;

import org.pdfclown.PDF;
import org.pdfclown.Version;
import org.pdfclown.VersionEnum;
import org.pdfclown.documents.Document;
import org.pdfclown.documents.Document.Configuration.CompatibilityModeEnum;
import org.pdfclown.files.File;
import org.pdfclown.util.NotImplementedException;

/**
  High-level representation of a PDF object.
  <h3>Remarks</h3>
  <p>Specialized objects don't inherit directly from their low-level counterparts
  (e.g. {@link org.pdfclown.documents.contents.Contents Contents} extends {@link org.pdfclown.objects.PdfStream PdfStream},
  {@link org.pdfclown.documents.Pages Pages} extends {@link org.pdfclown.objects.PdfArray PdfArray}
  and so on) because there's no plain one-to-one mapping between primitive PDF types and specialized instances:
  the <code>Content</code> entry of <code>Page</code> dictionaries may be a simple reference to a <code>PdfStream</code>
  or a <code>PdfArray</code> of references to <code>PdfStream</code>-s, <code>Pages</code> collections may be spread
  across a B-tree instead of a flat <code>PdfArray</code> and so on.</p>
  <p>So, <i>in order to hide all these annoying inner workings, I chose to adopt a composition pattern instead of
  the apparently-reasonable (but actually awkward!) inheritance pattern</i>.
  Nonetheless, users can navigate through the low-level structure accessing the {@link #getBaseDataObject()} method.</p>

  @author Stefano Chizzolini (http://www.stefanochizzolini.it)
  @version 0.1.0
*/
public abstract class PdfObjectWrapper<TDataObject extends PdfDataObject>
{
  // <class>
  // <dynamic>
  // <fields>
  private TDataObject baseDataObject;
  private PdfDirectObject baseObject;
  private PdfIndirectObject container;
  // </fields>

  // <constructors>
  protected PdfObjectWrapper(
    File context,
    TDataObject baseDataObject
    )
  {
    this(
      context.register(baseDataObject),
      null
      );
  }

  /**
    @param baseObject Base PDF object. MUST be a {@link PdfReference PdfReference}
    everytime available.
    @param container Indirect object containing the base object.
  */
  protected PdfObjectWrapper(
    PdfDirectObject baseObject,
    PdfIndirectObject container
    )
  {
    setBaseObject(baseObject);
    setContainer(container);
  }
  // </constructors>

  // <interface>
  // <public>
  /**
    Gets a clone of the object, registered inside the given document context.

    @param context Which document the clone has to be registered in.
  */
  public abstract Object clone(
    Document context
    );

  /**
    Removes the object from its document context.
    <h3>Remarks</h3>
    <p>The object is no more usable after this method returns.</p>

    @return Whether the object was actually decontextualized (only indirect objects can be
    decontextualized).
  */
  public boolean delete(
    )
  {
    // Is the object indirect?
    if(baseObject instanceof PdfReference) // Indirect object.
    {
      ((PdfReference)baseObject).delete();
      return true;
    }
    else // Direct object.
    {return false;}
  }

  /**
    Gets the underlying data object.
  */
  public TDataObject getBaseDataObject(
    )
  {return baseDataObject;}

  /**
    Gets the underlying reference object, if available;
    otherwise, behaves like {@link #getBaseDataObject() getBaseDataObject()}.
  */
  public PdfDirectObject getBaseObject(
    )
  {return baseObject;}

  /**
    Gets the indirect object containing the base object.
    <h3>Remarks</h3>
    <p>It's used for update purposes.</p>
  */
  public PdfIndirectObject getContainer(
    )
  {return container;}

  /**
    Gets the document context.
  */
  public Document getDocument(
    )
  {return container.getFile().getDocument();}

  /**
    Gets the file context.
  */
  public File getFile(
    )
  {return container.getFile();}

  /**
    Manually update the underlying indirect object.
  */
  public void update(
    )
  {container.update();}
  // </public>

  // <protected>
//TODO:remove?
//  /**
//    Checks whether the caller context is compatible with the {@link Document#getVersion() document's conformance version}.
//    <p>For performance reasons, the relevant feature should be passed as argument to {@link #checkCompatibility(Object)} whenever possible.</p>
//
//    @throws RuntimeException In case of version conflict (see {@link org.pdfclown.documents.Document.Configuration.CompatibilityModeEnum#Strict Strict compatibility mode}).
//    @see #checkCompatibility(Object)
//    @since 0.1.0
//  */
//  protected void checkCompatibility(
//    )
//  {checkCompatibility(null);}

  /**
    Checks whether the specified feature is compatible with the {@link Document#getVersion() document's conformance version}.

    @param feature Entity whose compatibility has to be checked. Supported types:
      <ul>
        <li>{@link VersionEnum}</li>
        <li>{@link String Property name} resolvable to an {@link AnnotatedElement annotated getter method}</li>
        <li>{@link AnnotatedElement}</li>
      </ul>
      In case of <code>null</code>, this method tries to retrieve the caller method's compatibility version.
    @throws RuntimeException In case of version conflict (see {@link org.pdfclown.documents.Document.Configuration.CompatibilityModeEnum#Strict Strict compatibility mode}).
    @since 0.1.0
  */
  protected void checkCompatibility(
    Object feature
    )
  {
    /*
      TODO: Caching!
    */
    CompatibilityModeEnum compatibilityMode = getDocument().getConfiguration().getCompatibilityMode();
    if(compatibilityMode == CompatibilityModeEnum.Passthrough) // No check required.
      return;

    if(feature instanceof Collection<?>)
    {
      for(Object featureItem : (Collection<?>)feature)
      {checkCompatibility(featureItem);}
      return;
    }

    Version featureVersion;
    if(feature instanceof VersionEnum) // Explicit version.
    {featureVersion = ((VersionEnum)feature).getVersion();}
    else // Implicit version (element annotation).
    {
      PDF annotation;
      {
        if(feature instanceof String) // Property name.
        {
          BeanInfo classInfo;
          try
          {classInfo = Introspector.getBeanInfo(getClass());}
          catch(IntrospectionException e)
          {throw new RuntimeException(e);}
          for(PropertyDescriptor property : classInfo.getPropertyDescriptors())
          {
            if(feature.equals(property.getName()))
            {
              feature = property.getReadMethod();
              break;
            }
          }
        }
        else if(feature instanceof Enum<?>) // Enum constant.
        {
          try
          {feature = feature.getClass().getField(((Enum<?>)feature).name());}
          catch(Exception e)
          {throw new RuntimeException(e);}
        }
//TODO:remove?
//        else if(feature == null) // Implicit feature.
//        {
//          try
//          {
//            /*
//              NOTE: I know this is a somewhat weird (considering both OO design and performance) technique,
//              but at the moment I couldn't figure out a better solution for dynamically retrieving
//              the caller context's annotations in case of no arguments.
//             */
//            StackTraceElement callerStackElement = (StackTraceElement)Thread.currentThread().getStackTrace()[2];
//            Class<?> callerClass = Class.forName(callerStackElement.getClassName());
//            String callerMethodName = callerStackElement.getMethodName();
//            for(Method method : callerClass.getMethods())
//            {
//              if(method.getName().equals(callerMethodName))
//              {
//                feature = method; // NOTE: I assume that any overload of the same method conforms to the same version.
//                break;
//              }
//            }
//          }
//          catch(Exception e)
//          {throw new RuntimeException(e);}
//        }
        if(!(feature instanceof AnnotatedElement))
          throw new IllegalArgumentException("Feature type '" + feature.getClass().getName() + "' not supported.");

        while(true)
        {
           annotation = ((AnnotatedElement)feature).getAnnotation(PDF.class);
           if(annotation != null)
             break;

           if(feature instanceof Member)
           {feature = ((Member)feature).getDeclaringClass();}
           else if(feature instanceof Class<?>)
           {
             Class<?> containerClass = ((Class<?>)feature).getDeclaringClass();
             feature = (containerClass != null ? containerClass : ((Class<?>)feature).getPackage());
           }
           else // Element hierarchy walk complete.
             return; // NOTE: As no annotation is available, we assume the feature has no specific compatibility requirements.
        }
      }
      featureVersion = annotation.value().getVersion();
    }
    // Is the feature version compatible?
    if(getDocument().getVersion().compareTo(featureVersion) >= 0)
      return;

    // The feature version is NOT compatible: how to solve the conflict?
    switch(compatibilityMode)
    {
      case Loose: // Accepts the feature version.
        // Synchronize the document version!
        getDocument().setVersion(featureVersion);
        break;
      case Strict: // Refuses the feature version.
        // Throw a violation to the document version!
        throw new RuntimeException("Incompatible feature (version " + featureVersion + " was required against document version " + getDocument().getVersion());
      default:
        throw new NotImplementedException("Unhandled compatibility mode: " + compatibilityMode.name());
    }
  }

  @SuppressWarnings("unchecked")
  protected void setBaseObject(
    PdfDirectObject value
    )
  {
    baseObject = value;
    baseDataObject = (TDataObject)File.resolve(baseObject);
  }
  // </protected>

  // <internal>
  /**
    For internal use only.
  */
  public void setContainer(
    PdfIndirectObject value
    )
  {
    if(baseObject instanceof PdfReference) // Base object is indirect (self-contained).
    {container = ((PdfReference)baseObject).getIndirectObject();}
    else // Base object is direct (contained).
    {container = value;}
  }
  // </internal>
  // </interface>
  // </dynamic>
  // </class>
}