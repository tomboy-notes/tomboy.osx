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
                <!--xsl:copy-of select="." /-->
                <xsl:apply-templates select="//body"/>
        </xsl:template>

        <xsl:template match="//b">
                <bold><xsl:value-of select="current()"></xsl:value-of></bold>
        </xsl:template>

        <xsl:template match="text()" mode="#all">
                <xsl:value-of select="." />
        </xsl:template>
</xsl:stylesheet>