using System;
using System.Collections.Generic;
using System.Text;

namespace FakeUnityEngine {
    public enum PlayerCharacter {
        Unknown,
        GunnarDavis,
        JaneFray,
        Ragnar
    }


    public class Debug {
        public static void Log(string log) {
            Console.WriteLine("Log: " + log);
        }

        public static void LogError(string log) {
            Console.WriteLine("Error: " + log);
        }

        public static void LogWarning(string log) {
            Console.WriteLine("Warning: " + log);
        }

        public static void LogException(Exception log) {
            Console.WriteLine("Exception: " + log);
        }
    }

    public class Vector2 {
        public float x, y;

        public static Vector2 zero = new Vector2(0.0f);

        public Vector2() {
            x = 0.0f;
            y = 0.0f;
        }
        
        public Vector2(float param) {
            x = param;
            y = param;
        }
    }

    public class Vector3 {
        public float x, y, z;

        public static Vector3 zero = new Vector3(0.0f);

        public Vector3() {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
        }

        public Vector3(float param) {
            x = param;
            y = param;
            z = param;
        }
    }

    public class Quaternion {
        public float x, y, z, w;

        public static Quaternion identity = new Quaternion(0.0f);

        public Quaternion() {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
            w = 0.0f;
        }

        public Quaternion(float param) {
            x = param;
            y = param;
            z = param;
            w = param;
        }
    }

    class SerializeField { }
}
