using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HoloToolkit.Unity;

namespace arcsin16.SpatialMappingPaint
{

    /// <summary>
    /// 弾の発射を管理するマネージャークラス
    /// </summary>
    public class ShootingManager : Singleton<ShootingManager>
    {
        public GameObject bulletPrefab;

        public AudioClip fireSound;

        private Color[] colors;
        private int colorIndex = 0;

        // 経過時間
        private float elapsedTime;

        private void Update()
        {
            // 3秒間隔で弾の色を変える
            this.elapsedTime += Time.deltaTime;
            if(elapsedTime > 3)
            {
                colorIndex = (colorIndex + 1) % this.Colors.Length;
                this.elapsedTime = 0;
            }
        }

        /// <summary>
        /// カメラの右斜め前辺りから弾を発射する
        /// </summary>
        public void Fire()
        {
            // 発射位置を算出
            var transform = Camera.main.transform;
            var position = transform.position + transform.forward * 0.5f + transform.right * 0.2f;

            // 初速を算出
            var velocity = transform.forward * 8f;

            var horizontalRandom = new Vector3(transform.right.x, 0, transform.right.z);
            horizontalRandom.Normalize();
            horizontalRandom = horizontalRandom * Random.Range(-0.5f, 0.5f);

            var verticalRandom = new Vector3(0, Random.Range(1.5f, 2f), 0);
            velocity = velocity + horizontalRandom + verticalRandom;

            // 発射位置、初速、色を指定して発射
            Fire(position, velocity, this.Colors[this.colorIndex]);
        }

        /// <summary>
        /// 指定された位置、初速、色を元に弾を発射する
        /// </summary>
        /// <param name="position">発射位置</param>
        /// <param name="velocity">初速</param>
        /// <param name="color">色</param>
        private void Fire(Vector3 position, Vector3 velocity, Color color)
        {
            var bullet = Instantiate(this.bulletPrefab, position, Quaternion.identity);

            var brs = bullet.GetComponentsInChildren<Renderer>();
            foreach (var br in brs)
            {
                br.material.color = color;
            }

            // RigidBody::AddForceで初速を設定する。
            var rigidbody = bullet.GetComponent<Rigidbody>();
            rigidbody.AddForce(velocity, ForceMode.VelocityChange);

            // 発射音をかける
            AudioSource.PlayClipAtPoint(this.fireSound, position, 0.6f);

            // 壁に当たらなかった場合、2秒後に自動で破棄されるように
            Destroy(bullet, 2.0f);
        }

        private Color[] Colors
        {
            get
            {
                if (this.colors == null)
                {
                    Color orange;
                    ColorUtility.TryParseHtmlString("#FD850AFF", out orange);
                    this.colors = new Color[]
                    {
                        orange, Color.yellow, Color.blue, Color.cyan, Color.green
                    };
                }

                return this.colors;
            }
        }
    }
}