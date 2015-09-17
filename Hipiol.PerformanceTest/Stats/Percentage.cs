using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hipiol.PerformanceTest.Stats
{
    /// <summary>
    /// Percentage distribution over values.
    /// </summary>
    class Percentage
    {
        internal static readonly IEnumerable<double> StandardPercentage = new[] { 0.01, 0.02, 0.03, 0.05, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 0.98, 0.99, 1.0 };

        /// <summary>
        /// How many percents are available.
        /// </summary>
        internal int PercentCount { get { return _percents.Length; } }

        /// <summary>
        /// Percents where the distribution will be computed.
        /// </summary>
        private readonly double[] _percents;

        /// <summary>
        /// Values which distribution is provided.
        /// </summary>
        private readonly double[] _values;

        internal Percentage(IEnumerable<double> values, params double[] percents)
        {
            _percents = percents.ToArray();

            _values = values.OrderBy(v => v).ToArray();

            if (_values.Length <= 0)
                throw new NotSupportedException("Cannot compute percentage of empty sequence");
        }

        internal Percentage(IEnumerable<double> values, IEnumerable<double> percents)
            : this(values, percents.ToArray())
        {

        }

        /// <summary>
        /// Gets how many values is smaller than given percentage id.
        /// </summary>
        /// <param name="percentId">The percentage id.</param>
        /// <returns>Count of values.</returns>
        internal int GetCount(int percentId)
        {
            var threshold = GetThreshold(percentId);
            var index = 0;
            while (index<_values.Length && _values[index] <= threshold )
                ++index;

            return index;
        }

        /// <summary>
        /// Gets threshold for given percentage id
        /// </summary>
        /// <param name="percentId">The percentage id.</param>
        /// <returns>The threshold.</returns>
        internal double GetThreshold(int percentId)
        {
            var max = _values.Last();

            if (percentId >= _percents.Length)
                return max;

            var percent = _percents[percentId];
            var threshold = max * percent;

            return threshold;
        }

        /// <summary>
        /// Gets percent for given percentage id
        /// </summary>
        /// <param name="percentId">The percentage id.</param>
        /// <returns>The percent.</returns>
        internal double GetPercent(int percentId)
        {
            if (percentId >= _percents.Length)
                return 1.0;

            return _percents[percentId];
        }
    }
}
