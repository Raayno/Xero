#if SPLINES_1_OR_NEWER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Splines.Interpolators;

namespace AxonGenesis
{
    [ExecuteInEditMode]
    public class SplinePathProvider : PathProvider
    {
        public SplineContainer Container;
        public int SplineIndex = 0;

        public override float Length {
            get {
                if (Container == null) return 0;
                return Container.CalculateLength(SplineIndex);
            }
        }

        protected override void OnAwake()
        {
            base.OnAwake();
            if (Container == null) Container = GetComponent<SplineContainer>();
        }

        public override void Interpolate(float amount, out Vector3 position, out Quaternion rotation)
        {
            if (Container == null) {
                position = Vector3.zero;
                rotation = Quaternion.identity;
                return;
            }

            float3 pos;
            float3 tan;
            float3 up;
            Container.Evaluate(SplineIndex, amount, out pos, out tan, out up);

            position = pos;
            rotation = Quaternion.Euler(tan);
        }
    }
}

#endif