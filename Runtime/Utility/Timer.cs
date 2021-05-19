using System;
using UnityEngine;

namespace Saro.BT.Utility
{
    [Serializable]
    public class Timer
    {
        [Min(0)]
        public float interval = 1f;

        [Tooltip("Adds a random range value to the interval between [-Deviation, +Deviation]")]
        [Min(0)]
        public float deviation = 0.1f;

        public float TTL { get; private set; } = 0f;
        public bool AutoRestart { get; set; } = false;

        public event Action OnTimeout;

        public void Start()
        {
            TTL = interval;

            if (deviation > 0.033f)
            {
                TTL += UnityEngine.Random.Range(-deviation, deviation);
            }
        }

        public void Tick(float delta)
        {
            if (TTL > 0f)
            {
                TTL -= delta;
                if (IsDone)
                {
                    OnTimeout?.Invoke();
                    if (AutoRestart)
                    {
                        Start();
                    }
                }
            }
        }

        public bool IsDone => TTL <= 0f;

        public bool IsRunning => !IsDone;

        /// <summary>
        /// for 
        /// <see cref="BTRunTimeValueAttribute"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TTL.ToString();
        }

        public string GetIntervalInfo()
        {
            if (deviation > 0.033f)
                return $"{interval}±{deviation}";
            else
                return interval.ToString();
        }
    }
}