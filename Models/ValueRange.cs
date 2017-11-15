using System;
namespace HappyTokenApi.Models
{
    public class ValueRange
    {
        public int Base { get; set; }
        public int RandomMax { get; set; }

        public int GetNewRandomValue()
        {
            var random = new Random();
            return this.Base + random.Next(0, this.RandomMax);
        }

        public int GetNewValueWithProportion(float proportion)
        {
            return (int)(this.Base + this.RandomMax * Math.Clamp(proportion, 0, 1));
        }
    }
}
