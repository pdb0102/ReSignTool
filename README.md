# ReSignTool
Application to replace references and re-sign .Net assemblies.

### Use this tool if you:
* Want to change the key an assembly is signed with
* Want to change a reference in an assembly to point to an assembly with a different version or signing key

It is a command-line tool, and is configured via XML file.

### Example Usage:
`resigntool myconfig.xml`

### Sample XML configuration File
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
		<dll path="c:\mydlls\Microsoft.IdentityModel.Clients.ActiveDirectory.dll" key="libraries" />
		<dll path="c:\mydlls\Microsoft.IdentityModel.Clients.ActiveDirectory.WindowsForms.dll" key="libraries" />
		<dll path="c:\mydlls\Microsoft.Rest.ClientRuntime.Azure.dll" key="libraries" />
		<dll path="c:\mydlls\Microsoft.Rest.ClientRuntime.dll" key="libraries" />
	</sign>
</resign>

