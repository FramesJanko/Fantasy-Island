using TMPro;
using UnityEngine;

namespace FantasyIsland
{
    /// Lives on the nameplate prefab (Canvas, World Space). Not baked - this prefab
    /// is instantiated at runtime as a plain GameObject by NameplateSpawnSystem,
    /// never converted to an entity. Just exposes references for that system to read.
    public class NameplateViewAuthoring : MonoBehaviour
    {
        public TMP_Text NameText;
        public TMP_Text TargetText;
    }
}
