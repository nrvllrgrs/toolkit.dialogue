using Unity.VisualScripting;

namespace ToolkitEngine.Dialogue.VisualScripting
{
	[UnitCategory("Dialogue")]
	[UnitTitle("Yarn Node")]
	public class YarnNodeUnit : Unit
    {
		#region Fields

		[DoNotSerialize, UnitHeaderInspectable]
		public YarnNode node { get; set; }

		[DoNotSerialize]
		public ValueOutput yarnNode;

		[DoNotSerialize]
		public ValueOutput yarnProject;

		[DoNotSerialize]
		public ValueOutput nodeName;

		#endregion

		#region Methods

		protected override void Definition()
		{
			yarnNode = ValueOutput(nameof(yarnNode), (flow) => node);
			yarnProject = ValueOutput(nameof(yarnProject), (flow) => node.project);
			nodeName = ValueOutput(nameof(nodeName), (flow) => node.name);
		}

		#endregion
	}
}