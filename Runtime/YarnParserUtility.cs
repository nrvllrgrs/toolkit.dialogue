using System.Text.RegularExpressions;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public static class YarnParserUtility
    {
		private const string LINE_PREFIX = "line:";
		private const string METADATA_PREFIX = "Line metadata: ";

		public static string GetID(StringTableEntry entry)
		{
			return entry.ID.Substring(LINE_PREFIX.Length);
		}

		public static bool TryGetSpeakerAndText(StringTableEntry entry, out string speaker, out string text)
		{
			var match = Regex.Match(entry.Text, @"^(?<speaker>\w*?): ?(?<text>.*)$");
			if (match.Success)
			{
				speaker = match.Groups["speaker"].Value;
				text = match.Groups["text"].Value;
				return true;
			}

			speaker = string.Empty;
			text = entry.Text;
			return false;
		}

		public static string GetMetadata(StringTableEntry entry)
		{
			return entry.Comment.StartsWith(METADATA_PREFIX)
				? entry.Comment.Substring(METADATA_PREFIX.Length)
				: entry.Comment;
		}
	}
}