using System.Collections.Generic;
using System.Linq;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public static class NodeCommands
    {
		#region Fields

		private static List<string> s_lockedNodes = new();

		#endregion

		#region Methods

		[YarnFunction("isLocked")]
		public static bool IsNodeLocked(string nodeName)
		{
			return s_lockedNodes.Contains(nodeName);
		}

		[YarnCommand("lock")]
		public static void LockNode(string nodeName)
		{
			s_lockedNodes.Add(nodeName);
		}

		[YarnCommand("unlock")]
		public static void UnlockNode(string nodeName)
		{
			if (s_lockedNodes.Contains(nodeName))
			{
				s_lockedNodes.Remove(nodeName);
			}
		}

		[YarnCommand("unlockNext")]
		public static void UnlockNextNode(int count = 1)
		{
			for (int i = 0; i < count; ++i)
			{
				if (s_lockedNodes.Count > 0)
				{
					s_lockedNodes.RemoveAt(0);
				}
			}
		}

		[YarnCommand("shuffle")]
		public static void ShuffleNodes()
		{
			s_lockedNodes = s_lockedNodes.Shuffle().ToList();
		}

		#endregion
	}
}