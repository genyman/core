using System;

namespace Genyman.Core
{
	[AttributeUsage(AttributeTargets.Property)]
	public class IgnoreAttribute : Attribute
	{
	}
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
	public class DocumentationAttribute : Attribute
	{
		public string Remarks { get; set; }	
		public string Source { get; set; }	
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class RequiredAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class DescriptionAttribute : Attribute
	{
		public string Description { get; }

		public DescriptionAttribute(string description)
		{
			Description = description;
		}
	}
}