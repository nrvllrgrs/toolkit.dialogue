using System.Text.RegularExpressions;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public static class YarnParserUtil
    {
		private const string LINE_PREFIX = "line:";
		private const string METADATA_PREFIX = "Line metadata: ";

		public static string GetID(StringTableEntry entry)
		{
			return entry.ID.StartsWith(LINE_PREFIX)
				? entry.ID.Substring(LINE_PREFIX.Length)
				: entry.ID;
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

		public static bool TryGetMetadataTag(StringTableEntry entry, string key, out string value)
		{
			value = null;
			var metadata = GetMetadata(entry);
			if (string.IsNullOrWhiteSpace(metadata))
				return false;

			var match = Regex.Match(metadata, $@"{key}:(?<data>\S*)");
			if (!match.Success)
				return false;

			value = match.Groups["data"].Value;
			return true;
		}
	}
}