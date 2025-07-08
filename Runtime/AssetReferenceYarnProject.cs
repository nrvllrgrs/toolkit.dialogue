using UnityEngine.AddressableAssets;
using Yarn.Unity;

namespace ToolkitEngine.Dialogue
{
#if USE_ADDRESSABLES
	public class AssetReferenceYarnProject : AssetReferenceT<YarnProject>, IKeyEvaluator
	{
		public AssetReferenceYarnProject(string guid)
			: base(guid)
		{ }
    }
#endif
}