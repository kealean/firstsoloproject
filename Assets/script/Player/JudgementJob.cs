using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Script.Player {
    [BurstCompile]
    public struct JudgementJob : IJobFor {
        [ReadOnly] public NativeArray<double> TargetDspTimes;
        [ReadOnly] public double InputTimestamp;

        public NativeArray<int> Results;

        public void Execute(int index) {
            var diff = InputTimestamp - TargetDspTimes[index];
            var absDiff = diff < 0 ? -diff : diff;

            Results[index] = absDiff switch {
                <= 0.016 => 1,
                <= 0.050 => 2,
                <= 0.070 => 3,
                <= 0.100 => 4,
                _ => 5
            };
        }
    }
}