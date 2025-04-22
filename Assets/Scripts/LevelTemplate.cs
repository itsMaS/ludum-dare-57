using MarTools;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    public class LevelTemplate : MonoBehaviour
    {
        public const float blockSize = 10;

        public int heightInBlocks = 5;
        public int blockOffset = 0;

        public float height => heightInBlocks * blockSize;
        public float offset => blockOffset * blockSize;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(200, heightInBlocks * blockSize));
            Gizmos.DrawWireCube(transform.position + (heightInBlocks * blockSize * 0.5f * Vector3.down) + (blockOffset * blockSize * Vector3.right), new Vector3(2*blockSize, blockSize*0.2f));
            Gizmos.DrawWireCube(transform.position + (-heightInBlocks * blockSize * 0.5f * Vector3.down) + (blockOffset * blockSize * Vector3.right), new Vector3(2*blockSize, blockSize*0.2f));
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelTemplate))]
    public class LevelTemplateEditor : MarToolsEditor<LevelTemplate>
    {

    }
#endif
}
