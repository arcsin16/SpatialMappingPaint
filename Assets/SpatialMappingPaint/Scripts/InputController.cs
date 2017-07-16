using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA.Input;

namespace arcsin16.SpatialMappingPaint
{

    /// <summary>
    /// 入力制御コントローラークラス
    /// AirTapイベントをトリガーに、弾の発射処理とメニューの選択処理を呼び出す。
    /// </summary>
    public class InputController : MonoBehaviour
    {
        private float timeElapsed;
        private int fireCount;

        void OnEnable()
        {
            // AirTapイベント登録
            InteractionManager.SourcePressed += OnSourcePressed;
        }

        void OnDisable()
        {
            // AirTapイベント解除
            InteractionManager.SourcePressed -= OnSourcePressed;
        }

        void OnSourcePressed(InteractionSourceState state)
        {
            // AirTapイベント
            Fire();
        }

        void Update()
        {
            // Keyboardイベント（Unity Player用の処理）
            if (Input.GetKeyDown(KeyCode.Space))
            {
                fireCount = 0;
                Fire();
            }

            // 連射モード
            if (Input.GetKey(KeyCode.Space))
            {
                //Fire(0.07f, 3);
                Fire(0.1f);
            }
        }

        /// <summary>
        /// 弾の発射処理
        /// </summary>
        /// <param name="interval"></param>
        private void Fire(float interval = 0, int burst=0)
        {
            if(burst != 0 && fireCount >= burst)
            {
                return;
            }

            timeElapsed += Time.deltaTime;
            if (timeElapsed > interval)
            {
                ShootingManager.Instance.Fire();
                timeElapsed = 0f;
                fireCount++;
            }
        }
    }
}
