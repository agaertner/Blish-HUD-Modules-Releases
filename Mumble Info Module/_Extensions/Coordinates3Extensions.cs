﻿using System.Numerics;
using Gw2Sharp.Models;
using Gw2Sharp.WebApi.V2.Models;

namespace Nekres.Mumble_Info
{
    internal static class Coordinates3Extensions
    {
        private const float INCH_TO_METER = 0.0254F;

        public static Coordinates3 SwapYZ(this Coordinates3 coords) {
            return new Coordinates3(coords.X, coords.Z, coords.Y);
        }

        public static Coordinates2 ToPlane(this Coordinates3 coords) {
            return new Coordinates2(coords.X, coords.Y);
        }

        public static Coordinates3 ToUnit(this Coordinates3 coords, CoordsUnit fromUnit, CoordsUnit toUnit)
        {
            if (fromUnit == CoordsUnit.Meters && toUnit == CoordsUnit.Inches)
                return new Coordinates3(coords.X / INCH_TO_METER, coords.Y / INCH_TO_METER, coords.Z / INCH_TO_METER);
            else if (fromUnit == CoordsUnit.Inches && toUnit == CoordsUnit.Meters)
                return new Coordinates3(coords.X * INCH_TO_METER, coords.Y * INCH_TO_METER, coords.Z * INCH_TO_METER);
            return coords;
        }

        public static Coordinates3 ToMapCoords(this Coordinates3 coords, CoordsUnit fromUnit)
        {
            coords = coords.ToUnit(fromUnit, CoordsUnit.GameWorld);
            return new Coordinates3(coords.X, coords.Y, coords.Z);
        }

        public static Coordinates3 ToContinentCoords(this Coordinates3 coords, CoordsUnit fromUnit, Rectangle mapRectangle, Rectangle continentRectangle)
        {
            var mapCoords = coords.ToMapCoords(fromUnit);
            double x = ((mapCoords.X - mapRectangle.TopLeft.X) / mapRectangle.Width * continentRectangle.Width) + continentRectangle.TopLeft.X;
            double z = ((1 - ((mapCoords.Z - mapRectangle.BottomRight.Y) / mapRectangle.Height)) * continentRectangle.Height) + continentRectangle.TopRight.Y;
            return new Coordinates3(x, mapCoords.Y, z);
        }

        public static Vector3 ToVector3(this Coordinates3 coords)
        {
            return new Vector3((float)coords.X, (float)coords.Y,(float)coords.Z);
        }

    }

    public enum CoordsUnit
    {
        Inches,
        GameWorld = Inches,

        Meters,
        Mumble = Meters
    }
}
