using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Emitter
{
    public class EmitterMenu
    {
        #region BulletEmitter
        [MenuItem("Emitter/BulletEmitter/LinearEmitter", false, 6)]
        static public Emitter_Bullet CreateLinearEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "LinearEmitter";
            bullet.EmitInterval = 0.1f;
            bullet.Cooldown = 1f;
            bullet.EmitTimes = 10;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/LinearLockOnEmitter", false, 6)]
        static public Emitter_Bullet CreateLinearLockOnEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "LinearLockOnEmitter";
            bullet.EmitInterval = 0.1f;
            bullet.Cooldown = 1f;
            bullet.EmitTimes = 10;
        //    bullet.OrientateByParent = true;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/LinearAccelEmitter", false, 6)]
        static public Emitter_Bullet CreateLinearAccelEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "LinearAccelEmitter";
            bullet.EmitInterval = 0.1f;
            bullet.Cooldown = 1f;
            bullet.EmitTimes = 10;
            bullet.Acc = 10;
            bullet.AccDuration = 1f;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/LinearDecelEmitter", false, 6)]
        static public Emitter_Bullet CreateLinearDecelEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "LinearDecelEmitter";
            bullet.EmitInterval = 0.1f;
            bullet.Cooldown = 1f;
            bullet.EmitTimes = 10;
            bullet.InitialVeloc = 10;
            bullet.Acc = -5;
            bullet.AccDuration = 1f;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/LinearAccelAimingEmitter", false, 6)]
        static public Emitter_Bullet CreateLinearAccelAimingEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "LinearAccelAimingEmitter";
            bullet.EmitInterval = 0.1f;
            bullet.Cooldown = 1f;
            bullet.EmitTimes = 10;
            bullet.InitialVeloc = 10;
            bullet.Acc = -5;
            bullet.AccDuration = 1f;
      //      bullet.OrientateByParent = true;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralLeftEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralLeftEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "SpiralLeftEmitter";
            bullet.InitialVeloc = 5;
            bullet.BulletRotatingSpeed = 150f;
            bullet.EmitInterval = 0.02f;
            bullet.Cooldown = 2f;
            bullet.RotatingSpeed = 360f;
            bullet.EmitTimes = 500;
            bullet.EmitNumPerShot = 1;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralRightEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralRightEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "SpiralRightEmitter";
            bullet.InitialVeloc = 5;
            bullet.BulletRotatingSpeed = 150f;
            bullet.EmitInterval = 0.02f;
            bullet.Cooldown = 2f;
            bullet.RotatingSpeed = 360f;
            bullet.EmitTimes = 500;
            bullet.EmitNumPerShot = 1;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralMultiLeftEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralMultiLeftEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "SpiralMultiLeftEmitter";
            bullet.InitialVeloc = 5;
            bullet.BulletRotatingSpeed = 150f;
            bullet.EmitInterval = 0.05f;
            bullet.Cooldown = 5f;
            bullet.RotatingSpeed = -100f;
            bullet.EmitTimes = 100;
            bullet.EmitNumPerShot = 4;
            bullet.DistributionAngle = 90;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralMultiRightEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralMultiRightEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "SpiralMultiRightEmitter";
            bullet.InitialVeloc = 5;
            bullet.BulletRotatingSpeed = 150f;
            bullet.EmitInterval = 0.05f;
            bullet.Cooldown = 5f;
            bullet.RotatingSpeed = 100f;
            bullet.EmitTimes = 100;
            bullet.EmitNumPerShot = 4;
            bullet.DistributionAngle = 90;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralMultiDoubleEmitter", false, 6)]
        static public EmitterParent CreateSpiralMultiDoubleEmitter()
        {
            Emitter_Bullet BL = CreateSpiralMultiLeftEmitter();
            Emitter_Bullet BR = CreateSpiralMultiRightEmitter();
            BL.transform.rotation = Quaternion.Euler(0, 45, 0);
            BR.transform.rotation = Quaternion.Euler(0, 45, 0);
            EmitterParent MD = new GameObject().AddComponent<EmitterParent>();
            MD.name = "SpiralMultiDoubleEmitter";
            BL.transform.parent = MD.transform;
            BR.transform.parent = MD.transform;
            Selection.activeGameObject = MD.gameObject;
            return MD;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralLeftAccelEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralLeftAccelEmitter()
        {
            Emitter_Bullet bullet = CreateSpiralLeftEmitter();
            bullet.InitialVeloc = 1;
            bullet.Acc = 10;
            bullet.AccDuration = 1;
            bullet.name = "SpiralLeftAccelEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralRightAccelEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralRightAccelEmitter()
        {
            Emitter_Bullet bullet = CreateSpiralRightEmitter();
            bullet.InitialVeloc = 1;
            bullet.Acc = 10;
            bullet.AccDuration = 1;
            bullet.name = "SpiralRightAccelEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralMultiLeftAccelEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralMultiLeftAccelEmitter()
        {
            Emitter_Bullet bullet = CreateSpiralMultiLeftEmitter();
            bullet.InitialVeloc = 1;
            bullet.Acc = 10;
            bullet.AccDuration = 1;
            bullet.name = "SpiralMultiLeftAccelEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpiralMultiRightAccelEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralMultiRightAccelEmitter()
        {
            Emitter_Bullet bullet = CreateSpiralMultiRightEmitter();
            bullet.InitialVeloc = 1;
            bullet.Acc = 10;
            bullet.AccDuration = 1;
            bullet.name = "SpiralMultiRightAccelEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/SpirallCrossEmitter", false, 6)]
        static public Emitter_Bullet CreateSpiralCrossEmitter()
        {
            Emitter_Bullet bullet = new GameObject().AddComponent<Emitter_Bullet>();
            bullet.transform.localPosition = Vector3.zero;
            bullet.transform.localRotation = Quaternion.identity;
            bullet.transform.localScale = Vector3.one;
            bullet.name = "SpirallCrossEmitter";
            bullet.EmitInterval = 0.05f;
            bullet.Cooldown = 2f;
            bullet.EmitTimes = 50;
            bullet.EmitNumPerShot = 3;
            bullet.DistributionAngle = 120;
            bullet.InitialVeloc = 10;
            bullet.Acc = -20;
            bullet.AccDuration = 2f;
            bullet.BulletRotatingSpeed = 150f;
            bullet.RotatingSpeed = 360f;
            Selection.activeGameObject = bullet.gameObject;
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/5LinearEmitter", false, 6)]
        static public Emitter_Bullet Create5LinearEmitter()
        {
            Emitter_Bullet bullet = CreateLinearEmitter();
            bullet.EmitNumPerShot = 5;
            bullet.DistributionAngle = 10;
            bullet.name = "5LinearEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/5LinearAccelEmitter", false, 6)]
        static public Emitter_Bullet Create5LinearAccelEmitter()
        {
            Emitter_Bullet bullet = Create5LinearEmitter();
            bullet.InitialVeloc = 1;
            bullet.Acc = 10;
            bullet.AccDuration = 1f;
            bullet.name = "5LinearAccelEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/5LinearDecelEmitter", false, 6)]
        static public Emitter_Bullet Create5LinearDecelEmitter()
        {
            Emitter_Bullet bullet = Create5LinearEmitter();
            bullet.InitialVeloc = 10;
            bullet.Acc = -8;
            bullet.AccDuration = 1f;
            bullet.name = "5LinearDecelEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/5LinearReverseEmitter", false, 6)]
        static public Emitter_Bullet Create5LinearReverseEmitter()
        {
            Emitter_Bullet bullet = Create5LinearEmitter();
            bullet.InitialVeloc = 20;
            bullet.Acc = -20;
            bullet.AccDuration = 5f;
            bullet.Cooldown = 2f;
            bullet.name = "5LinearReverseEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/30WayAllRangeEmitter", false, 6)]
        static public Emitter_Bullet Create30WayAllRangeEmitter()
        {
            Emitter_Bullet bullet = CreateLinearEmitter();
            bullet.EmitNumPerShot = 30;
            bullet.DistributionAngle = 360 / 30;
            bullet.InitialVeloc = 5;
            bullet.EmitInterval = 0.2f;
            bullet.Cooldown = 2f;
            bullet.name = "30WayAllRangeEmitter";
            return bullet;
        }

        [MenuItem("Emitter/BulletEmitter/5LinearRevertEmitter", false, 6)]
        static public Emitter_Bullet Create5LinearRevertEmitter()
        {
            Emitter_Bullet bullet = CreateLinearEmitter();
            bullet.InitialVeloc = 2;
            bullet.EmitInterval = 0.2f;
            bullet.Cooldown = 1f;
            bullet.RotatingSpeed = 30f;
            bullet.RevertAngle = 45f;
            bullet.EmitTimes = 1000;
            bullet.EmitNumPerShot = 5;
            bullet.DistributionAngle = 20;
            bullet.name = "5LinearRevertEmitter";
            return bullet;
        }
        /*
        [MenuItem("Emitter/BulletEmitter/MeshEmitter", false, 6)]
        static public EmitterParent CreateMeshEmitter()
        {
            Emitter_Bullet bullet1 = CreateLinearEmitter();
            bullet1.EmitNumPerShot = 8;
            bullet1.DistributionAngle = 20;
            bullet1.name = "LinearEmitter";
            Emitter_Bullet bullet2 = CreateLinearEmitter();
            bullet2.EmitNumPerShot = 9;
            bullet2.DistributionAngle = 20;
            bullet2.name = "LinearEmitter";

            EmitterParent meshEmitter = new GameObject().AddComponent<EmitterParent>();
            meshEmitter.name = "MeshEmitter";
            bullet1.transform.parent = meshEmitter.transform;
            bullet2.transform.parent = meshEmitter.transform;
            Selection.activeGameObject = meshEmitter.gameObject;
            return meshEmitter;
        }*/

        [MenuItem("Emitter/BulletEmitter/OverTakeEmitter", false, 6)]
        static public EmitterParent CreateOverTakeEmitter()
        {
            EmitterParent OverTakeEmitter = new GameObject().AddComponent<EmitterParent>();
            OverTakeEmitter.name = "OverTakeEmitter";
            OverTakeEmitter.SequentialInterval = 0.2f;
            Selection.activeGameObject = OverTakeEmitter.gameObject;
            for (int i = 0; i < 7; i++)
            {
                Emitter_Bullet bullet = CreateLinearEmitter();
                bullet.InitialVeloc = 2 + i * 0.5f;
                bullet.Cooldown = -1f;
                bullet.EmitTimes = 1;
                bullet.EmitNumPerShot = 6;
                bullet.DistributionAngle = 15;
                bullet.transform.parent = OverTakeEmitter.transform;
            }
            return OverTakeEmitter;
        }

        [MenuItem("Emitter/BulletEmitter/OverTakeCurveEmitter", false, 6)]
        static public EmitterParent CreateOverTakeCurveEmitter()
        {
            EmitterParent OverTakeEmitter = new GameObject().AddComponent<EmitterParent>();
            OverTakeEmitter.name = "OverTakeCurveEmitter";
            OverTakeEmitter.SequentialInterval = 0.2f;
            Selection.activeGameObject = OverTakeEmitter.gameObject;
            for (int i = 0; i < 7; i++)
            {
                Emitter_Bullet bullet = CreateLinearEmitter();
                bullet.transform.rotation = Quaternion.Euler(0, -30 + i * 10, 0);
                bullet.InitialVeloc = 2 + i * 0.5f;
                bullet.Cooldown = -1f;
                bullet.EmitTimes = 1;
                bullet.EmitNumPerShot = 6;
                bullet.DistributionAngle = 15;
                bullet.transform.parent = OverTakeEmitter.transform;
            }
            return OverTakeEmitter;
        }
        #endregion

        #region MissileEmitter
        [MenuItem("Emitter/MissileEmitter/OverTakeCurveEmitter", false, 6)]
        static public EmitterParent CreateOverTakeCurveEmitter3()
        {
            EmitterParent OverTakeEmitter = new GameObject().AddComponent<EmitterParent>();
            OverTakeEmitter.name = "OverTakeCurveEmitter";
            OverTakeEmitter.SequentialInterval = 0.2f;
            Selection.activeGameObject = OverTakeEmitter.gameObject;
            for (int i = 0; i < 7; i++)
            {
                Emitter_Bullet bullet = CreateLinearEmitter();
                bullet.transform.rotation = Quaternion.Euler(0, -30 + i * 10, 0);
                bullet.InitialVeloc = 2 + i * 0.5f;
                bullet.Cooldown = -1f;
                bullet.EmitTimes = 1;
                bullet.EmitNumPerShot = 6;
                bullet.DistributionAngle = 15;
                bullet.transform.parent = OverTakeEmitter.transform;
            }
            return OverTakeEmitter;
        }

        #endregion

        #region LaserEmitter
        [MenuItem("Emitter/LaserEmitter/OverTakeCurveEmitter22", false, 6)]
        static public EmitterParent CreateOverTakeCurveEmitter4()
        {
            EmitterParent OverTakeEmitter = new GameObject().AddComponent<EmitterParent>();
            OverTakeEmitter.name = "OverTakeCurveEmitter";
            OverTakeEmitter.SequentialInterval = 0.2f;
            Selection.activeGameObject = OverTakeEmitter.gameObject;
            for (int i = 0; i < 7; i++)
            {
                Emitter_Bullet bullet = CreateLinearEmitter();
                bullet.transform.rotation = Quaternion.Euler(0, -30 + i * 10, 0);
                bullet.InitialVeloc = 2 + i * 0.5f;
                bullet.Cooldown = -1f;
                bullet.EmitTimes = 1;
                bullet.EmitNumPerShot = 6;
                bullet.DistributionAngle = 15;
                bullet.transform.parent = OverTakeEmitter.transform;
            }
            return OverTakeEmitter;
        }
        #endregion

        #region FireEmitter
        [MenuItem("Emitter/FireEmitter/FireEmitter", false, 6)]
        static public EmitterParent CreateOverTakeCurveEmitter5()
        {
            EmitterParent OverTakeEmitter = new GameObject().AddComponent<EmitterParent>();
            OverTakeEmitter.name = "OverTakeCurveEmitter";
            OverTakeEmitter.SequentialInterval = 0.2f;
            Selection.activeGameObject = OverTakeEmitter.gameObject;
            for (int i = 0; i < 7; i++)
            {
                Emitter_Bullet bullet = CreateLinearEmitter();
                bullet.transform.rotation = Quaternion.Euler(0, -30 + i * 10, 0);
                bullet.InitialVeloc = 2 + i * 0.5f;
                bullet.Cooldown = -1f;
                bullet.EmitTimes = 1;
                bullet.EmitNumPerShot = 6;
                bullet.DistributionAngle = 15;
                bullet.transform.parent = OverTakeEmitter.transform;
            }
            return OverTakeEmitter;
        }

        #endregion
    }
}