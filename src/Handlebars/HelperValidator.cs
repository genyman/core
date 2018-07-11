namespace Genyman.Core.Handlebars
{
	internal static class HelperValidator
	{
		public static bool MustBeString(this object[] parameters, int index = 0)
		{
			if (parameters.Length < index + 1) return false;
			return parameters[index] is string;
		}

		public static bool MustBeBooleanOrNull(this object[] parameters, int index = 0)
		{
			if (parameters.Length < index + 1) return true; // no parameter is OK = Null
			var tryParse = bool.TryParse(parameters[index].ToString(), out var result);
			return tryParse;
		}
	}
}