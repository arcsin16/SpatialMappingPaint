using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace arcsin16.SpatialMappingPaint
{

    /// <summary>
    /// 発射体の振る舞いを定義するBehaviourクラス.
    /// Collider接触時に着弾のエフェクトを発生させて、PaintManagerで接触した箇所にペイントして、自身をDestoryする
    /// </summary>
    public class BulletBehaviour : MonoBehaviour
    {
        /// <summary>
        /// 着弾時のパーティクルエフェクト
        /// </summary>
        public GameObject particlePrefab;

        /// <summary>
        /// 着弾時のサウンドエフェクト
        /// </summary>
        public AudioClip splashSound;

        /// <summary>
        /// 着弾時のサウンドボリューム
        /// </summary>
        public float volume = 2.0f;

        /// <summary>
        /// ペイント可否
        /// </summary>
        public bool enablePaint = true;

        /// <summary>
        /// SpatialMappingのMesに対する衝突判定が不安定なため、RayCastを使って独自に判定する用の変数
        /// </summary>
        private Rigidbody rigidBody;
        private Vector3 lastPos;
        private Vector3 lastHitPos;
        private Vector3 lastHitNormal;
        private bool lastHit;

        public void Start()
        {
            this.rigidBody = this.GetComponent<Rigidbody>();

            this.updateCollisionDetectInfo();
        }

        public void Update()
        {
            var pos = this.transform.position;

            // Pos(t) < 衝突予測位置(t-1) < Pos(t-1) であれば、衝突と判定
            if (this.lastHit && Vector3.SqrMagnitude(this.lastHitPos - this.lastPos) < Vector3.SqrMagnitude(pos - this.lastPos))
            {
                this.OnHit(this.lastHitPos, this.lastHitNormal);
                return;
            }

            this.updateCollisionDetectInfo();
        }

        /// <summary>
        /// 衝突判定用に各フレームの位置、Raycast位置情報を更新する
        /// </summary>
        private void updateCollisionDetectInfo()
        {
            var pos = this.transform.position;
            var dir = this.rigidBody.velocity;
            this.lastHit = false;
            RaycastHit hitInfo;
            if (Physics.Raycast(pos, dir.normalized, out hitInfo, 5.0f))
            {
                if (hitInfo.collider.gameObject.layer == 31)
                {
                    this.lastHitPos = hitInfo.point;
                    this.lastHitNormal = hitInfo.normal;
                    this.lastHit = true;
                }
            }
            this.lastPos = pos;

        }

        /// <summary>
        /// 壁に衝突した際に呼び出されるコールバック
        /// </summary>
        /// <param name="point">衝突位置</param>
        /// <param name="normal">衝突位置の法線ベクトル</param>
        private void OnHit(Vector3 point, Vector3 normal)
        {
            // エフェクト
            if (particlePrefab != null)
            {
                Instantiate(particlePrefab, point, Quaternion.identity);
            }
            AudioSource.PlayClipAtPoint(splashSound, point, volume);

            // 接触した箇所にペイントする
            if (enablePaint)
            {
                var renderer = GetComponentInChildren<Renderer>();
                PaintManager.Instance.Paint(point, normal, renderer.material.color);
            }

            // 解放
            Destroy(this.gameObject);
        }
    }
}
