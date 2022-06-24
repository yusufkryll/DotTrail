using System;
using Ra.Trail;
using UnityEngine;


namespace Ra
{
    public static class RaUtils
    {
        public static float ToFloat(this object obj)
        {
            return float.Parse(obj.ToString());
        }
        public static int ToInt(this object obj)
        {
            return int.Parse(obj.ToString());
        }
        public static bool ToBool(this object obj)
        {
            return bool.Parse(obj.ToString());
        }
    }
    

    public static class Vectors
    {
        public static Vector3 x(float value) => new Vector3(value, 0, 0);
        public static Vector3 y(float value) => new Vector3(0, value, 0);
        public static Vector3 z(float value) => new Vector3(0, 0, value);
        public static Vector3 mouseInputDelta => Application.platform == RuntimePlatform.Android || 
                                                 Application.platform == RuntimePlatform.IPhonePlayer
            ? new Vector3(Input.GetTouch(0).deltaPosition.x, Input.GetTouch(0).deltaPosition.y, 0)
            : new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
        
        public static Vector3 mouseInputDeltaHorizontal => new Vector3(mouseInputDelta.x, 0, mouseInputDelta.y);

        public static Vector3 standardInputDelta => new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        public static Vector3 standardInputDeltaRaw => new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        public static Vector3 standardInputDeltaHorizontal => new Vector3(standardInputDelta.x, 0, standardInputDelta.y);
        public static Vector3 standardInputDeltaRawHorizontal => new Vector3(standardInputDeltaRaw.x, 0, standardInputDeltaRaw.y);
    }

    [Serializable]
    public class Vector3Range
    {
        public float xMin, xMax, yMin, yMax, zMin, zMax;
        public Vector3Range(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.zMin = zMin;
            this.zMax = zMax;
        }
    }
}