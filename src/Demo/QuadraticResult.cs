using System.Collections.Immutable;

namespace Demo
{
    public sealed class QuadraticResult
    {
        public readonly ImmutableArray<double> Solution;

        private QuadraticResult(ImmutableArray<double> solution) => 
            Solution = solution;

        private QuadraticResult(double[] solution) :
            this(solution.ToImmutableArray())
        {
        }

        public static QuadraticResult Create(double[] solution) => new(solution);
    }
}
