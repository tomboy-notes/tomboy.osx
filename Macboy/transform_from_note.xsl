<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:tomboy="http://beatniksoftware.com/tomboy" xmlns:link="http://beatniksoftware.com/tomboy/link" xmlns:size="http://beatniksoftware.com/tomboy/size" exclude-result-prefixes="xsl tomboy link size">
	<xsl:output method="html" version="1.0" indent="yes" />
	<xsl:strip-space elements="*" />
	<xsl:template match="/">
		<!-- xsl:copy-of select="." /-->
		<!--xsl:apply-templates select="tomboy:text/tomboy:note-content" /-->
		<xsl:apply-templates />
	</xsl:template>
	
	<!--xsl:template match="tomboy:note-content">
		<xsl:apply-templates />
	</xsl:template-->
	
	<xsl:template match="link:internal">
		<a href="{text()}">
			<xsl:value-of select="text()" />
		</a>
	</xsl:template>
	<xsl:template match="tomboy:bold">
		<b>
			<xsl:apply-templates select="node()" />
		</b>
	</xsl:template>
	<xsl:template match="tomboy:italic">
		<i>
			<xsl:apply-templates select="node()" />
		</i>
	</xsl:template>
	<xsl:template match="tomboy:strikethrough">
		<strike>
			<xsl:apply-templates select="node()" />
		</strike>
	</xsl:template>
	<xsl:template match="tomboy:highlight">
		<span>
			<xsl:apply-templates select="node()" />
		</span>
	</xsl:template>
	<xsl:template match="tomboy:datetime">
		<span>
			<xsl:apply-templates select="node()" />
		</span>
	</xsl:template>
	<xsl:template match="size:small">
		<small>
			<xsl:apply-templates select="node()" />
		</small>
	</xsl:template>
	<xsl:template match="size:large">
		<big>
			<xsl:apply-templates select="node()" />
		</big>
	</xsl:template>
	<xsl:template match="size:huge">
		<big>
			<xsl:apply-templates select="node()" />
		</big>
	</xsl:template>
	<xsl:template match="link:broken">
		<span>
			<xsl:value-of select="node()" />
		</span>
	</xsl:template>
	<xsl:template match="link:internal">
		<a>
			<xsl:value-of select="node()" />
		</a>
	</xsl:template>
	<xsl:template match="link:url">
		<!--<a href="{node()}">-->
		<xsl:value-of select="node()" />
		<!-- </a> -->
	</xsl:template>
	<xsl:template match="tomboy:list">
		<ul>
			<xsl:apply-templates select="tomboy:list-item" />
		</ul>
	</xsl:template>
	<xsl:template match="tomboy:list-item">
		<li>
			<xsl:if test="normalize-space(text()) = '' and count(tomboy:list) = 1 and count(*) = 1">
				<xsl:attribute name="style">list-style-type: none</xsl:attribute>
			</xsl:if>
			<xsl:attribute name="dir">
				<xsl:value-of select="@dir" />
			</xsl:attribute>
			<xsl:apply-templates select="node()" />
		</li>
	</xsl:template>
	<!-- Evolution.dll Plugin -->
	<xsl:template match="link:evo-mail">
		<a href="{./@uri}">
			<img alt="Open Email Link" width="16" height="10" border="0">
				<!-- Inline Base64 encoded stock_mail.png =) -->
				<xsl:attribute name="src">data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABAAAAAKCAYAAAC9vt6cAAAABmJLR0QA/wD/AP+gvaeTAAAACXBI WXMAAAsQAAALEAGtI711AAAAB3RJTUUH1QkeAjYaRAvZgAAAALxJREFUKM+NkjGKw1AMRN+GhRS/ 2xP4EHZr0E1UxFVuoiKdikCKfxMfwKdw+3t1gb/F4hASe50BgZjRDEII/jAAtWmaCnxSAy+oZlYj YrfMbAkB4GsJiAjcnfPpRNzvrCHnjIjQdd3De3geUFX8diMdj6tmVX3jD6+EquLXKz9p37waANC2 LRfPpJTIOdP3PXuoEVFLKdXMaills5+m6f8jbq26dcTvRXR3RIR5njcDRIRxHFe14cMHenukX9eX mbvfl0q9AAAAAElFTkSuQmCC</xsl:attribute>
			</img>
			<xsl:value-of select="node()" />
		</a>
	</xsl:template>
	<!-- FixedWidth.dll Plugin -->
	<xsl:template match="tomboy:monospace">
		<tt>
			<xsl:apply-templates select="node()" />
		</tt>
	</xsl:template>
	<!-- Bugzilla.dll Plugin -->
	<xsl:template match="link:bugzilla">
		<a href="{@uri}">
			<xsl:value-of select="node()" />
		</a>
	</xsl:template>
	<!-- Underline Plugin -->
	<xsl:template match="underline">
		<u>
			<xsl:value-of select="node()" />
		</u>
	</xsl:template>
	<xsl:template match="text()" mode="#all">
		<xsl:value-of select="." />
	</xsl:template>
</xsl:stylesheet>