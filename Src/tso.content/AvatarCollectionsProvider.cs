using FSO.Content.Codecs;
using FSO.Content.Framework;
using FSO.Vitaboy;
using System.Text.RegularExpressions;

namespace FSO.Content
{
    public class AvatarCollectionsProvider : FAR3Provider<Collection>
    {
        public AvatarCollectionsProvider(GameContent contentManager)
            : base(contentManager, new CollectionCodec(), new Regex(".*/collections/.*\\.dat"))
        {
        }
    }
}