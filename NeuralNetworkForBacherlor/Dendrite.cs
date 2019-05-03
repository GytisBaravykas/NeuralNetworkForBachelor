namespace NeuralNetworkForBacherlor
{
    public class Dendrite
    {
        public double Weight { get; set; }

        public Dendrite()
        {
            this.Weight = CryptoRandom.RandomValue;
        }
    }

}
