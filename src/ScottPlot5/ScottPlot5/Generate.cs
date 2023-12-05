﻿namespace ScottPlot;

#nullable enable

/// <summary>
/// This class contains methods which generate sample data for testing and demonstration purposes
/// </summary>
public static class Generate
{
    public static RandomDataGenerator RandomData { get; } = new(0);

    #region numerical 1D

    /// <summary>
    /// Return an array of evenly-spaced numbers
    /// </summary>
    public static double[] Consecutive(int count = 51, double delta = 1, double first = 0)
    {
        double[] ys = new double[count];
        for (int i = 0; i < ys.Length; i++)
            ys[i] = i * delta + first;
        return ys;
    }

    /// <summary>
    /// Return an array of sine waves between -1 and 1.
    /// Values are multiplied by <paramref name="mult"/> then shifted by <paramref name="offset"/>.
    /// Phase shifts the sine wave horizontally between 0 and 2 Pi.
    /// </summary>
    public static double[] Sin(int count = 51, double mult = 1, double offset = 0, double oscillations = 1, double phase = 0)
    {
        double sinScale = 2 * Math.PI * oscillations / (count - 1);
        double[] ys = new double[count];
        for (int i = 0; i < ys.Length; i++)
            ys[i] = Math.Sin(i * sinScale + phase * Math.PI * 2) * mult + offset;
        return ys;
    }

    /// <summary>
    /// Return an array of cosine waves between -1 and 1.
    /// Values are multiplied by <paramref name="mult"/> then shifted by <paramref name="offset"/>.
    /// Phase shifts the sine wave horizontally between 0 and 2 Pi.
    /// </summary>
    public static double[] Cos(int count = 51, double mult = 1, double offset = 0, double oscillations = 1, double phase = 0)
    {
        double sinScale = 2 * Math.PI * oscillations / (count - 1);
        double[] ys = new double[count];
        for (int i = 0; i < ys.Length; i++)
            ys[i] = Math.Cos(i * sinScale + phase * Math.PI * 2) * mult + offset;
        return ys;
    }

    public static double[] NoisySin(Random rand, int count = 51, double noiseLevel = 1)
    {
        double[] data = Sin(count);
        for (int i = 0; i < data.Length; i++)
        {
            data[i] += rand.NextDouble() * noiseLevel;
        }
        return data;
    }

    public static double[] SquareWave(uint cycles = 20, uint pointsPerCycle = 1_000, double duty = .5, double low = 0, double high = 1)
    {
        if (duty < 0 || duty > 1)
            throw new ArgumentException($"{nameof(duty)} must be in the range [0, 1]");

        uint points = cycles * pointsPerCycle;
        uint cyclePointsHigh = (uint)(pointsPerCycle * duty);
        uint cyclePointsLow = pointsPerCycle - cyclePointsHigh;

        double[] values = new double[points];

        uint i = 0;

        for (int c = 0; c < cycles; c++)
        {
            for (int p = 0; p < cyclePointsLow; p++)
                values[i++] = low;

            for (int p = 0; p < cyclePointsHigh; p++)
                values[i++] = high;
        }

        return values;
    }

    public static double[] Zeros(int count)
    {
        return Repeating(count, 0);
    }

    public static double[] Ones(int count)
    {
        return Repeating(count, 1);
    }

    public static double[] Repeating(int count, double value)
    {
        double[] values = new double[count];

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = value;
        }

        return values;
    }

    #endregion

    #region numerical 2D

    /// <summary>
    /// Generates a 2D array of numbers with constant spacing.
    /// </summary>
    /// <param name="rows"></param>
    /// <param name="columns"></param>
    /// <param name="spacing">The space between points.</param>
    /// <param name="offset">The first point.</param>
    /// <returns></returns>
    public static double[,] Consecutive2D(int rows, int columns, double spacing = 1, double offset = 0)
    {
        double[,] data = new double[rows, columns];

        var count = offset;
        for (var y = 0; y < data.GetLength(0); y++)
            for (int x = 0; x < data.GetLength(1); x++)
            {
                data[y, x] = count;
                count += spacing;
            }

        return data;
    }

    /// <summary>
    /// Generates a 2D sine pattern.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="xPeriod">Frequency factor in x direction.</param>
    /// <param name="yPeriod">Frequency factor in y direction.</param>
    /// <param name="multiple">Intensity factor.</param>
    public static double[,] Sin2D(int width, int height, double xPeriod = .2, double yPeriod = .2, double multiple = 100)
    {
        double[,] intensities = new double[height, width];

        for (int y = 0; y < height; y++)
        {
            double siny = Math.Cos(y * yPeriod) * multiple;
            for (int x = 0; x < width; x++)
            {
                double sinx = Math.Sin(x * xPeriod) * multiple;
                intensities[y, x] = sinx + siny;
            }
        }

        return intensities;
    }

    /// <summary>
    /// Generate a 2D array in a diagonal gradient pattern
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static double[,] Ramp2D(int width, int height, double min = 0, double max = 1)
    {
        double[,] intensities = new double[height, width];

        double span = max - min;

        for (int y = 0; y < height; y++)
        {
            double fracY = (double)y / height;
            double valY = fracY * span + min;

            for (int x = 0; x < width; x++)
            {
                double fracX = (double)x / width;
                double valX = fracX * span + min;

                intensities[y, x] = (valX + valY) / 2;
            }
        }

        return intensities;
    }

    #endregion

    #region numerical random

    /// <summary>
    /// Return a series of values starting with <paramref name="offset"/> and
    /// each randomly deviating from the previous by at most <paramref name="mult"/>.
    /// </summary>
    public static double[] RandomWalk(int count, double mult = 1, double offset = 0)
    {
        return RandomData.RandomWalk(count, mult, offset);
    }

    /// <summary>
    /// Return an array of <paramref name="count"/> random values 
    /// from <paramref name="min"/> to <paramref name="max"/>
    /// </summary>
    public static double[] Random(int count, double min = 0, double max = 1)
    {
        return Enumerable.Range(0, count)
            .Select(_ => RandomData.RandomNumber(min, max))
            .ToArray();
    }

    public static double[] RandomNormal(int count, double mean = 0, double stdDev = 1)
    {
        return Enumerable.Range(0, count)
            .Select(_ => RandomData.RandomNormalNumber(mean, stdDev))
            .ToArray();
    }

    /// <summary>
    /// Return a copy of the given array with random values added to each point
    /// </summary>
    public static double[] AddNoise(double[] input, double magnitude = 1)
    {
        double[] output = new double[input.Length];
        Array.Copy(input, 0, output, 0, input.Length);
        AddNoiseInPlace(output, magnitude);
        return output;
    }

    /// <summary>
    /// Mutate the given array by adding a random value to each point
    /// </summary>
    public static void AddNoiseInPlace(double[] values, double magnitude = 1)
    {
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i] + RandomData.RandomNumber(-magnitude / 2, magnitude / 2);
        }
    }

    #endregion

    #region DateTime

    /// <summary>
    /// Contains methods for generating DateTime sequences
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// Date of the first ScottPlot commit
        /// </summary>
        public static readonly System.DateTime ExampleDate = new(2018, 01, 03);

        /// <summary>
        /// Evenly-spaced DateTimes
        /// </summary>
        public static System.DateTime[] Consecutive(int count, System.DateTime start, TimeSpan step)
        {
            System.DateTime dt = start;
            System.DateTime[] values = new System.DateTime[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = dt;
                dt += step;
            }
            return values;
        }

        public static System.DateTime[] Weekdays(int count, System.DateTime start)
        {
            System.DateTime[] dates = new System.DateTime[count];
            int i = 0;
            while (i < count)
            {
                if (start.DayOfWeek != DayOfWeek.Saturday && start.DayOfWeek != DayOfWeek.Sunday)
                {
                    dates[i] = start;
                    i++;
                }

                start = start.AddDays(1);
            }
            return dates;
        }

        public static System.DateTime[] Weekdays(int count) => Weekdays(count, ExampleDate);

        public static System.DateTime[] Days(int count, System.DateTime start) => Consecutive(count, start, TimeSpan.FromDays(1));

        public static System.DateTime[] Days(int count) => Days(count, ExampleDate);

        public static System.DateTime[] Hours(int count, System.DateTime start) => Consecutive(count, start, TimeSpan.FromHours(1));

        public static System.DateTime[] Hours(int count) => Hours(count, ExampleDate);

        public static System.DateTime[] Minutes(int count, System.DateTime start) => Consecutive(count, start, TimeSpan.FromMinutes(1));

        public static System.DateTime[] Minutes(int count) => Hours(count, ExampleDate);

        public static System.DateTime[] Seconds(int count, System.DateTime start) => Consecutive(count, start, TimeSpan.FromSeconds(1));

        public static System.DateTime[] Seconds(int count) => Hours(count, ExampleDate);
    }

    #endregion
}
