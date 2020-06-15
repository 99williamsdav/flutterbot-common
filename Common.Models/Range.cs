using System;

namespace Common.Models
{
    public class Range<T> where T : struct, IComparable<T>
    {
        public Range()
        {
        }

        public Range(T? min, T? max)
        {
            Minimum = min;
            Maximum = max;
        }

        public T? Minimum { get; set; }

        public T? Maximum { get; set; }

        public override string ToString()
        {
            return $"{Minimum} - {Maximum}";
        }

        public bool IsWithinRange(T? val)
        {
            if (!val.HasValue)
            {
                return false;
            }

            if (Minimum == null)
            {
                return val.Value.CompareTo(Maximum.Value) <= 0;
            }
            else if (Maximum == null)
            {
                return val.Value.CompareTo(Minimum.Value) >= 0;
            }

            return val.Value.CompareTo(Minimum.Value) >= 0 && val.Value.CompareTo(Maximum.Value) <= 0;
        }

        public Range<T> Merge(Range<T> other)
        {
            var min = Max(this.Minimum, other.Minimum);
            var max = Min(this.Maximum, other.Maximum);
            // TODO handle min > max differently?
            return min.HasValue && max.HasValue && min.Value.CompareTo(max.Value) > 0 ? new Range<T>(max, min) : new Range<T>(min, max);
        }

        private static T? Min(T? val1, T? val2)
        {
            if (val1 == null) return val2;
            if (val2 == null) return val1;
            return val1.Value.CompareTo(val2.Value) <= 0 ? val1 : val2;
        }

        private static T? Max(T? val1, T? val2)
        {
            if (val1 == null) return val2;
            if (val2 == null) return val1;
            return val1.Value.CompareTo(val2.Value) >= 0 ? val1 : val2;
        }
    }
}
