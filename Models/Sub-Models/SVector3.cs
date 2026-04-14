using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace JailPlugin.Models.Sub_Models
{
    public class SVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public SVector3()
        {
            
        }
        public SVector3(Vector3 Source)
        {
            X = Source.x;
            Y = Source.y;
            Z = Source.z;
        }
        public SVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3 ToVector3() => new Vector3(X, Y, Z);
    }
}
