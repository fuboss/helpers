using System;

namespace Assets.Code.Tools
{
    [Serializable]
    public struct Triple<F, S, T>
    {
        public readonly F First;
        public readonly S Second;
        public readonly T Third;

        public Triple(F first, S second, T third)
        {
            First = first;
            Second = second;
            Third = third;
        }
    }
}
