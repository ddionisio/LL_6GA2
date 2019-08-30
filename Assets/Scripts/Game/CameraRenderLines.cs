using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRenderLines : MonoBehaviour {
    private class Item {
        public string name;
        public Material material; //use Unlit/Color for shader
        public Vector3[] vtx; //ensure it is even number, and length >= 2

        public bool isVisible;
    }

    private List<Item> mItems = new List<Item>();

    public void Add(string aName, Vector3[] vtx, Material material, bool isVisible) {
        //ensure it doesn't exists
        int ind = -1;
        for(int i = 0; i < mItems.Count; i++) {
            if(mItems[i].name == aName) {
                ind = i;
                break;
            }
        }

        if(ind == -1)
            mItems.Add(new Item { name = aName, vtx = vtx, material = material, isVisible = isVisible });
    }

    public void Remove(string aName) {
        for(int i = 0; i < mItems.Count; i++) {
            if(mItems[i].name == aName) {
                mItems.RemoveAt(i);
                break;
            }
        }
    }

    public void SetVisible(string aName, bool visible) {
        for(int i = 0; i < mItems.Count; i++) {
            var itm = mItems[i];
            if(itm.name == aName) {
                itm.isVisible = visible;
                break;
            }
        }
    }

    void OnPostRender() {
        for(int i = 0; i < mItems.Count; i++) {
            var itm = mItems[i];
            if(itm.isVisible) {
                GL.Begin(GL.LINES);

                itm.material.SetPass(0);

                var vtx = itm.vtx;
                for(int v = 0; v < vtx.Length; v += 2) {
                    GL.Vertex(vtx[v]);
                    GL.Vertex(vtx[v+1]);
                }

                GL.End();
            }
        }
    }
}
