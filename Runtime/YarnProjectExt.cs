using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public static class YarnProjectExt
    {
        public static bool NodeExists(this YarnProject project, string nodeName)
        {
            for (int i = 0; i < project.NodeNames.Length; ++i)
            {
                if (Equals(project.NodeNames[i], nodeName))
                    return true;
            }

            return false;
        }
    }
}