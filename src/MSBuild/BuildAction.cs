namespace Genyman.Core.MSBuild
{
	public enum BuildAction
	{
		None,
		Compile,
		Content,
		EmbeddedResource,
		Resource,
		Reference,
		Folder,
		PackageReference,
		AndroidResource,
		ImageAsset,
		InterfaceDefinition,
		BundleResource
	}
}