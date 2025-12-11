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

		[YarnFunction("isNodeLocked")]
		public static bool IsNodeLocked(string nodeName)
		{
			return s_lockedNodes.Contains(nodeName);
		}

		[YarnCommand("lockNode")]
		public static void LockNode(string nodeName)
		{
			s_lockedNodes.Add(nodeName);
		}

		[YarnCommand("unlockNode")]
		public static void UnlockNode(string nodeName)
		{
			if (s_lockedNodes.Contains(nodeName))
			{
				s_lockedNodes.Remove(nodeName);
			}
		}

		[YarnCommand("unlockNextNode")]
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

		[YarnCommand("shuffleNodes")]
		public static void ShuffleNodes()
		{
			s_lockedNodes = s_lockedNodes.Shuffle().ToList();
		}

		#endregion
	}
}