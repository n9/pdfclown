<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet
  exclude-result-prefixes="m db"
  version="1.0"
  xmlns="http://docbook.org/ns/docbook"
  xmlns:db="http://docbook.org/ns/docbook"
  xmlns:m="http://www.stefanochizzolini.it/ns/mentor"
  xmlns:xl="http://www.w3.org/1999/xlink"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  >
  <!--
This is the DocBook-5.0 filter for Mentor 0.1.
v0.1.4 (2007-12-01). Edited by Stefano Chizzolini (http://www.stefanochizzolini.it).

Changes:
* 0.1.4 (2007-12-01):
m:entries support to child id attribute.
* 0.1.3 (2007-09-08):
m:issueList structure simplified.
* 0.1.2 (2007-04-28):
m:issueList and m:todoList added.
* 0.1.1 (2007-04-18):
m:tip elements are processed via 'apply-templates' instead of 'value-of' instructions to allow rich nested markup.
  -->
  <xsl:output omit-xml-declaration="no" method="xml" version="1.0" indent="no" encoding="utf-8"/>

  <xsl:strip-space elements="*"/>

  <!-- Mentor file directory. -->
  <xsl:param name="dir"/>
  <!-- Mentor file title. -->
  <xsl:param name="fileTitle"/>
  <!-- Translation. -->
  <xsl:param name="lang" select="'en'"/>

  <!-- Standard resource file title. -->
  <xsl:variable name="resourceFileTitle" select="'README'"/>
  <!-- Mentor file extension. -->
  <xsl:variable name="srcFileExtension" select="'.mentor'"/>
  <!-- Hypertextual file extension. -->
  <xsl:variable name="targetFileExtension" select="'.html'"/>

  <xsl:template match="/">
    <!-- Current-resource document root element. -->
    <xsl:variable name="resource" select="child::*[1]"/>
    <!-- Base-resource (project or generic parent resource) document root element. -->
    <xsl:variable name="baseResource" select="document(concat($dir,'/',$resource/@base,'/',$fileTitle,$srcFileExtension))/*"/>
    <!-- Current-resource reference inside base resource map. -->
    <xsl:variable name="resourceRef" select="$baseResource/m:resources/m:resource[@name=$resource/@name]"/>

    <xsl:comment>
<xsl:text>

*** NOTE ***
This file was AUTOMATICALLY GENERATED through Mentor 0.1 stylesheets.
Mentor 0.1 is an XML vocabulary for project metadocumentation.

DO NOT MODIFY THIS FILE BY HAND: TWEAK ITS SOURCE (</xsl:text><xsl:value-of select="concat($fileTitle,$srcFileExtension)"/><xsl:text> file) INSTEAD.

</xsl:text>
    </xsl:comment>
    <article xml:lang="{$lang}" version="5.0">
      <info>
        <title><xsl:value-of select="$resource/m:title"/><xsl:value-of select="concat(' - ',$fileTitle)"/></title>
        <!-- Is it a subresource? -->
        <xsl:if test="$resource/@base and $resource/@base!='.'">
          <subtitle><link xl:href="{concat($resource/@base,'/',$fileTitle,$targetFileExtension)}"><xsl:value-of select="$baseResource/m:title"/></link></subtitle>
        </xsl:if>
        <releaseinfo>
          <xsl:text>Project version: </xsl:text>
          <xsl:choose>
            <xsl:when test="$resource/@version">
              <xsl:value-of select="$resource/@version"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$baseResource/@version"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> -- </xsl:text><xsl:value-of select="$fileTitle"/>
          <xsl:text> revision: </xsl:text>
          <xsl:choose>
            <xsl:when test="$resource/m:meta">
              <xsl:value-of select="$resource/m:meta/m:revision"/><xsl:text> (</xsl:text><xsl:value-of select="$resource/m:meta/m:date"/><xsl:text>)</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$baseResource/m:meta/m:revision"/><xsl:text> (</xsl:text><xsl:value-of select="$baseResource/m:meta/m:date"/><xsl:text>)</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </releaseinfo>
        <authorgroup>
          <xsl:apply-templates select="$baseResource/m:meta/m:author"/>
        </authorgroup>
      </info>

      <section xml:id="Introduction">
        <title>Introduction</title>
        <xsl:apply-templates select="$resource/m:description"/>
      </section>

      <xsl:apply-templates select="$resource">
        <xsl:with-param name="baseResource" select="$baseResource"/>
      </xsl:apply-templates>

      <xsl:apply-templates select="$resource/m:comment"/>

      <xsl:apply-templates select="$resource/m:resources">
        <xsl:with-param name="baseResource" select="$baseResource"/>
      </xsl:apply-templates>
    </article>
  </xsl:template>

  <xsl:template match="m:project">
    <xsl:param name="baseResource"/>

    <section xml:id="Copyright">
      <title>Copyright</title>
      <para><xsl:value-of select="concat('Copyright &#169; ',m:copyright/m:year,' ',m:copyright/m:holder/m:name/m:first,' ',m:copyright/m:holder/m:name/m:last)"/></para>
      <para><xsl:text>Contacts:</xsl:text>
      <itemizedlist>
      <xsl:for-each select="m:copyright/m:holder/m:contact">
        <listitem><xsl:apply-templates select="."/></listitem>
      </xsl:for-each>
      </itemizedlist>
      </para>
    </section>

    <xsl:apply-templates select="m:license"/>
  </xsl:template>

  <xsl:template match="m:resource">
    <xsl:param name="baseResource"/>
  </xsl:template>

  <xsl:template match="m:whatsNew | m:changeLog">
    <xsl:param name="baseResource"/>

    <xsl:apply-templates select="m:entries/m:release"/>
  </xsl:template>

  <xsl:template match="m:todoList | m:issueList">
    <xsl:param name="baseResource"/>

    <section xml:id="list">
      <title>
        <xsl:choose>
          <xsl:when test="name() = 'todoList'">TODO list</xsl:when>
          <xsl:when test="name() = 'issueList'">ISSUES list</xsl:when>
        </xsl:choose>
      </title>
      <xsl:apply-templates select="m:entries"/>
    </section>
  </xsl:template>

  <xsl:template match="m:entries">
    <itemizedlist>
      <xsl:for-each select="*">
        <listitem>
          <xsl:value-of select="concat('[',name())"/>
          <xsl:if test="@id!=''">:<xsl:value-of select="@id"/></xsl:if>
          <xsl:text>] </xsl:text>
          <xsl:apply-templates select="."/>
        </listitem>
      </xsl:for-each>
    </itemizedlist>
  </xsl:template>

  <xsl:template match="m:release[ancestor::m:whatsNew]">
    <section xml:id="{concat('releasechanges_',@version)}">
      <title>Version <xsl:value-of select="@version"/></title>
      <para>
<literallayout>Release date: <xsl:value-of select="@date"/>
Backward compatibility: <xsl:value-of select="@compatible"/></literallayout>
      </para>
      <xsl:apply-templates select="m:description"/>

      <xsl:apply-templates select="m:entries"/>
    </section>
  </xsl:template>

  <xsl:template match="m:release[ancestor::m:changeLog]">
    <section xml:id="{concat('releasechanges_',@version)}">
      <title>Version <xsl:value-of select="@version"/></title>
      <para>
<literallayout>Release date: <xsl:value-of select="@date"/>
Backward compatibility: <xsl:value-of select="@compatible"/></literallayout>
      </para>
      <xsl:apply-templates select="m:description"/>

      <itemizedlist>
        <xsl:apply-templates select="m:feature"/>
      </itemizedlist>
    </section>
  </xsl:template>

  <xsl:template match="m:feature[ancestor::m:release]">
    <xsl:variable name="featureId" select="@idref"/>
    <listitem><xsl:value-of select="/*/m:features/m:feature[@id=$featureId]/m:title"/>
      <xsl:apply-templates select="m:entries"/>
    </listitem>
  </xsl:template>

  <xsl:template match="m:contact|m:src">
    <xsl:value-of select="@type"/><xsl:text>: </xsl:text>
    <xsl:choose>
      <xsl:when test="@type='url'">
        <link xl:href="{text()}"><xsl:value-of select="text()"/></link>
      </xsl:when>
      <xsl:when test="@type='email'">
        <link xl:href="{concat('mailto:',text())}"><xsl:value-of select="text()"/></link>
      </xsl:when>
      <xsl:when test="@type='mail'">
        <xsl:value-of select="m:name"/><xsl:text>, </xsl:text><xsl:value-of select="m:place"/><xsl:text>, </xsl:text><xsl:value-of select="m:city"/><xsl:text>, </xsl:text><xsl:value-of select="m:subCountry"/><xsl:text> </xsl:text><xsl:value-of select="m:postalCode"/><xsl:text> </xsl:text><xsl:value-of select="m:country"/><xsl:text>.</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="m:license">
    <section xml:id="License">
      <title>License</title>
      <xsl:apply-templates select="m:description"/>
      <para><xsl:text>References:</xsl:text>
        <itemizedlist>
          <xsl:for-each select="m:inherits">
            <listitem><xsl:value-of select="m:base/@name"/> (<xsl:value-of select="m:base/m:title"/>) version <xsl:value-of select="m:base/@version"/><xsl:text>:</xsl:text>
              <itemizedlist>
                <listitem><xsl:text>sources:</xsl:text>
                  <itemizedlist>
                    <xsl:for-each select="m:base/m:src">
                      <listitem><xsl:apply-templates select="."/></listitem>
                    </xsl:for-each>
                  </itemizedlist>
                </listitem>
                <listitem><xsl:text>restrictions:</xsl:text>
                  <xsl:choose>
                    <xsl:when test="m:restrictions/m:restriction">
                      <itemizedlist>
                        <xsl:for-each select="m:restrictions/m:restriction">
                          <listitem><xsl:apply-templates select="."/></listitem>
                        </xsl:for-each>
                      </itemizedlist>
                    </xsl:when>
                    <xsl:otherwise><xsl:text> none</xsl:text></xsl:otherwise>
                  </xsl:choose>
                </listitem>
                <listitem><xsl:text>extensions:</xsl:text>
                  <xsl:choose>
                    <xsl:when test="m:extensions/m:extension">
                      <itemizedlist>
                        <xsl:for-each select="m:extensions/m:extension">
                          <listitem><xsl:apply-templates select="."/></listitem>
                        </xsl:for-each>
                      </itemizedlist>
                    </xsl:when>
                    <xsl:otherwise><xsl:text> none</xsl:text></xsl:otherwise>
                  </xsl:choose>
                </listitem>
              </itemizedlist>
            </listitem>
          </xsl:for-each>
        </itemizedlist>
      </para>
    </section>

    <xsl:if test="m:disclaimer">
      <section xml:id="Disclaimer">
        <title>Disclaimer</title>
        <xsl:apply-templates select="m:disclaimer"/>
      </section>
    </xsl:if>
  </xsl:template>

  <xsl:template match="m:author">
    <author>
      <firstname><xsl:value-of select="m:name/m:first"/></firstname>
      <surname><xsl:value-of select="m:name/m:last"/></surname>
      <xsl:apply-templates select="m:contact"/>
    </author>
  </xsl:template>

  <xsl:template match="m:contact[parent::m:author]">
    <xsl:choose>
      <xsl:when test="@type='email'">
        <email><xsl:value-of select="."/></email>
      </xsl:when>
      <xsl:when test="@type='url'">
        <authorblurb>Contact: <link xl:href="{text()}"><xsl:value-of select="text()"/></link></authorblurb>
      </xsl:when>
      <xsl:otherwise/>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="m:resources">
    <xsl:param name="baseResource"/>

    <xsl:variable name="curResource" select=".."/>
    <xsl:variable name="curResourceRef" select="$baseResource/m:resources//m:resource[@name=$curResource/@name]"/>

    <section xml:id="Resources">
      <title>Resources</title>
      <itemizedlist>
        <!-- NOTE:
        Each project|resource has a 'base' attribute that points to its parent project|resource.
        When loading a descendant document, such a 'base' attribute is crucial:
        * a global document path MUST be constructed as:
          resourcePath = parentDir + {$parent//m:resource/@href} + fileTitle
          parentDir = dir + {$curResource/@base}
        * a local document path MUST be constructed as:
          resourcePath = dir + {$curResource//m:resource/@href} + fileTitle
        -->
        <!-- Global resources (retrieved from the parent resources list). -->
        <xsl:for-each select="$curResourceRef/child::m:resource">
          <listitem>
            <!-- Where is the resource described? -->
            <xsl:choose>
              <!-- The resource is internally described. -->
              <xsl:when test="m:title">
                <link xl:href="{@href}">
                  <xsl:value-of select="m:title"/>
                </link><xsl:text>: </xsl:text><xsl:apply-templates select="m:tip"/>
              </xsl:when>
              <!-- The resource is externally described. -->
              <xsl:otherwise>
                <xsl:variable name="resource" select="document(concat($dir,'/',$curResource/@base,'/',@href))/*"/>
                <link>
                  <xsl:attribute name="xl:href">
                    <xsl:call-template name="relativePath">
                      <xsl:with-param name="curUrl" select="$curResourceRef/@href"/>
                      <xsl:with-param name="targetUrl" select="substring-before(@href,$srcFileExtension)"/>
                    </xsl:call-template>
                    <xsl:value-of select="$targetFileExtension"/>
                  </xsl:attribute>
                  <xsl:value-of select="$resource/m:title"/>
                </link><xsl:text>: </xsl:text><xsl:apply-templates select="$resource/m:tip"/>
              </xsl:otherwise>
            </xsl:choose>
          </listitem>
        </xsl:for-each>

        <!-- Local resources (retrieved from the current-resource resources list). -->
        <xsl:for-each select="m:resource">
          <listitem>
            <xsl:choose>
              <!-- Is the resource internally described? -->
              <xsl:when test="m:title">
                <link xl:href="{@href}">
                  <xsl:value-of select="m:title"/>
                </link><xsl:text>: </xsl:text><xsl:apply-templates select="m:tip"/>
              </xsl:when>
              <!-- The resource is externally described. -->
              <xsl:otherwise>
                <xsl:variable name="resource" select="document(concat($dir,'/',@href))/*"/>
                <link xl:href="{concat(substring-before(@href,$srcFileExtension),$targetFileExtension)}">
                  <xsl:value-of select="$resource/m:title"/>
                  <xsl:if test="name($resource)!='project' and name($resource)!='resource'">
                    <xsl:text> (</xsl:text>
                    <xsl:choose>
                      <xsl:when test="name($resource)='changeLog'">
                        <xsl:text>CHANGELOG</xsl:text>
                      </xsl:when>
                      <xsl:when test="name($resource)='whatsNew'">
                        <xsl:text>WHATSNEW</xsl:text>
                      </xsl:when>
                      <xsl:when test="name($resource)='issueList'">
                        <xsl:text>ISSUES</xsl:text>
                      </xsl:when>
                      <xsl:when test="name($resource)='todoList'">
                        <xsl:text>TODO</xsl:text>
                      </xsl:when>
                    </xsl:choose>
                    <xsl:text>)</xsl:text>
                  </xsl:if>
                </link><xsl:text>: </xsl:text><xsl:apply-templates select="$resource/m:tip"/>
              </xsl:otherwise>
            </xsl:choose>
          </listitem>
        </xsl:for-each>

        <listitem xml:id="navigation"><xsl:text>Navigation:</xsl:text>
          <itemizedlist>
            <listitem><link xl:href=".">Current directory</link>: browse current section contents</listitem>

            <!-- Parent resource -->
            <xsl:variable name="parentResourceRef" select="$curResourceRef/.."/>
            <xsl:if test="$parentResourceRef">
              <xsl:variable name="href">
                <xsl:choose>
                  <xsl:when test="name($parentResourceRef)='resource'">
                    <xsl:value-of select="substring-before($parentResourceRef/@href,$srcFileExtension)"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$resourceFileTitle"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <listitem><link xl:href="{concat($curResource/@base,'/',$href,$targetFileExtension)}">Parent section</link>: move to parent section</listitem>
            </xsl:if>

            <!-- Previous resource -->
            <xsl:variable name="prevResourceRef" select="($curResourceRef/preceding-sibling::m:resource[contains(@href,$srcFileExtension)][position()=1]|$curResourceRef/..)[last()]"/>
            <xsl:if test="$prevResourceRef">
              <listitem><link xl:href="{concat($curResource/@base,'/',substring-before($prevResourceRef/@href,$srcFileExtension),$targetFileExtension)}">Previous section</link>: move to previous section</listitem>
            </xsl:if>

            <!-- Next resource -->
            <xsl:variable name="nextResourceRef" select="$curResourceRef/child::m:resource[contains(@href,$srcFileExtension)][position()=1]|$curResourceRef/following::m:resource[contains(@href,$srcFileExtension)][1]"/>
            <xsl:if test="$nextResourceRef">
              <listitem><link xl:href="{concat($curResource/@base,'/',substring-before($nextResourceRef/@href,$srcFileExtension),$targetFileExtension)}">Next section</link>: move to next section</listitem>
            </xsl:if>
          </itemizedlist>
        </listitem>
      </itemizedlist>
    </section>
  </xsl:template>

  <xsl:template match="db:*">
    <xsl:element name="{name()}">
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="relativePath">
    <xsl:param name="curUrl"/>
    <xsl:param name="targetUrl"/>

    <xsl:variable name="baseDir" select="concat(substring-before($curUrl,'/'),'/')"/>
    <xsl:choose>
      <!-- Subdirectory available in current URL. -->
      <xsl:when test="$baseDir!='/'">
        <xsl:choose>
          <xsl:when test="starts-with($targetUrl,$baseDir)">
            <xsl:call-template name="relativePath">
              <xsl:with-param name="curUrl" select="substring-after($curUrl,$baseDir)"/>
              <xsl:with-param name="targetUrl" select="substring-after($targetUrl,$baseDir)"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>../</xsl:text>
            <xsl:call-template name="relativePath">
              <xsl:with-param name="curUrl" select="substring-after($curUrl,$baseDir)"/>
              <xsl:with-param name="targetUrl" select="$targetUrl"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!-- No subdirectories in current URL. -->
      <xsl:otherwise>
        <xsl:value-of select="$targetUrl"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
