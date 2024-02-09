// com.unity.render-pipelines.core copyright © 2020 Unity Technologies ApS
//
// Licensed under the Unity Companion License for Unity-dependent projects--see [Unity Companion License](http://www.unity3d.com/legal/licenses/Unity_Companion_License).
//
// Unless expressly provided otherwise, the Software under this license is made available strictly on an “AS IS” BASIS WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED. Please review the license for details on these and other terms and conditions.

#if !IMUI_USING_CORE_RP

using UnityEngine;
using UnityEngine.Pool;

namespace Imui.Rendering.Atlas
{
    internal class TextureAtlasAllocator
    {
        private class AtlasNode
        {
            public AtlasNode m_RightChild = null;
            public AtlasNode m_BottomChild = null;

            public Vector4
                m_Rect = new Vector4(0, 0, 0, 0); // x,y is width and height (scale) z,w offset into atlas (offset)

            public AtlasNode Allocate(ref ObjectPool<AtlasNode> pool, int width, int height, bool powerOfTwoPadding)
            {
                // not a leaf node, try children
                if (m_RightChild != null)
                {
                    AtlasNode node = m_RightChild.Allocate(ref pool, width, height, powerOfTwoPadding);
                    if (node == null)
                    {
                        node = m_BottomChild.Allocate(ref pool, width, height, powerOfTwoPadding);
                    }

                    return node;
                }

                int wPadd = 0;
                int hPadd = 0;

                if (powerOfTwoPadding)
                {
                    wPadd = (int)m_Rect.x % width;
                    hPadd = (int)m_Rect.y % height;
                }

                //leaf node, check for fit
                if ((width <= m_Rect.x - wPadd) && (height <= m_Rect.y - hPadd))
                {
                    // perform the split
                    m_RightChild = pool.Get();
                    m_BottomChild = pool.Get();

                    m_Rect.z += wPadd;
                    m_Rect.w += hPadd;
                    m_Rect.x -= wPadd;
                    m_Rect.y -= hPadd;

                    if (width > height) // logic to decide which way to split
                    {
                        //  +--------+------+
                        m_RightChild.m_Rect.z = m_Rect.z + width; //  |        |      |
                        m_RightChild.m_Rect.w = m_Rect.w; //  +--------+------+
                        m_RightChild.m_Rect.x = m_Rect.x - width; //  |               |
                        m_RightChild.m_Rect.y = height; //  |               |
                        //  +---------------+
                        m_BottomChild.m_Rect.z = m_Rect.z;
                        m_BottomChild.m_Rect.w = m_Rect.w + height;
                        m_BottomChild.m_Rect.x = m_Rect.x;
                        m_BottomChild.m_Rect.y = m_Rect.y - height;
                    }
                    else
                    {
                        //  +---+-----------+
                        m_RightChild.m_Rect.z = m_Rect.z + width; //  |   |           |
                        m_RightChild.m_Rect.w = m_Rect.w; //  |   |           |
                        m_RightChild.m_Rect.x = m_Rect.x - width; //  +---+           +
                        m_RightChild.m_Rect.y = m_Rect.y; //  |   |           |
                        //  +---+-----------+
                        m_BottomChild.m_Rect.z = m_Rect.z;
                        m_BottomChild.m_Rect.w = m_Rect.w + height;
                        m_BottomChild.m_Rect.x = width;
                        m_BottomChild.m_Rect.y = m_Rect.y - height;
                    }

                    m_Rect.x = width;
                    m_Rect.y = height;
                    return this;
                }

                return null;
            }

            public void Release(ref ObjectPool<AtlasNode> pool)
            {
                if (m_RightChild != null)
                {
                    m_RightChild.Release(ref pool);
                    m_BottomChild.Release(ref pool);
                    pool.Release(m_RightChild);
                    pool.Release(m_BottomChild);
                }

                m_RightChild = null;
                m_BottomChild = null;
                m_Rect = Vector4.zero;
            }
        }

        private AtlasNode m_Root;
        private int m_Width;
        private int m_Height;
        private bool powerOfTwoPadding;
        private ObjectPool<AtlasNode> m_NodePool;

        public TextureAtlasAllocator(int width, int height, bool potPadding)
        {
            m_Root = new AtlasNode();
            m_Root.m_Rect.Set(width, height, 0, 0);
            m_Width = width;
            m_Height = height;
            powerOfTwoPadding = potPadding;
            m_NodePool = new ObjectPool<AtlasNode>(() => new AtlasNode());
        }

        public bool Allocate(ref Vector4 result, int width, int height)
        {
            AtlasNode node = m_Root.Allocate(ref m_NodePool, width, height, powerOfTwoPadding);
            if (node != null)
            {
                result = node.m_Rect;
                return true;
            }
            else
            {
                result = Vector4.zero;
                return false;
            }
        }

        public void Reset()
        {
            m_Root.Release(ref m_NodePool);
            m_Root.m_Rect.Set(m_Width, m_Height, 0, 0);
        }
    }
}

#endif