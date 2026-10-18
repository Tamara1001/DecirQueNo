using UnityEngine;
using UnityEngine.UI;

namespace DecirQueNo
{
    // Vertical color gradient on a plain Image: the top and bottom vertices get their own color.
    public class UiGradient : BaseMeshEffect
    {
        private Color _top = Color.white;
        private Color _bottom = Color.white;

        public void SetColors(Color top, Color bottom)
        {
            if (top == _top && bottom == _bottom)
            {
                return;
            }

            _top = top;
            _bottom = bottom;
            if (graphic != null)
            {
                graphic.SetVerticesDirty();
            }
        }

        public override void ModifyMesh(VertexHelper helper)
        {
            if (!IsActive() || helper.currentVertCount == 0)
            {
                return;
            }

            var vertex = new UIVertex();
            float min = float.MaxValue;
            float max = float.MinValue;
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                min = Mathf.Min(min, vertex.position.y);
                max = Mathf.Max(max, vertex.position.y);
            }

            float height = Mathf.Max(0.0001f, max - min);
            for (int i = 0; i < helper.currentVertCount; i++)
            {
                helper.PopulateUIVertex(ref vertex, i);
                vertex.color = Color.Lerp(_bottom, _top, (vertex.position.y - min) / height);
                helper.SetUIVertex(vertex, i);
            }
        }
    }
}
