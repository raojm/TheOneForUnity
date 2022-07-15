using System;
using System.Collections.Generic;
using System.Text;

namespace TheOneUnity.Platform.Objects
{
    /// <summary>
    /// Represents a distance between two TheOneGeoPoints.
    /// </summary>
    public struct TheOneGeoDistance
    {
        private const double EarthMeanRadiusKilometers = 6371.0;
        private const double EarthMeanRadiusMiles = 3958.8;

        /// <summary>
        /// Creates a ParseGeoDistance.
        /// </summary>
        /// <param name="radians">The distance in radians.</param>
        public TheOneGeoDistance(double radians)
          : this() => Radians = radians;

        /// <summary>
        /// Gets the distance in radians.
        /// </summary>
        public double Radians { get; private set; }

        /// <summary>
        /// Gets the distance in miles.
        /// </summary>
        public double Miles => Radians * EarthMeanRadiusMiles;

        /// <summary>
        /// Gets the distance in kilometers.
        /// </summary>
        public double Kilometers => Radians * EarthMeanRadiusKilometers;

        /// <summary>
        /// Gets a TheOneGeoDistance from a number of miles.
        /// </summary>
        /// <param name="miles">The number of miles.</param>
        /// <returns>A TheOneGeoDistance for the given number of miles.</returns>
        public static TheOneGeoDistance FromMiles(double miles) => new TheOneGeoDistance(miles / EarthMeanRadiusMiles);

        /// <summary>
        /// Gets a TheOneGeoDistance from a number of kilometers.
        /// </summary>
        /// <param name="kilometers">The number of kilometers.</param>
        /// <returns>A TheOneGeoDistance for the given number of kilometers.</returns>
        public static TheOneGeoDistance FromKilometers(double kilometers) => new TheOneGeoDistance(kilometers / EarthMeanRadiusKilometers);

        /// <summary>
        /// Gets a TheOneGeoDistance from a number of radians.
        /// </summary>
        /// <param name="radians">The number of radians.</param>
        /// <returns>A TheOneGeoDistance for the given number of radians.</returns>
        public static TheOneGeoDistance FromRadians(double radians) => new TheOneGeoDistance(radians);
    }
}
