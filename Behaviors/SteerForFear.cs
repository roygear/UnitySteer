//#define ANNOTATE_REPULSION
#define SUPPORT_2D

using UnityEngine;

namespace UnitySteer.Behaviors
{
    /// <summary>
    /// Post-processing that steers a vehicle to be afraid of some points, and repulsed by them.
    /// </summary>
    /// <remarks>
    /// This is only an experimental behaviour at this point and doesn't provide a 
    /// lot of configuration options. It is likely to change in the near future.
    /// </remarks>
    [AddComponentMenu("UnitySteer/Steer/... for Fear")]
    public class SteerForFear : Steering
    {
#region Private fields

        private int _currentEventIndex;

        private float _minDistanceForFearSqr;

        /// <summary>
        /// Where were the repulsive events located
        /// </summary>
#if SUPPORT_2D
        private Vector2[] _eventLocations;
#else
        private Vector3[] _eventLocations;
#endif

        /// <summary>
        /// Time that the repulsive events took place
        /// </summary>	
        private float[] _eventTimes;


        /// <summary>
        /// The maximum number of events that will be considered
        /// </summary>
        [SerializeField] private int _maxEvents = 5;

        /// <summary>
        /// How much should the time delta between now and when the event was 
        /// recorded affect the weight of an event.
        /// </summary>
        [SerializeField] private float _timeDeltaWeight = 1f;

        [SerializeField] private float _minDistanceForFear = 3f;

        [SerializeField] private float _estimationTime = 1;

#endregion

        public override bool IsPostProcess
        {
            get { return true; }
        }


        protected override void Start()
        {
            base.Start();

#if SUPPORT_2D
            _eventLocations = new Vector2[_maxEvents];
#else
            _eventLocations = new Vector3[_maxEvents];
#endif

            _eventTimes = new float[_maxEvents];
            _minDistanceForFearSqr = _minDistanceForFear * _minDistanceForFear;
        }

#if SUPPORT_2D
        protected override Vector2 CalculateForce()
        {
            Profiler.BeginSample("Accumulating repulsion");
            var accumulator = Vector2.zero;
            var totalCount = 0;
            var now = Time.time;
            var futurePosition = Vehicle.PredictFutureDesiredPosition(_estimationTime);

            for (var i = 0; i < _maxEvents; i++)
            {
                var timeDelta = now - _eventTimes[i];
                if (timeDelta > 0)
                {
                    var posDelta = futurePosition - _eventLocations[i];
                    if (posDelta.sqrMagnitude < _minDistanceForFearSqr)
                    {
                        totalCount++;
                        accumulator += posDelta / (timeDelta * _timeDeltaWeight);
#if ANNOTATE_REPULSION
				Debug.DrawLine(futurePosition, _eventLocations[i], Color.red / (timeDelta * _timeDeltaWeight));
#endif
                    }
                }
            }

            if (totalCount > 0)
            {
                accumulator.Normalize();
            }

#if ANNOTATE_REPULSION
	Debug.DrawLine(position, position + accumulator, Color.yellow);
	Debug.DrawLine(position, futurePosition, Color.blue);
	Debug.DrawLine(position + accumulator, futurePosition, Color.magenta);
#endif
            Profiler.EndSample();

            return accumulator;
        }
#else
        protected override Vector3 CalculateForce()
        {
            Profiler.BeginSample("Accumulating repulsion");
            var accumulator = Vector3.zero;
            var totalCount = 0;
            var now = Time.time;
            var futurePosition = Vehicle.PredictFutureDesiredPosition(_estimationTime);

            for (var i = 0; i < _maxEvents; i++)
            {
                var timeDelta = now - _eventTimes[i];
                if (timeDelta > 0)
                {
                    var posDelta = futurePosition - _eventLocations[i];
                    if (posDelta.sqrMagnitude < _minDistanceForFearSqr)
                    {
                        totalCount++;
                        accumulator += posDelta / (timeDelta * _timeDeltaWeight);
#if ANNOTATE_REPULSION
				Debug.DrawLine(futurePosition, _eventLocations[i], Color.red / (timeDelta * _timeDeltaWeight));
#endif
                    }
                }
            }

            if (totalCount > 0)
            {
                accumulator.Normalize();
            }

#if ANNOTATE_REPULSION
	Debug.DrawLine(position, position + accumulator, Color.yellow);
	Debug.DrawLine(position, futurePosition, Color.blue);
	Debug.DrawLine(position + accumulator, futurePosition, Color.magenta);
#endif
            Profiler.EndSample();

            return accumulator;
        }
#endif

        public void AddEvent(Vector3 location)
        {
            _eventLocations[_currentEventIndex] = location;
            _eventTimes[_currentEventIndex] = Time.time;
            if (++_currentEventIndex >= _maxEvents)
            {
                _currentEventIndex = 0;
            }
        }
    }
}