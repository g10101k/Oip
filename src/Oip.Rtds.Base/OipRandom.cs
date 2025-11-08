namespace Oip.Rtds.Base;

/// <summary>
/// Provides methods for generating random values based on mathematical functions
/// </summary>
public static class OipRandom
{
    /// <summary>
    /// Generates a sinusoidal value based on the current time, period, and amplitude
    /// </summary>
    /// <param name="periodMinutes">The period of the sine wave in minutes. Must be greater than zero</param>
    /// <param name="amplitude">The amplitude of the sine wave. Affects the peak value of the wave</param>
    /// <returns>The sinusoidal value calculated based on the current time, period, and amplitude</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="periodMinutes"/> is less than or equal to zero</exception>
    public static double Sinusoid(double periodMinutes = 60, double amplitude = 100)
    {
        if (periodMinutes <= 0)
            throw new ArgumentException("Period must be greater than zero.", nameof(periodMinutes));

        // Get the current time in minutes since the start of the day
        var now = DateTime.Now;
        var minutesSinceMidnight = now.TimeOfDay.TotalMinutes;

        // Convert the period in minutes to radians per minute for the sine calculation
        var radiansPerMinute = 2 * Math.PI / periodMinutes;

        // Calculate the sine value
        var value = Math.Sin(minutesSinceMidnight * radiansPerMinute) * amplitude;

        return value;
    }
    
    
    /// <summary>
    /// Generates a simulated waveform based on the current time.
    /// </summary>
    /// <param name="periodMinutes">The period of the ECG wave (in minutes). Typically, heartbeats occur every 60-90 seconds.</param>
    /// <param name="amplitude">The amplitude of the ECG wave. Controls the intensity of the heartbeat peaks.</param>
    /// <param name="frequency">The frequency of the additional smaller oscillations in the ECG pattern.</param>
    /// <returns>A simulated ECG value based on the current time.</returns>
    public static double Waveform(double periodMinutes = 60, double amplitude = 100, double frequency = 1)
    {
        if (periodMinutes <= 0)
            throw new ArgumentException("Period must be greater than zero.", nameof(periodMinutes));

        // Get the current time in minutes since the start of the day
        var now = DateTime.Now;
        var minutesSinceMidnight = now.TimeOfDay.TotalMinutes;

        // Base sinusoidal pattern simulating the main pulse of the heartbeat
        var radiansPerMinute = 2 * Math.PI / periodMinutes;
        var baseValue = Math.Sin(minutesSinceMidnight * radiansPerMinute) * amplitude;

        // Adding additional oscillations to simulate the complex waveform of a real ECG signal
        // This is achieved by adding smaller sine waves at different frequencies
        var complexPattern = 0.3 * Math.Sin(minutesSinceMidnight * (2 * Math.PI / (periodMinutes / frequency)));
        complexPattern += 0.2 * Math.Sin(minutesSinceMidnight * (2 * Math.PI / (periodMinutes / (frequency * 2))));

        // Combine base value with additional oscillations to form the ECG-like signal
        var ecgValue = baseValue + complexPattern;

        return ecgValue;
    }
}