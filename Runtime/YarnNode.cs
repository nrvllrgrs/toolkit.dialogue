using UnityEngine;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
    [System.Serializable]
    public class YarnNode
    {
		#region Fields

		[SerializeField]
		private YarnProject m_project;

		[SerializeField]
		private string m_name;

		#endregion

		#region Properties

		public YarnProject project => m_project;
		public string name => m_name;

		#endregion
	}
}