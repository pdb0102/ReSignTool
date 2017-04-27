# ReSignTool
Application to replace references and re-sign .Net assemblies.

### Use this tool if you:
* Want to change the key an assembly is signed with
* Want to change a reference in an assembly to point to an assembly with a different version or signing key
* Want to create a unique version of a common assembly if you want to ensure that an update of a referenced assembly on the target system does not change what version you're using

It is a command-line tool, and is configured via XML file.

### Example Usage:
`resigntool myconfig.xml`

### Sample XML configuration File
This (real) example replaces the Newtonsoft.Json reference in MS Azure assemblies with your own (or newer) version of Newtonsoft. It results in versions of the MS assemblies signed with your own key.
	<?xml version="1.0" encoding="utf-8"?>
	<resign>
		<!-- This sample file switches the Newtonsoft.Json reference in the MS Azure assemblies to a different version and resigns them -->
		<!-- List all signing keys to be used -->
		<signingkey name="libraries" file="c:\mykeys\librarykey.snk" />
		<signingkey name="product" file="c:\mykeys\productkey.snk" />

		<!-- List of references that need to be fixed up in the DLLs to be signed -->
		<references>
			<dll name="Newtonsoft.Json.dll" path="c:\mydlls\Newtonsoft.Json.dll"/>
		</references>

		<!-- List of DLLs to sign and fix references in -->
		<sign outputdir="c:\mydlls\resigned" changedonly="true">
			<dll path="c:\mydlls\Microsoft.Azure.KeyVault.dll" key="libraries" />
			<dll path="c:\mydlls\Microsoft.Azure.KeyVault.WebKey.dll" key="libraries" />
			<dll path="c:\mydlls\Microsoft.Rest.ClientRuntime.Azure.dll" key="libraries" />
			<dll path="c:\mydlls\Microsoft.Rest.ClientRuntime.dll" key="libraries" />
		</sign>
	</resign>

