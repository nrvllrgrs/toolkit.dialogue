using Yarn.Unity;

#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
#endif

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