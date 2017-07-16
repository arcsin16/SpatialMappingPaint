using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

namespace arcsin16.SpatialMappingPaint
{

    public class PaintManager : Singleton<PaintManager>
    {
        /// <summary>
        /// 透過色
        /// </summary>
        private static readonly Color TRANSPARENT = new Color(0, 0, 0, 0);

        /// <summary>
        /// 床面のテクスチャ
        /// </summary>
        private RenderTexture renderTextureFloor;

        /// <summary>
        /// 描画用の一時テクスチャ
        /// </summary>
        private RenderTexture renderTextureTemp;

        /// <summary>
        /// 描画用テクスチャサイズ(px)
        /// </summary>
        public int texturePixels = 2048;

        /// <summary>
        /// メッシュのシェーダーのMaterial
        /// </summary>
        public Material meshSurfaceMaterial;

        /// <summary>
        /// 塗領域のサイズ（World座標
        /// </summary>
        public Vector2 areaSize;

        public Shader brushTextureShader;
        public Texture2D brushTexture;
        private Material paintingMaterial;

        /// <summary>
        /// ブラシのサイズ（World座標
        /// </summary>
        public float radius;

        void Start()
        {
            // 描画テクスチャ初期化
            this.renderTextureFloor = new RenderTexture(this.texturePixels, this.texturePixels, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            this.renderTextureFloor.Create();

            this.renderTextureTemp = new RenderTexture(this.texturePixels, this.texturePixels, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            this.renderTextureTemp.Create();

            // メッシュ描画用マテリアル初期化
            this.meshSurfaceMaterial.SetTexture("_MainTex", this.renderTextureFloor);
            this.meshSurfaceMaterial.SetVector("_AreaSize", new Vector4(areaSize.x, areaSize.y, 0, 0));

            this.paintingMaterial = new Material(this.brushTextureShader);
            this.paintingMaterial.SetTexture("_BrushTex", this.brushTexture);

            // 書き込み先のTextureを透過色で初期化
            this.Clear();
        }

        /// <summary>
        /// 描画用のCanvasに、ブラシで色を描画する
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color"></param>
        public void Paint(Vector3 point, Vector3 normal, Color color)
        {
            var start = Time.realtimeSinceStartup;

            // 領域外を塗らないよう端はざっくり除外しておく
            if (Mathf.Abs(point.x) >= areaSize.x / 2 || Mathf.Abs(point.z) >= areaSize.y / 2)
            {
                Debug.LogFormat("OutOfRange {0},{1}", point.x, point.z);
                return;
            }

            PaintFloor(point, color);

            //Debug.LogFormat("Paint {0}s", Time.realtimeSinceStartup - start);
        }

        private void PaintFloor(Vector3 point, Color color)
        {
            // ブラシの描画領域を計測（描画対象Texture上のuv座標）
            var paintArea = new Vector4(
                (point.x + areaSize.x / 2 - radius / 2) / areaSize.x,   // 描画領域の左上のu
                (point.z + areaSize.y / 2 - radius / 2) / areaSize.y,   // 描画領域の左上のv
                radius / areaSize.x,    // 描画領域の幅(uv換算）
                radius / areaSize.y);   // 描画領域の高さ(uv換算）

            // ブラシ描画用のMaterialにパラメータを設定
            this.paintingMaterial.SetTexture("_MainTex", this.renderTextureFloor);
            this.paintingMaterial.SetVector("_PaintArea", paintArea);
            this.paintingMaterial.SetColor("_BrushColor", color);

            // ブラシをRenderTextureに書き込む
            Graphics.Blit(this.renderTextureFloor, this.renderTextureTemp, this.paintingMaterial);
            Graphics.Blit(this.renderTextureTemp, this.renderTextureFloor);
        }
        /// <summary>
        /// 描画用のCanvasを初期化する
        /// </summary>
        public void Clear()
        {
            // GLを使って透過色で初期化する
            Graphics.SetRenderTarget(this.renderTextureFloor);
            GL.Clear(false, true, TRANSPARENT);
        }
    }
}
