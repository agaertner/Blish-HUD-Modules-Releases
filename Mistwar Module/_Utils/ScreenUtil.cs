using Microsoft.Xna.Framework;
using System;

namespace Nekres.Mistwar
{
    internal class ScreenUtil
    {
        public static Vector3 WorldToLocal(Vector3 aCamPos, Quaternion aCamRot, Vector3 aPos)
        {
            return Vector3.Zero; //Quaternion.Inverse(aCamRot) * (aPos - aCamPos);
        }
        public static Vector3 Project(Vector3 aPos, float aFov, float aAspect)
        {
            float f = (float) (1f / Math.Tan(aFov * MathUtil.DegToRad * 0.5f));
            f /= aPos.Z;
            aPos.X *= f / aAspect;
            aPos.Y *= f;
            return aPos;
        }
        public static Vector3 ClipSpaceToViewport(Vector3 aPos)
        {
            aPos.X = aPos.X * 0.5f + 0.5f;
            aPos.Y = aPos.Y * 0.5f + 0.5f;
            return aPos;
        }

        public static Vector3 WorldToViewport(Vector3 aCamPos, Quaternion aCamRot, float aFov, float aAspect, Vector3 aPos)
        {
            Vector3 p = WorldToLocal(aCamPos, aCamRot, aPos);
            p = Project(p, aFov, aAspect);
            return ClipSpaceToViewport(p);
        }

        public static Vector3 WorldToScreenPos(Vector3 aCamPos, Quaternion aCamRot, float aFov, float aScrWidth, float aScrHeight, Vector3 aPos)
        {
            Vector3 p = WorldToViewport(aCamPos, aCamRot, aFov, aScrWidth / aScrHeight, aPos);
            p.X *= aScrWidth;
            p.Y *= aScrHeight;
            return p;
        }

        public static Vector3 WorldToGUIPos(Vector3 aCamPos, Quaternion aCamRot, float aFov, float aScrWidth, float aScrHeight, Vector3 aPos)
        {
            Vector3 p = WorldToScreenPos(aCamPos, aCamRot, aFov, aScrWidth, aScrHeight, aPos);
            p.Y = aScrHeight - p.Y;
            return p;
        }
    }
}
