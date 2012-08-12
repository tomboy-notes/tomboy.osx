<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:tomboy="http://beatniksoftware.com/tomboy"
xmlns:link="http://beatniksoftware.com/tomboy/link"
xmlns:size="http://beatniksoftware.com/tomboy/size"
exclude-result-prefixes="xsl tomboy link size"
>

	<xsl:output method="html" version="1.0" indent="yes" />
	<xsl:strip-space elements="*" />

	<xsl:template match="/">
		<!-- xsl:copy-of select="." /-->
		<xsl:apply-templates select="tomboy:text/tomboy:note-content"/>
	</xsl:template>
	
	<xsl:template match="tomboy:note-content">
		<xsl:apply-templates />
	</xsl:template>
	
	<xsl:template match="link:internal">
		<a href="{text()}"><xsl:value-of select="text()" /></a>
	</xsl:template>
	
	<xsl:template match="size:large">
		<b><xsl:value-of select="text()" /></b>
	</xsl:template>
	
	<xsl:template match="tomboy:note-content/tomboy:list">
		<xsl:for-each select="tomboy:list-item">
			<ul>
				<li><xsl:value-of select="text()"/></li>
			</ul>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template match="link:url">
		<a href="{text()}"><xsl:value-of select="text()" /></a>
		<xsl:if test="link:internal">
			<a href="{link:internal/text()}"><xsl:value-of select="link:internal/text()" /></a>
		</xsl:if>
	</xsl:template>

	<xsl:template match="text()" mode="#all">
		<xsl:value-of select="." />
	</xsl:template>
</xsl:stylesheet>