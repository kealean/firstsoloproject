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

            if (absDiff <= 0.016)
                Results[index] = 1;
            else if (absDiff <= 0.050)
                Results[index] = 2;
            else if (absDiff <= 0.070)
                Results[index] = 3;
            else if (absDiff <= 0.100)
                Results[index] = 4;
            else
                Results[index] = 5;
        }
    }
}