using System;
namespace NutmegRunner {

    public class Arity {

        public int Count { get; }
        public bool More { get; }

        public Arity( int count, bool more ) {
            this.Count = count;
            this.More = more;
        }

        public bool HasExactArity( int n ) {
            return !this.More && this.Count == n;
        }

        public string AsString() {
            string more = More ? "+" : "";
            return $"{Count}{more}";
        }
        
    }

}
