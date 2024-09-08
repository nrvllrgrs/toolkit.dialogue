using UnityEngine.AddressableAssets;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
	public class AssetReferenceYarnProject : AssetReferenceT<YarnProject>, IKeyEvaluator
	{
		public AssetReferenceYarnProject(string guid)
			: base(guid)
		{ }
    }
}