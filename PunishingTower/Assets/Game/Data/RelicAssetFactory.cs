using UnityEngine;

namespace PunishingTower.Data
{
    /// <summary>
    /// Creates relic assets (doc 204: Grey Raven Badge - three match grants 1 energy).
    /// </summary>
    public static class RelicAssetFactory
    {
        public static RelicData GreyRavenBadge()
        {
            var relic = ScriptableObject.CreateInstance<RelicData>();
#if UNITY_EDITOR
            relic.AssignIdentity("grey_raven_badge", "灰鸦徽章");
            relic.AssignRelic(RelicTrigger.ThreeMatch, EffectType.Energy, 1,
                "三消时:当前构造体获得 1 点能量");
#endif
            return relic;
        }
    }
}
